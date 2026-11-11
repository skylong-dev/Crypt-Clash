using UnityEngine;
using UnityEngine.InputSystem;

public class BattleCharacterSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Backup Player Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    private void Start()
    {
        SpawnCharacters();
    }

    private void SpawnCharacters()
    {
        GameObject player1ToSpawn = CharacterSelectionData.Player1Prefab != null
            ? CharacterSelectionData.Player1Prefab
            : player1Prefab;

        GameObject player2ToSpawn = CharacterSelectionData.Player2Prefab != null
            ? CharacterSelectionData.Player2Prefab
            : player2Prefab;

        if (player1ToSpawn == null || player2ToSpawn == null)
        {
            Debug.LogError("Assign a fighter prefab for both players.", this);
            return;
        }

        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("Assign both spawn points in the Inspector.", this);
            return;
        }

        GameObject spawnedPlayer1 = Instantiate(
            player1ToSpawn, player1SpawnPoint.position, Quaternion.identity
        );

        var gamepad = Gamepad.current;
        var joystick = Joystick.current;

        if (gamepad == null && joystick != null)
            ConfigureInput(spawnedPlayer1, "Joystick", joystick);
        else
            ConfigureInput(spawnedPlayer1, "Gamepad", gamepad);

        GameObject spawnedPlayer2 = Instantiate(
            player2ToSpawn, player2SpawnPoint.position, Quaternion.identity
        );
        ConfigureInput(spawnedPlayer2, "Keyboard", Keyboard.current);
    }

    private static void ConfigureInput(
        GameObject fighter, string scheme, InputDevice device)
    {
        PlayerInput input = fighter.GetComponent<PlayerInput>();
        if (input == null || !input.isActiveAndEnabled)
        {
            Debug.LogError(
                $"{fighter.name} needs an enabled Player Input component on its root.",
                fighter
            );
            return;
        }

        input.neverAutoSwitchControlSchemes = true;

        if (device == null)
        {
            input.DeactivateInput();

            // Release any device Unity automatically paired to this fighter.
            var user = input.user;
            if (user.valid)
                user.UnpairDevices();

            Debug.LogWarning(
                $"No device detected for the {scheme} scheme. Connect it and reload the match.",
                fighter
            );
            return;
        }

        var actions = input.actions;
        if (actions == null ||
            !actions.FindControlScheme(scheme).HasValue ||
            actions.FindActionMap("Player") == null)
        {
            input.DeactivateInput();
            Debug.LogError(
                $"{fighter.name} needs the updated Controls asset with the {scheme} scheme and Player map.",
                fighter
            );
            return;
        }

        input.SwitchCurrentControlScheme(scheme, device);
        input.SwitchCurrentActionMap("Player");
    }
}