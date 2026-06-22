using Fusion;
using UnityEngine;

public class MageRSkill : NetworkBehaviour, ISkillR, ISkillCooldown

{
    [SerializeField]
    private NetworkObject Prefab;

    [SerializeField]
    private Transform spawnPoint;

    private Animator _animator;

    private static readonly int SkillRHash = Animator.StringToHash("SkillR");
    private static readonly int CancelHash = Animator.StringToHash("Cancel");

    private PlayerCharacter _player;

    private CharacterCombat _combat;

    private NetworkObject spawnedLaser;

    [SerializeField]
    private float manaCost = 20f;

    [SerializeField]
    private float cooldown = 5f;

    [Networked]
    public TickTimer Cooldown { get; set; }
    private CharacterActionLock _actionLock;
    private NetworkThirdPersonController _controller;
    public float CooldownDuration => cooldown;
    public float CooldownNormalized
    {
        get
        {
            if (Cooldown.ExpiredOrNotRunning(Runner))
                return 0f;

            float remain = Cooldown.RemainingTime(Runner) ?? 0f;

            return remain / cooldown;
        }
    }

    public TickTimer CooldownTimer => Cooldown;

    private bool isUsingR;
    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _combat = GetComponent<CharacterCombat>();
        _animator = GetComponent<Animator>();
        _actionLock = GetComponent<CharacterActionLock>();
        _controller = GetComponent<NetworkThirdPersonController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (spawnedLaser == null || spawnPoint == null)
            return;

        spawnedLaser.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }

    public void UseR()
    {
        if (!HasStateAuthority)
            return;
        if (isUsingR)
        {
            CancelR();
            return;
        }
        if (_player.IsDead)
            return;

        if (!_controller.Grounded) return;
        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        Cooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlaySkillR();
        _controller.SetRotationOnly(true);
        _actionLock?.Lock(ActionLockType.Attack);
        _actionLock?.Lock(ActionLockType.Dash);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillR()
    {
        _animator.SetTrigger(SkillRHash);
    }

    private void CancelR()
    {
        EndLaserEffect();
        CanMove();

        isUsingR = false;

        // 필요하면 애니메이터도 R 상태에서 빠져나오게 처리
        _animator.SetTrigger(CancelHash);
    }
    public void SpawnLaserEffect()
    {
        if (!HasStateAuthority)
            return;

        if (spawnedLaser != null)
        {
            Runner.Despawn(spawnedLaser);
            spawnedLaser = null;
        }

        spawnedLaser = Runner.Spawn(Prefab, spawnPoint.position,
            spawnPoint.rotation, Object.InputAuthority,
            (runner, obj) => {
                obj.GetComponent<MageLaserAttack>().Init(GetComponent<IAttacker>());
            });
    }

    public void EndLaserEffect()
    {
        if (!HasStateAuthority)
            return;

        if (spawnedLaser == null)
            return;

        spawnedLaser.GetComponent<MageLaserAttack>().Destroy();
        spawnedLaser = null;
    }

    public void CanMove()
    {
        if (!HasStateAuthority)
            return;

        _controller.SetRotationOnly(false);
        _actionLock?.Unlock(ActionLockType.Move);
        _actionLock?.Unlock(ActionLockType.Attack);
        _actionLock?.Unlock(ActionLockType.Dash);
    }
}
