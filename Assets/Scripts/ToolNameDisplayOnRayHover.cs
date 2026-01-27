using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToolNameDisplayOnRayHover : MonoBehaviour
{
    [Header("Raycasting")]
    public XRRayInteractor rayInteractor;
    public LayerMask toolLayerMask;

    [Header("UI Elements")]
    public Image background;
    public TextMeshProUGUI toolNameText;

    private void Update()
    {
        if (rayInteractor == null || background == null || toolNameText == null)
            return;

        // Try to get what the ray is hitting
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            GameObject hitObj = hit.collider.gameObject;

            // Check tag and layer mask
            if (hitObj.CompareTag("Tool") && ((1 << hitObj.layer) & toolLayerMask) != 0)
            {
                ShowToolName(hitObj.name);
                return;
            }
        }

        // Not hitting a valid tool
        HideToolName();
    }

    private void ShowToolName(string toolName)
    {
        background.gameObject.SetActive(true);
        toolNameText.gameObject.SetActive(true);
        toolNameText.text = toolName;
    }

    private void HideToolName()
    {
        background.gameObject.SetActive(false);
        toolNameText.gameObject.SetActive(false);
    }
}
