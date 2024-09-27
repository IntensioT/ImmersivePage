const express = require("express");
const keys = require("./config/keys.js");
const bodyParser = require("body-parser");
const cors = require("cors");
const cookieParser = require("cookie-parser");

const app = express();

const corsOptions = {
  exposedHeaders: 'Authorization, Refresh-Token',
  allowedHeaders: ['Authorization', 'Refresh-Token','Content-Type', 'Access-Control-Allow-Headers', 'Content-Type'],
  methods: 'GET,POST,PUT,DELETE,OPTIONS',
  origin: 'http://127.0.0.1:5052',
  credentials: true
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

app.listen(keys.port, () => {
  console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});
