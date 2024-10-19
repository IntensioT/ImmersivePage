const {
  GraphQLObjectType,
  GraphQLString,
  GraphQLSchema,
  GraphQLID,
  GraphQLList,
} = require("graphql");
const keys = require("../config/keys.js");
const mongoose = require("mongoose");
mongoose.connect(keys.mongoURI);
const Task = mongoose.model("tasks");

const jwt = require("jsonwebtoken");
const argon2 = require("argon2");

const Account = mongoose.model("accounts");

const authMiddlewareGraphQL = require("../middlewares/authMiddleware");

const UserType = new GraphQLObjectType({
  name: "User",
  fields: () => ({
    username: { type: GraphQLString },
    adminFlag: { type: GraphQLString },
    accessToken: { type: GraphQLString },
    refreshToken: { type: GraphQLString },
  }),
});

// Task Type Definition
const TaskType = new GraphQLObjectType({
  name: "Task",
  fields: () => ({
    _id: { type: GraphQLID },
    title: { type: GraphQLString },
    status: { type: GraphQLString },
    dueDate: { type: GraphQLString },
    file: { type: GraphQLString },
    filePath: { type: GraphQLString },
  }),
});

// Root Query
// const RootQuery = new GraphQLObjectType({
//     name: "RootQueryType",
//     fields: {
//       tasks: {
//         type: new GraphQLList(TaskType),
//         resolve(parent, args, context) {
//           const user = authMiddlewareGraphQL(context); // Проверяем токен
//           if (!user) {
//             throw new Error("Unauthorized");
//           }
//           return Task.find(); // Возвращаем задачи только для авторизованных пользователей
//         }
//       },
//       task: {
//         type: TaskType,
//         args: { id: { type: GraphQLID } },
//         resolve(parent, args, context) {
//           const user = authMiddlewareGraphQL(context); // Проверяем токен
//           if (!user) {
//             throw new Error("Unauthorized");
//           }
//           return Task.findById(args.id); // Возвращаем задачу только для авторизованных пользователей
//         }
//       }
//     }
//   });
const RootQuery = new GraphQLObjectType({
  name: "RootQueryType",
  fields: {
    tasks: {
      type: new GraphQLList(TaskType),
      args: {
        status: { type: GraphQLString }, // Добавляем фильтр по статусу
      },
      resolve(parent, args, context) {
        const user = authMiddlewareGraphQL(context); // Проверяем токен
        if (!user) {
          throw new Error("Unauthorized");
        }

        // Создаем объект фильтра
        const filter = {};
        if (args.status) {
          filter.status = args.status;
        }

        // Возвращаем задачи с применением фильтра
        return Task.find(filter);
      }
    },
    task: {
      type: TaskType,
      args: { id: { type: GraphQLID } },
      resolve(parent, args, context) {
        const user = authMiddlewareGraphQL(context); // Проверяем токен
        if (!user) {
          throw new Error("Unauthorized");
        }
        return Task.findById(args.id); // Возвращаем задачу по ID
      }
    }
  }
});


// Mutations
const Mutation = new GraphQLObjectType({
  name: "Mutation",
  fields: {
    login: {
      type: UserType,
      args: {
        username: { type: GraphQLString },
        password: { type: GraphQLString },
      },
      async resolve(parent, args) {
        const { username, password } = args;

        const userAccount = await Account.findOne({ username });
        if (!userAccount) {
          throw new Error("Invalid credentials");
        }

        const isPasswordValid = await argon2.verify(
          userAccount.password,
          password
        );
        if (!isPasswordValid) {
          throw new Error("Invalid credentials");
        }

        const accessToken = jwt.sign(
          { username: userAccount.username, adminFlag: userAccount.adminFlag },
          keys.jwtSecret,
          { expiresIn: "3m" }
        );

        const refreshToken = jwt.sign(
          { username: userAccount.username },
          keys.jwtRefreshSecret,
          { expiresIn: "7d" }
        );

        userAccount.refreshToken = refreshToken;
        await userAccount.save();

        return {
          username: userAccount.username,
          adminFlag: userAccount.adminFlag,
          accessToken,
          refreshToken,
        };
      },
    },

    // Мутация для обновления токена
    refreshToken: {
      type: GraphQLString,
      args: {
        refreshToken: { type: GraphQLString },
      },
      async resolve(parent, args) {
        const { refreshToken } = args;
        try {
          const decoded = jwt.verify(refreshToken, keys.jwtRefreshSecret);
          const userAccount = await Account.findOne({
            username: decoded.username,
          });

          if (!userAccount || userAccount.refreshToken !== refreshToken) {
            throw new Error("Unauthorized: Invalid refresh token");
          }

          const newAccessToken = jwt.sign(
            {
              username: userAccount.username,
              adminFlag: userAccount.adminFlag,
            },
            keys.jwtSecret,
            { expiresIn: "3m" }
          );

          return newAccessToken;
        } catch (err) {
          throw new Error("Unauthorized: Invalid refresh token");
        }
      },
    },

    addTask: {
      type: TaskType,
      args: {
        title: { type: GraphQLString },
        status: { type: GraphQLString },
        dueDate: { type: GraphQLString },
        file: { type: GraphQLString },
        filePath: { type: GraphQLString },
      },
      resolve(parent, args) {
        const newTask = new Task({
          title: args.title,
          status: args.status,
          dueDate: args.dueDate,
          file: args.file,
          filePath: args.filePath,
        });
        return newTask.save();
      },
    },
    updateTask: {
      type: TaskType,
      args: {
        id: { type: GraphQLID },
        title: { type: GraphQLString },
        status: { type: GraphQLString },
        dueDate: { type: GraphQLString },
      },
      resolve(parent, args) {
        return Task.findByIdAndUpdate(
          args.id,
          {
            title: args.title,
            status: args.status,
            dueDate: args.dueDate,
          },
          { new: true }
        );
      },
    },
    deleteTask: {
      type: TaskType,
      args: { id: { type: GraphQLID } },
      resolve(parent, args) {
        return Task.findByIdAndDelete(args.id);
      },
    },
  },
});

module.exports = new GraphQLSchema({
  query: RootQuery,
  mutation: Mutation,
});
