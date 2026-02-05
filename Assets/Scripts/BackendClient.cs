using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BackendClient : MonoBehaviour
{
    // 1. Drag your BackendConfig object here in the Inspector!
    public BackendConfig Config; 

    private string _baseUrl;
    private string _jwtToken;

    // 2. We use Awake() instead of a Constructor for initialization
    private void Awake()
    {
        if (Config != null)
        {
            // Remove trailing slash if present
            _baseUrl = Config.BaseUrl.TrimEnd('/');
        }
        else
        {
            Debug.LogError("BackendClient: Config is missing! Please drag BackendConfig into the slot.");
            _baseUrl = "http://localhost:5000"; // Fallback
        }
    }

    public void SetToken(string jwtToken) => _jwtToken = jwtToken;
    public string GetToken() => _jwtToken;

    // -------------------- DTOs --------------------
    [Serializable] public class RegisterResponse { public bool ok; public string userId; public string error; }
    [Serializable] public class LoginResponse { public bool ok; public string token; public string role; public string error; }

    [Serializable] public class BasicOkResponse { public bool ok; public string error; }

    [Serializable]
    public class UserDTO {
        public string _id;
        public string email;
        public string role;
    }

    [Serializable]
    public class UserListResponse {
        public bool ok;
        public UserDTO[] users;
        public string error;
    }

    [Serializable]
    public class ReportDTO {
        public string _id;
        public string procedureType;
        public float finalScore;
        public int durationSec;
        public string createdAt;
        public UserDTO userId; 
    }

    [Serializable]
    public class ReportListResponse {
        public bool ok;
        public ReportDTO[] sessions;
        public string error;
    }

    [Serializable] public class EventResponse
    {
        public bool ok;
        public bool warn;
        public string errorType;
        public string evidence;
        public string error;
    }
    
    [Serializable] public class ChatResponse
    {
        public bool ok;
        public string answer;
        public string error;
        public string citations;
    }

    [Serializable] public class EndSessionResponse
    {
        public bool ok;
        public string sessionDbId;
        public int durationSec;
        public float finalScore;
        public string mistakesSummary;
        public string metricsSummary;
        public string error;
    }

    // -------------------- Public API --------------------
    public async Task<RegisterResponse> Register(string email, string password)
    {
        string body = "{"
            + $"\"email\":\"{Escape(email)}\","
            + $"\"password\":\"{Escape(password)}\""
            + "}";

        string raw = await PostRawJson("/auth/register", body, includeAuth: false);
        return SafeParse<RegisterResponse>(raw);
    }

    public async Task<LoginResponse> Login(string email, string password)
    {
        string body = "{"
            + $"\"email\":\"{Escape(email)}\","
            + $"\"password\":\"{Escape(password)}\""
            + "}";

        string raw = await PostRawJson("/auth/login", body, includeAuth: false);
        return SafeParse<LoginResponse>(raw);
    }

    public async Task<BasicOkResponse> StartSession(string sessionId, string procedureType, DateTime startedUtc)
    {
        string body = "{"
            + $"\"sessionId\":\"{Escape(sessionId)}\","
            + $"\"procedureType\":\"{Escape(procedureType)}\","
            + $"\"startedAt\":\"{Escape(startedUtc.ToString("o"))}\""
            + "}";

        string raw = await PostRawJson("/sessions/start", body, includeAuth: true);
        return SafeParse<BasicOkResponse>(raw);
    }

    public async Task<EventResponse> SendStepEvent(string sessionId, string stepName)
    {
        string body = "{"
            + $"\"sessionId\":\"{Escape(sessionId)}\","
            + "\"type\":\"STEP\","
            + $"\"payload\":{{\"step\":\"{Escape(stepName)}\"}}"
            + "}";

        string raw = await PostRawJson("/sessions/event", body, includeAuth: true);
        return SafeParseEventResponse(raw);
    }

    public async Task<EventResponse> SendToolEvent(string sessionId, string toolId)
    {
        string body = "{"
            + $"\"sessionId\":\"{Escape(sessionId)}\","
            + "\"type\":\"TOOL\","
            + $"\"payload\":{{\"toolId\":\"{Escape(toolId)}\"}}"
            + "}";

        string raw = await PostRawJson("/sessions/event", body, includeAuth: true);
        return SafeParseEventResponse(raw);
    }

    public async Task<EventResponse> SendDrillSample(string sessionId, float depthMm, float angleDeg)
    {
        string depth = depthMm.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string angle = angleDeg.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string body = "{"
            + $"\"sessionId\":\"{Escape(sessionId)}\","
            + "\"type\":\"DRILL_SAMPLE\","
            + $"\"payload\":{{\"depthMm\":{depth},\"angleDeg\":{angle}}}"
            + "}";

        string raw = await PostRawJson("/sessions/event", body, includeAuth: true);
        return SafeParseEventResponse(raw);
    }

    public async Task<EndSessionResponse> EndSession(string sessionId, string procedureType, DateTime endedUtc)
    {
        string body = "{"
            + $"\"sessionId\":\"{Escape(sessionId)}\","
            + $"\"procedureType\":\"{Escape(procedureType)}\","
            + $"\"endedAt\":\"{Escape(endedUtc.ToString("o"))}\""
            + "}";

        string raw = await PostRawJson("/sessions/end", body, includeAuth: true);
        return SafeParseEndResponse(raw);
    }

    public async Task<ChatResponse> Chat(string message)
    {
        string body = "{"
            + $"\"message\":\"{Escape(message)}\","
            + "\"history\":[]"
            + "}";

        string raw = await PostRawJson("/chat", body, includeAuth: true);
        return SafeParseChatResponse(raw);
    }

    public async Task<UserListResponse> AdminGetUsers(string searchTerm = "")
    {
        string url = "/admin/users";
        if (!string.IsNullOrEmpty(searchTerm)) url += $"?search={UnityWebRequest.EscapeURL(searchTerm)}";

        using var req = UnityWebRequest.Get(_baseUrl + url);
        req.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
            return new UserListResponse { ok = false, error = req.error };

        return JsonUtility.FromJson<UserListResponse>(req.downloadHandler.text);
    }

    public async Task<BasicOkResponse> AdminDeleteUser(string userId)
    {
        using var req = UnityWebRequest.Delete(_baseUrl + $"/admin/users/{userId}");
        req.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();
        
        if (req.result != UnityWebRequest.Result.Success)
            return new BasicOkResponse { ok = false, error = req.error };

        return new BasicOkResponse { ok = true };
    }

    public async Task<ReportListResponse> AdminGetReports()
    {
        using var req = UnityWebRequest.Get(_baseUrl + "/admin/reports");
        req.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
            return new ReportListResponse { ok = false, error = req.error };

        return JsonUtility.FromJson<ReportListResponse>(req.downloadHandler.text);
    }

    public async Task<BasicOkResponse> AdminCreateUser(string email, string password, string role)
    {
        string body = "{"
            + $"\"email\":\"{Escape(email)}\","
            + $"\"password\":\"{Escape(password)}\","
            + $"\"role\":\"{Escape(role)}\""
            + "}";

        // Note: You need to implement /admin/users POST in your Node backend for this to work
        // If you haven't, you can use the /auth/register endpoint, but that logs you in automatically.
        // Ideally, create a route: router.post("/users", ...) in admin.js
        string raw = await PostRawJson("/admin/users", body, includeAuth: true);
        return SafeParse<BasicOkResponse>(raw);
    }

public async Task<BasicOkResponse> AdminUpdateUser(string userId, string newEmail, string newRole)
    {
        string body = "{";
        if (!string.IsNullOrEmpty(newEmail)) body += $"\"email\":\"{Escape(newEmail)}\",";
        if (!string.IsNullOrEmpty(newRole)) body += $"\"role\":\"{Escape(newRole)}\"";
        body = body.TrimEnd(',') + "}";

        string raw = await PostRawJson($"/admin/users/{userId}", body, includeAuth: true); // Uses PATCH
        return SafeParse<BasicOkResponse>(raw);
    }

    // -------------------- Core HTTP --------------------
    private async Task<string> PostRawJson(string path, string rawJsonBody, bool includeAuth)
    {
        // Safety check to ensure BaseURL isn't null if Awake() didn't run yet
        if (string.IsNullOrEmpty(_baseUrl)) 
        {
            if (Config != null) _baseUrl = Config.BaseUrl.TrimEnd('/');
            else return "{\"ok\":false,\"error\":\"Base URL not configured.\"}";
        }

        string url = _baseUrl + path;

        using var req = new UnityWebRequest(url, "POST");
        byte[] payload = Encoding.UTF8.GetBytes(rawJsonBody);

        req.uploadHandler = new UploadHandlerRaw(payload);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (includeAuth)
        {
            if (string.IsNullOrWhiteSpace(_jwtToken))
                return "{\"ok\":false,\"error\":\"Missing JWT token. Login first.\"}";
            req.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        }

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        string text = req.downloadHandler != null ? req.downloadHandler.text : "";

        if (req.result != UnityWebRequest.Result.Success)
        {
            return "{"
                   + "\"ok\":false,"
                   + $"\"error\":\"{Escape($"{req.responseCode} {req.error}")}\","
                   + $"\"evidence\":\"{Escape(text)}\""
                   + "}";
        }

        return text;
    }

    // -------------------- Parsing helpers --------------------
    private static T SafeParse<T>(string raw) where T : class, new()
    {
        try
        {
            var obj = JsonUtility.FromJson<T>(raw);
            return obj ?? MakeError<T>("Parsed null response. Raw: " + raw);
        }
        catch (Exception e)
        {
            return MakeError<T>("JSON parse error: " + e.Message + " | Raw: " + raw);
        }
    }

    private EventResponse SafeParseEventResponse(string raw)
    {
        try
        {
            var obj = JsonUtility.FromJson<EventResponse>(raw);
            if (obj != null)
            {
                if (string.IsNullOrEmpty(obj.evidence) && raw.Contains("\"evidence\""))
                    obj.evidence = raw;
                return obj;
            }
        }
        catch { }

        var fallback = new EventResponse
        {
            ok = raw.Contains("\"ok\":true"),
            warn = raw.Contains("\"warn\":true"),
            errorType = ExtractJsonString(raw, "errorType"),
            error = ExtractJsonString(raw, "error"),
            evidence = raw
        };
        return fallback;
    }

    private ChatResponse SafeParseChatResponse(string raw)
    {
        try
        {
            var obj = JsonUtility.FromJson<ChatResponse>(raw);
            if (obj != null)
            {
                if (string.IsNullOrEmpty(obj.citations) && raw.Contains("\"citations\""))
                    obj.citations = raw;
                return obj;
            }
        }
        catch { }

        return new ChatResponse
        {
            ok = raw.Contains("\"ok\":true"),
            answer = ExtractJsonString(raw, "answer"),
            error = ExtractJsonString(raw, "error"),
            citations = raw
        };
    }


    private EndSessionResponse SafeParseEndResponse(string raw)
    {
        try
        {
            var obj = JsonUtility.FromJson<EndSessionResponse>(raw);
            if (obj != null)
            {
                if (string.IsNullOrEmpty(obj.mistakesSummary) && raw.Contains("\"mistakesSummary\""))
                    obj.mistakesSummary = raw;
                if (string.IsNullOrEmpty(obj.metricsSummary) && raw.Contains("\"metricsSummary\""))
                    obj.metricsSummary = raw;
                return obj;
            }
        }
        catch { }

        var fallback = new EndSessionResponse
        {
            ok = raw.Contains("\"ok\":true"),
            sessionDbId = ExtractJsonString(raw, "sessionDbId"),
            error = ExtractJsonString(raw, "error"),
            mistakesSummary = raw,
            metricsSummary = raw
        };

        fallback.durationSec = ExtractJsonInt(raw, "durationSec");
        fallback.finalScore = ExtractJsonFloat(raw, "finalScore");

        return fallback;
    }

    private static int ExtractJsonInt(string json, string key)
    {
        string needle = $"\"{key}\":";
        int i = json.IndexOf(needle, StringComparison.Ordinal);
        if (i < 0) return 0;
        i += needle.Length;
        while (i < json.Length && (json[i] == ' ')) i++;
        int j = i;
        while (j < json.Length && (char.IsDigit(json[j]) || json[j] == '-')) j++;
        if (j <= i) return 0;
        return int.TryParse(json.Substring(i, j - i), out var v) ? v : 0;
    }

    private static float ExtractJsonFloat(string json, string key)
    {
        string needle = $"\"{key}\":";
        int i = json.IndexOf(needle, StringComparison.Ordinal);
        if (i < 0) return 0f;
        i += needle.Length;
        while (i < json.Length && (json[i] == ' ')) i++;
        int j = i;
        while (j < json.Length && ("0123456789-+.eE".IndexOf(json[j]) >= 0)) j++;
        if (j <= i) return 0f;
        return float.TryParse(json.Substring(i, j - i),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : 0f;
    }

    private static string ExtractJsonString(string json, string key)
    {
        string needle = $"\"{key}\":";
        int i = json.IndexOf(needle, StringComparison.Ordinal);
        if (i < 0) return null;
        i += needle.Length;
        while (i < json.Length && json[i] == ' ') i++;

        if (i < json.Length && json[i] == '"')
        {
            i++;
            int j = json.IndexOf('"', i);
            if (j > i) return json.Substring(i, j - i);
        }
        return null;
    }

    private static T MakeError<T>(string message) where T : class, new()
    {
        var obj = new T();
        var field = typeof(T).GetField("error");
        if (field != null) field.SetValue(obj, message);
        return obj;
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}