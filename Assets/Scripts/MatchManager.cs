

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Match Settings")]
    public int startingStocks = 3;
    public float matchTime = 180f;
    public float respawnDelay = 2f;

    [Header("Spawner")]
    public BattlesCharacterSpawner playerSpawner;

    [Header("Player References")]
    private Damage player1;
    private Damage player2;

    [Header("UI Text")]
    public TMP_Text timerText;
    public TMP_Text player1StocksText;
    public TMP_Text player2StocksText;

    private int player1Stocks;
    private int player2Stocks;

    private bool matchEnded = false;

    public static string WinnerMessage = "";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player1Stocks = startingStocks;
        player2Stocks = startingStocks;

        if (playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner is not assigned!");
            return;
        }

        player1 = playerSpawner.SpawnPlayer(1);
        player2 = playerSpawner.SpawnPlayer(2);

        UpdateUI();
    }

    void Update()
    {
        if (matchEnded)
            return;

        matchTime -= Time.deltaTime;
        matchTime = Mathf.Max(0, matchTime);

        UpdateUI();

        if (matchTime <= 0)
            EndMatch();
    }

    public void PlayerLostStock(int playerNumber)
    {
        if (matchEnded)
            return;

        Damage deadPlayer;

        if (playerNumber == 1)
        {
            player1Stocks = Mathf.Max(0, player1Stocks - 1);
            deadPlayer = player1;
            player1 = null;
        }
        else if (playerNumber == 2)
        {
            player2Stocks = Mathf.Max(0, player2Stocks - 1);
            deadPlayer = player2;
            player2 = null;
        }
        else
        {
            return;
        }

        if (deadPlayer != null)
            Destroy(deadPlayer.gameObject);

        UpdateUI();

        if (player1Stocks <= 0 || player2Stocks <= 0)
        {
            EndMatch();
            return;
        }

        StartCoroutine(RespawnPlayer(playerNumber));
    }

    IEnumerator RespawnPlayer(int playerNumber)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (matchEnded)
            yield break;

        Damage newPlayer = playerSpawner.SpawnPlayer(playerNumber);

        if (playerNumber == 1)
            player1 = newPlayer;
        else
            player2 = newPlayer;
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(matchTime / 60);
        int seconds = Mathf.FloorToInt(matchTime % 60);

        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}";

        if (player1StocksText != null)
            player1StocksText.text = "P1  " + CreateHearts(player1Stocks);

        if (player2StocksText != null)
            player2StocksText.text = "P2  " + CreateHearts(player2Stocks);
    }

    string CreateHearts(int currentStocks)
    {
        string hearts = "";

        for (int i = 0; i < startingStocks; i++)
        {
            if (i < currentStocks)
                hearts += "<color=#FF3333>♥</color> ";
            else
                hearts += "<color=#777777>♡</color> ";
        }

        return hearts;
    }

    void EndMatch()
    {
        if (matchEnded)
            return;

        matchEnded = true;

        if (player1Stocks > player2Stocks)
            WinnerMessage = "Player 1 Wins!";
        else if (player2Stocks > player1Stocks)
            WinnerMessage = "Player 2 Wins!";
        else
            WinnerMessage = "It's a Draw!";

        Debug.Log(WinnerMessage);

        StopAllCoroutines();

        SceneManager.LoadScene("WinnerMenu");
    }
}
