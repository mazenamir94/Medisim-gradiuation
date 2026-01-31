using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FillingController : MonoBehaviour
{
    public FillingTip fillingTip; // Drag the child here

    public void StartTool()
    {
        if(fillingTip) fillingTip.SetToolActive(true);
    }

    public void StopTool()
    {
        if(fillingTip) fillingTip.SetToolActive(false);
    }
}