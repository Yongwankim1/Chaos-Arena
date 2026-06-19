using Fusion;
using System.Collections;
using System.Linq;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance
    {
        get;
        private set;
    }

    [Header("Round Rule")]
    [SerializeField]
    private int killsToWinRound = 3;

    [SerializeField]
    private int roundsToWinMatch = 3;

    [SerializeField]
    private int maxRoundCount = 5;

    [Networked]
    public int CurrentRound { get; set; }

    [Header("Time")]
    [SerializeField]
    private float waitingTime = 5f;

    [SerializeField]
    private float characterSelectTime = 30f;

    [SerializeField]
    private float preparationTime = 20f;

    [SerializeField]
    private float roundTime = 300f;

    [SerializeField]
    private CharacterSelectUI characterSelectUI;

    [Networked]
    public RoundState CurrentState { get; set; }

    [Networked]
    public float StateRemainTime { get; set; }

    [Networked]
    public int BlueScore { get; set; }

    [Networked]
    public int RedScore { get; set; }

    [Networked]
    public int BlueRoundWin { get; set; }

    [Networked]
    public int RedRoundWin { get; set; }

    [SerializeField]
    private GameObject[] blueWalls = new GameObject[3];

    [SerializeField]
    private GameObject[] redWalls = new GameObject[3];

    [SerializeField]
    private float respawnTime = 10f;

    [SerializeField]
    private EnemySpawnManager enemySpawnManager;
    [Networked]
    public string RoundMessage { get; set; }
    [Networked]
    public RoundResultType RoundResult { get; set; }

    [Networked]
    private NetworkBool MatchEnded { get; set; }
    private bool _isRoundEnding;

    [SerializeField]
    private SoundLibrary soundLibrary;
    private bool _roundStartVoicePlayed;
    private Coroutine _nextRoundRoutine;

    private bool isFirstStart = true;
    private void Awake()
    {
        Instance = this;

        if(enemySpawnManager == null) enemySpawnManager = GameObject.Find("#SpawnManager").GetComponent<EnemySpawnManager>();
    }

    public override void Spawned()
    {
        MatchEnded = false;

        if (!HasStateAuthority)
            return;

        CurrentRound = 1;

        enemySpawnManager.Spawn(Runner);

        ChangeState(
            RoundState.Waiting,
            waitingTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (MatchEnded)
            return;

        StateRemainTime -= Runner.DeltaTime;

        if (CurrentState == RoundState.Preparation)
        {
            if (!_roundStartVoicePlayed &&
                StateRemainTime <= 11f)
            {
                _roundStartVoicePlayed = true;

                RPC_PlayRoundStartVoice();
            }
        }

        if (StateRemainTime > 0f)
            return;

        switch (CurrentState)
        {
            case RoundState.Waiting:
                StartCharacterSelect();
                break;

            case RoundState.CharacterSelect:
                StartPreparation();
                break;

            case RoundState.Preparation:
                StartRound();
                break;

            case RoundState.Playing:
                DrawRound();
                break;
        }
    }

    private void ChangeState(
        RoundState state,
        float duration)
    {
        CurrentState =
            state;

        StateRemainTime =
            duration;

        Debug.Log(
            $"State : {state}");
    }

    private void StartCharacterSelect()
    {
        GameBootstrap.Instance
            .AssignTeams();

        RPC_ShowCharacterSelect();

        ChangeState(
            RoundState.CharacterSelect,
            characterSelectTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowCharacterSelect()
    {
        characterSelectUI.gameObject.SetActive(true);
    }
    private void StartPreparation()
    {
        if (MatchEnded)
            return;
        GameBootstrap.Instance.ForceSelectRemainingPlayers();

        SetSpawnWalls(true);

        if (isFirstStart)
        {
            isFirstStart = false;

            RPC_PlayWelcome();
        }

        _roundStartVoicePlayed = false;

        ChangeState(
            RoundState.Preparation,
            preparationTime);
    }
    private void StartRound()
    {
        Debug.Log("StartRound");
        SetSpawnWalls(
            false);

        ChangeState(
            RoundState.Playing,
            roundTime);
    }

    private void SetSpawnWalls(
    bool active)
    {
        if (!HasStateAuthority)
            return;

        RPC_SetWalls(active);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetWalls(
    bool active)
    {
        if (blueWalls != null && blueWalls.Length > 0)
        {
            foreach (GameObject blueWall in blueWalls)
            {
                blueWall.SetActive(active);
            }
        }

        if (redWalls != null && redWalls.Length > 0)
        {
            foreach(GameObject redWall in redWalls)
            {
                redWall.SetActive(active);
            }
        }

        Debug.Log($"Wall Active : {active}");
    }

    private void DrawRound()
    {
        if (_isRoundEnding)
            return;

        _isRoundEnding = true;

        if (BlueScore >= killsToWinRound)
        {
            BlueRoundWin++;

            RoundResult = RoundResultType.BlueWin;

            CheckMatchEnd();

            return;
        }

        if (RedScore >= killsToWinRound)
        {
            RedRoundWin++;

            RoundResult = RoundResultType.RedWin;

            CheckMatchEnd();

            return;
        }

        if (BlueScore > RedScore)
        {
            BlueRoundWin++;

            RoundResult = RoundResultType.BlueWin;

            RPC_PlayRoundResultVoice(RoundResultType.BlueWin);

            Debug.Log("Time Over : Blue Win");
        }
        else if (RedScore > BlueScore)
        {
            RedRoundWin++;

            RoundResult = RoundResultType.RedWin;

            RPC_PlayRoundResultVoice(RoundResultType.RedWin);

            Debug.Log("Time Over : Red Win");
        }
        else
        {
            RoundResult = RoundResultType.Draw;

            RPC_PlayRoundResultVoice(RoundResultType.Draw);

            Debug.Log("Time Over : Draw");
        }

        CheckMatchEnd();
    }
    public void OnPlayerDeath(
     PlayerCharacter victim,
     IAttacker attacker)
    {
        if (!HasStateAuthority)
            return;

        PlayerCharacter killer =
            attacker.GetAttacker()
                .GetComponent<PlayerCharacter>();

        if (killer != null)
        {
            if (killer.Team ==
                TeamType.Blue)
            {
                BlueScore++;
            }
            else
            {
                RedScore++;
            }
        }
        CheckRoundEnd();

        if (RoundResult == RoundResultType.None)
        {
            StartCoroutine(
                RespawnRoutine(
                    victim));
        }
    }

    private void CheckRoundEnd()
    {
        if (BlueScore >= killsToWinRound)
        {
            BlueRoundWin++;

            RoundResult = RoundResultType.BlueWin;

            RPC_PlayRoundResultVoice(RoundResultType.BlueWin);

            CheckMatchEnd();

            return;
        }

        if (RedScore >= killsToWinRound)
        {
            RedRoundWin++;

            RoundResult = RoundResultType.RedWin;

            RPC_PlayRoundResultVoice(RoundResultType.RedWin);

            CheckMatchEnd();

            return;
        }
    }
    private void CheckMatchEnd()
    {
        if (BlueRoundWin >= roundsToWinMatch)
        {
            Debug.Log("Blue Match Win");

            EndMatch();

            return;
        }

        if (RedRoundWin >= roundsToWinMatch)
        {
            Debug.Log("Red Match Win");

            EndMatch();

            return;
        }

        if (CurrentRound >= maxRoundCount)
        {
            Debug.Log("Max Round Reached");

            EndMatch();

            return;
        }

        StartNextRound();
    }
    private void StartNextRound()
    {
        if (_nextRoundRoutine != null)
        {
            StopCoroutine(_nextRoundRoutine);
        }

        _nextRoundRoutine = StartCoroutine(NextRoundRoutine());
    }
    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (MatchEnded)
            yield break;

        CurrentRound++;
        _isRoundEnding = false;

        RoundResult =
            RoundResultType.None;

        BlueScore = 0;
        RedScore = 0;

        RespawnAllPlayers();

        SetSpawnWalls(true);

        enemySpawnManager.ReSpawn(Runner);

        ChangeState(
            RoundState.Preparation,
            preparationTime);
    }
    private void RespawnAllPlayers()
    {
        PlayerCharacter[] players =
            FindObjectsByType<PlayerCharacter>(
                FindObjectsSortMode.None);

        foreach (PlayerCharacter player in players)
        {
            Vector3 spawnPosition =
                SpawnManager.Instance
                    .GetSpawnPosition(
                        player.Team);

            player.Respawn(
                spawnPosition);
        }
    }

    /*
       
     */
    private void EndMatch()
    {
        if (MatchEnded)
            return;

        MatchEnded = true;

        if (_nextRoundRoutine != null)
        {
            StopCoroutine(_nextRoundRoutine);
            _nextRoundRoutine = null;
        }

        string result;

        TeamType winner;

        if (BlueRoundWin > RedRoundWin)
        {
            result = "ºí·çÆÀ ½Â¸®";

            winner = TeamType.Blue;
        }
        else if (RedRoundWin > BlueRoundWin)
        {
            result = "·¹µåÆÀ ½Â¸®";

            winner = TeamType.Red;
        }
        else
        {
            result = "¹«½ÂºÎ";

            winner = TeamType.None;
        }

        RPC_ShowMatchResult(result);

        StartCoroutine(ReturnLobbyRoutine());

        RPC_PlayMatchResultVoice(winner);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowMatchResult(string result)
    {
        MatchResultUI.Instance.Show(result, 5f);
    }

    private IEnumerator RespawnRoutine(PlayerCharacter player)
    {
        if (respawnTime >= 6f)
        {
            yield return new WaitForSeconds(respawnTime - 6f);

            RPC_PlayRespawnVoice(player.Object.InputAuthority);

            yield return new WaitForSeconds(6f);
        }
        else
        {
            yield return new WaitForSeconds(respawnTime);
        }

        Vector3 spawnPosition = SpawnManager.Instance.GetSpawnPosition(player.Team);

        player.Respawn(spawnPosition);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayWelcome()
    {
        SoundManager.Instance.PlayVoice(
            soundLibrary.Narration.Welcome);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayRoundStartVoice()
    {
        SoundManager.Instance.PlayVoice(
            soundLibrary.Narration.RoundStart);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayRoundResultVoice(
    RoundResultType result)
    {
        switch (result)
        {
            case RoundResultType.BlueWin:

                SoundManager.Instance.PlayVoice(
                    soundLibrary.Narration.BlueWin);

                break;

            case RoundResultType.RedWin:

                SoundManager.Instance.PlayVoice(
                    soundLibrary.Narration.RedWin);

                break;

            case RoundResultType.Draw:

                SoundManager.Instance.PlayVoice(
                    soundLibrary.Narration.Draw);

                break;
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayMatchResultVoice(
    TeamType winner)
    {
        PlayerCharacter localPlayer =
            GetLocalPlayer();

        if (localPlayer == null)
            return;

        if (winner == TeamType.None)
        {
            SoundManager.Instance.PlayVoice(
                soundLibrary.Narration.Draw);

            return;
        }

        bool isWinner =
            localPlayer.Team == winner;

        if (isWinner)
        {
            SoundManager.Instance.PlayVoice(
                soundLibrary.Narration.Victory);
        }
        else
        {
            SoundManager.Instance.PlayVoice(
                soundLibrary.Narration.Defeat);
        }
    }
    private PlayerCharacter GetLocalPlayer()
    {
        PlayerCharacter[] players = FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);

        foreach (PlayerCharacter player in players)
        {
            if (player.Object != null && player.Object.HasInputAuthority)
            {
                return player;
            }
        }

        return null;
    }
    private IEnumerator ReturnLobbyRoutine()
    {
        yield return new WaitForSeconds(5f);

        NetworkRunner runner =
            FindFirstObjectByType<NetworkRunner>();

        if (runner == null)
            yield break;

        runner.Shutdown();

        UnityEngine.SceneManagement
            .SceneManager
            .LoadScene(0);
    }

    public void OnPlayerDisconnected()
    {
        if (!HasStateAuthority)
            return;

        if (MatchEnded)
            return;

        if (Runner.ActivePlayers.Count() != 1)
            return;

        PlayerRef remainPlayer =
            Runner.ActivePlayers.First();

        TeamType winner = GameBootstrap.Instance.GetPlayerTeam(remainPlayer);

        if (winner == TeamType.None)
            return;

        BlueRoundWin = 0;
        RedRoundWin = 0;

        if (winner == TeamType.Blue)
        {
            BlueRoundWin = roundsToWinMatch;
        }
        else
        {
            RedRoundWin = roundsToWinMatch;
        }

        EndMatch();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayRespawnVoice(PlayerRef targetPlayer)
    {
        PlayerCharacter localPlayer = GetLocalPlayer();

        if (localPlayer == null)
            return;

        if (localPlayer.Object.InputAuthority != targetPlayer)
            return;

        SoundManager.Instance.PlayVoice(
            soundLibrary.Narration.FiveSec);
    }
}
