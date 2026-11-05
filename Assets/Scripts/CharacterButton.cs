
using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public CharacterSelectManager characterSelectManager;

    public Sprite characterSprite;

    public GameObject characterPrefab;

    public Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnCharacterClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnCharacterClicked);
    }


    public void OnCharacterClicked()
    {
        if (characterSelectManager != null &&
            characterPrefab != null &&
            characterSprite != null)
        {
            characterSelectManager.SelectCharacter(
                characterSprite,
                characterPrefab
            );
        }
    }
}