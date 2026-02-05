using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatbotUIController : MonoBehaviour
{
    [Header("Config")]
    public BackendConfig config;

    [Header("UI")]
    public GameObject ChatPanel;
    public TMP_InputField MessageInput;
    public Button SendButton;
    public TextMeshProUGUI ChatLog;
    public UnityEngine.UI.ScrollRect ScrollRect;

    // --- FIXED: At the top level ---
    public BackendClient _client;

    private void Awake()
    {
        if (SendButton) SendButton.onClick.AddListener(() => _ = OnSend());
        if (ChatLog) ChatLog.text = "";
    }

    private void Start()
    {
        // 0. Auto-find client if missing
        if (_client == null) _client = FindFirstObjectByType<BackendClient>();

        // 1. Try to load the token we saved in the login scene
        string savedToken = PlayerPrefs.GetString("UserToken", "");

        // 2. If we found a token, log in automatically!
        if (!string.IsNullOrEmpty(savedToken))
        {
            SetJwtToken(savedToken);
        }
        else
        {
            if (ChatPanel != null) ChatPanel.SetActive(true);
            Append("⚠️ Debug: No login token found. Did you log in?");
        }
    }

    public void SetJwtToken(string token)
    {
        if (_client == null)
        {
            Debug.LogError("BackendClient is missing on ChatbotUIController!");
            return;
        }

        _client.SetToken(token);

        if (ChatPanel != null) ChatPanel.SetActive(true);
        Append("✅ Chat ready. Ask me anything.");
    }

    private async Task OnSend()
    {
        if (_client == null || string.IsNullOrWhiteSpace(_client.GetToken()))
        {
            Append("❌ Not logged in. Please login first.");
            return;
        }

        string msg = MessageInput.text;
        if (string.IsNullOrWhiteSpace(msg)) return;

        MessageInput.text = "";
        Append($"You: {msg}");
        Append("Bot: ...");

        var resp = await _client.Chat(msg);

        if (!string.IsNullOrEmpty(resp.error))
        {
            Append("⚠️ Error: " + resp.error);
            return;
        }

        if (!resp.ok)
        {
            Append("⚠️ Chat failed. " + resp.citations); // utilizing citations as raw fallback
            return;
        }

        Append("Bot: " + resp.answer);
    }

    private void Append(string line)
    {
        if (ChatLog == null) return;
        ChatLog.text += (ChatLog.text.Length > 0 ? "\n" : "") + line;

        // Force scroll to bottom
        Canvas.ForceUpdateCanvases();
        if (ScrollRect != null)
            ScrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }
}