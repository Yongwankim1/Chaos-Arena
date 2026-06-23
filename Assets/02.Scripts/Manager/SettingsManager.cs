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

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

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

        bool inGame = FindFirstObjectByType<PlayerCharacter>() != null;

        if (inGame)
        {
            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = false;
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.InputBlocked = false;
        }
    }
}