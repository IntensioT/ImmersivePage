const express = require("express");
const path = require("path");
const bodyParser = require('body-parser');
const multer = require('multer');

const app = express();
const port = 3000;

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

// Массив задач
let tasks = [];

// Маршруты

app.post('/add-task', upload.single('file'), (req, res) => {
  const { title, status, dueDate } = req.body;
  const file = req.file ? req.file.filename : null;
  tasks.push({ title, status, dueDate, file });
  res.redirect('/');
});

app.post('/filter-tasks', (req, res) => {
  const { status } = req.body;
  const filteredTasks = status ? tasks.filter(task => task.status === status) : tasks;
  res.render((path.join(__dirname, "public", "pages", "tasks.ejs")), { tasks: filteredTasks });
});


/////////////////////////////////////////////////////////////////
app.get("/", (req, res) => {
  const template = req.query.template || 'tasks';
  console.log(`Template parameter: ${template}`);

  if (template === 'index') {
    const data = {
      username: "Intensio Tempest",
    };
    res.render(path.join(__dirname, "public", "pages", "index.ejs"), data);
  } else if (template === 'tasks') {
    res.render((path.join(__dirname, "public", "pages", "tasks.ejs")), { tasks: tasks });
  } else if (template === 'slider') {
    res.render(path.join(__dirname, "public", "pages", "slider.ejs"));
  } else {
    res.status(400).send('Invalid template parameter');
  }
  // res.send("Hello World default!");
});



app.listen(port, () => {
  console.log(`Server running at http://127.0.0.1:${port}/`);
});
