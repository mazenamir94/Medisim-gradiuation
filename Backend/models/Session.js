const mongoose = require("mongoose");

const SessionSchema = new mongoose.Schema({
  userId: { type: mongoose.Schema.Types.ObjectId, required: true, index: true },
  procedureType: { type: String, required: true },

  startedAt: { type: Date, required: true },
  endedAt: { type: Date, required: true },
  durationSec: { type: Number, required: true },

  finalScore: { type: Number, required: true },
  rubricVersion: { type: String, default: "v1" },

  mistakesSummary: { type: Object, default: {} },
  metricsSummary: { type: Object, default: {} },

  createdAt: { type: Date, default: Date.now }
});

module.exports = mongoose.model("Session", SessionSchema);

