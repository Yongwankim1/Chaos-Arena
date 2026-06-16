using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, IAttacker attacker);
    GameObject GetDamageableObject();
}
