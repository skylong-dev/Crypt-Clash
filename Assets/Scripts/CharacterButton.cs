using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public CharacterSelectManager characterSelectManager;
    public Sprite characterSprite;
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
        if (characterSelectManager != null)
        {
            characterSelectManager.SelectCharacter(characterSprite);
        }
    }
}
