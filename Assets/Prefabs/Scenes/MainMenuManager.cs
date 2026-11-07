
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Scene names
    public string loadingSceneName = "LoadingScreen";

    // Play button
    public void PlayGame()
    {
        SceneManager.LoadScene(loadingSceneName);
    }

    // Settings button
    public void OpenSettings()
    {
        Debug.Log("Settings button clicked!");
        // Open your settings panel here
    }

    // Quit button
    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        Application.Quit();
    }
}

