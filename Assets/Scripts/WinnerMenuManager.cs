
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinnerMenu : MonoBehaviour
{
    [Header("Winner Display")]
    public TMP_Text winnerText;

    [Header("Scene Names")]
    public string matchScene = "Battle";
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        if (winnerText != null)
        {
            winnerText.text = MatchManager.WinnerMessage;
        }
    }

    public void Rematch()
    {
        SceneManager.LoadScene(matchScene);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
