using Fusion;
using UnityEngine;

public class BruteQSkill : NetworkBehaviour, ISkillQ, ISkillCooldown
{
    [SerializeField] private AttackData attackData;
    [SerializeField]
    private Transform attackSpawnPoint;


    [SerializeField] private float manaCost = 20f;

    [SerializeField] private float cooldown = 5f;

    [SerializeField]
    private ParticleSystem punchEffect;

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

        _combat.SpawnSkillHitBox(attackData);

        RPC_PlayPunchEffect();
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

    private void OnDrawGizmos()
    {
        if (attackSpawnPoint == null)
            return;

        if (attackData == null)
            return;

        Vector3 center =
            attackSpawnPoint.position +
            attackSpawnPoint.forward *
            (attackData.Range * 0.5f);

        Vector3 size =
            new Vector3(
                attackData.Radius * 2f,
                2f,
                attackData.Range);

        Gizmos.color = Color.cyan;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix =
            Matrix4x4.TRS(
                center,
                attackSpawnPoint.rotation,
                Vector3.one);

        Gizmos.DrawWireCube(
            Vector3.zero,
            size);

        Gizmos.matrix = oldMatrix;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayPunchEffect()
    {
        if (punchEffect == null)
            return;

        punchEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear);

        punchEffect.Play();
    }
}