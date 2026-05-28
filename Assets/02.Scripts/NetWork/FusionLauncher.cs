using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef warriorPrefab;

    private NetworkRunner _runner;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        //_runner = gameObject.AddComponent<NetworkRunner>();
        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var scene =
            SceneRef.FromIndex(
                SceneManager
                .GetActiveScene()
                .buildIndex
            );

        await _runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = "Room",
                Scene = scene,
                SceneManager =
                    GetComponent
                    <NetworkSceneManagerDefault>()
            });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPos =
                new Vector3(
                    UnityEngine.Random.Range(-5, 5),
                    0.5f,
                    UnityEngine.Random.Range(-5, 5)
                );

            NetworkPrefabRef selectedPrefab =
                warriorPrefab;

            runner.Spawn(
                selectedPrefab,
                spawnPos,
                Quaternion.identity,
                player
            );
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnInput(
    NetworkRunner runner,
    NetworkInput input)
    {
        NetworkInputData data =
            new NetworkInputData();

        if (Keyboard.current == null)
            return;

        Vector2 move =
            Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            move.y += 1;

        if (Keyboard.current.sKey.isPressed)
            move.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            move.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            move.x += 1;

        data.Move = move;

        Vector2 look =
            Vector2.zero;

        if (Mouse.current != null)
        {
            look.x =
                Mouse.current.delta.x.ReadValue();

            look.y =
                Mouse.current.delta.y.ReadValue();
        }

        data.Look = look;

        NetworkButtons buttons =
            new NetworkButtons();

        buttons.Set(
            (int)EInputButtons.Jump,
            Keyboard.current.spaceKey.isPressed
        );

        buttons.Set(
            (int)EInputButtons.Sprint,
            Keyboard.current.leftShiftKey.isPressed
        );

        data.Buttons = buttons;

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }
}