# ImmersivePage Project

This project is developed as part of the _Modern Programming Platforms_ course. The project consists of a **Node.js server** and a **Unity WebGL client**.

The server is built using **Node.js** with **Express** and **MongoDB**, while the client is built in **Unity** with a WebGL interface. Due to tight deadlines for some of the lab assignments, I had to make certain compromises in the architecture of the application. Although the project is functional, the code requires refactoring to improve its structure and maintainability.

## Current Features

Here are some of the features and tasks completed so far:

- Running **Node.js server** with **Express** and **body-parser**, utilizing configuration files based on the environment.
- **Nodemon** is used to enable live development without restarting the server.
- A **MongoDB cluster** has been created, and the server is successfully connected to the database.
- **MongoDB models** for user accounts have been implemented.
- Basic routes for **account login** are functional.
- **Unity** user interface scene with background music has been created.
- Unity can send **web requests** to the server and receive and correctly parse the responses.
- **argon2** is used for secure password hashing.
- Passwords are transferred securely in **POST requests**.
- Implemented proper **deserialization** of server responses on the Unity client.
- Response data is limited to reduce information exposure.
- Implemented **regex-based password validation**.
- Compression for Unity assets using the **.gz** format.
- **CORS** issues resolved to enable communication between the client and server.

## Future Improvements

Although the current version works, the codebase needs refactoring to improve overall architecture, code quality, and maintainability.

Feel free to explore the code, and if you'd like to contribute or suggest improvements, issues and pull requests are welcome!
