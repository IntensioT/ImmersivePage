const mongoose = require('mongoose');
const Account = mongoose.model("accounts");

module.exports = (app) => {
  //Routes
  app.get("/auth", async (req, res) => {
    const { reqUsername, reqPassword } = req.query;

    if (reqUsername == null || reqPassword == null) {
      res.send("Invalid credentials");
      return;
    }

    var userAccount = await Account.findOne({ username: reqUsername });
    if (userAccount == null) {
      //Create new account
      console.log("Create new account...");

      var newAccount = new Account({
        username: reqUsername,
        password: reqPassword,

        lastAuthentication: Date.now(),
      });
      await newAccount.save();

      res.send(newAccount);
      return;
    } else {
      //TODO: next improve for JWT
      if (reqPassword == userAccount.password) {
        userAccount.lastAuthentication = Date.now();
        await userAccount.save();

        console.log("Retrieving account...");
        res.send(userAccount);
        return;
      }
    }

    res.send("smth");
  });
};
