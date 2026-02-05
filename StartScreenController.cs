using UnityEngine;

public class StartScreenController : MonoBehaviour
{
    // Drag your Start Screen Canvas here
    public GameObject startScreenCanvas;
    
    // Drag your Login Panel (or the main Login Canvas) here
    public GameObject loginPanel;

    private void Start()
    {
        // Ensure Start screen is ON and Login is OFF when game begins
        if (startScreenCanvas != null) startScreenCanvas.SetActive(true);
        if (loginPanel != null) loginPanel.SetActive(false);
    }

    // We will link this function to the button
    public void OnStartClicked()
    {
        // Hide start screen
        if (startScreenCanvas != null) startScreenCanvas.SetActive(false);
        
        // Show login screen
        if (loginPanel != null) loginPanel.SetActive(true);
    }
}