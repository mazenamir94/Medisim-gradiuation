const mongoose = require("mongoose");

const BookChunkSchema = new mongoose.Schema({
  source: { type: String, required: true },     // filename, e.g. Book1.txt
  chunkIndex: { type: Number, required: true }, // 0..N
  text: { type: String, required: true }
}, { timestamps: true });

// Enables MongoDB keyword search
BookChunkSchema.index({ text: "text" });

module.exports = mongoose.model("BookChunk", BookChunkSchema);

