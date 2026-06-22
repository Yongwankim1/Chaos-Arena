using Fusion;
using UnityEngine;

public class AssassinSkill : NetworkBehaviour , ISkillQ, ISkillCooldown
{
    [SerializeField]
    private NetworkObject normalShurikenPrefab;

    private AssassinUltimate _ultimate;

    [SerializeField]
    private Transform shurikenSpawnPoint;

    private Animator _animator;

    private static readonly int SkillQHash = Animator.StringToHash("SkillQ");

    private PlayerCharacter _player;

    private CharacterCombat _combat;

    [SerializeField]
    private float manaCost = 20f;

    [SerializeField]
    private float cooldown = 5f;

    [SerializeField]
    private float actionLockTimeout = 1.2f;

    public TickTimer CooldownTimer => Cooldown;

    public float CooldownDuration => cooldown;

    [Networked]
    public TickTimer Cooldown { get; set; }

    [Networked]
    private TickTimer ActionLockTimer { get; set; }

    [Networked]
    private NetworkBool IsActionLocked { get; set; }

    private AssassinStealth _stealth;
    private CharacterActionLock _actionLock;
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

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _combat = GetComponent<CharacterCombat>();
        _animator = GetComponent<Animator>();
        _stealth = GetComponent<AssassinStealth>();
        _ultimate = GetComponent<AssassinUltimate>();
        _actionLock = GetComponent<CharacterActionLock>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!IsActionLocked)
            return;

        if (!ActionLockTimer.Expired(Runner))
            return;

        ReleaseActionLock();
    }

    public void UseQ()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (_player.IsDashing)
            return;

        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        _combat?.CancelCombo(); // 캔슬 필요한거

        _stealth?.ExitStealth();

        _actionLock?.Lock(ActionLockType.Attack);
        _actionLock?.Lock(ActionLockType.Dash);
        IsActionLocked = true;
        ActionLockTimer = TickTimer.CreateFromSeconds(Runner, actionLockTimeout);

        Cooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlaySkillQ();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillQ()
    {
        _animator.SetTrigger(SkillQHash);
    }

    public void SpawnShuriken()
    {
        if (!HasStateAuthority)
            return;

        NetworkObject prefab = normalShurikenPrefab;

        if (_ultimate != null &&
            _ultimate.IsUltimate &&
            _ultimate.ShadowShurikenPrefab != null)
        {
            prefab = _ultimate.ShadowShurikenPrefab;
        }

        if (prefab != null && shurikenSpawnPoint != null)
        {
            Runner.Spawn(
                prefab,
                shurikenSpawnPoint.position,
                Quaternion.LookRotation(transform.forward),
                Object.InputAuthority,
                (runner, obj) =>
                {
                    obj.GetComponent<NetworkShuriken>()
                        .Initialize(
                            GetComponent<IAttacker>(),
                            transform.forward);
                });
        }

        ReleaseActionLock();
    }

    private void ReleaseActionLock()
    {
        if (!HasStateAuthority)
            return;

        if (!IsActionLocked)
            return;

        _actionLock?.Unlock(ActionLockType.Attack);
        _actionLock?.Unlock(ActionLockType.Dash);
        IsActionLocked = false;
        ActionLockTimer = TickTimer.None;
    }
}