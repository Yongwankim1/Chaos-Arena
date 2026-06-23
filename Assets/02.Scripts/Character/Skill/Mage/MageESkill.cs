using Fusion;
using UnityEngine;

public class MageESkill : NetworkBehaviour, ISkillE, ISkillCooldown
{
    [SerializeField]
    private NetworkObject areaEffectPrefab;

    [SerializeField]
    private Transform spawnPoint;

    private Animator _animator;

    private static readonly int SkillEHash = Animator.StringToHash("SkillE");

    private PlayerCharacter _player;

    private CharacterCombat _combat;

    [SerializeField]
    private float manaCost = 20f;

    [SerializeField]
    private float cooldown = 5f;

    [SerializeField]
    private float damagePercent = 0.2f;

    [Networked]
    public TickTimer Cooldown { get; set; }

    public TickTimer CooldownTimer => Cooldown;

    public float CooldownDuration => cooldown;
    private CharacterActionLock _actionLock;
    private NetworkThirdPersonController _controller;
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
        _actionLock = GetComponent<CharacterActionLock>();
        _controller = GetComponent<NetworkThirdPersonController>();
    }
    public void UseE()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (!_controller.Grounded) return;
        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        Cooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlaySkillE();
        _actionLock?.Lock(ActionLockType.Move);
        _actionLock?.Lock(ActionLockType.Attack);
        _actionLock?.Lock(ActionLockType.Dash);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillE()
    {
        _animator.SetTrigger(SkillEHash);
    }


    public void SpawnAreaEffect()
    {
        if (!HasStateAuthority)
            return;
        float finalDamage = _player.AttackPower * damagePercent;
        Runner.Spawn(areaEffectPrefab, spawnPoint.position, 
            Quaternion.LookRotation(transform.forward), Object.InputAuthority, 
            (runner, obj) => { obj.GetComponent<AreaAttack>().Init(GetComponent<IAttacker>(),(int)finalDamage); 
            });
    }
}
