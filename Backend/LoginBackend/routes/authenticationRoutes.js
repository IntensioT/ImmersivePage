const mongoose = require("mongoose");
const Account = mongoose.model("accounts");
const argon2 = require("argon2");

module.exports = (app) => {
  //Routes
  app.post("/account/login", async (req, res) => {
    const { reqUsername, reqPassword } = req.body;

    if (reqUsername == null || reqPassword == null) {
      res.send("Invalid credentials");
      return;
    }

    var userAccount = await Account.findOne({ username: reqUsername });
    if (userAccount != null) {
      //TODO: next improve for JWT
      if (reqPassword == userAccount.password) {
        userAccount.lastAuthentication = Date.now();
        await userAccount.save();

        console.log("Retrieving account...");
        res.send(userAccount);
        return;
      }
    }

    res.send("Invalid credentials");
  });

  app.post("/account/create", async (req, res) => {
    const { reqUsername, reqPassword } = req.body;

    if (reqUsername == null || reqPassword == null) {
      res.send("Invalid credentials");
      return;
    }

    var userAccount = await Account.findOne({ username: reqUsername });
    if (userAccount == null) {
      //Create new account
      console.log("Creating new account...");

      try {
        const hash = await argon2.hash(reqPassword);
        
        var newAccount = new Account({
          username: reqUsername,
          password: hash,
  
          lastAuthentication: Date.now(),
        });

        await newAccount.save();

      } catch (err) {
        console.error("Error hashing password:", err);
        res.status(500).json({ message: "Internal server error" });
      }


      res.send(newAccount);
      return;
    } else {
      res.send("Username is already taken");
    }
  });
};
