using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScalpelCutAnimation : MonoBehaviour
{
    [Header("Cut Visuals")]
    public GameObject skinUncut;
    public GameObject skinCut;
    public ParticleSystem bloodParticles;
    public AudioSource cutSound;
    public float duration = 0.8f;
    public GameObject bloodStain;
    public GameObject blood1;
    public GameObject blood2;

    [Header("Tool Return")]
    public Transform toolHolder; // Player hand transform OR default resting position

    [Header("Cut Transform Values")]
    public Vector3 startCutPosition;
    public Vector3 endCutPosition;
    public Vector3 cutEulerAngles;

    private XRGrabInteractable grabInteractable;
    private bool isCutting = false;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Rigidbody rb;

    private System.Action onCutComplete;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public void StartCut(System.Action onComplete = null)
    {
        if (isCutting) return;
        onCutComplete = onComplete;
        StartCoroutine(CutRoutine());
    }

    private IEnumerator CutRoutine()
    {
        isCutting = true;

        // Disable interaction
        grabInteractable.enabled = false;
        if (rb != null) rb.isKinematic = true;

        // Detach from hand
        transform.SetParent(null);

        // VFX
        if (bloodParticles != null) bloodParticles.Play();
        cutSound?.Play();

        // Mesh swap
        skinCut.SetActive(true);
        SetMeshVisibility(skinCut, 0f);

        float timer = 0f;
        Quaternion targetRotation = Quaternion.Euler(cutEulerAngles);

        while (timer < duration)
        {
            float t = timer / duration;
            transform.position = Vector3.Lerp(startCutPosition, endCutPosition, t);
            transform.rotation = Quaternion.Slerp(targetRotation, targetRotation, t);
            SetMeshVisibility(skinCut, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // Final mesh state
        transform.position = endCutPosition;
        transform.rotation = targetRotation;
        SetMeshVisibility(skinCut, 1f);
        skinUncut.SetActive(false);
        bloodStain?.SetActive(true);
        blood1?.SetActive(true);
        blood2?.SetActive(true);
        if (bloodParticles != null) bloodParticles.Stop();

        yield return new WaitForSeconds(0.5f); // Optional wait

        ReturnToHandOrTable();

        isCutting = false;
        onCutComplete?.Invoke();
    }

    private void ReturnToHandOrTable()
    {
        bool wasHeld = grabInteractable.isSelected;

        // Re-enable interaction
        grabInteractable.enabled = true;
        if (rb != null) rb.isKinematic = false;

        if (wasHeld)
        {
            // Send scalpel back to hand
            transform.SetParent(originalParent);
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
        }
        else
        {
            // Drop onto table
            transform.position = toolHolder.position;
            transform.rotation = toolHolder.rotation;
            transform.SetParent(toolHolder);
        }
    }

    private void SetMeshVisibility(GameObject meshObj, float alpha)
    {
        var renderer = meshObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_CutoutAmount"))
                    mat.SetFloat("_CutoutAmount", 1 - alpha);
                else if (mat.HasProperty("_Alpha"))
                    mat.SetFloat("_Alpha", alpha);
            }
        }
    }

    public void ResetScalpelAndSkin()
    {
        skinCut.SetActive(false);
        skinUncut.SetActive(true);
        ReturnToHandOrTable();
    }
}
