require("dotenv").config();
const express = require("express");
const cors = require("cors");

const authRoutes = require("./routes/auth");
const sessionRoutes = require("./routes/sessions");
const chatRoutes = require("./routes/chat");
const app = express();

app.use(cors());
app.use(express.json({ limit: "2mb" }));

app.use("/auth", authRoutes);
app.use("/sessions", sessionRoutes);
app.use("/chat", chatRoutes);
app.use("/admin", require("./routes/admin"));

app.get("/health", (req, res) => res.json({ ok: true }));

const mongoose = require("mongoose");
mongoose.connect(process.env.MONGODB_URI).then(() => {
  console.log("MongoDB connected");
  app.listen(process.env.PORT, () =>
    console.log(`API running on http://localhost:${process.env.PORT}`)
  );
});
