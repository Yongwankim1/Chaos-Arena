using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> players = new();
    private void Start()
    {
        var runner = FindFirstObjectByType<NetworkRunner>();

        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
      
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        if (players.TryGetValue(player, out var obj))
        {
            runner.Despawn(obj);
            players.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        foreach (var player in runner.ActivePlayers)
        {
            if (players.ContainsKey(player))
                continue;

            Vector3 spawnPos = new Vector3(
                Random.Range(-3, 3),
                1,
                Random.Range(-3, 3));

            NetworkObject obj = runner.Spawn(
                playerPrefab,
                spawnPos,
                Quaternion.identity,
                player);

            players.Add(player, obj);
        }
    }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}