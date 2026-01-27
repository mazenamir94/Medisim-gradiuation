using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScalpelInteraction : MonoBehaviour
{
    public ScalpelCutAnimation cutAnimation;
    public InnerSkinCutAnimation innerSkinCutAnimation;

    public GameObject uncutSkinMesh;
    public GameObject cutSkinMesh;
    public GameObject uncutInnerSkinMesh;
    public GameObject cutInnerSkinMesh;

    private XRGrabInteractable grabInteractable;
    private bool hasCutSkin = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!grabInteractable || !grabInteractable.isSelected) return;

        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        if (layerName == "Skin" && !hasCutSkin)
        {
            hasCutSkin = true;
            cutAnimation.StartCut(() =>
            {
                // Return to hand or table handled inside animation
                hasCutSkin = false;
            });
        }

        if (layerName == "Inner Skin")
        {
            PerformDeeperIncision();
        }

        if (layerName == "Dura")
        {
            SliceDura();
        }
    }

    void PerformDeeperIncision()
    {
        if (uncutInnerSkinMesh != null && cutInnerSkinMesh != null)
        {
            innerSkinCutAnimation.StartCut();
        }
    }

    void SliceDura()
    {
        Debug.Log("Dura sliced open.");
        // Add dura logic
    }
}
