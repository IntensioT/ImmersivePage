const express = require("express");
const keys = require("./config/keys.js");
const bodyParser = require("body-parser");
const cors = require("cors");
const cookieParser = require("cookie-parser");

const http = require('http');
const { WebSocketServer } = require('ws');


const app = express();

// Create HTTP server
const server = http.createServer(app);

// Create WebSocket server
const wss = new WebSocketServer({ server });

const corsOptions = {
  exposedHeaders: 'Authorization, Refresh-Token',
  allowedHeaders: ['Authorization', 'Refresh-Token', 'Access-Control-Allow-Headers', 'Content-Type', 'Access-Control-Allow-Credentials'],
  methods: 'GET,POST,PUT,DELETE,OPTIONS',
  origin: 'http://127.0.0.1:5052',
  credentials: true
};

app.use(cors(corsOptions));
// app.options('*', cors(corsOptions));

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

// WebSocket connection handler
wss.on('connection', (ws, req) => {
  console.log('New WebSocket connection established');
    
  // Send a message to the client when they connect
  ws.send(JSON.stringify({ message: 'Connected to WebSocket server' }));

  // Handle incoming messages from client
  ws.on('message', (data) => {
    console.log('Message received from client:', data);
  });

  // Example: send updates to client every time a new task is created
  const sendTasksUpdate = async () => {
    try {
      const tasks = await Task.find();
      ws.send(JSON.stringify({ type: 'TASKS_UPDATE', tasks }));
    } catch (error) {
      console.error('Error fetching tasks:', error);
    }
  };

  // Here you can trigger updates based on events (like database changes)
  ws.on('close', () => {
    console.log('Client disconnected');
    // Optionally, clean up resources or listeners
  });
});

// Start the server
server.listen(keys.port, () => {
  console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});

// app.listen(keys.port, () => {
//   console.log(`Server running at http://127.0.0.1:${keys.port}/`);
// });
