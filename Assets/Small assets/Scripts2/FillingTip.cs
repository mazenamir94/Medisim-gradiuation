using UnityEngine;

public class FillingTip : MonoBehaviour
{
    private bool isToolActive = false;

    // The Manager (Controller) calls this
    public void SetToolActive(bool isActive)
    {
        isToolActive = isActive;
    }

   private void OnTriggerEnter(Collider other)
    {
        // 1. Check if tool is active
        if (!isToolActive) return;

        // 2. CHANGE THIS: Check for "Filling" instead of "Hole"
        if (other.CompareTag("Filling")) 
        {
            MeshRenderer mesh = other.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                mesh.enabled = true; 
                other.tag = "Untagged"; 
                Debug.Log("Tooth Filled!");
            }
        }
    }
}
