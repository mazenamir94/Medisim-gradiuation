using UnityEngine;

public class FillingTip : MonoBehaviour
{
    // Minimalist "Just Filling" Script
    
    // --- RESTORED: Needed for FillingController.cs to compile ---
    private bool isToolActive = false;
    
    // Reference to Manager (Assign in Inspector or Auto-find)
    public DentalSessionManager dentalManager;

    private void Start()
    {
        if (dentalManager == null)
            dentalManager = FindObjectOfType<DentalSessionManager>();
    }

    // Called when you press the trigger button
    public void SetToolActive(bool isActive)
    {
        isToolActive = isActive;
    }
    // -----------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if tool is active (Trigger pressed?)
        if (!isToolActive) return;

        // Check for tags: "Filling" (set by Drill) ONLY.
        // We removed "Untagged" to stop you from painting healthy teeth!
        if (other.CompareTag("Filling")) 
        {
            MeshRenderer mesh = other.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                // CRITICAL FIX: Only count this as a "Restored Voxel" if it was previously invisible (a hole)
                // If it's already enabled, it's either a healthy tooth or already filled.
                bool wasHole = !mesh.enabled;

                // 1. Make it visible
                mesh.enabled = true; 
                
                // 2. Make it look like clean filling (White)
                if (mesh.material != null)
                {
                    mesh.material.color = Color.white;
                    mesh.material.mainTexture = null; 
                }

                // 3. Update tag so we don't process it again
                other.tag = "Filling"; 
                
                // 4. Report to Manager ONLY if we actually filled a hole
                if (wasHole && dentalManager != null)
                    dentalManager.OnFillingAdded();
            }
        }
    }
}
