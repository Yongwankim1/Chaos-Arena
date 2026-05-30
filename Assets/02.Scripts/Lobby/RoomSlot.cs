using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text hostText;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Toggle isLockToggle;
    [SerializeField] private Button joinButton;

    private LobbyManager lobby;
    private SessionInfo sessionInfo;

    public void Init(LobbyManager lobby, SessionInfo sessionInfo)
    {
        this.lobby = lobby;
        this.sessionInfo = sessionInfo;
        if (sessionInfo.Properties.TryGetValue("hostName", out SessionProperty hostValue))
        {
            hostText.text = hostValue.PropertyValue.ToString();
        }
        else
        {
            hostText.text = "Unknown";
        }
        roomNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount} / {sessionInfo.MaxPlayers}";

        bool hasPassword = false;

        if (sessionInfo.Properties.TryGetValue("hasPassword", out SessionProperty value))
        {
            hasPassword = (bool)value;
        }

        isLockToggle.isOn = hasPassword ?  true : false;

        joinButton.interactable = sessionInfo.IsOpen && sessionInfo.PlayerCount < sessionInfo.MaxPlayers;

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnClickJoin);
    }

    private void OnClickJoin()
    {
        lobby.SelectRoom(sessionInfo);
    }
}
