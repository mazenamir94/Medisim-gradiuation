// PatientDetailsManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PatientDetailsManager : MonoBehaviour
{
    // Reference to UI elements
    //public TMP_InputField symptomsInputField;
    public TMP_Dropdown heartRateDropdown;

    // Scene names
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string GAME_SCENE = "clinic"; // Your actual game scene

    // Patient data to be passed to the game
    private PatientData patientData = new();

    // Initialize dropdown
    void Start()
    {
        // Make sure the dropdown has the heart rate options
        if (heartRateDropdown != null && heartRateDropdown.options.Count == 0)
        {
            heartRateDropdown.ClearOptions();
            heartRateDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Normal",
                "Tachycardia",
                "Bradycardia",
                "Flatline"
            });
        }
    }

    public void StartTraining()
    {
        // Save patient data
        //patientData.symptoms = symptomsInputField.text;
        patientData.heartRateStatus = heartRateDropdown.options[heartRateDropdown.value].text;

        // Save to PlayerPrefs or a more robust game state manager
        //PlayerPrefs.SetString("PatientSymptoms", patientData.symptoms);
        PlayerPrefs.SetString("PatientHeartRate", patientData.heartRateStatus);
        PlayerPrefs.Save();

        // Load the main game scene
        SceneManager.LoadScene(GAME_SCENE);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }
}