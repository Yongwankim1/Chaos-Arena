using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef characterPrefab;

    private NetworkRunner _runner;
    private StarterAssetsInputs _localInput;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var scene =
            SceneRef.FromIndex(
                SceneManager
                .GetActiveScene()
                .buildIndex);

        await _runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = "Room",
                Scene = scene,
                SceneManager =
                    GetComponent<NetworkSceneManagerDefault>()
            });
    }

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        if (_localInput == null)
        {
            _localInput =
                FindFirstObjectByType<StarterAssetsInputs>();

            if (_localInput == null)
                return;
        }

        NetworkInputData data =
            new NetworkInputData();

        data.Move =
            _localInput.move;

        data.Look =
            _localInput.look;

        data.Jump =
            _localInput.jump;

        data.Sprint =
            _localInput.sprint;

        if (_localInput.jump)
        {
            _localInput.ConsumeJump();
        }

        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPos =
                new Vector3(
                    UnityEngine.Random.Range(-5, 5),
                    0.5f,
                    UnityEngine.Random.Range(-5, 5));

            runner.Spawn(
                characterPrefab,
                spawnPos,
                Quaternion.identity,
                player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    { }

    public void OnConnectFailed(NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    { }

    public void OnUserSimulationMessage(NetworkRunner runner,
        SimulationMessagePtr message)
    { }

    public void OnSessionListUpdated(NetworkRunner runner,
        List<SessionInfo> sessionList)
    { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner,
        Dictionary<string, object> data)
    { }

    public void OnHostMigration(NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    { }

    public void OnObjectEnterAOI(NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    { }

    public void OnReliableDataReceived(NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ArraySegment<byte> data)
    { }

    public void OnReliableDataProgress(NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    { }
}