using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DentalTool : MonoBehaviour
{
    public Animator drillAnimator; // If you have an animation for the spinning
    public AudioSource drillSound; // The bzzzzzt sound
    
    private bool isRunning = false;

    // This function runs when you squeeze the trigger
    public void ActivateTool()
    {
        isRunning = true;
        if(drillSound) drillSound.Play();
        if(drillAnimator) drillAnimator.SetBool("Spin", true);
        Debug.Log("Drill Started!");
    }

    // This function runs when you let go of the trigger
    public void DeactivateTool()
    {
        isRunning = false;
        if(drillSound) drillSound.Stop();
        if(drillAnimator) drillAnimator.SetBool("Spin", false);
        Debug.Log("Drill Stopped!");
    }

    // This is called constantly while the tool is touching something
    private void OnTriggerStay(Collider other)
    {
        TryDrill(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDrill(other);
    }

    private void TryDrill(Collider other)
    {
        // 1. Check if the drill is actually on
        if (!isRunning) return;

        // 2. Check if we hit a "Decay" object
        if (other.CompareTag("Decay"))
        {
            // Simple drilling: Make it disappear
            // In a more complex version, we might shrink it or spawn particles
            other.gameObject.SetActive(false);
            Debug.Log("Drilled specific tooth part: " + other.name);
        }
    }

    void Update()
    {
        // Visual effects or continuous logic if needed
    }
}