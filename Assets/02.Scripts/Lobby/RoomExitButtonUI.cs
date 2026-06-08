using UnityEngine;
using UnityEngine.UI;

public class RoomExitButtonUI : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    private LobbyManager lobbyManager;

    private void OnEnable()
    {
        exitButton.onClick.AddListener(OnClickExit);
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(OnClickExit);
    }

    public void Init(LobbyManager lobbyManager)
    {
        this.lobbyManager = lobbyManager;
    }

    private void OnClickExit()
    {
        if (lobbyManager == null) return;

        lobbyManager.ExitRoom();
    }
}