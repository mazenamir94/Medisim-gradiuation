using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ToolHandler : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isReturning = false;

    public float returnSpeed = 2f;
    public float reenablePhysicsDelay = 0.5f;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Store starting position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Subscribe to events
        grabInteractable.selectExited.AddListener(OnDrop);
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        grabInteractable.selectExited.RemoveListener(OnDrop);
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        // Only return if not held by any other interactor
        if (grabInteractable.interactorsSelecting.Count == 0)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            isReturning = true;
        }
    }

    void Update()
    {
        if (isReturning)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, originalRotation, returnSpeed * 100f * Time.deltaTime);

            if (Vector3.Distance(transform.position, originalPosition) < 0.01f &&
                Quaternion.Angle(transform.rotation, originalRotation) < 1f)
            {
                isReturning = false;
                StartCoroutine(ReenablePhysicsAfterDelay(reenablePhysicsDelay));
            }
        }
    }

    private IEnumerator ReenablePhysicsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.isKinematic = false;
    }
}
