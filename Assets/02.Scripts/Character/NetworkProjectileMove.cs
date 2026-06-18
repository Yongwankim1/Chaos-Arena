using Fusion;
using UnityEngine;

public class NetworkProjectileMove : NetworkBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject hitEffectPrefab;

    [Networked] private Vector3 Direction { get; set; }
    [Networked] private Vector3 StartPosition { get; set; }

    private IAttacker attacker;

    public void Init(IAttacker attacker, Vector3 direction)
    {
        this.attacker = attacker;
        Direction = direction.normalized;
        StartPosition = transform.position;
        transform.forward = Direction;
    }

    public override void Spawned()
    {
        transform.forward = Direction;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        float moveDistance = speed * Runner.DeltaTime;

        if (Physics.SphereCast(transform.position, radius, Direction, out RaycastHit hit, moveDistance, hitMask))
        {
            transform.position = hit.point;
            Hit(hit.collider);
            return;
        }

        transform.position += Direction * moveDistance;

        if (Vector3.Distance(StartPosition, transform.position) >= maxDistance)
        {
            Runner.Despawn(Object);
        }
    }

    private void Hit(Collider collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable))
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
}