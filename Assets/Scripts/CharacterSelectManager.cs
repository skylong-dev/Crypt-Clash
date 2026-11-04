

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Player Portraits")]
    public Image player1Portrait;
    public Image player2Portrait;

    [Header("Confirm Buttons")]
    public Button player1ConfirmButton;
    public Button player2ConfirmButton;

    [Header("Player Selection Indicators")]
    public GameObject player1Highlight;
    public GameObject player2Highlight;

    [Header("Next Scene")]
    public string nextSceneName = "MapSelection";

    private Sprite player1Character;
    private Sprite player2Character;

    private GameObject player1Prefab;
    private GameObject player2Prefab;

    private bool player1Confirmed;
    private bool player2Confirmed;

    // 1 = Player 1, 2 = Player 2
    private int activePlayer = 1;

    private void Start()
    {
        if (player1ConfirmButton != null)
            player1ConfirmButton.onClick.AddListener(ConfirmPlayer1);

        if (player2ConfirmButton != null)
            player2ConfirmButton.onClick.AddListener(ConfirmPlayer2);

        UpdateHighlights();
        UpdateConfirmButtons();
    }

    // Called when a character button is clicked.
    public void SelectCharacter(Sprite characterSprite, GameObject characterPrefab)
    {
        if (characterSprite == null || characterPrefab == null)
        {
            Debug.LogWarning("Character sprite or prefab is missing!");
            return;
        }

        if (activePlayer == 1 && !player1Confirmed)
        {
            player1Character = characterSprite;
            player1Prefab = characterPrefab;

            if (player1Portrait != null)
            {
                player1Portrait.sprite = characterSprite;
                player1Portrait.enabled = true;
            }

            Debug.Log("Player 1 selected: " + characterPrefab.name);
        }
        else if (activePlayer == 2 && !player2Confirmed)
        {
            player2Character = characterSprite;
            player2Prefab = characterPrefab;

            if (player2Portrait != null)
            {
                player2Portrait.sprite = characterSprite;
                player2Portrait.enabled = true;
            }

            Debug.Log("Player 2 selected: " + characterPrefab.name);
        }

        UpdateConfirmButtons();
    }

    public void SelectPlayer1()
    {
        if (!player1Confirmed)
            activePlayer = 1;

        UpdateHighlights();
    }

    public void SelectPlayer2()
    {
        if (!player2Confirmed)
            activePlayer = 2;

        UpdateHighlights();
    }

    private void ConfirmPlayer1()
    {
        if (player1Character == null || player1Prefab == null)
        {
            Debug.Log("Player 1: Select a character first!");
            return;
        }

        player1Confirmed = true;

        UpdateConfirmButtons();
        UpdateHighlights();
        CheckBothConfirmed();
    }

    private void ConfirmPlayer2()
    {
        if (player2Character == null || player2Prefab == null)
        {
            Debug.Log("Player 2: Select a character first!");
            return;
        }

        player2Confirmed = true;

        UpdateConfirmButtons();
        UpdateHighlights();
        CheckBothConfirmed();
    }

    private void UpdateConfirmButtons()
    {
        if (player1ConfirmButton != null)
            player1ConfirmButton.interactable = !player1Confirmed;

        if (player2ConfirmButton != null)
            player2ConfirmButton.interactable = !player2Confirmed;
    }

    private void UpdateHighlights()
    {
        // Keep both selection buttons visible
        if (player1Highlight != null)
            player1Highlight.SetActive(true);

        if (player2Highlight != null)
            player2Highlight.SetActive(true);
    }

    private void CheckBothConfirmed()
    {
        if (player1Confirmed && player2Confirmed)
        {
            Debug.Log("Both players confirmed their characters!");

            // Save Player 1
            CharacterSelectionData.Player1Character = player1Character;
            CharacterSelectionData.Player1Prefab = player1Prefab;

            // Save Player 2
            CharacterSelectionData.Player2Character = player2Character;
            CharacterSelectionData.Player2Prefab = player2Prefab;

            // Move to Map Selection
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
