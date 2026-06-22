using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button assassinButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button confirmButton;

    [SerializeField] private GameObject assassinLockObj;

    [SerializeField] private GameObject mageLockObj;

    [Header("UI")]
    [SerializeField] private TMP_Text selectedText;
    [SerializeField] private GameObject[] inGame_UI;
    [SerializeField]
    private TMP_Text timerText;

    private float _lockRefreshTimer;
    public static CharacterSelectUI Instance
    {
        get;
        private set;
    }
    public CharacterClassType SelectedClass
    {
        get;
        private set;
    } = CharacterClassType.None;

    private void Awake()
    {
        Instance = this;

        assassinButton.onClick.AddListener(OnClickAssassin);
        mageButton.onClick.AddListener(OnClickMage);
        confirmButton.onClick.AddListener(OnClickConfirm);
    }
   
    private void Update()
    {
        if (RoundManager.Instance == null)
            return;

        if (!RoundManager.Instance.Object)
            return;

        if (!RoundManager.Instance.Object.IsValid)
            return;

        RoundManager round = RoundManager.Instance;

        if (round.CurrentState == RoundState.CharacterSelect)
        {
            timerText.text = $"{Mathf.CeilToInt(round.StateRemainTime)}";
        }

        _lockRefreshTimer += Time.deltaTime;

        if (_lockRefreshTimer >= 0.2f)
        {
            _lockRefreshTimer = 0f;

            RefreshCharacterLock();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        selectedText.text = "선택된 캐릭터가 없습니다";

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

        selectedText.text = "어쎄신 선택";

        confirmButton.interactable = true;
    }

    private void OnClickMage()
    {
        SelectedClass = CharacterClassType.Mage;

        selectedText.text = "마법사 선택";

        confirmButton.interactable = true;
    }

    private void OnClickConfirm()
    {
        TeamType myTeam = GameBootstrap.Instance.GetPlayerTeam(PlayerLobbyObject.Local.Object.InputAuthority);

        if (GameBootstrap.Instance.IsCharacterUsedInTeam(SelectedClass,myTeam,PlayerLobbyObject.Local.Object.InputAuthority))
        {
            selectedText.text = "같은 팀에서 이미 선택한 캐릭터입니다.";

            return;
        }

        PlayerLobbyObject.Local.RPC_SelectCharacter(SelectedClass);

        PlayerLobbyObject.Local.RPC_SetReady();

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

        OnPlayerUI();

        gameObject.SetActive(false);
    }

    public void OnPlayerUI()
    {
        if (inGame_UI == null)
            return;

        foreach (GameObject ui in inGame_UI)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }
    }

    private void RefreshCharacterLock()
    {
        if (PlayerLobbyObject.Local == null)
        {
            return;
        }
        Debug.Log("RefreshCharacterLock");

        TeamType myTeam = GameBootstrap.Instance.GetPlayerTeam(PlayerLobbyObject.Local.Object.InputAuthority);

        bool assassinUsed = GameBootstrap.Instance.IsCharacterUsedInTeam(CharacterClassType.Assassin,myTeam,PlayerLobbyObject.Local.Object.InputAuthority);

        bool mageUsed = GameBootstrap.Instance.IsCharacterUsedInTeam(CharacterClassType.Mage,myTeam,PlayerLobbyObject.Local.Object.InputAuthority);

        assassinLockObj.SetActive(assassinUsed);

        mageLockObj.SetActive(mageUsed);

        assassinButton.interactable =!assassinUsed;

        mageButton.interactable =!mageUsed;
    }
}