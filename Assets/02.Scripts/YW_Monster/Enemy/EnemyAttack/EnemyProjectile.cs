using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : EnemyAttackBase
{
    [SerializeField] float damage = 10f;
    private Coroutine fireRoutine;

    [SerializeField] LayerMask targetLayer;
    HashSet<IDamageable> targets;
    IAttacker attacker;
    public override void Init(IAttacker attacker)
    {
        base.Init();
        transform.SetParent(null);
        if (fireRoutine != null)
            StopCoroutine(fireRoutine);
        this.attacker = attacker;
        fireRoutine = StartCoroutine(FireDelayRoutine());
    }

    private IEnumerator FireDelayRoutine()
    {
        yield return new WaitForSeconds(fireDelay);

        rb.linearVelocity = transform.forward * velocity;

        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(((1 << other.gameObject.layer & targetLayer) != 0))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null) return;
            if (targets.Contains(damageable)) return;

            targets.Add(damageable);
            damageable.TakeDamage((int)damage, attacker);
        }
    }
}