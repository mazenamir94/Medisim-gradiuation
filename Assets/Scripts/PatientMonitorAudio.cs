using UnityEngine;
using UnityEngine.SceneManagement;

public class PatientMonitorAudio : MonoBehaviour
{
    // References to your audio clips
    public AudioClip normalHeartbeatSound;
    public AudioClip tachycardiaSound;
    public AudioClip cardiacArrestSound;
    public AudioClip flatlineSound;

    // Reference to the AudioSource component
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on Patient Monitor!");
            return;
        }

        // Load the selected heart rate from PlayerPrefs
        string selectedHeartRate = PlayerPrefs.GetString("PatientHeartRate", "Normal");

        // Select the appropriate audio clip
        PlayHeartRate(selectedHeartRate);
    }

    private AudioClip GetAudioClipForHeartRate(string heartRate)
    {
        switch (heartRate)
        {
            case "Normal":
                return normalHeartbeatSound;
            case "Tachycardia":
                return tachycardiaSound;
            case "Cardiac Arrest":
                return cardiacArrestSound;
            case "Flatline":
                return flatlineSound;
            default:
                Debug.LogWarning("Unknown heart rate type: " + heartRate);
                return normalHeartbeatSound; // Default to normal
        }
    }

    // Public method to switch heart rate from external script (raycast/button selector)
    public void PlayHeartRate(string heartRate)
    {
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found when trying to play heart rate.");
            return;
        }

        AudioClip selectedClip = GetAudioClipForHeartRate(heartRate);

        if (selectedClip != null)
        {
            audioSource.Stop();
            audioSource.clip = selectedClip;
            audioSource.loop = true;
            audioSource.Play();

            // Save selected heart rate
            PlayerPrefs.SetString("PatientHeartRate", heartRate);
            PlayerPrefs.Save();

            Debug.Log("Switched to heart rate: " + heartRate);
        }
    }
}
