const express = require('express');
const path = require('path');

const app = express();
const port = 3000;

// Указываем путь к папке с статическими файлами
app.use(express.static(path.join(__dirname, 'public')));

app.get('/', (req, res) => {
  res.send('Hello World default!');
});

app.listen(port, () => {
  console.log(`Server running at http://127.0.0.1:${port}/`);
});

///////////////////////////////////////////////////////////////////
// Assume add.wasm file exists that contains a single function adding 2 provided arguments
// const fs = require('node:fs');

// const wasmBuffer = fs.readFileSync('./public/test.wasm');
// WebAssembly.instantiate(wasmBuffer).then(wasmModule => {
//   // Exported function live under instance.exports
//   const { add } = wasmModule.instance.exports;
//   const sum = add(5, 6);
//   console.log(sum); // Outputs: 11
// });

///////////////////////////////////////////////////////////////////

// const fs = require('fs');
// var source = fs.readFileSync('./public/test.wasm');

// var typedArray = new Uint8Array(source);

// const env = {
//   memoryBase: 0,
//   tableBase: 0,
//   memory: new WebAssembly.Memory({
//     initial: 256
//   }),
//   table: new WebAssembly.Table({
//     initial: 0,
//     element: 'anyfunc'
//   })
// }

// WebAssembly.instantiate(typedArray, {
// env: env
// }).then(result => {
// console.log(util.inspect(result, true, 0));
// console.log(result.instance.exports._add(9, 9));
// }).catch(e => {
// // error caught
// console.log(e);
// });