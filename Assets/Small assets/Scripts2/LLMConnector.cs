using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro; 

public class LLMConnector : MonoBehaviour
{
    // Updated IP as requested by User for "Anything LLM" compatibility
    private string url = "http://127.0.0.1:1234/v1/chat/completions";
    private string apiKey = "8RSAN5Z-TDE42YK-QPEN5EJ-4GBD1YK"; // Added API Key
    
    [Header("UI Reference")]
    public TextMeshProUGUI chatText; 
    public AutoScroll autoScrollScript; 

    // --- JSON WRAPPERS ---
    [System.Serializable]
    public class Request {
        public string model;
        public Message[] messages;
        public float temperature = 0.7f;
    }

    [System.Serializable]
    public class Message {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Response {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice {
        public Message message;
    }
    // ---------------------

    public void SendToAI(string data)
    {
        if (chatText != null) 
        {
            chatText.text = "Consulting AI Mentor...";
        }
        StartCoroutine(PostRequest(data));
    }

    IEnumerator PostRequest(string data)
    {
        // 1. Create Data Object
        Request requestData = new Request();
        requestData.model = "local-model"; 
        requestData.messages = new Message[] 
        {
            // PROMPT UPDATE: Explicitly ask for citation
            new Message { role = "system", content = "You are a senior dental instructor. Grade the student. Be brief. YOU MUST CITE the provided Book Title and Chapter in your response." },
            new Message { role = "user", content = data }
        };

        // 2. Convert to JSON
        string json = JsonUtility.ToJson(requestData);

        // 3. Send Request
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.timeout = 60; 

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 4. Parse Response safely
                string responseText = request.downloadHandler.text;
                Response responseObj = JsonUtility.FromJson<Response>(responseText);
                
                if (responseObj != null && responseObj.choices != null && responseObj.choices.Length > 0)
                {
                    string content = responseObj.choices[0].message.content;
                    
                    // Remove <think> tags if present
                    content = CleanThinking(content);

                    if (chatText != null) 
                    {
                        chatText.text = content;
                        if (autoScrollScript != null)
                        {
                            yield return new WaitForEndOfFrame();
                            autoScrollScript.ScrollToBottom();
                        }
                    }
                    Debug.Log("AI Response: " + content);
                }
            }
            else
            {
                if (chatText != null) chatText.text = "Connection Error: Check LM Studio.";
                Debug.LogError("LLM Error: " + request.error);
            }
        }
    }

    // Helper to remove internal chain-of-thought
    string CleanThinking(string raw)
    {
        if (raw.Contains("</think>"))
        {
            return raw.Substring(raw.IndexOf("</think>") + 8).Trim();
        }
        return raw;
    }
}