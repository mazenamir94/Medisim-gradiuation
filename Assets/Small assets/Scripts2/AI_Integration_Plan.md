# AI Connection & Knowledge Plan

## 1. Fix "No Response" Issue
The current AI connection script works "blindly" and uses fragile text searching that might fail if the server answers slightly differently.
**Code Changes:**
- Update `LLMConnector.cs` to use **Unity JsonUtility**. This is the standard, crash-proof way to read JSON.
- Add a **Debug Mode** that prints exactly what the server says (even errors) to the Console.
- **Model Name Fix**: Change the model name to "local-model". Most tools (LM Studio, Ollama) treat this as "use whatever is currently running", preventing "Model not found" errors.

## 2. "Upload Books" (Context Injection)
You cannot literally "upload" a PDF to this simple connection, but we can **feed the book text** to the AI's memory before it answers.
**Code Changes:**
- Create a new script `DentalKnowledge.cs`.
- Add a large text area where you can paste important chapters (e.g., "Cavity Preparation Rules", "Bonding Steps").
- When the session finishes, we send: `[Book Context] + [Student Performance]`.
- The AI will read the rules first, then judge the student.

## 3. Simplified Prompts
The user requested "simple things".
**Action:**
- Change the "System Prompt" to be strict: "You are a teacher. Be short. Be kind. Use the provided book rules."

---

## Technical Steps to Perform Now
1. [ ] Refactor `LLMConnector.cs` for robust JSON parsing.
2. [ ] Add `DentalKnowledge.cs` for book text.
3. [ ] Connect them in `DentalSessionManager.cs`.
