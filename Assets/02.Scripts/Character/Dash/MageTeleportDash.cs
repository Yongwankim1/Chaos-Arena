using Fusion;
using UnityEngine;

public class MageTeleportDash : NetworkBehaviour, ISkillCooldown, IDash
{
    [SerializeField] private float teleportDistance = 8f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private ParticleSystem teleportStartEffect;
    [SerializeField] private ParticleSystem teleportEndEffect;

    [SerializeField] private float teleportLockTime = 0.3f;

    private CharacterActionLock _actionLock;

    [Networked]
    public TickTimer DashCooldown { get; set; }

    public TickTimer CooldownTimer => DashCooldown;
    public float CooldownDuration => cooldown;

    private PlayerCharacter _player;
    private CharacterController _cc;
    private NetworkCharacterController _controller;

    public bool IsDashing => false;

    public Vector3 DashDirection => Vector3.zero;

    public float GetMoveThisTick()
    {
        return 0f;
    }

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _cc = GetComponent<CharacterController>();
        _controller = GetComponent<NetworkCharacterController>();
        _actionLock = GetComponent<CharacterActionLock>();
    }
    public void Dash()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (!DashCooldown.ExpiredOrNotRunning(Runner))
            return;

        Vector3 targetPosition = GetTeleportPosition();
        targetPosition = AdjustTargetPosition(targetPosition);

        Vector3 oldPosition = transform.position;

        DashCooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        _actionLock?.Lock(ActionLockType.Move);
        _actionLock?.Lock(ActionLockType.Attack);
        _actionLock?.Lock(ActionLockType.Dash);

        RPC_PlayTeleportStart();

        _player.IsDashing = true;

        _controller.Teleport(targetPosition);

        _player.IsDashing = false;

        RPC_PlayTeleportEnd();

        RPC_WarpCamera(oldPosition, targetPosition);

        Runner.StartCoroutine(UnlockRoutine());
    }
    private System.Collections.IEnumerator UnlockRoutine()
    {
        yield return new WaitForSeconds(teleportLockTime);

        _actionLock?.Unlock(ActionLockType.Move);
        _actionLock?.Unlock(ActionLockType.Attack);
        _actionLock?.Unlock(ActionLockType.Dash);
    }
    private Vector3 GetTeleportPosition()
    {
        Vector3 start = transform.position;

        Vector3 direction = transform.forward;

        Vector3 p1 = start + _cc.center + Vector3.up * (-_cc.height * 0.5f + _cc.radius);

        Vector3 p2 = start + _cc.center + Vector3.up * (_cc.height * 0.5f - _cc.radius);

        if (Physics.CapsuleCast(
            p1,
            p2,
            _cc.radius,
            direction,
            out RaycastHit hit,
            teleportDistance,
            obstacleMask))
        {
            return start + direction * Mathf.Max(hit.distance - 0.5f, 0f);
        }

        return start + direction * teleportDistance;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayTeleportStart()
    {
        if (teleportStartEffect == null)
            return;

        teleportStartEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        teleportStartEffect.Play();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayTeleportEnd()
    {
        if (teleportEndEffect == null)
            return;

        teleportEndEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        teleportEndEffect.Play();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_WarpCamera(Vector3 oldPosition, Vector3 newPosition)
    {
        if (!HasInputAuthority)
            return;

        var cam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();

        if (cam == null)
            return;

        Vector3 delta = newPosition - oldPosition;

        cam.OnTargetObjectWarped(transform, delta);
    }
    private Vector3 AdjustTargetPosition(Vector3 targetPosition)
    {
        Collider[] hits = Physics.OverlapCapsule(
            targetPosition + _cc.center + Vector3.up * (-_cc.height * 0.5f + _cc.radius),
            targetPosition + _cc.center + Vector3.up * (_cc.height * 0.5f - _cc.radius),
            _cc.radius);

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            if (hit.isTrigger)
                continue;

            Vector3 dir = (targetPosition - transform.position).normalized;

            return hit.ClosestPoint(targetPosition) - dir * 0.5f;
        }

        return targetPosition;
    }
}
