const { SessionEvaluator } = require("./evaluator");

const active = new Map();
function key(userId, sessionId) {
  return `${userId}:${sessionId}`;
}

function createSession(userId, sessionId, startedAtIso) {
  const ev = new SessionEvaluator();
  ev.start(startedAtIso);
  active.set(key(userId, sessionId), ev);
  return ev;
}

function getSession(userId, sessionId) {
  return active.get(key(userId, sessionId));
}

function removeSession(userId, sessionId) {
  active.delete(key(userId, sessionId));
}

module.exports = { createSession, getSession, removeSession };

