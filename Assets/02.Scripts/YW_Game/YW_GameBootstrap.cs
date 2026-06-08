using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class YW_GameBootstrap : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Header("Settings")]
    [SerializeField] private NetworkObject playerPrefab;

    // 플레이어 리스트 관리 (참조용)
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private bool _assignedImposter;

    public override void Spawned()
    {
        // 씬 오브젝트로 배치된 경우, Runner에 콜백 등록
        Runner.AddCallbacks(this);
        SpawnExistingPlayers(Runner);
    }

    // [중요] 씬 로딩이 완료된 후 호출됨
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene Load Done - Spawning Players...");
        SpawnExistingPlayers(runner);
    }

    private void SpawnExistingPlayers(NetworkRunner runner)
    {
        if (!runner.IsServer) return; // Host만 스폰 권한 가짐

        List<YW_PlayerController> spawnedList = new List<YW_PlayerController>();
        // 이미 접속해 있는 모든 플레이어에 대해 캐릭터 생성
        foreach (var player in runner.ActivePlayers)
        {
            if (_spawnedCharacters.ContainsKey(player)) continue;

            spawnedList.Add(SpawnPlayer(runner, player));
        }

        if (!_assignedImposter && spawnedList.Count > 0)
        {
            int imposterIndex = Random.Range(0, spawnedList.Count);
            spawnedList[imposterIndex].IsImposter = true;
            Debug.Log($"서버 : Player {spawnedList[imposterIndex].Object.InputAuthority}를 임포스터로 지정했습니다.");
            _assignedImposter = true;
        }
    }
    private readonly Color[] rainbowColors = new Color[]
    {
        Color.red, new Color(1f,0.5f,0f), Color.yellow,
        Color.green, Color.blue, new Color(0.29f,0f, 0.51f), new Color(0.56f,0,1f)
    };

    private  YW_PlayerController SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // 1. 랜덤 위치 결정 (예시)
        Vector3 spawnPos = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));

        // 2. NetworkRunner.Spawn 호출 (가장 핵심!)
        // inputAuthority: 해당 PlayerRef가 이 객체의 입력을 제어함
        NetworkObject networkPlayerObject = runner.Spawn(
            playerPrefab, spawnPos, Quaternion.identity, player,
            (runner, obj) =>
            {
                YW_PlayerController pc = obj.GetComponent<YW_PlayerController>();
                if (pc != null)
                {
                    pc.PlayerColor = rainbowColors[Random.Range(0,rainbowColors.Length)];
                }
            } 
        );

        _spawnedCharacters.Add(player, networkPlayerObject);
        return networkPlayerObject.GetComponent<YW_PlayerController>();
    }

    // 게임 도중 새로 들어온 플레이어 처리
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && !_spawnedCharacters.ContainsKey(player))
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && _spawnedCharacters.TryGetValue(player, out var networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    #region Unused Callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new YW_NetworkInputData();

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) data.movementInput.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) data.movementInput.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) data.movementInput.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) data.movementInput.y += 1f;

            data.isRuning = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            data.KillInput = keyboard.fKey.isPressed;
        }

        input.Set(data); // 서버로 전송
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        //hrow new System.NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        //throw new System.NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //throw new System.NotImplementedException();
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

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
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

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
    // ... 나머지 인터페이스 메서드들 (공백으로 유지) ...
    #endregion
}