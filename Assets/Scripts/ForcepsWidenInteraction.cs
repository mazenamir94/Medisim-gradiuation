using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ForcepsWidenInteraction : MonoBehaviour
{
    [Header("Trigger Input")]
    public InputActionProperty triggerInput;

    [Header("Collision")]
    public string targetTag = "CutSkin";
    private bool isColliding = false;
    private GameObject currentCollidingObject;

    [Header("Skin Mesh Swap (Optional)")]
    public GameObject meshToShow;
    public GameObject meshToHide;

    [Header("UI Images")]
    public GameObject image1;
    public GameObject image2;
    public GameObject image3;
    public GameObject image4;

    [Header("Animation Settings")]
    public Transform animatedChild; // Child of forceps to rotate
    public float targetYRotation = -25f;
    public float rotationDuration = 0.5f;

    [Header("Surgical Positioning")]
    public Vector3 surgicalPosition = new Vector3(0.0186f, 0.7393f, -0.8749f);
    public Vector3 surgicalEulerRotation = new Vector3(-174.961f, -97.20001f, -9.845001f);
    public float moveDuration = 1.0f;

    private bool animationPlayed = false;
    private XRGrabInteractable grabInteractable;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void OnEnable() => triggerInput.action.Enable();
    private void OnDisable() => triggerInput.action.Disable();

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        image1?.SetActive(false);
        image2?.SetActive(false);
        image3?.SetActive(false);
        image4?.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            isColliding = true;
            currentCollidingObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentCollidingObject)
        {
            isColliding = false;
            currentCollidingObject = null;
        }
    }

    private void Update()
    {
        if (isColliding && triggerInput.action.WasPressedThisFrame() && !animationPlayed)
        {
            animationPlayed = true;
            grabInteractable.enabled = false; // Disable grabbing while animating
            StartCoroutine(PerformSurgicalAction());
        }
    }

    private System.Collections.IEnumerator PerformSurgicalAction()
    {
        // Move to surgical position
        yield return StartCoroutine(MoveOverTime(
            transform,
            transform.position,
            surgicalPosition,
            transform.rotation,
            Quaternion.Euler(surgicalEulerRotation),
            moveDuration
        ));

        // Show images
        image1?.SetActive(true);
        image2?.SetActive(true);
        image3?.SetActive(true);
        image4?.SetActive(true);

        // Swap meshes
        if (meshToHide != null) meshToHide.SetActive(false);
        if (meshToShow != null) meshToShow.SetActive(true);

        // Rotate open and close
        if (animatedChild != null)
        {
            yield return StartCoroutine(RotateChild(animatedChild, targetYRotation, rotationDuration));
        }

        // Move back to tray
        yield return StartCoroutine(MoveOverTime(
            transform,
            surgicalPosition,
            initialPosition,
            Quaternion.Euler(surgicalEulerRotation),
            initialRotation,
            moveDuration
        ));

        grabInteractable.enabled = true; // Re-enable grabbing
    }

    private System.Collections.IEnumerator MoveOverTime(Transform target, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            target.position = Vector3.Lerp(fromPos, toPos, t);
            target.rotation = Quaternion.Slerp(fromRot, toRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.position = toPos;
        target.rotation = toRot;
    }

    private System.Collections.IEnumerator RotateChild(Transform target, float yRotation, float duration)
    {
        Quaternion initialRotation = target.localRotation;
        Quaternion openRotation = Quaternion.Euler(target.localEulerAngles.x, yRotation, target.localEulerAngles.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            target.localRotation = Quaternion.Slerp(initialRotation, openRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localRotation = openRotation;

        yield return new WaitForSeconds(0.25f); // Optional pause

        // Rotate back
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            target.localRotation = Quaternion.Slerp(openRotation, initialRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localRotation = initialRotation;
    }
}
