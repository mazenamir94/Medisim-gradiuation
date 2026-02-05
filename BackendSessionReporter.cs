using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BackendSessionReporter : MonoBehaviour
{
    [Header("Config")]
    public BackendConfig config;
    public string procedureType = "ClassIComposite";

    [Header("References")]
    public ProcedureStateMachine stateMachine;
    public ToolManager toolManager;
    public DrillMetricsSampler drillSampler;

    [Header("UI (Optional)")]
    public TextMeshProUGUI liveFeedbackText;   // shows warnings only (no score)
    public TextMeshProUGUI finalResultText;    // shows final score only at end

    [Header("Sampling")]
    [Tooltip("How often to send drill samples to backend while drilling.")]
    public float drillSendHz = 10f;

    [Tooltip("Only send drill samples if contacting tooth (recommended).")]
    public bool onlySendWhenContacting = true;

    // --- FIXED: Public slot so you can drag it in Inspector ---
    public BackendClient _client;

    private string _sessionId;
    private float _drillTimer;

    private void Awake()
    {
        // --- FIXED: Find the client automatically if not assigned ---
        if (_client == null) _client = FindFirstObjectByType<BackendClient>();

        if (liveFeedbackText != null) liveFeedbackText.text = "";
        if (finalResultText != null) finalResultText.text = "";

        if (stateMachine != null)
            stateMachine.OnStepChanged += HandleStepChanged;

        if (toolManager != null)
            toolManager.OnToolChanged += HandleToolChanged;
    }

    public void SetJwtToken(string token)
    {
        if (_client == null) _client = FindFirstObjectByType<BackendClient>();

        if (_client != null)
        {
            _client.SetToken(token);
        }
        else
        {
            Debug.LogError("BackendSessionReporter: Could not find BackendClient in the scene!");
        }
    }

    private void OnDestroy()
    {
        if (stateMachine != null)
            stateMachine.OnStepChanged -= HandleStepChanged;

        if (toolManager != null)
            toolManager.OnToolChanged -= HandleToolChanged;
    }

    // Call this when the procedure starts (button/UI/trigger)
    public async void StartProcedure()
    {
        if (_client == null) { Debug.LogError("BackendClient not initialized."); return; }
        if (string.IsNullOrWhiteSpace(_client.GetToken()))
        {
            Debug.LogError("JWT token missing. Call SetJwtToken after login.");
            return;
        }

        _sessionId = "sess_" + Guid.NewGuid().ToString("N").Substring(0, 10);

        if (liveFeedbackText != null) liveFeedbackText.text = "Starting session...";
        if (finalResultText != null) finalResultText.text = "";

        var resp = await _client.StartSession(_sessionId, procedureType, DateTime.UtcNow);
        if (!string.IsNullOrEmpty(resp.error))
        {
            SetLive($"❌ StartSession error: {resp.error}");
            return;
        }

        SetLive("✅ Session started. (Score hidden)");
    }

    // Call this when the procedure ends (button/UI/trigger)
    public async void EndProcedure()
    {
        if (string.IsNullOrWhiteSpace(_sessionId))
        {
            SetLive("Start procedure first.");
            return;
        }

        SetLive("Ending session...");

        var resp = await _client.EndSession(_sessionId, procedureType, DateTime.UtcNow);
        if (!string.IsNullOrEmpty(resp.error))
        {
            SetLive($"❌ EndSession error: {resp.error}");
            return;
        }

        // ✅ Only show score at end
        if (finalResultText != null)
        {
            finalResultText.text =
                $"Final Score: {resp.finalScore}\n" +
                $"Duration: {resp.durationSec} sec\n" +
                $"Session: {resp.sessionDbId}";
        }

        SetLive("✅ Session ended.");
        _sessionId = null;
    }

    private void Update()
    {
        if (string.IsNullOrWhiteSpace(_sessionId)) return;

        // Update drill metrics continuously
        if (drillSampler != null)
            drillSampler.UpdateMetrics();

        // Send drill samples only in drilling step
        if (stateMachine == null || drillSampler == null) return;
        if (stateMachine.CurrentStep != ProcedureStateMachine.Step.DRILLING) return;

        if (onlySendWhenContacting && !drillSampler.IsContactingTooth) return;

        float interval = drillSendHz <= 0 ? 0.1f : (1f / drillSendHz);
        _drillTimer += Time.deltaTime;
        if (_drillTimer < interval) return;
        _drillTimer = 0f;

        _ = SendDrillSampleAsync(drillSampler.CurrentDepthMm, drillSampler.CurrentAngleDeg);
    }

    private async void HandleStepChanged(ProcedureStateMachine.Step step)
    {
        if (string.IsNullOrWhiteSpace(_sessionId)) return;

        string stepStr = ProcedureStateMachine.StepToBackendString(step);
        var resp = await _client.SendStepEvent(_sessionId, stepStr);
        HandleWarnResponse(resp, $"Step: {stepStr}");
    }

    private async void HandleToolChanged(string toolId)
    {
        if (string.IsNullOrWhiteSpace(_sessionId)) return;

        // Tool changed (send it)
        var resp = await _client.SendToolEvent(_sessionId, toolId ?? "");
        HandleWarnResponse(resp, $"Tool: {toolId}");
    }

    private async Task SendDrillSampleAsync(float depthMm, float angleDeg)
    {
        if (string.IsNullOrWhiteSpace(_sessionId)) return;

        var resp = await _client.SendDrillSample(_sessionId, depthMm, angleDeg);
        HandleWarnResponse(resp, $"Drill: depth={depthMm:F2}mm angle={angleDeg:F1}deg");
    }

    private void HandleWarnResponse(BackendClient.EventResponse resp, string context)
    {
        if (!string.IsNullOrEmpty(resp.error))
        {
            SetLive($"❌ {context}\n{resp.error}");
            return;
        }

        // ✅ Only speak when wrong
        if (resp.warn)
        {
            // Turn backend errorType into nice message (you can expand this)
            string msg = resp.errorType switch
            {
                "WRONG_TOOL" => "Wrong tool for this step. Switch to the correct instrument.",
                "TOO_DEEP" => "Too deep. Reduce bur penetration depth.",
                "ANGLE_TOO_STEEP" => "Angle too steep. Adjust your handpiece angulation.",
                _ => $"Incorrect: {resp.errorType}"
            };

            SetLive($"⚠️ {msg}");
        }
        else
        {
            // Keep silent when correct
        }
    }

    private void SetLive(string msg)
    {
        if (liveFeedbackText != null) liveFeedbackText.text = msg;
        Debug.Log(msg);
    }

    public void SendManualDrillSample(float depthMm, float angleDeg)
    {
        if (string.IsNullOrWhiteSpace(_sessionId)) { SetLive("Start procedure first."); return; }
        _ = SendDrillSampleAsync(depthMm, angleDeg);
    }
}