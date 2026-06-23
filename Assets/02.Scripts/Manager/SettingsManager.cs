using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [SerializeField]
    private GameObject settingsPanel;

    public bool IsOpen
    {
        get;
        private set;
    }

    public float MouseSensitivity =>
        SettingsData.MouseSensitivity;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SettingsData.Load();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Toggle();
            }
        }
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        IsOpen = true;

        settingsPanel.SetActive(true);

        RefreshCursor();

        if (InputManager.Instance != null)
        {
            InputManager.Instance.InputBlocked = true;
            InputManager.Instance.ClearAllInput();
        }
    }

    public void Close()
    {
        IsOpen = false;

        settingsPanel.SetActive(false);

        RefreshCursor();

        if (InputManager.Instance != null)
        {
            InputManager.Instance.InputBlocked = false;
        }
    }

    public void RefreshCursor()
    {
        // 설정창이 열려있으면 최우선
        if (IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 캐릭터 선택창
        if (CharacterSelectUI.Instance != null &&
            CharacterSelectUI.Instance.gameObject.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 인게임
        if (FindFirstObjectByType<PlayerCharacter>() != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        // 로비
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}