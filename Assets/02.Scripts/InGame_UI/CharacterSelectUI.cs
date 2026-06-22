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

        RefreshCharacterLock();
    }

    private void OnEnable()
    {
        PlayerLobbyObject.OnCharacterSelectionChanged += RefreshCharacterLock;

        RefreshCharacterLock();
    }

    private void OnDisable()
    {
        PlayerLobbyObject.OnCharacterSelectionChanged -= RefreshCharacterLock;
    }

    private void OnDestroy()
    {
        assassinButton.onClick.RemoveListener(OnClickAssassin);
        mageButton.onClick.RemoveListener(OnClickMage);
        confirmButton.onClick.RemoveListener(OnClickConfirm);
    }

    private void OnClickAssassin()
    {
        TrySelectClass(CharacterClassType.Assassin,"어쎄신 선택");
    }

    private void OnClickMage()
    {
        TrySelectClass(CharacterClassType.Mage,"마법사 선택");
    }

    private void TrySelectClass(CharacterClassType classType,string message)
    {
        if (IsClassLockedForLocalTeam(classType))
        {
            ClearSelectedClass("같은 팀에서 이미 선택한 캐릭터입니다.");

            RefreshCharacterLock();

            return;
        }

        SelectedClass = classType;

        selectedText.text = message;

        RefreshConfirmButton();
    }

    private void OnClickConfirm()
    {
        if (SelectedClass == CharacterClassType.None)
        {
            RefreshConfirmButton();

            return;
        }

        if (IsClassLockedForLocalTeam(SelectedClass))
        {
            ClearSelectedClass("같은 팀에서 이미 선택한 캐릭터입니다.");

            RefreshCharacterLock();

            return;
        }

        confirmButton.interactable = false;

        PlayerLobbyObject.Local.RPC_ConfirmCharacter(SelectedClass);
    }

    public void OnConfirmAccepted()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

        OnPlayerUI();

        gameObject.SetActive(false);
    }

    public void OnConfirmRejected()
    {
        ClearSelectedClass("같은 팀에서 이미 선택한 캐릭터입니다.");

        RefreshCharacterLock();
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

    public void RefreshCharacterLock()
    {
        if (PlayerLobbyObject.Local == null || GameBootstrap.Instance == null)
        {
            RefreshConfirmButton();

            return;
        }

        TeamType myTeam = GameBootstrap.Instance.GetPlayerTeam(PlayerLobbyObject.Local.Object.InputAuthority);

        bool assassinUsed = GameBootstrap.Instance.IsCharacterUsedInTeam(CharacterClassType.Assassin,myTeam,PlayerLobbyObject.Local.Object.InputAuthority);

        bool mageUsed = GameBootstrap.Instance.IsCharacterUsedInTeam(CharacterClassType.Mage,myTeam,PlayerLobbyObject.Local.Object.InputAuthority);

        assassinLockObj.SetActive(assassinUsed);

        mageLockObj.SetActive(mageUsed);

        assassinButton.interactable = !assassinUsed;

        mageButton.interactable = !mageUsed;

        if (SelectedClass == CharacterClassType.Assassin && assassinUsed ||
            SelectedClass == CharacterClassType.Mage && mageUsed)
        {
            ClearSelectedClass("같은 팀에서 이미 선택한 캐릭터입니다.");

            return;
        }

        RefreshConfirmButton();
    }

    private void ClearSelectedClass(string message)
    {
        SelectedClass = CharacterClassType.None;

        selectedText.text = message;

        RefreshConfirmButton();
    }

    private void RefreshConfirmButton()
    {
        confirmButton.interactable = SelectedClass != CharacterClassType.None &&
                                     !IsClassLockedForLocalTeam(SelectedClass);
    }

    private bool IsClassLockedForLocalTeam(CharacterClassType classType)
    {
        if (classType == CharacterClassType.None)
        {
            return false;
        }

        if (PlayerLobbyObject.Local == null || GameBootstrap.Instance == null)
        {
            return false;
        }

        TeamType myTeam = GameBootstrap.Instance.GetPlayerTeam(PlayerLobbyObject.Local.Object.InputAuthority);

        return GameBootstrap.Instance.IsCharacterUsedInTeam(classType,myTeam,PlayerLobbyObject.Local.Object.InputAuthority);
    }
}