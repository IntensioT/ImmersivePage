const express = require('express');
const keys = require('./config/keys.js');

const app = express();

// Setting DB up
const mongoose = require('mongoose');
mongoose.connect(keys.mongoURI);

//Set up DB model 
require('./models/account.js');

//Set up the routes 
require('./routes/authenticationRoutes.js')(app);

app.listen(keys.port, () => {
    console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});