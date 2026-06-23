using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class BruteDash : NetworkBehaviour, IDash, ISkillCooldown
{
    [SerializeField]
    private float dashDistance = 6f;

    [SerializeField]
    private float dashDuration = 0.2f;

    [SerializeField]
    private float cooldown = 4f;

    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    private float hitRadius = 1.2f;

    [SerializeField]
    private Vector3 hitOffset = new Vector3(0f, 1f, 0f);

    [SerializeField]
    private float knockbackPower = 8f;

    [Networked]
    public TickTimer DashCooldown { get; set; }

    [Networked]
    public float DashRemainDistance { get; set; }

    [Networked]
    public Vector3 DashDirection { get; set; }

    public TickTimer CooldownTimer => DashCooldown;

    public float CooldownDuration => cooldown;

    public bool IsDashing =>_player != null &&_player.IsDashing;
    [SerializeField]
    private ParticleSystem dashTrailEffect;
    private PlayerCharacter _player;

    private CharacterController _cc;

    private CharacterActionLock _actionLock;

    private NetworkThirdPersonController _controller;

    private readonly HashSet<PlayerCharacter> _hitPlayers =
        new HashSet<PlayerCharacter>();

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();

        _cc = GetComponent<CharacterController>();

        _actionLock = GetComponent<CharacterActionLock>();

        _controller =
            GetComponent<NetworkThirdPersonController>();
    }

    public void Dash()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (_player.IsDashing)
            return;

        if (!_controller.Grounded)
            return;

        if (!DashCooldown.ExpiredOrNotRunning(Runner))
            return;

        Vector3 direction =
            transform.forward;

        float distance = GetAvailableDistance(direction);

        if (distance <= 0.1f)
            return;

        DashDirection = direction;

        DashRemainDistance = distance;

        _player.IsDashing = true;
        RPC_PlayDashStart();
        DashCooldown = TickTimer.CreateFromSeconds(Runner,cooldown);

        _actionLock.Lock(ActionLockType.Move);
        _actionLock.Lock(ActionLockType.Attack);
        _actionLock.Lock(ActionLockType.Dash);

        _hitPlayers.Clear();

    }

    private float GetAvailableDistance(Vector3 direction)
    {
        Vector3 start = transform.position;

        Vector3 p1 = start +_cc.center + Vector3.up *(-_cc.height * 0.5f + _cc.radius);

        Vector3 p2 =
            start +
            _cc.center +
            Vector3.up *
            (_cc.height * 0.5f - _cc.radius);

        if (Physics.CapsuleCast(
                p1,
                p2,
                _cc.radius,
                direction,
                out RaycastHit hit,
                dashDistance,
                obstacleMask))
        {
            return Mathf.Max(
                hit.distance - 0.3f,
                0f);
        }

        return dashDistance;
    }

    public float GetMoveThisTick()
    {
        if (!HasStateAuthority &&
            !HasInputAuthority)
        {
            return 0f;
        }

        float speed =
            dashDistance /
            dashDuration;

        float moveThisTick =
            speed *
            Runner.DeltaTime;

        moveThisTick =
            Mathf.Min(
                moveThisTick,
                DashRemainDistance);

        if (HasStateAuthority)
        {
            DashRemainDistance -= moveThisTick;

            CheckHit();

            if (DashRemainDistance <= 0f)
            {
                EndDash();
            }
        }

        return moveThisTick;
    }

    private void CheckHit()
    {
        Vector3 hitCenter =
            transform.position +
            transform.TransformDirection(hitOffset);

        Collider[] hits =
            Physics.OverlapSphere(
                hitCenter,
                hitRadius,
                playerMask);

        foreach (Collider hit in hits)
        {
            PlayerCharacter target =
                hit.GetComponentInParent<PlayerCharacter>();

            if (target == null)
                continue;

            if (target == _player)
                continue;

            if (target.Team == _player.Team)
                continue;

            if (_hitPlayers.Contains(target))
                continue;

            _hitPlayers.Add(target);

            NetworkThirdPersonController controller =
                target.GetComponent<NetworkThirdPersonController>();

            if (controller == null)
                continue;

            Vector3 dir =
                (target.transform.position -
                 transform.position).normalized;

            dir.y = 0f;

            controller.AddKnockback(
                dir * knockbackPower);
        }
    }

    private void EndDash()
    {
        _player.IsDashing = false;

        _actionLock.Unlock(ActionLockType.Move);
        _actionLock.Unlock(ActionLockType.Attack);
        _actionLock.Unlock(ActionLockType.Dash);

        _hitPlayers.Clear();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDashStart()
    {
        dashTrailEffect?.Play();
    }

    private void OnDrawGizmos()
    {
        Vector3 hitCenter =
            transform.position +
            transform.TransformDirection(hitOffset);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            hitCenter,
            hitRadius);

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            transform.forward * dashDistance);
    }
}