require("dotenv").config();
const fs = require("fs");
const path = require("path");
const mongoose = require("mongoose");
const BookChunk = require("../models/BookChunk");

function chunkText(text, chunkSize = 2000, overlap = 300) {
  const chunks = [];
  let i = 0;
  while (i < text.length) {
    chunks.push(text.slice(i, i + chunkSize));
    i += (chunkSize - overlap);
  }
  return chunks;
}

(async () => {
  await mongoose.connect(process.env.MONGODB_URI);
  console.log("MongoDB connected for ingestion.");

  const dir = path.join(__dirname, "..", "data", "books");
  if (!fs.existsSync(dir)) {
    console.error("Folder not found:", dir);
    process.exit(1);
  }

  const files = fs.readdirSync(dir).filter(f => f.toLowerCase().endsWith(".txt"));
  if (files.length === 0) {
    console.error("No .txt files found in:", dir);
    process.exit(1);
  }

  for (const file of files) {
    const fullPath = path.join(dir, file);
    const text = fs.readFileSync(fullPath, "utf8");

    const chunks = chunkText(text, 2000, 300);

    await BookChunk.deleteMany({ source: file });
    await BookChunk.insertMany(chunks.map((t, idx) => ({
      source: file,
      chunkIndex: idx,
      text: t
    })));

    console.log(`✅ Ingested ${file}: ${chunks.length} chunks`);
  }

  const count = await BookChunk.countDocuments();
  console.log("✅ Total chunks in DB:", count);

  process.exit(0);
})();

