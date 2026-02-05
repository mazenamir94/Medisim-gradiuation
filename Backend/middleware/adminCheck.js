const User = require("../models/User");

module.exports = async function (req, res, next) {
  try {
    // req.userId comes from the previous 'auth' middleware
    const user = await User.findById(req.userId);
    if (!user || user.role !== "admin") {
      return res.status(403).json({ ok: false, error: "Access denied. Admins only." });
    }
    next();
  } catch (err) {
    res.status(500).json({ ok: false, error: "Server error checking admin." });
  }
};
