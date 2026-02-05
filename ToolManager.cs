using System;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    [Header("Current Tool")]
    [SerializeField] private string currentToolId = null;
    public string CurrentToolId => currentToolId;

    public event Action<string> OnToolChanged;

    // Call this from your grab/activate logic when student picks a tool
    public void SetTool(string toolId)
    {
        toolId = string.IsNullOrWhiteSpace(toolId) ? null : toolId.Trim();
        if (toolId == currentToolId) return;

        currentToolId = toolId;
        OnToolChanged?.Invoke(currentToolId);
    }

    // Optional: keyboard test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) SetTool("HIGH_SPEED_BUR");
        if (Input.GetKeyDown(KeyCode.W)) SetTool("SCALER");
        if (Input.GetKeyDown(KeyCode.E)) SetTool("COMPOSITE_GUN");
    }
}
