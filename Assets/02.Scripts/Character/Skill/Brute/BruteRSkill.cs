using Fusion;
using UnityEngine;

public class BruteRSkill : NetworkBehaviour, ISkillR, ISkillCooldown
{
    [Header("Mana")]
    [SerializeField]
    private float manaCost = 50f;

    [SerializeField]
    private float cooldown = 20f;

    [Networked]
    public TickTimer Cooldown { get; set; }

    public TickTimer CooldownTimer => Cooldown;

    public float CooldownDuration => cooldown;

    [Header("Jump")]
    [SerializeField]
    private float jumpForce = 15f;

    private float _originalGravity;

    [Header("Damage")]
    [SerializeField]
    private float radius = 5f;

    [SerializeField]
    private float maxDamagePercent = 150f;

    [SerializeField]
    private float minDamagePercent = 50f;

    [SerializeField]
    private float knockbackForce = 10f;

    [SerializeField]
    private LayerMask playerMask;

    [Header("Effect")]
    [SerializeField]
    private ParticleSystem jumpEffect;

    [SerializeField]
    private ParticleSystem landEffect;

    private PlayerCharacter _player;
    private CharacterCombat _combat;
    private CharacterActionLock _actionLock;
    private NetworkThirdPersonController _controller;
    private Animator _animator;
    private NetworkCharacterController _cc;

    private static readonly int SkillRHash =
        Animator.StringToHash("SkillR");

    private static readonly int IsUsingUltimateHash =
        Animator.StringToHash("IsUsingUltimate");

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _combat = GetComponent<CharacterCombat>();
        _actionLock = GetComponent<CharacterActionLock>();
        _controller = GetComponent<NetworkThirdPersonController>();
        _animator = GetComponent<Animator>();

        _cc = GetComponent<NetworkCharacterController>();


    }

    public void UseR()
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
        _originalGravity = _cc.gravity;
        _cc.gravity = -35f;

        Cooldown =
            TickTimer.CreateFromSeconds(
                Runner,
                cooldown);

        _player.IsUsingUltimate = true;

        _controller.IgnoreLandAnimation = true;
        _animator.SetBool(
            IsUsingUltimateHash,
            true);

        _actionLock.Lock(ActionLockType.Attack);
        _actionLock.Lock(ActionLockType.Dash);
        _actionLock.Lock(ActionLockType.Jump);
        _actionLock.Lock(ActionLockType.Skill);

        RPC_PlayUltimate();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayUltimate()
    {
        _animator.SetTrigger(SkillRHash);
    }

    // 애니메이션 이벤트 1
    public void StartJump()
    {
        if (!HasStateAuthority)
            return;

        _cc.Jump(
            true,
            jumpForce);

        RPC_PlayJumpEffect();
    }
    // 애니메이션 이벤트 2
    public void LandImpact()
    {
        if (!HasStateAuthority)
            return;

        ApplyLandingDamage();

        RPC_PlayLandEffect();

        EndSkill();
    }

    private void ApplyLandingDamage()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                radius,
                playerMask);

        foreach (Collider hit in hits)
        {
            PlayerCharacter target =
                hit.GetComponentInParent<PlayerCharacter>();

            if (target == null)
                continue;

            if (target.Team == _player.Team)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    target.transform.position);

            float t =
                Mathf.Clamp01(distance / radius);

            float damagePercent =
                Mathf.Lerp(
                    maxDamagePercent,
                    minDamagePercent,
                    t);

            int damage =
                Mathf.RoundToInt(
                    _player.AttackPower *
                    damagePercent *
                    0.01f);

            target.TakeDamage(
                damage,
                GetComponent<IAttacker>());

            if (distance >= radius * 0.7f)
            {
                NetworkThirdPersonController controller =
                    target.GetComponent<NetworkThirdPersonController>();

                if (controller != null)
                {
                    Vector3 dir =
                        (target.transform.position -
                         transform.position).normalized;

                    dir.y = 0f;

                    controller.AddKnockback(
                        dir * knockbackForce);
                }
            }
        }
    }

    private void EndSkill()
    {
        _player.IsUsingUltimate = false;
        _cc.gravity = _originalGravity;

        _animator.SetBool(
            IsUsingUltimateHash,
            false);

        _actionLock.Unlock(ActionLockType.Attack);
        _actionLock.Unlock(ActionLockType.Dash);
        _actionLock.Unlock(ActionLockType.Jump);
        _actionLock.Unlock(ActionLockType.Skill);
        _controller.IgnoreLandAnimation = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayJumpEffect()
    {
        jumpEffect?.Play();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayLandEffect()
    {
        landEffect?.Play();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            radius);
    }
}