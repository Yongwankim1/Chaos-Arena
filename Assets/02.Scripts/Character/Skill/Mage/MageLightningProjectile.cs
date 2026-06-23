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
    [Networked] private Vector3 ExplosionPosition { get; set; }

    public void Init(IAttacker attacker)
    {
        this.attacker = attacker;
    }

    public override void Spawned()
    {
        spawnPosition = transform.position;
        lastPlayedExplosionCount = 0;

        if (Object.HasStateAuthority)
        {
            ExplosionCount = 0;
            ExplosionPosition = transform.position;
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

        PlayExplosionEffect(ExplosionPosition);
    }

    [ContextMenu("TestBoom")]
    private void TestBoom()
    {
        PlayExplosionEffect(transform.position);
    }

    private void Explode()
    {
        ExplosionCount++;
        ExplosionPosition = transform.position;

        Collider[] hits = Physics.OverlapSphere(ExplosionPosition, explosionRadius, targetMask);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable damageable))
            {
                continue;
            }

            if (hit.TryGetComponent(out IAttacker hitAttacker))
            {
                if (hitAttacker == attacker)
                    continue;
            }

            damageable.TakeDamage(damage, attacker);

            CharacterCombat combat =
                attacker?.GetAttacker()?.GetComponent<CharacterCombat>();

            combat?.TryApplyRedBuffSlow(
                damageable.GetDamageableObject());
        }
    }

    private void PlayExplosionEffect(Vector3 position)
    {
        if (effect == null)
            return;

        ParticleSystem fx = Instantiate(effect, position, transform.rotation);
        fx.Play();

        Destroy(fx.gameObject, GetEffectLifeTime(fx));
    }

    private float GetEffectLifeTime(ParticleSystem root)
    {
        float lifeTime = 0f;
        ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            lifeTime = Mathf.Max(lifeTime, main.duration + main.startLifetime.constantMax);
        }

        return lifeTime;
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
