using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab")]
    [SerializeField]
    private NetworkObject assassinPrefab;
    [SerializeField]
    private NetworkObject magePrefab;
    [SerializeField]
    private NetworkObject playerLobbyPrefab;



    public static GameBootstrap Instance
    {
        get;
        private set;
    }

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    private readonly Dictionary<PlayerRef, CharacterClassType>_selectedCharacters = new Dictionary<PlayerRef, CharacterClassType>();
    private readonly Dictionary<PlayerRef, PlayerLobbyObject> _playerLobbies = new Dictionary<PlayerRef, PlayerLobbyObject>();
    private readonly Dictionary<PlayerRef, TeamType> _playerTeams = new Dictionary<PlayerRef, TeamType>();
    public override async void Spawned()
    {
        Instance = this;

        Runner.AddCallbacks(this);

        await LoadClassDatas();
    }
    private async System.Threading.Tasks.Task LoadClassDatas()
    {
        ClassData assassin =
            await ClassDataLoader.LoadClassData(
                CharacterClassType.Assassin);

        ClassDataManager.AddData(
            CharacterClassType.Assassin,
            assassin);

        ClassData mage =
            await ClassDataLoader.LoadClassData(
                CharacterClassType.Mage);

        ClassDataManager.AddData(
            CharacterClassType.Mage,
            mage);

        Debug.Log("ClassData Load Complete");
    }
    // 씬 로딩 완료 후 기존 플레이어 생성
    public void OnSceneLoadDone(
     NetworkRunner runner)
    {
        if (!runner.IsServer)
            return;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (_playerLobbies.ContainsKey(player))
                continue;

            NetworkObject lobbyObject =
                runner.Spawn(
                    playerLobbyPrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    player);

            PlayerLobbyObject lobby =
                lobbyObject.GetComponent<PlayerLobbyObject>();

            _playerLobbies.Add(
                player,
                lobby);

            Debug.Log(
                $"Lobby Object Spawned : {player.PlayerId}");
        }
    }
    public void AssignTeams()
    {
        _playerTeams.Clear();

        List<PlayerRef> players =
            new List<PlayerRef>(
                Runner.ActivePlayers);

        if (players.Count < 2)
            return;

        int randomIndex =
            Random.Range(
                0,
                players.Count);

        PlayerRef bluePlayer =
            players[randomIndex];

        PlayerRef redPlayer =
            players.Find(
                p => p != bluePlayer);

        _playerTeams.Add(
            bluePlayer,
            TeamType.Blue);

        _playerTeams.Add(
            redPlayer,
            TeamType.Red);

        Debug.Log(
            $"Blue : {bluePlayer.PlayerId}");

        Debug.Log(
            $"Red : {redPlayer.PlayerId}");
    }
    private void SpawnPlayer(
     NetworkRunner runner,
     PlayerRef player,
     CharacterClassType classType)
    {
        if (_spawnedPlayers.ContainsKey(player))
            return;

        if (!_playerTeams.TryGetValue(
                player,
                out TeamType team))
        {
            Debug.LogError(
                $"Team Not Assigned : {player.PlayerId}");

            return;
        }

        Vector3 spawnPosition =
            SpawnManager.Instance
                .GetSpawnPosition(
                    team);

        NetworkObject prefab =
            GetCharacterPrefab(
                classType);

        NetworkObject playerObject =
            runner.Spawn(
                prefab,
                spawnPosition,
                Quaternion.identity,
                player);

        PlayerCharacter playerCharacter =
            playerObject.GetComponent<PlayerCharacter>();

        if (playerCharacter != null)
        {
            playerCharacter.ClassType =
                classType;

            playerCharacter.Team =
                team;
        }

        _spawnedPlayers.Add(
            player,
            playerObject);

        Debug.Log(
            $"Spawned {playerObject.name} Team:{team}");
    }
    public void SpawnSelectedCharacter(
     PlayerRef player)
    {
        if (!_playerLobbies.TryGetValue(
                player,
                out PlayerLobbyObject lobby))
        {
            Debug.LogWarning(
                $"Lobby not found : {player.PlayerId}");

            return;
        }

        SpawnPlayer(
            Runner,
            player,
            lobby.SelectedClass);
    }

    private NetworkObject GetCharacterPrefab(
    CharacterClassType classType)
    {
        switch (classType)
        {
            case CharacterClassType.Assassin:
                return assassinPrefab;

            case CharacterClassType.Mage:
                return magePrefab;

            default:
                return assassinPrefab;
        }
    }

    public void OnPlayerJoined(
      NetworkRunner runner,
      PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        NetworkObject lobbyObject =
            runner.Spawn(
                playerLobbyPrefab,
                Vector3.zero,
                Quaternion.identity,
                player);

        PlayerLobbyObject lobby =
            lobbyObject.GetComponent<PlayerLobbyObject>();

        _playerLobbies.Add(
            player,
            lobby);

        Debug.Log($"Lobby Object Spawned : {player.PlayerId}");
         
    }
    public void OnPlayerLeft(
     NetworkRunner runner,
     PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (_playerLobbies.TryGetValue(
                player,
                out PlayerLobbyObject lobby))
        {
            runner.Despawn(
                lobby.Object);

            _playerLobbies.Remove(player);
        }

        if (_spawnedPlayers.TryGetValue(
                player,
                out var playerObject))
        {
            runner.Despawn(playerObject);

            _spawnedPlayers.Remove(player);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (InputManager.Instance == null)
            return;

        NetworkInputData data = new NetworkInputData();

        data.Move = InputManager.Instance.Move;
        data.Look = InputManager.Instance.Look;

        data.Yaw = Camera.main.transform.eulerAngles.y;

        data.Jump = InputManager.Instance.ConsumeJump();
        data.Sprint = InputManager.Instance.Sprint;
        data.Attack = InputManager.Instance.ConsumeAttack();
        data.Dash = InputManager.Instance.ConsumeDash();
        data.SkillQ = InputManager.Instance.ConsumeSkillQ();
        data.SkillE = InputManager.Instance.ConsumeSkillE();
        data.SkillR = InputManager.Instance.ConsumeSkillR();

        input.Set(data);
    }

    public void OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }

    public void OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(
        NetworkRunner runner,
        List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        System.ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
    }

    public void OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void ForceSelectRemainingPlayers()
    {
        if (!Runner.IsServer)
            return;

        foreach (var pair in _playerLobbies)
        {
            PlayerRef player =
                pair.Key;

            PlayerLobbyObject lobby =
                pair.Value;

            if (lobby.SelectedClass !=
                CharacterClassType.None)
            {
                continue;
            }

            lobby.SelectedClass = CharacterClassType.Assassin;

            Debug.Log(
                $"Force Assassin : {player.PlayerId}");

            CharacterSelectUI.Instance.OnPlayerUI();

            SpawnSelectedCharacter(player);
        }

        RPC_CloseCharacterSelectUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CloseCharacterSelectUI()
    {
        if (CharacterSelectUI.Instance == null)
            return;

        CharacterSelectUI.Instance
            .gameObject.SetActive(false);

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }
}