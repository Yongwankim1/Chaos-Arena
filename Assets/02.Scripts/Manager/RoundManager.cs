using Fusion;
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

    [SerializeField]
    private GameObject blueWall;

    [SerializeField]
    private GameObject redWall;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

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
        Debug.Log(
            "Round Draw");

        ChangeState(
            RoundState.RoundEnd,
            0f);
    }
}