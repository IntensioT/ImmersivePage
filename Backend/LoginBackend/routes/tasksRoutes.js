const mongoose = require("mongoose");
const Task = mongoose.model("tasks");

module.exports = (app) => {
  ////////////////////////////////////////////////////

  //////////////////////////////////////////////////

  //Routes

  app.post("/tasks/add-task", async (req, res) => {
    try {
      const { title, status, dueDate } = req.body;
      const newTask = new Task({ title, status, dueDate });
      await newTask.save();
      res
        .status(201)
        .json({ message: "Task created successfully", task: newTask });
    } catch (error) {
      console.error("Error creating task:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.post("/filter-tasks", async (req, res) => {
    try {
      const { status, overdue } = req.body;
      let filter = {};

      if (status) {
        filter.status = status;
      }

      if (overdue === "overdue") {
        filter.dueDate = { $lt: new Date() };
      } else if (overdue === "upcoming") {
        filter.dueDate = { $gte: new Date() };
      }

      const tasks = await Task.find(filter).sort({ dueDate: 1 });
      res.status(200).json({ tasks });
    } catch (error) {
      console.error("Error filtering tasks:", error);
      res.status(500).json({ message: "Internal server error" });
    }
  });

  app.post("/update-task", async (req, res) => {
    try {
      const { index, title, status, dueDate } = req.body;
      const updatedTask = await Task.findByIdAndUpdate(
        index,
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

  app.get('/tasks', async (req, res) => {
    try {
      const tasks = await Task.find();
      res.status(200).json({ tasks });
    } catch (error) {
      console.error('Error fetching tasks:', error);
      res.status(500).json({ message: 'Internal server error' });
    }
  });
};
