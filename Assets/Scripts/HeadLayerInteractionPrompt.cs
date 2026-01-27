using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeadLayerInteractionPrompt : MonoBehaviour
{
    public float rayDistance = 3f;
    public LayerMask headLayerMask;
    public TextMeshProUGUI interactionText;
    public Image promptBackground;
    public ToolRaycast toolRaycastScript;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (interactionText != null)
            interactionText.enabled = false;

        if (promptBackground != null)
        {
            promptBackground.enabled = false;
            promptBackground.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, headLayerMask))
        {
            string layerName = hit.collider.gameObject.name;
            string currentTool = toolRaycastScript.GetHeldToolName();

            if (!string.IsNullOrEmpty(currentTool))
            {
                string promptMessage = GetPromptForToolAndLayer(currentTool, layerName);
                ShowPrompt(promptMessage);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GameObject hitObject = hit.collider.gameObject;

                    //if (currentTool == "Scalpel")
                    //{
                    //    var scalpel = toolRaycastScript.GetHeldTool();
                    //    var interaction = scalpel.GetComponent<ScalpelInteraction>();
                    //    if (interaction != null)
                    //    {
                    //        interaction.TryInteractWithLayer(hitObject.name);
                    //    }
                    //} else if (currentTool == "Forceps")
                    //{
                    //    var forceps = toolRaycastScript.GetHeldTool();
                    //    var interaction = forceps.GetComponent<ForcepsInteraction>();
                    //    if (interaction != null)
                    //    {
                    //        interaction.TryInteractWithLayer(hitObject.name);
                    //    }
                    //}

                    // You can add more tools here like:
                    // else if (currentTool == "Forceps") { ... }
                    // else if (currentTool == "Drill") { ... }
                }
            }
            else
            {
                ShowPrompt("Pick up a tool to interact");
            }
        }
        else
        {
            HidePrompt();
        }
    }

    string GetPromptForToolAndLayer(string tool, string layer)
    {
        // SCALPEL INTERACTIONS
        if (tool == "Scalpel")
        {
            switch (layer)
            {
                case "Skin":
                    return "Press E to make an incision in the skin";
                case "Inner Skin":
                    return "Press E to make a deeper incision";
                case "Outer Skull":
                    return "Scalpel ineffective on bone";
                case "Skull Diploe":
                case "Inner Skull":
                    return "Scalpel can't penetrate skull layers";
                case "Dura":
                    return "Press E to slice open the dura";
                case "Brain":
                    return "Scalpel interaction not advised on brain";
                case "skin_cut":
                    return "Skin already incised";
            }
        }

        // FORCEPS INTERACTIONS
        else if (tool == "Forceps")
        {
                Debug.Log(layer);
            switch (layer)
            {
                case "Skin":
                    return "Make an incision first";
                case "skin_cut":
                    return "Press E to open the skin wider";
                case "Inner Skin":
                    return "Make a deeper incision first";
                case "inner_skin_cut":
                    return "Make a deeper incision first";
                case "Outer Skull":
                    return "Forceps ineffective on bone";
                case "Skull Diploe":
                case "Inner Skull":
                    return "Forceps not usable on skull layers";
                case "Dura":
                    return "Press E to lift the dura";
                case "Brain":
                    return "Press E to gently hold brain tissue";
            }
        }

        // DEFAULT
        return $"No interaction available with {layer} using {tool}";
    }

    void ShowPrompt(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
            interactionText.enabled = true;
            interactionText.gameObject.SetActive(true);
        }

        if (promptBackground != null)
        {
            promptBackground.enabled = true;
            promptBackground.gameObject.SetActive(true);
        }
    }

    void HidePrompt()
    {
        if (interactionText != null)
        {
            interactionText.enabled = false;
            interactionText.gameObject.SetActive(false);
        }

        if (promptBackground != null)
        {
            promptBackground.enabled = false;
            promptBackground.gameObject.SetActive(false);
        }
    }
}
