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
    private NetworkObject brutePrefab;
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
    private readonly Dictionary<int, TeamType> _playerTeamsById = new Dictionary<int, TeamType>();
    private readonly Dictionary<PlayerRef, int> _spawnSlotIndex = new Dictionary<PlayerRef, int>();
    private readonly Dictionary<int, int> _spawnSlotIndexById = new Dictionary<int, int>();
    private readonly List<PlayerRef> _bluePlayers = new();
    private readonly List<PlayerRef> _redPlayers = new();

    public IReadOnlyDictionary<PlayerRef, PlayerLobbyObject> PlayerLobbies
    {
        get
        {
            return _playerLobbies;
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
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

        ClassData brute =
    await ClassDataLoader.LoadClassData(
        CharacterClassType.Brute);

        ClassDataManager.AddData(
            CharacterClassType.Brute,
            brute);

        Debug.Log("ClassData Load Complete");
    }
    // 씬 로딩 완료 후 기존 플레이어 생성
    public void OnSceneLoadDone(NetworkRunner runner)
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
        _bluePlayers.Clear();

        _redPlayers.Clear();

        _playerTeams.Clear();

        _playerTeamsById.Clear();

        _spawnSlotIndex.Clear();

        _spawnSlotIndexById.Clear();

        List<PlayerRef> randomPlayers =
            new List<PlayerRef>();

        int maxTeamCount = ((int)RoomSessionData.MatchType) / 2;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (!RoomSessionData.TeamSelections.TryGetValue(
                    player.PlayerId,
                    out TeamSelectType teamSelect))
            {
                teamSelect = TeamSelectType.Random;
            }

            switch (teamSelect)
            {
                case TeamSelectType.Blue:

                    if (_bluePlayers.Count < maxTeamCount)
                    {
                        _bluePlayers.Add(player);
                    }
                    else
                    {
                        randomPlayers.Add(player);
                    }

                    break;

                case TeamSelectType.Red:

                    if (_redPlayers.Count < maxTeamCount)
                    {
                        _redPlayers.Add(player);
                    }
                    else
                    {
                        randomPlayers.Add(player);
                    }

                    break;

                default:

                    randomPlayers.Add(player);

                    break;
            }
        }

        while (randomPlayers.Count > 0)
        {
            PlayerRef player =
                randomPlayers[0];

            randomPlayers.RemoveAt(0);

            if (_bluePlayers.Count < maxTeamCount)
            {
                _bluePlayers.Add(player);

                continue;
            }

            if (_redPlayers.Count < maxTeamCount)
            {
                _redPlayers.Add(player);

                continue;
            }
        }

        for (int i = 0; i < _bluePlayers.Count; i++)
        {
            PlayerRef player =
                _bluePlayers[i];

            _playerTeams[player] =
                TeamType.Blue;

            _playerTeamsById[player.PlayerId] =
                TeamType.Blue;

            _spawnSlotIndex[player] =
                i;

            _spawnSlotIndexById[player.PlayerId] =
                i;

            RPC_SetPlayerTeam(player.PlayerId, TeamType.Blue, i);

            Debug.Log(
                $"Blue : {player.PlayerId} Slot : {i}");
        }

        for (int i = 0; i < _redPlayers.Count; i++)
        {
            PlayerRef player =
                _redPlayers[i];

            _playerTeams[player] =
                TeamType.Red;

            _playerTeamsById[player.PlayerId] =
                TeamType.Red;

            _spawnSlotIndex[player] =
                i;

            _spawnSlotIndexById[player.PlayerId] =
                i;

            RPC_SetPlayerTeam(player.PlayerId, TeamType.Red, i);

            Debug.Log(
                $"Red : {player.PlayerId} Slot : {i}");
        }

        Debug.Log(
            $"Blue Count : {_bluePlayers.Count}");

        Debug.Log(
            $"Red Count : {_redPlayers.Count}");

        Debug.Log(
            $"Team Assigned Count : {_playerTeams.Count}");
    }
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player, CharacterClassType classType)
    {
        if (_spawnedPlayers.ContainsKey(player))
            return;

        if (!_playerTeams.TryGetValue(player, out TeamType team))
        {
            Debug.LogError($"Team Not Assigned : {player.PlayerId}");

            return;
        }

        int slot = GetSpawnSlot(player);

        Vector3 spawnPosition = SpawnManager.Instance.GetSpawnPosition(team,slot);

        NetworkObject prefab =GetCharacterPrefab(classType);

        NetworkObject playerObject = runner.Spawn(prefab,spawnPosition,Quaternion.identity, player);

        PlayerCharacter playerCharacter = playerObject.GetComponent<PlayerCharacter>();

        if (playerCharacter != null)
        {
            playerCharacter.ClassType = classType;

            playerCharacter.Team = team;
        }

        _spawnedPlayers.Add(player, playerObject);

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

        SpawnPlayer(Runner,player,lobby.SelectedClass);
    }

    private NetworkObject GetCharacterPrefab(CharacterClassType classType)
    {
        switch (classType)
        {
            case CharacterClassType.Assassin:
                return assassinPrefab;

            case CharacterClassType.Mage:
                return magePrefab;

            case CharacterClassType.Brute:
                return brutePrefab;

            default:
                return assassinPrefab;
        }
    }

    public void OnPlayerJoined(NetworkRunner runner,PlayerRef player)
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
    public void OnPlayerLeft(NetworkRunner runner,PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        TeamType loserTeam = GetPlayerTeam(player);

        if (_playerLobbies.TryGetValue(player,out PlayerLobbyObject lobby))
        {
            runner.Despawn(lobby.Object);

            _playerLobbies.Remove(player);
        }

        if (_spawnedPlayers.TryGetValue(player,out var playerObject))
        {
            runner.Despawn(playerObject);

            _spawnedPlayers.Remove(player);
        }

        _playerTeams.Remove(player);
        _playerTeamsById.Remove(player.PlayerId);
        _spawnSlotIndex.Remove(player);
        _spawnSlotIndexById.Remove(player.PlayerId);

        RoundManager.Instance?.OnPlayerDisconnected(loserTeam);
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

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (runner.IsServer)
        {
            return;
        }

        if (shutdownReason == ShutdownReason.Ok)
        {
            return;
        }

        if (RoundManager.Instance == null)
        {
            return;
        }

       RoundManager.Instance.ShowHostDisconnectResult();
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

        SpawnManager.Instance.ResetSpawnIndex();

        foreach (TeamType team in new[] { TeamType.Blue, TeamType.Red })
        {
            List<CharacterClassType> usedClasses =
                new List<CharacterClassType>();

            foreach (var pair in _playerLobbies)
            {
                PlayerRef player = pair.Key;

                PlayerLobbyObject lobby = pair.Value;

                if (GetPlayerTeam(player) != team)
                    continue;

                if (lobby.SelectedClass == CharacterClassType.None)
                    continue;

                usedClasses.Add(lobby.SelectedClass);
            }

            foreach (var pair in _playerLobbies)
            {
                PlayerRef player = pair.Key;

                PlayerLobbyObject lobby = pair.Value;

                if (GetPlayerTeam(player) != team)
                    continue;

                if (lobby.SelectedClass != CharacterClassType.None)
                    continue;

                CharacterClassType autoClass =
                    GetAutoCharacter(usedClasses);

                lobby.SelectedClass = autoClass;

                usedClasses.Add(autoClass);
            }
        }

        foreach (var pair in _playerLobbies)
        {
            SpawnSelectedCharacter(pair.Key);
        }

        RPC_CloseCharacterSelectUI();
    }
    private CharacterClassType GetAutoCharacter(List<CharacterClassType> usedClasses)
    {
        List<CharacterClassType> allClasses =
            new()
            {
            CharacterClassType.Assassin,
            CharacterClassType.Mage,
            CharacterClassType.Brute
            };

        List<CharacterClassType> available =
            new();

        foreach (CharacterClassType classType in allClasses)
        {
            if (!usedClasses.Contains(classType))
            {
                available.Add(classType);
            }
        }

        if (available.Count > 0)
        {
            int index =
                Random.Range(0, available.Count);

            return available[index];
        }

        int randomIndex =
            Random.Range(0, allClasses.Count);

        return allClasses[randomIndex];
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CloseCharacterSelectUI()
    {
        if (CharacterSelectUI.Instance == null)
            return;

        CharacterSelectUI.Instance.OnPlayerUI();

        CharacterSelectUI.Instance.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
    }
    public TeamType GetPlayerTeam(PlayerRef player)
    {
        if (_playerTeams.TryGetValue(player, out TeamType team))
        {
            return team;
        }

        if (_playerTeamsById.TryGetValue(player.PlayerId, out TeamType teamById))
        {
            return teamById;
        }

        return TeamType.None;
    }

    public int GetSpawnSlot(PlayerRef player)
    {
        if (_spawnSlotIndex.TryGetValue(player, out int slot))
        {
            return slot;
        }

        if (_spawnSlotIndexById.TryGetValue(player.PlayerId, out int slotById))
        {
            return slotById;
        }

        return 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetPlayerTeam(int playerId, TeamType team, int slotIndex)
    {
        _playerTeamsById[playerId] = team;
        _spawnSlotIndexById[playerId] = slotIndex;

        CharacterSelectUI.Instance?.RefreshCharacterLock();
    }

    public bool IsCharacterUsedInTeam(CharacterClassType classType,TeamType team)
    {
        foreach (PlayerLobbyObject lobby in FindObjectsByType<PlayerLobbyObject>(FindObjectsSortMode.None))
        {
            if (lobby.SelectedClass != classType)
                continue;

            if (GetPlayerTeam(lobby.Object.InputAuthority) != team)
                continue;

            Debug.Log($"Check : {lobby.Object.InputAuthority.PlayerId} " +$"Class:{lobby.SelectedClass} " +$"Team:{GetPlayerTeam(lobby.Object.InputAuthority)}");
            return true;
        }

        return false;
    }
    public bool IsCharacterUsedInTeam(CharacterClassType classType,TeamType team,PlayerRef exceptPlayer)
    {
        foreach (PlayerLobbyObject lobby in FindObjectsByType<PlayerLobbyObject>(FindObjectsSortMode.None))
        {
            PlayerRef player = lobby.Object.InputAuthority;

            if (player == exceptPlayer)
            {
                continue;
            }

            if (lobby.SelectedClass != classType)
            {
                continue;
            }

            if (GetPlayerTeam(player) != team)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}