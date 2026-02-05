using UnityEngine;

public class DrillTip : MonoBehaviour
{
    // Speed at which the object shrinks
    public float drillSpeed = 0.00f;
    
    // Is the drill currently activated by the user?
    private bool isDrillActive = false;
    
    // Cache the manager (Public so you can drag it in Inspector!)
    public DentalSessionManager dentalManager;

    private void Start()
    {
        if (dentalManager == null)
            dentalManager = FindObjectOfType<DentalSessionManager>();
            
        if (dentalManager == null)
            Debug.LogError("DrillTip: DentalSessionManager not found in scene!");
    }

    // Call this from VRDrillController
    public void SetDrillActive(bool isActive)
    {
        isDrillActive = isActive;
    }

    // Shared logic for drilling
    private void ProcessDrilling(Collider other)
    {
        // Only drill if the motor is running!
        if (!isDrillActive) return;

        // Check if the object we hit has the "Decay" tag
        if (other.CompareTag("Decay"))
        {
             // Batch the update: don't call manager yet
             pendingDecayCount++;

             // Immediately hide the object (can be restored later)
             // Don't destroy! Hide it so we can fill it later.
             // 1. Hide the mesh (Use TryGetComponent for better performance)
             if(other.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                mesh.enabled = false;
            
             // 2. Change Tag to "Filling" so the Filling Tool can find it
             other.tag = "Filling"; 
             
             // 3. Optional: Disable collider if you don't want the drill to hit it again?
             // But we need the collider for the Filling Tip! So keep it enabled.
        }
    }

    // Batching Mechanism
    private int pendingDecayCount = 0;

    private void LateUpdate()
    {
        if (pendingDecayCount > 0 && dentalManager != null)
        {
            dentalManager.OnDecayRemoved(pendingDecayCount);
            pendingDecayCount = 0;
        }
    }

    // Ensure we count everything even if the tool is disabled immediately
    private void OnDisable()
    {
        if (pendingDecayCount > 0 && dentalManager != null)
        {
            dentalManager.OnDecayRemoved(pendingDecayCount);
            pendingDecayCount = 0;
        }
    }

    // This function runs when the drill first touches something
    private void OnTriggerEnter(Collider other)
    {
        ProcessDrilling(other);
    }
    
    // Also handle continuous contact if you want to drill while holding against the surface
    private void OnTriggerStay(Collider other)
    {
         ProcessDrilling(other);
    }
}