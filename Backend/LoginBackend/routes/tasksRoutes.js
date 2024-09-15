const mongoose = require("mongoose");
const Task = mongoose.model("tasks");
const express = require("express");
const cors = require("cors");

const multer = require("multer");
const path = require("path");

const storage = multer.diskStorage({
  destination: function (req, file, cb) {
    cb(null, "/public/uploads/");
  },
  filename: function (req, file, cb) {
    cb(null, Date.now() + path.extname(file.originalname));
  },
});

module.exports = (app) => {
  ////////////////////////////////////////////////////

  //////////////////////////////////////////////////

  //Routes
  const upload = multer({ storage: storage });

  app.use(express.json());
  app.use(cors());

  app.post("/tasks/upload", upload.single("file"), (req, res) => {
    if (!req.file) {
      return res.status(400).send("No file uploaded.");
    }
    res.status(200).send("File uploaded successfully: " + req.file.path);
  });

  app.post("/tasks/add-task", upload.single("file"), async (req, res) => {
    try {
      console.log(req.body);
      const { title, status, dueDate, file } = req.body;

      const newTask = new Task({ title, status, dueDate, file });
      await newTask.save();
      res
        .status(201)
        .json({ message: "Task created successfully", task: newTask });
    } catch (error) {
      console.error("Error creating task:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.post("/tasks/filter-tasks", async (req, res) => {
    try {
      const { status, overdue } = req.body;
      console.log({ status, overdue });
      let filter = {};

      if (status == "Status:") {
        res.status(200).json({ tasks });
        return;
      }

      if (status) {
        filter.status = status;
      }

      // if (overdue === "overdue") {
      //   filter.dueDate = { $lt: new Date() };
      // } else if (overdue === "upcoming") {
      //   filter.dueDate = { $gte: new Date() };
      // }

      const tasks = await Task.find(filter).sort({ dueDate: 1 });
      res.status(200).json({ tasks });
    } catch (error) {
      console.error("Error filtering tasks:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.put("/tasks/update-task", async (req, res) => {
    try {
      const { _id, title, status, dueDate } = req.body;
      const updatedTask = await Task.findByIdAndUpdate(
        _id,
        { title, status, dueDate },
        { new: true }
      );
      if (!updatedTask) {
        return res.status(404).json({ message: "Task not found" });
      }
      res
        .status(200)
        .json({ message: "Task updated successfully", task: updatedTask });
    } catch (error) {
      console.error("Error updating task:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.get("/tasks", async (req, res) => {
    try {
      const tasks = await Task.find();
      res.status(200).json({ tasks });
    } catch (error) {
      console.error("Error fetching tasks:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });
};
