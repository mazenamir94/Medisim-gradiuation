using UnityEngine;

public class DrillTip : MonoBehaviour
{
    // Speed at which the object shrinks
    public float drillSpeed = 0.00f;
    
    // Is the drill currently activated by the user?
    private bool isDrillActive = false;

    // Call this from VRDrillController
    public void SetDrillActive(bool isActive)
    {
        isDrillActive = isActive;
    }

    // This function runs when the drill first touches something
    private void OnTriggerEnter(Collider other)
    {
        // Only drill if the motor is running!
        if (!isDrillActive) return;

        // Check if the object we hit has the "Decay" tag
        if (other.CompareTag("Decay"))
        {
             // Immediately hide the object (can be restored later)
             other.gameObject.SetActive(false);
        }
    }
    
    // Also handle continuous contact if you want to drill while holding against the surface
    private void OnTriggerStay(Collider other)
    {
         if (!isDrillActive) return;

         if (other.CompareTag("Decay"))
         {
             other.gameObject.SetActive(false);
         }
    }
}