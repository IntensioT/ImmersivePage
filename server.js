const express = require("express");
const path = require("path");

const app = express();
const port = 3000;

//To tell express to use EJS as the default view engine
app.set("view engine", "ejs");

// Указываем путь к папке с статическими файлами
app.use(express.static(path.join(__dirname, "public")));

app.get("/", (req, res) => {
  const template = req.query.template || 'islands';
  console.log(`Template parameter: ${template}`);

  if (template === 'index') {
    const data = {
      username: "Intensio Tempest",
    };
    res.render(path.join(__dirname, "public", "pages", "index.ejs"), data);
  } else if (template === 'tasks') {
    // res.render("tasks", );
  } else if (template === 'islands') {
    res.render(path.join(__dirname, "public", "pages", "islands.ejs"));
  } else {
    res.status(400).send('Invalid template parameter');
  }
  // res.send("Hello World default!");
});

app.listen(port, () => {
  console.log(`Server running at http://127.0.0.1:${port}/`);
});
