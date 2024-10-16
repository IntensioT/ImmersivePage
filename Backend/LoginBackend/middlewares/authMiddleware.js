const jwt = require('jsonwebtoken');
const keys = require('../config/keys');

const authMiddleware = (req, res, next) => {
  // console.log("client req headers: " + JSON.stringify(req.headers, null, 2));
  // console.log("client request: " + req.headers.authorization);
  console.log("client request: " + req.cookies.refreshToken);
  const token = req.cookies.refreshToken;

  // const token = req.headers.authorization;

  if (!token) {
    return res.status(401).json({ message: 'Unauthorized: Authentication required' });
  }

  try {
    const decoded = jwt.verify(token, keys.jwtSecret);
    req.user = decoded;
    next();
  } catch (err) {
    return res.status(401).json({ message: 'Unauthorized: Invalid token'});
  }
};

module.exports = authMiddleware;

const authMiddlewareGraphQL = (context) => {
  console.log("client request: " + context.headers.authorization);
  const token = context.headers.authorization;

  if (!token) {
    throw new Error('Unauthorized: Authentication required');
  }

  try {
    const tokenValue = token.startsWith('Bearer ') ? token.slice(7, token.length) : token;
    const decoded = jwt.verify(tokenValue, keys.jwtSecret);
    return decoded; // Возвращаем данные пользователя, если токен валидный
  } catch (err) {
    throw new Error('Unauthorized: Invalid token');
  }
};

module.exports = authMiddlewareGraphQL;
