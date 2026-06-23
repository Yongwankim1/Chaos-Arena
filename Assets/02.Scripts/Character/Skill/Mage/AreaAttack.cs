using Fusion;
using UnityEngine;

public class AreaAttack : NetworkBehaviour
{
    [SerializeField] private ParticleSystem startEffect;
    [SerializeField] private float areaDuration = 5f;
    [SerializeField] private float areaRadius = 3f;
    private int damage = 10;
    [SerializeField] private float damageInterval = 0.25f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private bool isDebug = true;

    private IAttacker attacker;
    private TickTimer lifeTimer;
    private TickTimer damageTimer;

    public void Init(IAttacker attacker, int damage)
    {
        this.attacker = attacker;
        this.damage = damage;
    }

    public override void Spawned()
    {
        if (startEffect != null)
        {
            startEffect.Play();
        }

        if (Object.HasStateAuthority)
        {
            lifeTimer = TickTimer.CreateFromSeconds(Runner, areaDuration);
            damageTimer = TickTimer.None;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        if (damageTimer.ExpiredOrNotRunning(Runner))
        {
            DealDamage();
            damageTimer = TickTimer.CreateFromSeconds(Runner, damageInterval);
        }
    }

    private void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, areaRadius, targetMask);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

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

    private void OnDrawGizmos()
    {
        if (!isDebug)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}
