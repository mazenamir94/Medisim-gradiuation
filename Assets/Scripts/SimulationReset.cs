using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationReset : MonoBehaviour
{
    public void ResetSimulation()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
