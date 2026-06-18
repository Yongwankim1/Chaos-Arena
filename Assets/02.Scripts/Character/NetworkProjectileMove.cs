using Fusion;
using UnityEngine;

public class NetworkProjectileMove : NetworkBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] bool isDebug = true;
    [Networked] private Vector3 Direction { get; set; }
    [Networked] private Vector3 StartPosition { get; set; }

    private IAttacker attacker;

    public void Init(IAttacker attacker, Vector3 direction, float damage)
    {
        this.attacker = attacker;
        Direction = direction.normalized;
        StartPosition = transform.position;
        transform.forward = Direction;
        this.damage = (int) damage;
    }

    public override void Spawned()
    {
        transform.forward = Direction;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (Direction == Vector3.zero)
            return;

        float moveDistance = speed * Runner.DeltaTime;

        transform.position += Direction * moveDistance;

        GetCapsulePoints(Direction, out Vector3 point1, out Vector3 point2);

        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, hitMask);

        foreach (Collider hit in hits)
        {
            if (attacker != null &&
                hit.transform.root == attacker.GetAttacker().transform.root)
            {
                continue;
            }

            Hit(hit);
            return;
        }

        if (Vector3.Distance(StartPosition, transform.position) >= maxDistance)
        {
            Runner.Despawn(Object);
        }
    }

    private void Hit(Collider collider)
    {
        IDamageable damageable = collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage, attacker);
        }

        RPC_PlayHitEffect(transform.position, transform.rotation);

        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayHitEffect(Vector3 position, Quaternion rotation)
    {
        if (hitEffectPrefab == null)
            return;

        GameObject effect = Instantiate(hitEffectPrefab, position, rotation);
        Destroy(effect, 2f);
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
            return;

        Vector3 direction = transform.forward;
        Vector3 start = transform.position;

        if (Object != null && Object.IsValid)
        {
            if (Direction != Vector3.zero)
                direction = Direction;

            if (StartPosition != Vector3.zero)
                start = StartPosition;
        }

        direction.Normalize();

        GetCapsulePoints(direction, out Vector3 point1, out Vector3 point2);
        Vector3 end = start + direction * maxDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(point1, radius);
        Gizmos.DrawWireSphere(point2, radius);
        Gizmos.DrawLine(point1 + transform.up * radius, point2 + transform.up * radius);
        Gizmos.DrawLine(point1 - transform.up * radius, point2 - transform.up * radius);
        Gizmos.DrawLine(point1 + transform.right * radius, point2 + transform.right * radius);
        Gizmos.DrawLine(point1 - transform.right * radius, point2 - transform.right * radius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + direction * 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, radius);
    }

    private void GetCapsulePoints(Vector3 direction, out Vector3 point1, out Vector3 point2)
    {
        if (direction == Vector3.zero)
            direction = transform.forward;

        direction.Normalize();

        float halfSegmentLength = Mathf.Max(0f, height * 0.5f - radius);
        Vector3 halfSegment = direction * halfSegmentLength;

        point1 = transform.position - halfSegment;
        point2 = transform.position + halfSegment;
    }
}
