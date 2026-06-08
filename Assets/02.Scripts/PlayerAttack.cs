using UnityEngine;

public class PlayerAttack : MonoBehaviour, IAttacker
{
    [SerializeField] GameObject targetTest;
    [SerializeField] int Damage = 5;
    [SerializeField] EnemyHP targetHP;
    IDamageable damageable;

    [ContextMenu("AttackTest")]
    public void DefaultAttack()
    {
        damageable = targetHP.GetComponent<IDamageable>();
        damageable.TakeDamage(Damage, this);
    }

    public GameObject GetAttacker()
    {
        return targetTest;
    }
}
