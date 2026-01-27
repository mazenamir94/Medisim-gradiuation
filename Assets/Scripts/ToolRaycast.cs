using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ToolRaycast : MonoBehaviour
{
    public float rayDistance = 3f;
    public LayerMask toolLayer;

    public TextMeshProUGUI toolNameText;
    public GameObject pickupPromptUI;
    public Transform handMountPoint;
    public Image background;

    private GameObject currentTool = null;
    private GameObject heldTool = null;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Coroutine moveCoroutine = null;

    private Collider playerCollider;

    void Start()
    {
        // Try to find the player collider automatically (assuming you're using Starter Assets)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponentInChildren<CapsuleCollider>();
        }
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Drop tool if H is pressed
        if (heldTool != null)
        {
            if (Input.GetKeyDown(KeyCode.H))
                DropTool();

            ClearUI(); // No pickup prompt while holding
            return;
        }

        if (Physics.Raycast(ray, out hit, rayDistance, toolLayer))
        {
            if (hit.collider.CompareTag("Tool"))
            {
                currentTool = hit.collider.gameObject;
                Debug.Log("Looking at tool: " + currentTool.name);

                toolNameText.text = currentTool.name;
                toolNameText.gameObject.SetActive(true);
                toolNameText.enabled = true;
                pickupPromptUI.SetActive(true);
                background.enabled = true;
                background.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickUpTool(currentTool);
                }

                return; // avoid hiding UI too early
            }
        }

        ClearUI();
    }

    void PickUpTool(GameObject tool)
    {
        if (heldTool != null) return;

        heldTool = tool;
        originalPosition = tool.transform.position;
        originalRotation = tool.transform.rotation;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        Rigidbody rb = tool.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        //Disable collider completely while holding
        Collider toolCol = tool.GetComponent<Collider>();
        if (toolCol != null)
            toolCol.enabled = false;

        tool.transform.SetParent(handMountPoint);

        Quaternion desiredLocalRotation;
        Vector3 desiredLocalPosition;
        if (tool.name == "Drill")
        {
            desiredLocalRotation = Quaternion.Euler(-109.67f, -90f, -29f);
            desiredLocalPosition = new Vector3(0.05f, -0.04f, -0.19f);
        }
        else
        {
            desiredLocalRotation = Quaternion.Euler(158.024f, -248.193f, -130.746f);
            desiredLocalPosition = Vector3.zero;
        }
        moveCoroutine = StartCoroutine(MoveToTargetLocal(tool, desiredLocalPosition, desiredLocalRotation));

        ClearUI();
    }

    void DropTool()
    {
        if (heldTool == null) return;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(DropAndReturn(heldTool, originalPosition, originalRotation));
    }

    IEnumerator MoveToTargetLocal(GameObject obj, Vector3 targetLocalPos, Quaternion targetLocalRot)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startPos = obj.transform.localPosition;
        Quaternion startRot = obj.transform.localRotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, t);
            obj.transform.localRotation = Quaternion.Slerp(startRot, targetLocalRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localPosition = targetLocalPos;
        obj.transform.localRotation = targetLocalRot;
    }

    IEnumerator DropAndReturn(GameObject tool, Vector3 targetWorldPos, Quaternion targetWorldRot)
    {
        tool.transform.SetParent(null);

        Vector3 startPos = tool.transform.position;
        Quaternion startRot = tool.transform.rotation;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            tool.transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
            tool.transform.rotation = Quaternion.Slerp(startRot, targetWorldRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        tool.transform.position = targetWorldPos;
        tool.transform.rotation = targetWorldRot;

        yield return new WaitForSeconds(0.1f);

        Rigidbody rb = tool.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // Re-enable collider now that it's dropped
        Collider toolCol = tool.GetComponent<Collider>();
        if (toolCol != null)
            toolCol.enabled = true;

        heldTool = null;
    }

    public bool IsHoldingTool()
    {
        return heldTool != null;
    }

    public string GetHeldToolName()
    {
        return heldTool != null ? heldTool.name : "";
    }

    public GameObject GetHeldTool()
    {
        return heldTool;
    }

    void ClearUI()
    {
        toolNameText.enabled = false;
        pickupPromptUI.SetActive(false);
        currentTool = null;
        background.enabled = false;
    }
}
