const express = require("express");
const keys = require("./config/keys.js");
const bodyParser = require("body-parser");
const cors = require("cors");
const cookieParser = require("cookie-parser");

const http = require("http");
const { WebSocketServer } = require("ws");

const jwt = require('jsonwebtoken');


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
      if (client.readyState === client.OPEN) { // Используем client.OPEN вместо WebSocket.OPEN
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

  // Обработка входящих сообщений от клиента (опционально)
  ws.on("message", (message) => {
    console.log("Message received from client:", message);
  });
});


// server.on('upgrade', (request, socket, head) => {
//   wss.handleUpgrade(request, socket, head, (ws) => {
//     wss.emit('connection', ws, request);
//   });
// });

const server = http.createServer(app);


server.on('upgrade', (request, socket, head) => {
  const url = new URL(request.url, `http://${"127.0.0.1:5052"}`);
  const token = url.searchParams.get('token');

  if (!token) {
    socket.write('HTTP/1.1 401 Unauthorized\r\n\r\n');
    socket.destroy();
    return;
  }

  jwt.verify(token, keys.jwtSecret, (err, decoded) => {
    if (err) {
      console.log("token is wrong");
      console.log(token);
      socket.write('HTTP/1.1 401 Unauthorized\r\n\r\n');
      socket.destroy();
      return;
    }
    
    // If the token is valid, handle the upgrade
    wss.handleUpgrade(request, socket, head, (ws) => {  
      console.log("token is right");
      wss.emit('connection', ws, request);
    });
  });
});


server.listen(keys.port, () => {
  console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});

// app.listen(keys.port, () => {
//   console.log(`Server running at http://127.0.0.1:${keys.port}/`);
// });
