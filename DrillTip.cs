using UnityEngine;

public class DrillTip: MonoBehaviour
{
    // Optional: Add a sound or particle effect slot here later
    // public GameObject dustParticle; 

    void OnTriggerEnter(Collider other)
    {
        // Check if the object we touched has the "Decay" tag
        if (other.CompareTag("Decay"))
        {
            // Destroy the specific cube we touched
            Destroy(other.gameObject);
        }
    }
}