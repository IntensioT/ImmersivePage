const mongoose = require("mongoose");
const Account = mongoose.model("accounts");
const argon2 = require("argon2");

const jwt = require("jsonwebtoken");
const keys = require("../config/keys");


const passwordRegEx = new RegExp("(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{5,})");

module.exports = (app) => {
  ////////////////////////////////////////////////////
  const checkUserExists = async (username) => {
    return await Account.findOne({ username });
  };

  const hashPassword = async (password) => {
    return await argon2.hash(password);
  };

  const verifyPassword = async (hashedPassword, password) => {
    return await argon2.verify(hashedPassword, password);
  };
  //////////////////////////////////////////////////

  //Routes
  app.post("/account/login", async (req, res) => {
    
    const { reqUsername, reqPassword } = req.body;

    if (
      reqUsername == null
      // || !passwordRegEx.test(reqPassword)
    ) {
      return res
        .status(400)
        .json({ message: "Username and password are required" });
    }

    var userAccount = await Account.findOne(
      { username: reqUsername },
      "username adminFlag password"
    );
    if (userAccount == null) {
      return res.status(401).json({ message: "Invalid credentials" });
    }

    try {
      if (await argon2.verify(userAccount.password, reqPassword)) {
        // password match
        userAccount.lastAuthentication = Date.now();
        await userAccount.save();

        const token = jwt.sign(
          { username: userAccount.username, adminFlag: userAccount.adminFlag },
          keys.jwtSecret,
          { expiresIn: 60 * 3}
        );
        const refreshToken = jwt.sign(
          { username: userAccount.username },
          keys.jwtRefreshSecret,
          { expiresIn: "7d" }
        );
        userAccount.refreshToken = refreshToken;
        await userAccount.save();

        // res.setHeader("Authorization", token);
        // res.setHeader("Refresh-Token", refreshToken);
        // console.log("Token set: " + token);
        // console.log("Server send refresh token to user: " + refreshToken);

        // // Create a new object with only the desired fields
        // const responseUserAccount = {
        //   username: userAccount.username,
        //   adminFlag: userAccount.adminFlag,
        // };

        // Set cookies for browser
        res.cookie("refreshToken", token, { httpOnly: true,  sameSite: 'None', secure: true });
        // res.setHeader('Access-Control-Allow-Credentials', 'true');

        // Create a new object with only the desired fields
        const responseUserAccount = {
          username: userAccount.username,
          adminFlag: userAccount.adminFlag,
          accessToken: token,
          refreshToken: refreshToken,
        };

        return res.status(200).json({ data: responseUserAccount });
      } else {
        // password did not match
        return res.status(401).json({ message: "Invalid credentials" });
      }
    } catch (err) {
      console.error("Error hashing password:", err);
      return res.status(500).json({ message: "Internal server error" });
    }
  });

  app.post("/account/refresh-token", async (req, res) => {
    const { refreshToken } = req.body;
    console.log("Server trying to refresh: " + refreshToken);

    if (!refreshToken) {
      return res
        .status(401)
        .json({ message: "Unauthorized: Refresh token required" });
    }

    try {
      const decoded = jwt.verify(refreshToken, keys.jwtRefreshSecret);
      const userAccount = await Account.findOne({ username: decoded.username });
      console.log("Server got Username:" + userAccount);

      if (!userAccount || userAccount.refreshToken !== refreshToken) {
        return res
          .status(401)
          .json({ message: "Unauthorized: Invalid refresh token" });
      }

      const token = jwt.sign(
        { username: userAccount.username, adminFlag: userAccount.adminFlag },
        keys.jwtSecret,
        { expiresIn: 60 * 3 }
      );
      res.setHeader("Authorization", token);
      res.cookie("refreshToken", token, { httpOnly: true,  sameSite: 'None', secure: true });
      console.log("Token refreshed: " + token);
      

      return res.status(200).json({ message: "Token refreshed" });
    } catch (err) {
      return res
        .status(401)
        .json({ message: "Unauthorized: Invalid refresh token" });
    }
  });

  app.post("/account/create", async (req, res) => {
    const { reqUsername, reqPassword } = req.body;

    if (
      reqUsername == null ||
      reqUsername.length < 3 ||
      reqUsername.length > 16
    ) {
      return res
        .status(400)
        .json({ message: "Username and password are required" });
    }
    // if (!passwordRegEx.test(reqPassword))
    // {
    //   return res
    //     .status(422)
    //     .json({ message: "Unsafe password for registration" });
    // }

    var userAccount = await Account.findOne({ username: reqUsername }, "_id");
    if (userAccount != null) {
      return res.status(409).json({ message: "Username is already taken" });
    } else {
      //Create new account
      try {
        const hash = await argon2.hash(reqPassword);

        var newAccount = new Account({
          username: reqUsername,
          password: hash,

          lastAuthentication: Date.now(),
        });

        await newAccount.save();

        // Create a new object with only the desired fields
        const responseNewAccount = {
          username: newAccount.username,
        };
        return res.status(201).json({ data: responseNewAccount });
      } catch (err) {
        console.error("Error hashing password:", err);
        return res.status(500).json({ message: "Internal server error" });
      }
    }
  });
};
