// AboutManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class AboutManager : MonoBehaviour
{
    private const string MAIN_MENU_SCENE = "MainMenu";

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }
}