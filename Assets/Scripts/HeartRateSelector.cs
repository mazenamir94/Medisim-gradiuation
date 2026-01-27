using UnityEngine;
using TMPro;

public class HeartRateSelector : MonoBehaviour
{
    public float rayDistance = 5f;
    public LayerMask uiLayer;
    public PatientMonitorAudio patientMonitor;
    public TextMeshProUGUI currentSelection;

    private GameObject currentButton = null;

    void Update()
    {
        currentSelection.text = PlayerPrefs.GetString("PatientHeartRate", "Normal");
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, uiLayer))
        {
            if (hit.collider.CompareTag("HeartRateOption"))
            {
                currentButton = hit.collider.gameObject;
                string option = currentButton.name;

                // Show feedback (e.g., highlight or debug log)
                Debug.Log("Looking at: " + option);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    SelectOption(option);
                }

                return;
            }
        }

        currentButton = null; // Reset if not looking at a valid button
    }

    void SelectOption(string optionName)
    {
        if (patientMonitor == null)
        {
            Debug.LogWarning("PatientMonitorAudio reference not set.");
            return;
        }

        patientMonitor.PlayHeartRate(optionName);
        currentSelection.text = optionName;
        Debug.Log("Selected: " + optionName);
    }
}
