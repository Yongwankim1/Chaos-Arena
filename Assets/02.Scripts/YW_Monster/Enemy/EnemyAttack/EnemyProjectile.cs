using System.Collections;
using UnityEngine;

public class EnemyProjectile : EnemyAttackBase
{
    private Coroutine fireRoutine;


    public override void Init()
    {
        base.Init();
        if (fireRoutine != null)
            StopCoroutine(fireRoutine);

        fireRoutine = StartCoroutine(FireDelayRoutine());
    }

    private IEnumerator FireDelayRoutine()
    {
        yield return new WaitForSeconds(fireDelay);

        rb.linearVelocity = transform.forward * velocity;

        Destroy(gameObject, 3f);
    }
}