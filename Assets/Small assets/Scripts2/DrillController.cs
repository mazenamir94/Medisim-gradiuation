using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DrillController : MonoBehaviour
{
    [Header("Setup")]
    public DrillTip drillTipScript;  // Drag your DrillTip script here
    public Transform drillBitModel;  // The visual part that spins
    public float spinSpeed = 1000f;

    private bool isRunning = false;

    // Connect this to "Activate" in XR Grab Interactable
    public void StartDrill()
    {
        isRunning = true;
        
        // This is the magic line: Tell your other script to wake up!
        if(drillTipScript != null) drillTipScript.SetDrillActive(true);
        
        Debug.Log("Drill ON");
    }

    // Connect this to "Deactivate"
    public void StopDrill()
    {
        isRunning = false;
        
        // Tell the other script to sleep
        if(drillTipScript != null) drillTipScript.SetDrillActive(false);
        
        Debug.Log("Drill OFF");
    }

    void Update()
    {
        if (isRunning && drillBitModel != null)
        {
            // Spin visual only
            drillBitModel.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
        }
    }
}