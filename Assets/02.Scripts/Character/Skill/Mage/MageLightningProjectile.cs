using Fusion;
using UnityEngine;

public class MageLightningProjectile : NetworkBehaviour
{
    [SerializeField] private float stepDistance = 2f;
    [SerializeField] private float maxTravelDistance = 10f;
    [SerializeField] private float explosionDelay = 0.25f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int damage = 30;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private bool isDebug = true;
    [SerializeField] private ParticleSystem effect;

    private TickTimer explosionTimer;
    private Vector3 spawnPosition;
    private IAttacker attacker;

    private int lastPlayedExplosionCount;

    [Networked] private int ExplosionCount { get; set; }

    public void Init(IAttacker attacker)
    {
        this.attacker = attacker;
    }

    public override void Spawned()
    {
        spawnPosition = transform.position;
        lastPlayedExplosionCount = ExplosionCount;

        if (Object.HasStateAuthority)
        {
            ExplosionCount = 0;
            explosionTimer = TickTimer.CreateFromSeconds(Runner, explosionDelay);

            Explode();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!explosionTimer.Expired(Runner))
            return;

        float traveledDistance = Vector3.Distance(spawnPosition, transform.position);

        if (traveledDistance >= maxTravelDistance)
        {
            Runner.Despawn(Object);
            return;
        }

        transform.position += transform.forward * stepDistance;

        Explode();

        explosionTimer = TickTimer.CreateFromSeconds(Runner, explosionDelay);
    }

    public override void Render()
    {
        if (ExplosionCount == lastPlayedExplosionCount)
            return;

        lastPlayedExplosionCount = ExplosionCount;

        if (effect == null)
            return;

        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.Play();
    }

    [ContextMenu("TestBoom")]
    private void TestBoom()
    {
        if (effect == null)
            return;

        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.Play();
    }

    private void Explode()
    {
        ExplosionCount++;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, targetMask);

        foreach (Collider hit in hits)
        {

            if (!hit.TryGetComponent(out IDamageable damageable))
            {
                continue;
            }
            if(hit.TryGetComponent(out IAttacker hitAttacker))
            {
                if (hitAttacker == attacker) continue;
            }
            damageable.TakeDamage(damage, attacker);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}