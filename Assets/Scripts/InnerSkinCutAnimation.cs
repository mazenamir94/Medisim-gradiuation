using UnityEngine;
using System.Collections;

public class InnerSkinCutAnimation : MonoBehaviour
{
    [Header("Cut Visuals")]
    public GameObject skinUncut;
    public GameObject skinCut;
    public ParticleSystem bloodParticles;
    public AudioSource cutSound;
    public float duration = 2f;

    [Header("Tool Return")]
    public Transform toolHolder; // The hand or rest position

    [Header("Cut Transform Values")]
    public Vector3 startCutPosition = new Vector3(-0.006f, 0.71f, -0.84f);
    public Vector3 endCutPosition = new Vector3(-0.006f, 0.645f, -0.85f);
    public Vector3 cutEulerAngles = new Vector3(71.6f, -96.9f, 0.77f);

    private bool isCutting = false;
    public bool IsCutting => isCutting; // For external checks

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Transform originalParent;
    private Collider toolCollider;

    void Start()
    {
        // Store scalpel's starting transform for reset
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public void StartCut()
    {
        if (!isCutting)
        {
            StartCoroutine(CutRoutine());
        }
    }

    private IEnumerator CutRoutine()
    {
        isCutting = true;

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        // Detach and disable collider
        transform.SetParent(null);
        if (toolCollider != null) toolCollider.enabled = false;

        // Play blood & sound
        if (bloodParticles != null) bloodParticles.Play();
        if (cutSound != null) cutSound.Play();

        // Prepare visual mesh
        skinCut.SetActive(true);
        SetMeshVisibility(skinCut, 0f);

        float timer = 0f;
        Quaternion targetRotation = Quaternion.Euler(cutEulerAngles);

        while (timer < duration)
        {
            float t = timer / duration;

            // Lerp position and rotation
            transform.position = Vector3.Lerp(startCutPosition, endCutPosition, t);
            transform.rotation = targetRotation;

            SetMeshVisibility(skinCut, t); // Progressive cut reveal
            timer += Time.deltaTime;
            yield return null;
        }

        // Final position and rotation
        transform.position = endCutPosition;
        transform.rotation = targetRotation;

        SetMeshVisibility(skinCut, 1f);
        skinUncut.SetActive(false);

        if (bloodParticles != null) bloodParticles.Stop();

        yield return new WaitForSeconds(0.5f); // Optional pause before return

        ReturnScalpelToHand();
        isCutting = false;
    }

    private void ReturnScalpelToHand()
    {
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
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
        ReturnScalpelToHand();
    }
}
