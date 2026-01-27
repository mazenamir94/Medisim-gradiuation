using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ResetButtonInteractionVR : MonoBehaviour
{
    [Header("Controller Inputs")]
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    // You�ll assign these via XRI Default Input Actions
    public InputActionProperty leftTriggerAction;
    public InputActionProperty rightTriggerAction;

    [Header("Feedback")]
    public float interactionDistance = 3f;
    public float hoverScaleMultiplier = 1.2f;
    public float hoverMoveOffset = 0.05f;
    public AudioSource pressAudio;
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    [Header("Cooldown")]
    public float debounceTime = 1.5f;
    private bool isCooldown = false;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isHovering = false;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;

        // Enable input actions
        leftTriggerAction.action.Enable();
        rightTriggerAction.action.Enable();
    }

    void Update()
    {
        bool hoveringLeft = IsHovering(leftRayInteractor, leftTriggerAction, true);
        bool hoveringRight = IsHovering(rightRayInteractor, rightTriggerAction, false);

        isHovering = hoveringLeft || hoveringRight;

        if (!isHovering)
        {
            transform.position = originalPosition;
            transform.localScale = originalScale;
        }
    }

    private bool IsHovering(XRRayInteractor interactor, InputActionProperty triggerAction, bool isLeftHand)
    {
        if (interactor == null)
            return false;

        if (interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // Visual feedback
                Vector3 direction = (Camera.main.transform.position - transform.position).normalized;
                transform.localScale = originalScale * hoverScaleMultiplier;
                transform.position = originalPosition + direction * -hoverMoveOffset;

                // Haptic feedback
                SendHaptics(interactor);

                // Press trigger
                if (!isCooldown && triggerAction.action.WasPressedThisFrame())
                {
                    if (pressAudio != null)
                        pressAudio.Play();

                    ResetSimulation();
                    StartCoroutine(DebounceCooldown());
                }

                return true;
            }
        }

        return false;
    }

    private IEnumerator DebounceCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(debounceTime);
        isCooldown = false;
    }

    private void SendHaptics(XRRayInteractor interactor)
    {
        if (interactor.TryGetComponent(out XRBaseInputInteractor controllerInteractor))
        {
            if (controllerInteractor.xrController != null)
            {
                controllerInteractor.xrController.SendHapticImpulse(hapticIntensity, hapticDuration);
            }
        }
    }

    private void ResetSimulation()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
