using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyFlow : MonoBehaviour
{
    [Header("Runner")]
    public NetworkRunner runnerPrefab;

    [Header("UI")]
    public TMP_InputField roomInput;
    public Button createButton;
    public Button joinButton;
    public Button startButton;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public TMP_Text roomNumberText;
    public TMP_Text playerListText;
    public TMP_Text roomStatusText;
    private void Start()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        createButton.onClick.AddListener(CreateRoom);
        joinButton.onClick.AddListener(JoinRoom);
        startButton.onClick.AddListener(StartGame);
    }
    private void Update()
    {
        var lobby = FindFirstObjectByType<FusionLobbyManager>();

        if (lobby == null) return;

        roomNumberText.text =
            $"Room_Num : {lobby.CurrentRoomCode}";

        playerListText.text = "";

        foreach (var player in lobby.Players)
        {
            playerListText.text +=
                $"Player {player.PlayerId}\n";
        }

        roomStatusText.text =
            $"Player_Count : {lobby.Players.Count}";
    }
    private void CreateRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);

        FindFirstObjectByType<FusionLobbyManager>()
            .StartHost();
    }
    private void JoinRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);

        FindFirstObjectByType<FusionLobbyManager>()
            .StartClient(roomInput.text);
    }
    private void StartGame()
    {
        FindFirstObjectByType<FusionLobbyManager>()
            .StartGame();
    }
}