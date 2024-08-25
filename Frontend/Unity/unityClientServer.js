// unityClientServer.js
const express = require('express');
const path = require('path');
const app = express();
const compression = require('compression');

// Включение сжатия unity требует
app.use(compression());

// app.use(express.static(path.join(__dirname, 'public')));

// Сервирование статических файлов из папки public
app.use(express.static(path.join(__dirname, 'public'), {
    setHeaders: (res, path) => {
      if (path.endsWith('.wasm.gz')) {
        res.set('Content-Encoding', 'gzip');
        res.set('Content-Type', 'application/wasm');
      } else if (path.endsWith('.wasm')) {
        res.set('Content-Type', 'application/wasm');
      } else if (path.endsWith('.gz')) {
        res.set('Content-Encoding', 'gzip');
      }
    }
  }));
  

app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
  });
  
  app.get('/api/example', (req, res) => {
    res.json({ message: 'Hello from the server!' });
  });

const PORT = process.env.PORT || 5052;
app.listen(PORT, () => {
  console.log(`Client Server running at http://127.0.0.1:${PORT}/`);
});
