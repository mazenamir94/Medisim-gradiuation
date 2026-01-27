using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HeartbeatVideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    public VideoClip normalClip;
    public VideoClip tachycardiaClip;
    public VideoClip cardiacArrestClip;
    public VideoClip flatlineClip;

    void Start()
    {
        PlayVideoBasedOnHeartRate();
    }

    void Update()
    {
        PlayVideoBasedOnHeartRate();
    }

    public void PlayVideoBasedOnHeartRate()
    {
        string heartRateType = PlayerPrefs.GetString("PatientHeartRate", "Normal");

        switch (heartRateType)
        {
            case "Normal":
                videoPlayer.clip = normalClip;
                break;
            case "Tachycardia":
                videoPlayer.clip = tachycardiaClip;
                break;
            case "Cardiac Arrest":
                videoPlayer.clip = cardiacArrestClip;
                break;
            case "Flatline":
                videoPlayer.clip = flatlineClip;
                break;
            default:
                Debug.LogWarning("Unknown PatientHeartRate type: " + heartRateType);
                videoPlayer.clip = normalClip;
                break;
        }

        videoPlayer.Play();
    }
}
