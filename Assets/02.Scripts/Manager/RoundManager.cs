using Fusion;
using System.Collections;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance
    {
        get;
        private set;
    }

    [Header("Time")]
    [SerializeField]
    private float waitingTime = 5f;

    [SerializeField]
    private float characterSelectTime = 30f;

    [SerializeField]
    private float preparationTime = 10f;

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
    private GameObject blueWall;

    [SerializeField]
    private GameObject redWall;

    [SerializeField]
    private float respawnTime = 10f;

    [SerializeField]
    private EnemySpawnManager enemySpawnManager;
    [Networked]
    public string RoundMessage { get; set; }
    [Networked]
    public RoundResultType RoundResult { get; set; }

    private void Awake()
    {
        Instance = this;

        if(enemySpawnManager == null) enemySpawnManager = GameObject.Find("#SpawnManager").GetComponent<EnemySpawnManager>();
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        enemySpawnManager.Spawn(Runner);

        ChangeState(
            RoundState.Waiting,
            waitingTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        StateRemainTime -= Runner.DeltaTime;

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
        GameBootstrap.Instance
            .ForceSelectRemainingPlayers();

        SetSpawnWalls(true);

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
        if (blueWall != null)
        {
            blueWall.SetActive(active);
        }

        if (redWall != null)
        {
            redWall.SetActive(active);
        }

        Debug.Log(
            $"Wall Active : {active}");
    }

    private void DrawRound()
    {
        RoundResult =
            RoundResultType.Draw;

        Debug.Log(
            "Round Draw");

        StartNextRound();
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
        if (BlueScore >= 3)
        {
            BlueRoundWin++;
            RoundResult = RoundResultType.BlueWin;
            Debug.Log(
                $"Blue Round Win : {BlueRoundWin}");

            CheckMatchEnd();

            return;
        }

        if (RedScore >= 3)
        {
            RedRoundWin++;
            RoundResult = RoundResultType.RedWin;
            Debug.Log(
                $"Red Round Win : {RedRoundWin}");

            CheckMatchEnd();

            return;
        }
    }
    private void CheckMatchEnd()
    {
        if (BlueRoundWin >= 3)
        {
            Debug.Log(
                "Blue Match Win");

            EndMatch();

            return;
        }

        if (RedRoundWin >= 3)
        {
            Debug.Log(
                "Red Match Win");

            EndMatch();

            return;
        }

        StartNextRound();
    }
    private void StartNextRound()
    {
        StartCoroutine(NextRoundRoutine());
    }
    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(2f);

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
    private void EndMatch()
    {
        Debug.Log(
            "Match End");

        BlueScore = 0;
        RedScore = 0;

        BlueRoundWin = 0;
        RedRoundWin = 0;

        RespawnAllPlayers();

        SetSpawnWalls(true);

        ChangeState(
            RoundState.Waiting,
            waitingTime);
    }
    private IEnumerator RespawnRoutine(PlayerCharacter player)
    {
        yield return new WaitForSeconds(respawnTime);

        Vector3 spawnPosition =
            SpawnManager.Instance
                .GetSpawnPosition(
                    player.Team);

        player.Respawn(spawnPosition);
    }
}
