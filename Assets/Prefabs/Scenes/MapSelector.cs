using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelector : MonoBehaviour
{
    public void LoadMap(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
