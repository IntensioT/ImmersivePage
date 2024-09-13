const express = require("express");
const path = require("path");
const bodyParser = require('body-parser');
const multer = require('multer');

const keys = require("./config/keys.js");
const mongoose = require('mongoose');

const app = express();

//To tell express to use EJS as the default view engine
app.set("view engine", "ejs");

// Указываем путь к папке с статическими файлами
app.use(express.static(path.join(__dirname, "public")));
//////////////////////////////////////////////////////////////////
// body-parser
app.use(bodyParser.urlencoded({ extended: true }));

// Настройка multer для загрузки файлов
const storage = multer.diskStorage({
  destination: function (req, file, cb) {
    cb(null, 'public/uploads/');
  },
  filename: function (req, file, cb) {
    cb(null, Date.now() + path.extname(file.originalname));
  }
});

const upload = multer({ storage: storage });

// Setting DB up
mongoose.connect(keys.mongoURI);

// Model config
const taskSchema = new mongoose.Schema({
  title: String,
  status: String,
  dueDate: Date,
  file: String
});
const Task = mongoose.model('tasks', taskSchema);

// // Массив задач
// let tasks = [];

// Маршруты

app.post('/add-task', upload.single('file'), async (req, res) => {
  const { title, status, dueDate } = req.body;
  const file = req.file ? req.file.filename : null;
  const newTask = new Task({ title, status, dueDate, file });
  await newTask.save();
  // tasks.push({ title, status, dueDate, file });
  res.redirect('/');
});

app.post('/filter-tasks', async (req, res) => {
  const { status, overdue } = req.body;
  let filter = {};

  if (status) {
    filter.status = status;
  }

  if (overdue === 'overdue') {
    filter.dueDate = { $lt: new Date() };
  } else if (overdue === 'upcoming') {
    filter.dueDate = { $gte: new Date() };
  }

  const tasks = await Task.find(filter).sort({ dueDate: 1 });
  res.render(path.join(__dirname, "public", "pages", "tasks.ejs"), { tasks });
});

app.post('/update-task', async (req, res) => {
  const { index, title, status, dueDate } = req.body;
  console.log('Update task called with:', req.body);
  try {
    await Task.findByIdAndUpdate(index, { title, status, dueDate });
    res.redirect('/');
  } catch (error) {
    console.error('Error updating task:', error);
    res.status(500).send('Error updating task');
  }
});


/////////////////////////////////////////////////////////////////
app.get("/", async (req, res) => {
  const template = req.query.template || 'tasks';
  console.log(`Template parameter: ${template}`); 

  if (template === 'index') {
    const data = {
      username: "Intensio Tempest",
    };
    res.render(path.join(__dirname, "public", "pages", "index.ejs"), data);
  } else if (template === 'tasks') {
    const tasks = await Task.find();
    res.render((path.join(__dirname, "public", "pages", "tasks.ejs")), { tasks: tasks });
  } else if (template === 'slider') {
    res.render(path.join(__dirname, "public", "pages", "slider.ejs"));
  } else {
    res.status(400).send('Invalid template parameter');
  }
  // res.send("Hello World default!");
});



app.listen(keys.port, () => {
  console.log(`Server running at http://127.0.0.1:${keys.port}/`);
});
