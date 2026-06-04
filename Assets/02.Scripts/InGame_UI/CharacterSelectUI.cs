using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button assassinButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button confirmButton;

    [Header("UI")]
    [SerializeField] private TMP_Text selectedText;

    public CharacterClassType SelectedClass
    {
        get;
        private set;
    } = CharacterClassType.None;

    private void Awake()
    {
        assassinButton.onClick.AddListener(OnClickAssassin);
        mageButton.onClick.AddListener(OnClickMage);
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        selectedText.text = "Selected : None";

        confirmButton.interactable = false;
    }

    private void OnDestroy()
    {
        assassinButton.onClick.RemoveListener(OnClickAssassin);
        mageButton.onClick.RemoveListener(OnClickMage);
        confirmButton.onClick.RemoveListener(OnClickConfirm);
    }

    private void OnClickAssassin()
    {
        SelectedClass = CharacterClassType.Assassin;

        selectedText.text = "Selected : Assassin";

        confirmButton.interactable = true;
    }

    private void OnClickMage()
    {
        SelectedClass = CharacterClassType.Mage;

        selectedText.text = "Selected : Mage";

        confirmButton.interactable = true;
    }

    private void OnClickConfirm()
    {
        PlayerLobbyObject.Local
            .RPC_SelectCharacter(
                SelectedClass);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameObject.SetActive(false);
    }
}