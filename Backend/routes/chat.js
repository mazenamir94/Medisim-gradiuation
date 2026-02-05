const router = require("express").Router();
const { z } = require("zod");
const auth = require("../middleware/auth");
const BookChunk = require("../models/BookChunk");

// Require login for chat
router.use(auth);

router.post("/", async (req, res) => {
  try {
    // ---------------------------------------------------------
    // 1. INPUT VALIDATION (Fixing the format mismatch)
    // ---------------------------------------------------------
    // Unity sends history as an array of strings: ["User: hi", "Assistant: hello"]
    // We update the schema to accept that.
    const schema = z.object({
      message: z.string().min(1),
      history: z.array(z.string()).optional() 
    });

    const parsed = schema.safeParse(req.body);
    if (!parsed.success) {
      console.log("Validation Error:", parsed.error.errors);
      return res.status(400).json({ ok: false, error: parsed.error.errors });
    }

    const { message, history = [] } = parsed.data;

    // ---------------------------------------------------------
    // 2. INSTANT GREETING CHECK (Fast & Polite)
    // ---------------------------------------------------------
    const lowerMsg = message.toLowerCase().trim();
    // List of common greetings to catch immediately
    const greetings = ["hello", "hi", "hey", "good morning", "how are you", "greetings"];
    
    // Check if message is JUST a greeting or very short greeting
    if (greetings.includes(lowerMsg) || (lowerMsg.length < 15 && lowerMsg.includes("hello"))) {
      return res.json({
        ok: true,
        answer: "Hello! I am your Virtual Dental Tutor. I can help you with procedures, tools, and dental theory. What would you like to learn?",
        citations: []
      });
    }

    // ---------------------------------------------------------
    // 3. RETRIEVE CONTEXT (Database Search)
    // ---------------------------------------------------------
    const retrieved = await BookChunk
      .find(
        { $text: { $search: message } },
        { score: { $meta: "textScore" }, text: 1, source: 1, chunkIndex: 1 }
      )
      .sort({ score: { $meta: "textScore" } })
      .limit(6);

    const contextBlocks = retrieved.map((c, i) =>
      `[#${i + 1}] Source: ${c.source} | Chunk: ${c.chunkIndex}\n${c.text}`
    ).join("\n\n---\n\n");

    // ---------------------------------------------------------
    // 4. PREPARE HISTORY FOR OLLAMA
    // ---------------------------------------------------------
    // We need to convert Unity's "User: msg" strings into Ollama's object format
    const formattedHistory = history.map(h => {
      if (h.startsWith("User:")) return { role: "user", content: h.replace("User: ", "") };
      if (h.startsWith("Assistant:")) return { role: "assistant", content: h.replace("Assistant: ", "") };
      return { role: "user", content: h }; // Fallback
    });

    // ---------------------------------------------------------
    // 5. THE "HYBRID" SYSTEM PROMPT
    // ---------------------------------------------------------
    const systemPrompt = 
      "You are a helpful and friendly Virtual Dental Tutor for a VR simulation. " +
      "RULES:\n" +
      "1. CONTEXT: Use the provided [CONTEXT] blocks to answer dental questions. Cite them like [#1].\n" +
      "2. BAD CONTEXT TRAP: If the user asks a follow-up (like 'explain more', 'why?') and the [CONTEXT] blocks seem unrelated to the [CONVERSATION HISTORY], IGNORE the context. Instead, use your memory of the conversation to elaborate on the previous topic.\n" +
      "3. CONVERSATION: You may chat normally if the user asks for clarification or greets you.\n" +
      "4. HONESTY: If the answer is not in the context and it's a specific medical fact, say 'I don't have that information in my current library.'\n"

    const messages = [
      { role: "system", content: systemPrompt },
      ...formattedHistory.slice(-6), // Keep last 6 turns for memory
      {
        role: "user",
        content: `CONTEXT:\n${contextBlocks || "No specific book context found."}\n\nQUESTION:\n${message}`
      }
    ];

    // ---------------------------------------------------------
    // 6. CALL OLLAMA
    // ---------------------------------------------------------
    const ollamaResp = await fetch("http://localhost:11434/api/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        model: "llama3.1:8b",
        messages,
        stream: false
      })
    });

    if (!ollamaResp.ok) {
      const text = await ollamaResp.text();
      return res.status(500).json({ ok: false, error: "Ollama request failed", details: text });
    }

    const data = await ollamaResp.json();
    const answer = data?.message?.content ?? "";

    // ---------------------------------------------------------
    // 7. RETURN RESPONSE
    // ---------------------------------------------------------
    return res.json({
      ok: true,
      answer,
      citations: retrieved.map((c, idx) => ({
        ref: `#${idx + 1}`,
        source: c.source,
        chunkIndex: c.chunkIndex
      }))
    });

  } catch (err) {
    console.error("CHAT ERROR:", err);
    return res.status(500).json({
      ok: false,
      error: "Chat endpoint failed",
      details: err?.message || String(err)
    });
  }
});

module.exports = router;
