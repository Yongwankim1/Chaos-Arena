using UnityEngine;

public class EnemyMelee : EnemyAttackBase
{
    [SerializeField] float lifeTime = 0.4f;
    public override void Init()
    {
        base.Init();

        Destroy(gameObject, lifeTime);
    }

}
