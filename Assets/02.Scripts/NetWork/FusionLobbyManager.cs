using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionLobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runnerPrefab;

    private NetworkRunner runner;
    public void StartGame()
    {
        if (runner != null && runner.IsServer)
        {
            runner.LoadScene(SceneRef.FromIndex(2));
        }
    }
    public async void StartHost()
    {
        runner = Instantiate(runnerPrefab);

        runner.ProvideInput = true;

        var result = await runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = Random.Range(1000, 9999).ToString(),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });

        if (result.Ok)
        {
            Debug.Log("Host Ready");
        }

        runner.AddCallbacks(this);
    }

    public async void StartClient(string roomCode)
    {
        runner = Instantiate(runnerPrefab);

        runner.ProvideInput = true;

        await runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = roomCode,
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });

        runner.AddCallbacks(this);
    }

    public void LoadGameScene()
    {
        if (runner != null && runner.IsServer)
        {
            runner.LoadScene(SceneRef.FromIndex(2));
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var starter = FindFirstObjectByType<StarterAssetsInputs>();

        if (starter == null) return;

        NetworkInputData data = new();

        data.Move = starter.move;
        data.Look = starter.look;
        data.Jump = starter.jump;
        data.Sprint = starter.sprint;

        if (starter.jump)
            starter.ConsumeJump();

        input.Set(data);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}