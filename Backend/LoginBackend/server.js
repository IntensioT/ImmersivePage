const express = require("express");
const keys = require("./config/keys.js");
const bodyParser = require("body-parser");
const cors = require("cors");

const app = express();

const corsOptions = {
  origin: "http://127.0.0.1:5052",
  methods: "GET,POST", 
  allowedHeaders: "Content-Type,Authorization", 
};

app.use(cors());

// Some middleware that will parse string in request
// body-parser
app.use(bodyParser.urlencoded({ extended: false }));

// Setting DB up
const mongoose = require("mongoose");
mongoose.connect(keys.mongoURI);

//Set up DB model
require("./models/account.js");

//Set up the routes
require("./routes/authenticationRoutes.js")(app);

app.listen(keys.port, () => {
  console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});
