const mongoose = require("mongoose");
const Task = mongoose.model("tasks");
const express = require("express");
const cors = require("cors");

const multer = require("multer");
const path = require("path");
const fs = require("fs");

const authMiddleware = require('../middlewares/authMiddleware');

const uploadDir = path.join(__dirname, "public", "uploads");
if (!fs.existsSync(uploadDir)) {
  fs.mkdirSync(uploadDir, { recursive: true });
}

const storage = multer.diskStorage({
  destination: function (req, file, cb) {
    cb(null, uploadDir);
  },
  filename: function (req, file, cb) {
    cb(null, Date.now() + path.extname(file.originalname));
  },
});

// Отправляем обновление задач всем подключенным клиентам
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

module.exports = (app) => {
  ////////////////////////////////////////////////////

  //////////////////////////////////////////////////

  //Routes
  const upload = multer({ storage: storage });

  app.use(express.json());

  app.post("/tasks/upload", upload.single("file"), (req, res) => {
    if (!req.file) {
      return res.status(400).send("No file uploaded.");
    }
    res.status(200).send("File uploaded successfully: " + req.file.path);
  });

  app.post("/tasks/add-task", upload.single("file"), async (req, res) => {
    try {
      console.log(req.body);
      const { title, status, dueDate, filePath } = req.body;
      const file = req.file ? req.file.filename : null; 

      const newTask = new Task({ title, status, dueDate, file, filePath });
      await newTask.save();

      broadcastTasksUpdate();

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

      broadcastTasksUpdate();

      res
        .status(200)
        .json({ message: "Task updated successfully", task: updatedTask });
    } catch (error) {
      console.error("Error updating task:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.get("/tasks", authMiddleware, async (req, res) => {
    try {
      const tasks = await Task.find();
      res.status(200).json({ tasks });
    } catch (error) {
      console.error("Error fetching tasks:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.delete("/tasks/delete-task", async (req, res) => {
    try {
        const { _id } = req.body;
        const deletedTask = await Task.findByIdAndDelete(_id);
        if (!deletedTask) {
            return res.status(404).json({ message: "Task not found" });
        }

        broadcastTasksUpdate();

        res.status(200).json({ message: "Task deleted successfully", task: deletedTask });
    } catch (error) {
        console.error("Error deleting task:", error);
        res.status(500).json({ message: "Internal server error" });
    }
});


  app.get("/tasks/download/:filename", (req, res) => {
    const filePath = path.join(__dirname, "public", "uploads", req.params.filename);
    if (fs.existsSync(filePath)) {
      res.download(filePath, (err) => {
        if (err) {
          console.error("Error downloading file:", err);
          res.status(500).send("Error downloading file");
        }
      });
    } else {
      res.status(404).send("File not found");
    }
  });
};
