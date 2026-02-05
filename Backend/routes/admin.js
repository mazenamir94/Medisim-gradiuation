const router = require("express").Router();
const User = require("../models/User");
const Session = require("../models/Session");
const auth = require("../middleware/auth");
const adminCheck = require("../middleware/adminCheck");

// Protect all routes here: Must be Logged In AND be an Admin
router.use(auth);
router.use(adminCheck);

// --- TAB 1: USER MANAGEMENT ---

// GET: Search Users
router.get("/users", async (req, res) => {
  try {
    const { search } = req.query;
    let query = {};
    
    if (search) {
      query = { email: { $regex: search, $options: "i" } }; // Case-insensitive search
    }

    const users = await User.find(query).select("-passwordHash").sort({ createdAt: -1 });
    res.json({ ok: true, users });
  } catch (err) {
    res.status(500).json({ ok: false, error: err.message });
  }
});

// DELETE: Delete a user
router.delete("/users/:id", async (req, res) => {
  try {
    await User.findByIdAndDelete(req.params.id);
    // Optional: Delete their sessions too?
    await Session.deleteMany({ userId: req.params.id }); 
    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ ok: false, error: err.message });
  }
});

// PATCH: Update User (e.g. promote to admin)
router.patch("/users/:id", async (req, res) => {
  try {
    const { role, email } = req.body;
    const updates = {};
    if (role) updates.role = role;
    if (email) updates.email = email;

    await User.findByIdAndUpdate(req.params.id, updates);
    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ ok: false, error: err.message });
  }
});

// --- TAB 2: REPORTS ---

// GET: All Reports (Sessions)
router.get("/reports", async (req, res) => {
  try {
    // We populate 'userId' to get the email of the student who did the session
    const sessions = await Session.find()
      .populate("userId", "email")
      .sort({ createdAt: -1 })
      .limit(100);

    res.json({ ok: true, sessions });
  } catch (err) {
    res.status(500).json({ ok: false, error: err.message });
  }
});

module.exports = router;
