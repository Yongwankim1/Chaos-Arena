using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runnerPrefab;

    [Header("Chat")]
    [SerializeField] private MainLobbyChat chat;
    [SerializeField] private RoomChat roomChat;
    [Header("NickNameChecker")]
    [SerializeField] private UserNickNameChecker nicknameChecker;
    private NetworkRunner currentRunner;
    private bool isJoiningRoom;
    private bool isLeavingRoom;
    private bool isRecoveringLobby;

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

    [SerializeField]
    private TMP_Dropdown matchTypeDropdown;

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
    [SerializeField] private RoomExitButtonUI roomExitButtonUI;
    [SerializeField] private RoomInfoView roomInfoView;
    [SerializeField] private LobbyMapSelector lobbyMapSelector;

    void Awake()
    {
        if(nicknameChecker == null) nicknameChecker = GetComponent<UserNickNameChecker>();
        if (roomUserListUI == null) roomUserListUI = FindFirstObjectByType<RoomUserListUI>(FindObjectsInactive.Include);
        if (roomActionButtonUI == null) roomActionButtonUI = FindFirstObjectByType<RoomActionButtonUI>(FindObjectsInactive.Include);
        if (roomInfoView == null) roomInfoView = FindFirstObjectByType<RoomInfoView>(FindObjectsInactive.Include);
        if (lobbyMapSelector == null) lobbyMapSelector = FindFirstObjectByType<LobbyMapSelector>(FindObjectsInactive.Include);
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
        currentRunner.ProvideInput = true;
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
        int selectedMapIndex = lobbyMapSelector != null
            ? lobbyMapSelector.GetCreateRoomMapIndex()
            : 0;

        properties.Add("hostName", UserDataManager.Instance.UserData.UserName);
        properties.Add("isPlaying", false);
        properties.Add("mapIndex", selectedMapIndex);

        MatchType matchType =(MatchType)(matchTypeDropdown.value == 0 ? 2 :matchTypeDropdown.value == 1 ? 4 : 6); // H

        properties.Add("matchType", matchTypeDropdown.value); // H

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
            PlayerCount = (int)matchType,
            SessionProperties = properties,
            SceneManager = currentRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("방 생성 성공: " + sessionName);

            EscManager.Instance.ClosePanel();
            RoomSessionData.MatchType = matchType;

            mainLobbyPanel.SetActive(false);
            roomPanel.SetActive(true);

            chat.Unsubscribe();
            roomChat?.ConnectToRoom(sessionName);

            roomUserListUI.Refresh();
            roomActionButtonUI.Init(currentRunner);
            roomExitButtonUI.Init(this);
            roomInfoView.SetRoomInfo(sessionName, UserDataManager.Instance.UserData.UserName);
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
        if (isJoiningRoom)
            return;

        if (sessionInfo == null)
            return;

        if (!sessionInfo.IsOpen || sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
        {
            Debug.Log("Cannot join this room right now.");
            return;
        }

        if (IsSessionPlaying(sessionInfo))
        {
            Debug.Log("Game already started.");
            return;
        }

        selectedSession = sessionInfo;

        Debug.Log("Selected room: " + sessionInfo.Name);

        if (sessionInfo.Properties.TryGetValue("hasPassword", out SessionProperty value) && (bool)value)
        {
            Debug.Log("Password required.");
            // EscManager.Instance.OpenPanel(passwordPanel);
            return;
        }

        JoinSelectedRoom("");
    }
    public async void JoinSelectedRoom(string inputPassword)
    {
        if (isJoiningRoom)
            return;

        if (selectedSession == null) return;

        if (currentRunner == null)
        {
            Debug.Log("Runner is not ready.");
            FusionConnect();
            return;
        }

        if (!selectedSession.IsOpen || selectedSession.PlayerCount >= selectedSession.MaxPlayers)
        {
            Debug.Log("Cannot join this room right now.");
            return;
        }

        if (IsSessionPlaying(selectedSession))
        {
            Debug.Log("Game already started.");
            return;
        }

        if (selectedSession.Properties.TryGetValue("password", out SessionProperty pw))
        {
            if (pw.PropertyValue.ToString() != inputPassword)
            {
                Debug.Log("Wrong password.");
                return;
            }
        }

        isJoiningRoom = true;
        roomCreateBtn.interactable = false;

        try
        {
            var result = await currentRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = selectedSession.Name,
                SceneManager = currentRunner.GetComponent<NetworkSceneManagerDefault>()
            });

            if (result.Ok)
            {
                Debug.Log("Joined room: " + selectedSession.Name);

                if (selectedSession.Properties.TryGetValue("matchType", out SessionProperty property))
                {
                    int value =(int)property;

                    RoomSessionData.MatchType =
                        value == 0
                        ? MatchType.OneVsOne
                        : value == 1
                        ? MatchType.TwoVsTwo
                        : MatchType.ThreeVsThree;
                }

                mainLobbyPanel.SetActive(false);
                roomPanel.SetActive(true);

                chat.Unsubscribe();
                roomChat?.ConnectToRoom(selectedSession.Name);

                roomUserListUI.Refresh();
                roomActionButtonUI.Init(currentRunner);
                roomExitButtonUI.Init(this);
                roomInfoView.SetRoomInfo(selectedSession.Name, GetHostName(selectedSession));
            }
            else
            {
                Debug.Log("Join room failed: " + result.ShutdownReason);
                await RecoverLobbyAfterFailedJoin();
            }
        }
        catch (Exception exception)
        {
            Debug.Log("Join room exception: " + exception.Message);
            await RecoverLobbyAfterFailedJoin();
        }
        finally
        {
            isJoiningRoom = false;
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
            if (IsSessionPlaying(session)) continue;

            RoomSlot slot = Instantiate(roomSlotPrefab, roomListParent);
            slot.Init(this, session);
        }
    }

    private bool IsSessionPlaying(SessionInfo sessionInfo)
    {
        if (sessionInfo == null)
            return false;

        if (!sessionInfo.Properties.TryGetValue("isPlaying", out SessionProperty isPlayingProperty))
            return false;

        return (bool)isPlayingProperty;
    }

    private bool CanUseLobbyUI()
    {
        return this != null && roomPanel != null && mainLobbyPanel != null && roomCreateBtn != null;
    }

    private void ScheduleRefreshRoomUI()
    {
        if (!CanUseLobbyUI() || !isActiveAndEnabled)
            return;

        Invoke(nameof(RefreshRoomUI), 0.2f);
    }

    private void RefreshRoomUI()
    {
        if (!CanUseLobbyUI())
            return;

        if (!roomPanel.activeInHierarchy)
            return;

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

        RoomPlayerData roomPlayerData =
            obj.GetComponent<RoomPlayerData>();

        if (roomPlayerData != null &&
            player == runner.LocalPlayer)
        {
            roomPlayerData.SetReadyFromState(true);
        }

        roomPlayerDataObjects.Add(player, obj);

        ScheduleRefreshRoomUI();
    }


    public async void ExitRoom()
    {
        if (currentRunner == null)
            return;

        Debug.Log("방 나가기 시작");

        isLeavingRoom = true;
        NetworkRunner runner = currentRunner;

        if (runner.IsServer && HasRemotePlayersInRoom())
        {
            RequestRemotePlayersExitRoom();
            await WaitUntilRemotePlayersLeft(2000);
        }

        await runner.Shutdown();

        if (runner != null)
        {
            Destroy(runner.gameObject);
            currentRunner = null;
        }

        ClearRoomUI();
        roomChat?.LeaveRoom();

        roomPanel.SetActive(false);
        mainLobbyPanel.SetActive(true);

        roomCreateBtn.interactable = false;

        FusionConnect();

        if (chat != null)
        {
            chat.Connect();
        }

        isLeavingRoom = false;
        Debug.Log("방 나가기 완료");
    }

    public async void ExitRoomRequestedByHost()
    {
        if (currentRunner == null)
            return;

        Debug.Log("호스트가 방을 종료하여 로비로 돌아갑니다.");

        isLeavingRoom = true;
        NetworkRunner runner = currentRunner;

        await runner.Shutdown();

        if (runner != null)
        {
            Destroy(runner.gameObject);
            currentRunner = null;
        }

        ClearRoomUI();
        roomChat?.LeaveRoom();

        roomPanel.SetActive(false);
        mainLobbyPanel.SetActive(true);

        roomCreateBtn.interactable = false;

        FusionConnect();

        if (chat != null)
        {
            chat.Connect();
        }

        isLeavingRoom = false;
    }

    private void RequestRemotePlayersExitRoom()
    {
        RoomPlayerData[] players =
            FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null) continue;
            if (player.Object == null) continue;
            if (!player.Object.IsValid) continue;
            if (player.Object.InputAuthority == currentRunner.LocalPlayer) continue;

            player.RPC_RequestExitRoomByHost();
        }
    }

    private async System.Threading.Tasks.Task WaitUntilRemotePlayersLeft(int timeoutMilliseconds)
    {
        int waitedMilliseconds = 0;
        const int intervalMilliseconds = 100;

        while (HasRemotePlayersInRoom() && waitedMilliseconds < timeoutMilliseconds)
        {
            await System.Threading.Tasks.Task.Delay(intervalMilliseconds);
            waitedMilliseconds += intervalMilliseconds;
        }
    }

    private bool HasRemotePlayersInRoom()
    {
        if (currentRunner == null)
            return false;

        RoomPlayerData[] players =
            FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null) continue;
            if (player.Object == null) continue;
            if (!player.Object.IsValid) continue;

            if (player.Object.InputAuthority != currentRunner.LocalPlayer)
                return true;
        }

        return false;
    }

    private void ClearRoomUI()
    {
        if (this == null)
            return;

        CancelInvoke(nameof(RefreshRoomUI));

        if (!CanUseLobbyUI())
            return;

        if (roomUserListUI != null)
        {
            roomUserListUI.Clear();
        }

        if (roomActionButtonUI != null)
        {
            roomActionButtonUI.Clear();
        }

        if (roomInfoView != null)
        {
            roomInfoView.Clear();
        }
    }

    private string GetHostName(SessionInfo sessionInfo)
    {
        if (sessionInfo != null &&
            sessionInfo.Properties.TryGetValue("hostName", out SessionProperty hostNameProperty))
        {
            return hostNameProperty.PropertyValue.ToString();
        }

        return "Unknown";
    }

    private async System.Threading.Tasks.Task RecoverLobbyAfterFailedJoin()
    {
        if (isRecoveringLobby)
            return;

        isRecoveringLobby = true;
        selectedSession = null;
        ClearRoomUI();
        roomChat?.LeaveRoom();

        roomPanel.SetActive(false);
        mainLobbyPanel.SetActive(true);
        roomCreateBtn.interactable = false;

        if (currentRunner != null)
        {
            NetworkRunner runner = currentRunner;
            currentRunner = null;

            await runner.Shutdown();

            if (runner != null)
            {
                Destroy(runner.gameObject);
            }
        }

        FusionConnect();

        if (chat != null)
        {
            chat.Connect();
        }

        isRecoveringLobby = false;
    }
    public void OnConnectedToServer(NetworkRunner runner) {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){ }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){ }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){ }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){ }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!CanUseLobbyUI())
            return;

        if (!runner.IsServer)
            return;

        SpawnRoomPlayerData(runner, player);

        ScheduleRefreshRoomUI();
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!CanUseLobbyUI())
            return;

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

        ScheduleRefreshRoomUI();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner){ }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner 종료: " + shutdownReason);

        if (!CanUseLobbyUI())
        {
            roomPlayerDataObjects.Clear();

            if (currentRunner == runner)
            {
                currentRunner = null;
            }

            return;
        }

        bool wasInRoom = roomPanel.activeSelf;

        ClearRoomUI();
        roomChat?.LeaveRoom();

        roomPlayerDataObjects.Clear();

        if (currentRunner == runner)
        {
            currentRunner = null;
        }

        roomPanel.SetActive(false);
        mainLobbyPanel.SetActive(true);

        roomCreateBtn.interactable = false;

        if (wasInRoom && !isLeavingRoom && !isRecoveringLobby)
        {
            RecoverLobbyAfterRemoteShutdown(runner);
        }
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    private void RecoverLobbyAfterRemoteShutdown(NetworkRunner runner)
    {
        if (isRecoveringLobby)
            return;

        isRecoveringLobby = true;

        if (runner != null)
        {
            Destroy(runner.gameObject);
        }

        FusionConnect();

        if (chat != null)
        {
            chat.Connect();
        }

        isRecoveringLobby = false;
    }

    //h
    public int GetMaxTeamCount()
    {
        if (currentRunner == null)
        {
            return 1;
        }

        if (!currentRunner.SessionInfo.Properties.TryGetValue("matchType",out SessionProperty property))
        {
            return 1;
        }

        int matchType =
            (int)property;

        switch (matchType)
        {
            case 0:
                return 1;

            case 1:
                return 2;

            case 2:
                return 3;
        }

        return 1;
    }
}
