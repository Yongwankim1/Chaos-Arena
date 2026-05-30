using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyManagerRefactorring : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runnerPrefab;

    [Header("Chat")]
    [SerializeField] private MainLobbyChat chat;
    [Header("NickNameChecker")]
    [SerializeField] private UserNickNameChecker nicknameChecker;
    private NetworkRunner currentRunner;

    [Header("Panels")]
    [SerializeField] private GameObject mainLobbyPanel;
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("RoomBtns")]
    [SerializeField] private Button roomCreateBtn;
    [SerializeField] private Button yesCreateBtn;
    [SerializeField] private Button closeBtn;

    [Header("InputField")]
    [SerializeField] private TMP_InputField roomNameIF;
    [SerializeField] private TMP_InputField roomPasswordIF;


    [Header("Room List")]
    [SerializeField] private Transform roomListParent;
    [SerializeField] private RoomSlot roomSlotPrefab;

    private SessionInfo selectedSession;
    private List<SessionInfo> cachedSessionList = new List<SessionInfo>();

    [Header("Network Room Player Data")]
    [SerializeField] private NetworkObject roomPlayerDataPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> roomPlayerDataObjects = new Dictionary<PlayerRef, NetworkObject>();
    [Header("Room UI")]
    [SerializeField] private RoomUserListUI roomUserListUI;
    [SerializeField] private RoomActionButtonUI roomActionButtonUI;

    void Awake()
    {
        if(nicknameChecker == null) nicknameChecker = GetComponent<UserNickNameChecker>();
        roomCreateBtn.interactable = false;
    }
    void OnEnable()
    {
        if (nicknameChecker != null) nicknameChecker.OnSetNickName += FusionConnect;

        roomCreateBtn.onClick.AddListener(OpenCreateRoomPanel);
        yesCreateBtn.onClick.AddListener(CreateRoom);
        closeBtn.onClick.AddListener(CloseCreateRoomPanel);
    }
    void OnDisable()
    {
        if (nicknameChecker != null) nicknameChecker.OnSetNickName -= FusionConnect;

        roomCreateBtn.onClick.RemoveAllListeners();
        yesCreateBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();
    }

    public async void FusionConnect()
    {
        if (currentRunner != null)
        {
            await currentRunner.Shutdown();

            if (currentRunner != null)
            {
                Destroy(currentRunner.gameObject);
                currentRunner = null;
            }
        }

        currentRunner = Instantiate(runnerPrefab);
        currentRunner.AddCallbacks(this);

        var result = await currentRunner.JoinSessionLobby(SessionLobby.Shared);

        if (result.Ok)
        {
            Debug.Log("Fusion 로비 접속 성공");
            roomCreateBtn.interactable = true;
        }
        else
        {
            Debug.Log("Fusion 로비 접속 실패: " + result.ShutdownReason);
            roomCreateBtn.interactable = false;
        }
    }

    private void CloseCreateRoomPanel()
    {
        EscManager.Instance.ClosePanel();
    }
    private void OpenCreateRoomPanel()
    {
        EscManager.Instance.OpenPanel(createRoomPanel);
    }
    private async void CreateRoom()
    {
        if(currentRunner == null)
        {
            Debug.Log("러너 준비되지 않음");
            return;
        }
        string sessionName = roomNameIF.text.Trim();
        string password = roomPasswordIF.text.Trim();

        if (string.IsNullOrWhiteSpace(sessionName))
        {
            sessionName = Random.Range(1,10000).ToString();
        }

        if (IsRoomNameExists(sessionName))
        {
            Debug.Log("이미 존재하는 방 이름입니다.");
            return;
        }
        Dictionary<string, SessionProperty> properties = new Dictionary<string, SessionProperty>();

        properties.Add("hostName", UserDataManager.Instance.UserData.UserName);

        if (!string.IsNullOrWhiteSpace(password))
        {
            properties.Add("password", password);
            properties.Add("hasPassword", true);
        }
        else
        {
            properties.Add("hasPassword", false);
        }

        var result = await currentRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            PlayerCount = 2,
            SessionProperties = properties,
            SceneManager = currentRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("방 생성 성공: " + sessionName);

            EscManager.Instance.ClosePanel();

            mainLobbyPanel.SetActive(false);
            roomPanel.SetActive(true);

            chat.Unsubscribe();

            roomUserListUI.Refresh();
            roomActionButtonUI.Init(currentRunner);
        }
        else
        {
            Debug.Log("방 생성 실패: " + result.ShutdownReason);
        }
    }

    private bool IsRoomNameExists(string roomName)
    {
        foreach (SessionInfo session in cachedSessionList)
        {
            if (session.Name == roomName)
            {
                return true;
            }
        }

        return false;
    }
    public void SelectRoom(SessionInfo sessionInfo)
    {
        selectedSession = sessionInfo;

        Debug.Log("선택한 방: " + sessionInfo.Name);

        // 비밀번호 있는 방이면 비밀번호 입력 패널 열기
        if (sessionInfo.Properties.TryGetValue("hasPassword", out SessionProperty value) && (bool)value)
        {
            Debug.Log("비밀번호 필요");
            // EscManager.Instance.OpenPanel(passwordPanel);
            return;
        }

        JoinSelectedRoom("");
    }
    public async void JoinSelectedRoom(string inputPassword)
    {
        if (selectedSession == null) return;

        if (selectedSession.Properties.TryGetValue("password", out SessionProperty pw))
        {
            if (pw.PropertyValue.ToString() != inputPassword)
            {
                Debug.Log("비밀번호가 틀렸습니다.");
                return;
            }
        }

        var result = await currentRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = selectedSession.Name,
            SceneManager = currentRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("방 입장 성공: " + selectedSession.Name);

            mainLobbyPanel.SetActive(false);
            roomPanel.SetActive(true);

            chat.Unsubscribe();

            roomUserListUI.Refresh();
            roomActionButtonUI.Init(currentRunner);
        }
        else
        {
            Debug.Log("방 입장 실패: " + result.ShutdownReason);
        }
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        cachedSessionList = sessionList;

        foreach (Transform child in roomListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (SessionInfo session in sessionList)
        {
            if (!session.IsOpen) continue;
            if (!session.IsVisible) continue;

            RoomSlot slot = Instantiate(roomSlotPrefab, roomListParent);
            slot.Init(this, session);
        }
    }

    private void RefreshRoomUI()
    {
        if (roomUserListUI != null)
        {
            roomUserListUI.Refresh();
        }

        if (roomActionButtonUI != null)
        {
            roomActionButtonUI.Refresh();
        }
    }
    private void SpawnRoomPlayerData(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (roomPlayerDataObjects.ContainsKey(player))
            return;

        NetworkObject obj = runner.Spawn(
            roomPlayerDataPrefab,
            Vector3.zero,
            Quaternion.identity,
            player
        );

        roomPlayerDataObjects.Add(player, obj);

        Invoke(nameof(RefreshRoomUI), 0.2f);
    }


    public async void ExitRoom()
    {
        if (currentRunner == null)
            return;

        Debug.Log("방 나가기 시작");

        await currentRunner.Shutdown();

        if (currentRunner != null)
        {
            Destroy(currentRunner.gameObject);
            currentRunner = null;
        }

        ClearRoomUI();

        roomPanel.SetActive(false);
        mainLobbyPanel.SetActive(true);

        roomCreateBtn.interactable = false;

        FusionConnect();

        if (chat != null)
        {
            chat.Connect();
        }

        Debug.Log("방 나가기 완료");
    }

    private void ClearRoomUI()
    {
        if (roomUserListUI != null)
        {
            roomUserListUI.Clear();
        }

        if (roomActionButtonUI != null)
        {
            roomActionButtonUI.Clear();
        }
    }
    public void OnConnectedToServer(NetworkRunner runner) {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){ }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){ }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){ }

    public void OnInput(NetworkRunner runner, NetworkInput input){ }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){ }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        SpawnRoomPlayerData(runner, player);

        Invoke(nameof(RefreshRoomUI), 0.2f);
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("플레이어 나감: " + player);

        if (runner.IsServer)
        {
            if (roomPlayerDataObjects.TryGetValue(player, out NetworkObject obj))
            {
                if (obj != null && obj.IsValid)
                {
                    runner.Despawn(obj);
                }

                roomPlayerDataObjects.Remove(player);
            }
        }

        Invoke(nameof(RefreshRoomUI), 0.2f);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner){ }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner 종료: " + shutdownReason);

        ClearRoomUI();

        roomPlayerDataObjects.Clear();

        if (currentRunner == runner)
        {
            currentRunner = null;
        }

        roomPanel.SetActive(false);
        mainLobbyPanel.SetActive(true);

        roomCreateBtn.interactable = false;
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
