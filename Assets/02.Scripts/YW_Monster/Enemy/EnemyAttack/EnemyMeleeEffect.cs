using UnityEngine;

public class EnemyMeleeEffect : EnemyAttackBase
{
    [SerializeField] float lifeTime = 0.4f;
    public override void Init()
    {
        base.Init();
        Destroy(gameObject, lifeTime);
    }

}
