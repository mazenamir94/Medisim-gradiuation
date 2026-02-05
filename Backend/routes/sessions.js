const router = require("express").Router();
const { z } = require("zod");
const auth = require("../middleware/auth");

const SessionModel = require("../models/Session");
const { createSession, getSession, removeSession } = require("../services/sessionStore");

router.use(auth);

// START
router.post("/start", (req, res) => {
  const schema = z.object({
    sessionId: z.string().min(4),
    procedureType: z.string(),
    startedAt: z.string()
  });

  const parsed = schema.safeParse(req.body);
  if (!parsed.success) return res.status(400).json({ ok: false, error: parsed.error.errors });

  const { sessionId, startedAt } = parsed.data;
  createSession(req.userId, sessionId, startedAt);

  res.json({ ok: true });
});

// EVENT
router.post("/event", (req, res) => {
  const schema = z.object({
    sessionId: z.string(),
    type: z.enum(["STEP", "TOOL", "DRILL_SAMPLE"]),
    payload: z.any()
  });

  const parsed = schema.safeParse(req.body);
  if (!parsed.success) return res.status(400).json({ ok: false, error: parsed.error.errors });

  const { sessionId, type, payload } = parsed.data;
  const ev = getSession(req.userId, sessionId);
  if (!ev) return res.status(404).json({ ok: false, error: "Session not found" });

  let result = null;

  if (type === "STEP") {
    ev.setStep(payload.step);
    result = ev.evaluateToolAllowed();
  } else if (type === "TOOL") {
    ev.setTool(payload.toolId);
    result = ev.evaluateToolAllowed();
  } else if (type === "DRILL_SAMPLE") {
    result = ev.drillSample(payload.depthMm, payload.angleDeg);
  }

  const shouldWarn = result && result.applied > 0;

  // send evidence as STRING to make Unity easy
  const lastMistake = shouldWarn ? ev.mistakes[ev.mistakes.length - 1] : null;

  res.json({
    ok: true,
    warn: !!shouldWarn,
    errorType: shouldWarn ? lastMistake.errorType : null,
    evidence: shouldWarn ? JSON.stringify(lastMistake.evidence) : null
  });
});

// END
router.post("/end", async (req, res) => {
  const schema = z.object({
    sessionId: z.string(),
    procedureType: z.string(),
    endedAt: z.string()
  });

  const parsed = schema.safeParse(req.body);
  if (!parsed.success) return res.status(400).json({ ok: false, error: parsed.error.errors });

  const { sessionId, procedureType, endedAt } = parsed.data;
  const ev = getSession(req.userId, sessionId);
  if (!ev) return res.status(404).json({ ok: false, error: "Session not found" });

  ev.end(endedAt);
  const summary = ev.finalize();

  const doc = await SessionModel.create({
    userId: req.userId,
    procedureType,
    startedAt: summary.startedAt,
    endedAt: summary.endedAt,
    durationSec: summary.durationSec,
    finalScore: summary.finalScore,
    mistakesSummary: summary.mistakesSummary,
    metricsSummary: summary.metricsSummary,
    rubricVersion: "v1"
  });

  removeSession(req.userId, sessionId);

  res.json({
    ok: true,
    sessionDbId: doc._id.toString(),
    durationSec: summary.durationSec,
    finalScore: summary.finalScore,
    mistakesSummary: summary.mistakesSummary,
    metricsSummary: summary.metricsSummary
  });
});

// HISTORY (optional)
router.get("/me", async (req, res) => {
  const sessions = await SessionModel.find({ userId: req.userId })
    .sort({ createdAt: -1 })
    .limit(50);

  res.json({ ok: true, sessions });
});

module.exports = router;



