using Fusion;
using UnityEngine;

public class BruteESkill : NetworkBehaviour, ISkillE, ISkillCooldown
{
    [Header("Mana")]
    [SerializeField]
    private float manaCost = 30f;

    [SerializeField]
    private float cooldown = 12f;

    [Networked]
    public TickTimer Cooldown { get; set; }

    public TickTimer CooldownTimer => Cooldown;

    public float CooldownDuration => cooldown;

    [Header("Buff")]
    [SerializeField]
    private float radius = 8f;

    [SerializeField]
    private float buffDuration = 20f;

    [SerializeField]
    private float damageReduction = 30f;

    [SerializeField]
    private LayerMask playerMask;

    [Header("Effect")]
    [SerializeField]
    private ParticleSystem castEffect;

    private Animator _animator;

    private PlayerCharacter _player;

    private CharacterActionLock _actionLock;

    private NetworkThirdPersonController _controller;

    private static readonly int SkillEHash =
        Animator.StringToHash("SkillE");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _player = GetComponent<PlayerCharacter>();

        _actionLock = GetComponent<CharacterActionLock>();

        _controller = GetComponent<NetworkThirdPersonController>();
    }

    public void UseE()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (!_controller.Grounded)
            return;

        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        Cooldown =
            TickTimer.CreateFromSeconds(
                Runner,
                cooldown);

        _actionLock.Lock(ActionLockType.Move);
        _actionLock.Lock(ActionLockType.Attack);
        _actionLock.Lock(ActionLockType.Dash);
        _actionLock.Lock(ActionLockType.Jump);
        _actionLock.Lock(ActionLockType.Skill);

        RPC_PlaySkillE();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillE()
    {
        _animator.SetTrigger(SkillEHash);
    }

    // Animation Event
    public void ApplyBuff()
    {
        if (!HasStateAuthority)
            return;

        RPC_PlayCastEffect();

        float radiusSqr = radius * radius;

        PlayerCharacter[] players =
            FindObjectsByType<PlayerCharacter>(
                FindObjectsSortMode.None);

        foreach (PlayerCharacter target in players)
        {
            if (target == null)
                continue;

            if (target.Team != _player.Team)
                continue;

            float distanceSqr =
                (target.transform.position -
                 transform.position).sqrMagnitude;

            if (distanceSqr > radiusSqr)
                continue;

            BruteDefenseBuff buff = target.GetComponent<BruteDefenseBuff>();

            if (buff == null)
                continue;

            buff.RequestApply(
                damageReduction,
                buffDuration);
        }
    }

    // Animation Event
    public void EndSkilla()
    {
        if (!HasStateAuthority)
            return;

        _actionLock.Unlock(ActionLockType.Move);
        _actionLock.Unlock(ActionLockType.Attack);
        _actionLock.Unlock(ActionLockType.Dash);
        _actionLock.Unlock(ActionLockType.Jump);
        _actionLock.Unlock(ActionLockType.Skill);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            radius);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayCastEffect()
    {
        castEffect?.Play();
    }
}
