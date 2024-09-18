const mongoose = require('mongoose');
const { Schema } = mongoose;

const taskSchema = new mongoose.Schema({
  title: String,
  status: String,
  dueDate: Date,
  file: String,
  filaPath: String
});
mongoose.model('tasks', taskSchema);