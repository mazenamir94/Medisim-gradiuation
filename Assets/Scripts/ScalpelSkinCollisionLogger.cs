using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ScalpelSkinCollisionLogger : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    public ScalpelCutAnimation animation;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to grab events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isGrabbed) return;

        if (other.CompareTag("Skin") && other.gameObject.layer == LayerMask.NameToLayer("LayerInteractable"))
        {
            Debug.Log("Scalpel is grabbed and entered trigger with Skin!");
            animation.StartCut();
        }
    }   
}
