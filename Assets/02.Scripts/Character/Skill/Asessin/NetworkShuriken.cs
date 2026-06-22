using Fusion;
using UnityEngine;

public class NetworkShuriken : NetworkBehaviour
{
    [Header("OBJ")]
    [SerializeField] Transform shuriken_Obj;
    [Header("Move")]
    [SerializeField] private float speed = 30f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField]
    private float rotateSpeed = 2160f;
    [Networked]
    private TickTimer DropTimer { get; set; }
    [SerializeField]
    private float dropTime = 0.5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float dropAmount = 2f;

    [Header("DOT")]
    [SerializeField] private float damagePercentPerSecond = 5f;
    [SerializeField] private float damageInterval = 0.2f;
    [SerializeField] private float stopDuration = 3f;
    [SerializeField] private float shrinkDuration = 0.5f;
    [SerializeField] private float dotRadius = 2f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField]
    private float firstHitDamagePercent = 110f;

    [SerializeField]
    private AttackData firstHitAttackData;

    [SerializeField]
    private ParticleSystem stopEffect;

    [Networked] private Vector3 Direction { get; set; }
    [Networked] private Vector3 StartPosition { get; set; }
    [Networked] private float TraveledDistance { get; set; }
    [Networked] private NetworkBool IsStopped { get; set; }
    [Networked] private TickTimer StopTimer { get; set; }
    [Networked] private TickTimer DamageTimer { get; set; }
    [Networked] private TickTimer ShrinkTimer { get; set; }

    private Vector3 _originScale;
    private IAttacker _owner;

    public void Initialize(IAttacker owner, Vector3 direction)
    {
        _owner = owner;
        Direction = direction.normalized;
        StartPosition = transform.position;
        TraveledDistance = 0f;
        transform.forward = Direction;
        DropTimer = TickTimer.CreateFromSeconds(Runner, dropTime);
    }

    public override void Spawned()
    {
        _originScale = transform.localScale;
        transform.forward = Direction;
    }

    public override void Render()
    {
        shuriken_Obj.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!IsStopped)
        {
            Move();
        }
        else
        {
            HandleDot();

            HandleDisappear();
        }
    }

    private void Move()
    {
        float moveDistance = speed * Runner.DeltaTime;

        if (Physics.SphereCast(transform.position, 0.2f, Direction, out RaycastHit hit, moveDistance, obstacleMask))
        {
            transform.position = hit.point - Direction * 0.1f;

            StopShuriken();

            return;
        }

        TraveledDistance += moveDistance;

        Vector3 nextPosition = transform.position + Direction * moveDistance;

        float remain = DropTimer.RemainingTime(Runner) ?? 0f;

        float t = 1f - Mathf.Clamp01(remain / dropTime);

        float drop = t * dropAmount;

        nextPosition.y = StartPosition.y - drop;

        transform.position = nextPosition;

        if (TraveledDistance >= maxDistance)
        {
            StopShuriken();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
            return;

        if (IsStopped)
            return;

        if (_owner != null && other.transform.root == _owner.GetAttacker().transform.root)
            return;

        IDamageable damageable =other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        PlayerCharacter player =_owner.GetAttacker().GetComponent<PlayerCharacter>();

        if (player != null)
        {
            int damage = Mathf.RoundToInt(player.AttackPower * (firstHitDamagePercent * 0.01f));


            PlayerCharacter attackerPlayer = _owner.GetAttacker().GetComponent<PlayerCharacter>();

            PlayerCharacter targetPlayer = damageable.GetDamageableObject().GetComponent<PlayerCharacter>();

            if (attackerPlayer != null && targetPlayer != null)
            {
                if (attackerPlayer.Team == targetPlayer.Team)
                {
                    return;
                }
            }

            damageable.TakeDamage(damage, _owner);

            if (firstHitAttackData != null)
            {
                HitFeedbackSystem.Apply(_owner, damageable, firstHitAttackData);
            }
        }

        StopShuriken();
    }

    private void StopShuriken()
    {
        if (IsStopped)
            return;

        IsStopped = true;

        RPC_PlayStopEffect();

        StopTimer = TickTimer.CreateFromSeconds(Runner, stopDuration);

        DamageTimer = TickTimer.CreateFromSeconds(Runner, damageInterval);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayStopEffect()
    {
        if (stopEffect == null)
            return;

        stopEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        stopEffect.Play();
    }

    private void HandleDot()
    {
        if (!DamageTimer.ExpiredOrNotRunning(Runner))
            return;

        DamageTimer = TickTimer.CreateFromSeconds(Runner, damageInterval);

        Collider[] hits = Physics.OverlapSphere(transform.position, dotRadius, playerMask);

        foreach (Collider hit in hits)
        {
            if (_owner != null && hit.transform.root == _owner.GetAttacker().transform.root)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            PlayerCharacter attackerPlayer = _owner.GetAttacker().GetComponent<PlayerCharacter>();

            PlayerCharacter targetPlayer = damageable.GetDamageableObject().GetComponent<PlayerCharacter>();

            if (attackerPlayer != null &&targetPlayer != null)
            {
                if (attackerPlayer.Team ==targetPlayer.Team)
                {
                    continue;
                }
            }

          

            PlayerCharacter player = _owner.GetAttacker().GetComponent<PlayerCharacter>();

            if (player == null)
                continue;

            float damageFloat = player.AttackPower * (damagePercentPerSecond * 0.01f) * damageInterval;

            int damage = Mathf.Max(1, Mathf.RoundToInt(damageFloat));

            damageable.TakeDamage(damage, _owner);
        }
    }

    private void HandleDisappear()
    {
        if (StopTimer.Expired(Runner) && !ShrinkTimer.IsRunning)
        {
            ShrinkTimer = TickTimer.CreateFromSeconds(Runner, shrinkDuration);
        }

        if (!ShrinkTimer.IsRunning)
            return;

        float remain = ShrinkTimer.RemainingTime(Runner) ?? 0f;

        float t = 1f - remain / shrinkDuration;

        transform.localScale = Vector3.Lerp(_originScale, Vector3.zero, t);

        if (ShrinkTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, dotRadius);
    }
}
