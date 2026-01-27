using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ScalpelScript : MonoBehaviour
{
    public AudioSource grabSound;

    private XRGrabInteractable grabInteractable;
    private bool hasPlayed = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!hasPlayed && grabSound != null)
        {
            grabSound.Play();
            hasPlayed = true;
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        hasPlayed = false; // Reset so it can play again next grab
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
