using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class BruteQSkill : NetworkBehaviour, ISkillQ, ISkillCooldown
{
    [SerializeField] private AttackData attackData;

    [SerializeField] private float manaCost = 20f;

    [SerializeField] private float cooldown = 5f;

    [Networked]
    public TickTimer Cooldown { get; set; }

    public TickTimer CooldownTimer => Cooldown;

    public float CooldownDuration => cooldown;

    private Animator _animator;

    private PlayerCharacter _player;

    private CharacterActionLock _actionLock;

    private CharacterCombat _combat;

    private NetworkThirdPersonController _controller;

    private static readonly int SkillQHash =
        Animator.StringToHash("SkillQ");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _player = GetComponent<PlayerCharacter>();

        _actionLock = GetComponent<CharacterActionLock>();

        _combat = GetComponent<CharacterCombat>();

        _controller = GetComponent<NetworkThirdPersonController>();
    }

    public void UseQ()
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

        RPC_PlaySkillQ();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillQ()
    {
        _animator.SetTrigger(SkillQHash);
    }

    // Animation Event
    public void SpawnPunchHit()
    {
        if (!HasStateAuthority)
            return;

        //_combat.SpawnSkillHitBox(attackData);
    }

    // Animation Event
    public void EndSkill()
    {
        if (!HasStateAuthority)
            return;

        _actionLock.Unlock(ActionLockType.Move);
        _actionLock.Unlock(ActionLockType.Attack);
        _actionLock.Unlock(ActionLockType.Dash);
    }
}