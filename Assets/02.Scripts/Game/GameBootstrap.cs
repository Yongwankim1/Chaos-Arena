using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab")]
    [SerializeField] private NetworkObject playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers =
        new Dictionary<PlayerRef, NetworkObject>();

    public override void Spawned()
    {
        Runner.AddCallbacks(this);
    }

    // 씬 로딩 완료 후 기존 플레이어 생성
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer)
            return;

        foreach (var player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedPlayers.ContainsKey(player))
            return;

        Vector3 spawnPosition =
            new Vector3(
                Random.Range(-3f, 3f),
                1f,
                Random.Range(-3f, 3f)
            );

        NetworkObject playerObject =
            runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player
            );

        _spawnedPlayers.Add(player, playerObject);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (_spawnedPlayers.TryGetValue(player, out var playerObject))
        {
            runner.Despawn(playerObject);
            _spawnedPlayers.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkStarterAssetsInput localInput =
            FindObjectOfType<NetworkStarterAssetsInput>();

        if (localInput == null)
            return;

        NetworkInputData data = new NetworkInputData();

        data.Move = localInput.Move;
        data.Look = localInput.Look;
        data.Jump = localInput.ConsumeJump();
        data.Sprint = localInput.Sprint;

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