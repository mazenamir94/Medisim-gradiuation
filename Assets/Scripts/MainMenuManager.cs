// MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Scene names - you'll need to add these scenes to your build settings
    private const string PATIENT_DETAILS_SCENE = "PatientDetails";
    private const string ABOUT_SCENE = "About";

    public void StartGame()
    {
        // Load the patient details scene
        SceneManager.LoadScene(PATIENT_DETAILS_SCENE);
    }

    public void OpenAbout()
    {
        // Load the about scene
        SceneManager.LoadScene(ABOUT_SCENE);
    }

    public void QuitGame()
    {
        // Quit the application (works in built game, not in editor)
        Application.Quit();

        // Log message for testing in editor
        Debug.Log("Quit Game");
    }
}