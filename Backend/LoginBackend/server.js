const express = require("express");
const keys = require("./config/keys.js");
const bodyParser = require("body-parser");
const cors = require("cors");
const cookieParser = require("cookie-parser");

const http = require("http");
const { WebSocketServer } = require("ws");

const jwt = require("jsonwebtoken");

const app = express();

// Create WebSocket server
const wss = new WebSocketServer({ noServer: true });

const corsOptions = {
  exposedHeaders: "Authorization, Refresh-Token",
  allowedHeaders: [
    "Authorization",
    "Refresh-Token",
    "Access-Control-Allow-Headers",
    "Content-Type",
    "Access-Control-Allow-Credentials",
  ],
  methods: "GET,POST,PUT,DELETE,OPTIONS",
  origin: "http://127.0.0.1:5052",
  credentials: true,
};

app.use(cors(corsOptions));

app.use(cookieParser());

// Some middleware that will parse string in request
// body-parser
app.use(bodyParser.urlencoded({ extended: false }), bodyParser.json());
app.use(bodyParser.json());

// Setting DB up
const mongoose = require("mongoose");
mongoose.connect(keys.mongoURI);

//Set up DB model
require("./models/account.js");
require("./models/task.js");

//Set up the routes
require("./routes/authenticationRoutes.js")(app);
require("./routes/tasksRoutes.js")(app);

const Task = mongoose.model("tasks");
// all clients
const clients = new Set();

const broadcastTasksUpdate = async () => {
  try {
    const tasks = await Task.find(); // Получаем задачи из базы данных
    const tasksUpdateMessage = JSON.stringify({ type: "TASKS_UPDATE", tasks });

    // Отправляем обновление всем подключенным клиентам
    clients.forEach((client) => {
      if (client.readyState === client.OPEN) {
        // Используем client.OPEN вместо WebSocket.OPEN
        client.send(tasksUpdateMessage); // Отправляем JSON-сообщение с задачами
      }
    });
  } catch (error) {
    console.error("Error fetching tasks:", error);
  }
};

// new websocket connection
wss.on("connection", (ws) => {
  console.log("New WebSocket client connected");
  clients.add(ws);

  broadcastTasksUpdate();

  ws.on("close", () => {
    console.log("WebSocket client disconnected");
    clients.delete(ws);
  });

  ws.on("message", async (message) => {
    console.log("Raw message buffer:", message);
    const messageStr = Buffer.isBuffer(message) ? message.toString() : message;

    console.log("Message received from client:", messageStr);

    try {
      const parsedMessage = JSON.parse(messageStr); // Parse the string as JSON

      switch (parsedMessage.type) {
        case "REFRESH_TOKEN": {
          const { refreshToken } = parsedMessage;
          console.log("Server trying to refresh: " + refreshToken);

          if (!refreshToken) {
            ws.send(
              JSON.stringify({
                type: "ERROR",
                message: "Unauthorized: Refresh token required",
              })
            );
            return;
          }

          try {
            const decoded = jwt.verify(refreshToken, keys.jwtRefreshSecret);
            const userAccount = await Account.findOne({
              username: decoded.username,
            });
            console.log("Server got Username:" + userAccount);

            if (!userAccount || userAccount.refreshToken !== refreshToken) {
              ws.send(
                JSON.stringify({
                  type: "ERROR",
                  message: "Unauthorized: Invalid refresh token",
                })
              );
              return;
            }

            const newToken = jwt.sign(
              {
                username: userAccount.username,
                adminFlag: userAccount.adminFlag,
              },
              keys.jwtSecret,
              { expiresIn: 60 * 3 } // 3 minutes
            );

            console.log("Token refreshed: " + newToken);
            ws.send(JSON.stringify({ type: "NEW_TOKEN", token: newToken }));
          } catch (err) {
            ws.send(
              JSON.stringify({
                type: "ERROR",
                message: "Unauthorized: Invalid refresh token",
              })
            );
          }
          break;
        }
        case "ADD_TASK":
          await addTask(parsedMessage.data);
          break;
        case "UPDATE_TASK":
          await updateTask(parsedMessage.data);
          break;
        case "DELETE_TASK":
          await deleteTask(parsedMessage.data);
          break;
        case "FILTER_TASKS":
          await filterTasks(parsedMessage.data, ws);
          break;
        case "UPLOAD_FILE":
          await uploadFile(parsedMessage.data, ws);
          break;
        default:
          console.log("Unknown message type:", parsedMessage.type);
      }
    } catch (error) {
      console.error("Error parsing message:", error);
    }
  });
});

//////////////////////////////////////////////////////////////////
const addTask = async (data) => {
  try {
    const { title, status, dueDate, file, filePath } = data;
    const newTask = new Task({ title, status, dueDate, file, filePath });
    await newTask.save();
    await broadcastTasksUpdate();
  } catch (error) {
    console.error("Error creating task:", error);
  }
};

const updateTask = async (data) => {
  try {
    const { _id, title, status, dueDate } = data;
    const updatedTask = await Task.findByIdAndUpdate(
      _id,
      { title, status, dueDate },
      { new: true }
    );
    if (updatedTask) {
      await broadcastTasksUpdate();
    }
  } catch (error) {
    console.error("Error updating task:", error);
  }
};

const deleteTask = async (data) => {
  try {
    const { _id } = data;
    const deletedTask = await Task.findByIdAndDelete(_id);
    if (deletedTask) {
      await broadcastTasksUpdate();
    }
  } catch (error) {
    console.error("Error deleting task:", error);
  }
};

const filterTasks = async (data, ws) => {
  try {
    const { status } = data;
    let filter = {};
    if (status) {
      filter.status = status;
    }
    const tasks = await Task.find(filter).sort({ dueDate: 1 });
    ws.send(JSON.stringify({ type: "FILTERED_TASKS", tasks }));
  } catch (error) {
    console.error("Error filtering tasks:", error);
  }
};

//////////////////////////////////////////////////////////////////

const server = http.createServer(app);

server.on("upgrade", (request, socket, head) => {
  const url = new URL(request.url, `http://${"127.0.0.1:5052"}`);
  const token = url.searchParams.get("token");

  if (!token) {
    console.log("No token provided in WebSocket connection");
    socket.write("HTTP/1.1 401 Unauthorized\r\n\r\n");
    socket.destroy();
    return;
  }

  console.log("Received token:", token);

  wss.handleUpgrade(request, socket, head, (ws) => {
    jwt.verify(token, keys.jwtSecret, (err, decoded) => {
      if (err) {
        if (err.name === "TokenExpiredError") {
          console.log("Token expired:", token);
          ws.send(JSON.stringify({ type: "TOKEN_EXPIRED" }));

          ws.on("message", (newToken) => {
            console.log("Received new token:", newToken);
            jwt.verify(newToken, keys.jwtSecret, (err, decoded) => {
              if (err) {
                console.log("Invalid new token:", newToken);
                ws.close(4001, "Invalid token");
              } else {
                console.log("New token is valid:", decoded);
                wss.emit("connection", ws, request); // Подключение с новым токеном
              }
            });
          });
        } else {
          console.log("Invalid token:", token);
          ws.close(4001, "Invalid token");
        }
        return;
      }

      console.log("Token is valid:", decoded);

      console.log("Token is valid:", decoded);
      wss.emit("connection", ws, request); // Подключение с валидным токеном
    });
  });
});

server.listen(keys.port, () => {
  console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});

// app.listen(keys.port, () => {
//   console.log(`Server running at http://127.0.0.1:${keys.port}/`);
// });
