using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AppController : MonoBehaviour
{
    [Header("Config")]
    public BackendConfig Config;

    // --- FIXED: This is now at the top, not inside a function ---
    public BackendClient _client; 

    [Header("Login UI")]
    public GameObject LoginPanel;
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public Button LoginButton;
    public Button RegisterButton;
    public TextMeshProUGUI StatusText;

    [Header("Session UI")]
    public GameObject SessionPanel;
    public Button StartSessionButton;
    public Button StepDrillingButton;
    public Button ToolWrongButton;
    public Button DrillTooDeepButton;
    public Button EndSessionButton;
    public TextMeshProUGUI OutputText;
    public BackendSessionReporter backendSessionReporter;

    private string _sessionId;
    private readonly string _procedureType = "ClassIComposite";

    private void Awake()
    {
        if (Config == null)
        {
            Debug.LogError("BackendConfig is not assigned.");
            return;
        }

        // UI state
        if(LoginPanel) LoginPanel.SetActive(true);
        if(SessionPanel) SessionPanel.SetActive(false);

        if(StatusText) StatusText.text = "";
        if(OutputText) OutputText.text = "";

        // Hook UI
        if(LoginButton) LoginButton.onClick.AddListener(() => _ = OnLogin());
        if(RegisterButton) RegisterButton.onClick.AddListener(() => _ = OnRegister());

        if(StartSessionButton) StartSessionButton.onClick.AddListener(() => _ = OnStartSession());
        if(StepDrillingButton) StepDrillingButton.onClick.AddListener(() => _ = OnStepDrilling());
        if(ToolWrongButton) ToolWrongButton.onClick.AddListener(() => _ = OnToolWrong());
        if(DrillTooDeepButton) DrillTooDeepButton.onClick.AddListener(() => _ = OnDrillTooDeep());
        if(EndSessionButton) EndSessionButton.onClick.AddListener(() => _ = OnEndSession());
    }

    // Auto-find client if forgot to drag it
    private void Start()
    {
       if (_client == null) _client = FindFirstObjectByType<BackendClient>();
    }

    private async Task OnRegister()
    {
        StatusText.text = "Registering...";
        var email = EmailInput.text.Trim();
        var pass = PasswordInput.text;

        var resp = await _client.Register(email, pass);
        if (!string.IsNullOrEmpty(resp.error))
        {
            StatusText.text = "❌ Register error: " + resp.error;
            return;
        }

        StatusText.text = resp.ok ? $"✅ Registered. userId={resp.userId}" : "❌ Register failed.";
    }

   private async Task OnLogin()
    {
        // --- STEP 1: UI Feedback & Input ---
        StatusText.text = "Logging in...";
        var email = EmailInput.text.Trim();
        var pass = PasswordInput.text;

        // --- STEP 2: Network Request ---
        var resp = await _client.Login(email, pass);

        // --- STEP 3: Error Handling ---
        if (!string.IsNullOrEmpty(resp.error))
        {
            StatusText.text = "❌ Login error: " + resp.error;
            return;
        }

        if (!resp.ok || string.IsNullOrWhiteSpace(resp.token))
        {
            StatusText.text = "❌ Login failed (No token).";
            return;
        }

        // --- STEP 4: Success & Data Persistence ---
        StatusText.text = "✅ Login success. Redirecting...";

        // Save strictly what is needed for the NEXT scene
        PlayerPrefs.SetString("UserToken", resp.token);
        PlayerPrefs.SetString("UserRole", resp.role); 
        PlayerPrefs.Save();

        // Configure the client immediately
        _client.SetToken(resp.token);

        // --- STEP 5: UX Delay (Readability) ---
        await Task.Delay(1000);

        // --- STEP 6: Navigation ---
        if (resp.role == "admin")
        {
            SceneManager.LoadScene("AdminDashboardScene");
        }
        else
        {
            SceneManager.LoadScene("SimulationScene");
        }
    }

    private async Task OnStartSession()
    {
        _sessionId = "sess_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        OutputText.text = $"Starting session {_sessionId}...";

        var resp = await _client.StartSession(_sessionId, _procedureType, DateTime.UtcNow);
        if (!string.IsNullOrEmpty(resp.error))
        {
            OutputText.text = "❌ StartSession error:\n" + resp.error;
            return;
        }

        OutputText.text = "✅ Session started.\n(Score is hidden until you end the session.)";
    }

    private async Task OnStepDrilling()
    {
        if (EnsureSession()) return;
        var resp = await _client.SendStepEvent(_sessionId, "DRILLING");
        PrintEventResult("STEP: DRILLING", resp);
    }

    private async Task OnToolWrong()
    {
        if (EnsureSession()) return;
        var resp = await _client.SendToolEvent(_sessionId, "SCALER");
        PrintEventResult("TOOL: SCALER", resp);
    }

    private async Task OnDrillTooDeep()
    {
        if (EnsureSession()) return;
        var resp = await _client.SendDrillSample(_sessionId, depthMm: 3.2f, angleDeg: 10f);
        PrintEventResult("DRILL_SAMPLE: depth=3.2 angle=10", resp);
    }

    private async Task OnEndSession()
    {
        if (EnsureSession()) return;

        OutputText.text = "Ending session...";
        var resp = await _client.EndSession(_sessionId, _procedureType, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(resp.error))
        {
            OutputText.text = "❌ EndSession error:\n" + resp.error + "\n\nRaw:\n" + resp.mistakesSummary;
            return;
        }

        OutputText.text =
            $"✅ Session Ended\n\n" +
            $"Final Score: {resp.finalScore}\n" +
            $"Duration (sec): {resp.durationSec}\n" +
            $"DB Id: {resp.sessionDbId}\n\n" +
            $"(Details raw JSON stored in backend)";
    }

    private void PrintEventResult(string label, BackendClient.EventResponse resp)
    {
        if (!string.IsNullOrEmpty(resp.error))
        {
            OutputText.text = $"{label}\n\n❌ Error:\n{resp.error}\n\nRaw:\n{resp.evidence}";
            return;
        }

        if (resp.warn)
        {
            OutputText.text = $"{label}\n\n⚠️ WRONG: {resp.errorType}\n\nRaw:\n{resp.evidence}";
        }
        else
        {
            OutputText.text = $"{label}\n\n✅ OK (no feedback)";
        }
    }

    private bool EnsureSession()
    {
        if (string.IsNullOrWhiteSpace(_sessionId))
        {
            OutputText.text = "Start a session first.";
            return true;
        }
        return false;
    }
}