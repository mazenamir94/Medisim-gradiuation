using UnityEngine;

public class DrillTip : MonoBehaviour
{
    // Speed at which the object shrinks
    public float drillSpeed = 0.00f;

    // This function runs when the drill first touches something
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we hit has the "Decay" tag
        if (other.CompareTag("Decay"))
        {
             // Immediately hide the object (can be restored later)
             other.gameObject.SetActive(false);
        }
    }
}