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

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    private readonly Dictionary<PlayerRef, CharacterClassType>_selectedCharacters = new Dictionary<PlayerRef, CharacterClassType>();
    private readonly Dictionary<PlayerRef, PlayerLobbyObject> _playerLobbies = new Dictionary<PlayerRef, PlayerLobbyObject>();
    public override void Spawned()
    {
        Runner.AddCallbacks(this);
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

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player, CharacterClassType classType)
    {
        if (_spawnedPlayers.ContainsKey(player))
            return;
        Debug.Log(
    $"Spawning {classType} for {player.PlayerId}");
        Vector3 spawnPosition =
            new Vector3
            (
                Random.Range(-3f, 3f),
                1f,
                Random.Range(-3f, 3f)
                );

        NetworkObject prefab = GetCharacterPrefab(classType);

        NetworkObject playerObject =
            runner.Spawn(
                prefab,
                spawnPosition,
                Quaternion.identity,
                player);

        _spawnedPlayers.Add(player, playerObject);
        Debug.Log(
    $"Spawned {playerObject.name} InputAuthority:{player}");
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

        Debug.Log(
            $"Lobby Object Spawned : {player.PlayerId}");
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
}