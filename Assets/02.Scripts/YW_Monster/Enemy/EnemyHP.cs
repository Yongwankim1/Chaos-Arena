using System;
using UnityEngine;

public class EnemyHP : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject target;
    public GameObject Target => target;
    public bool HasTarget => target != null;
    public int MaxHP = 100;
    public int currentHP = 100;
    public bool IsDead => currentHP <= 0;

    public event Action<int, int> OnHPChange;

    public void ClearTarget()
    {
        target = null;
    }

    public void TakeDamage(int damage, IAttacker attacker)
    {
        if (IsDead) return;
        if (damage <= 0)
        {
            Debug.Log("Damage is 0. Return.");
            return;
        }

        target = attacker.GetAttacker();
        currentHP = Mathf.Max(currentHP - damage, 0);

        OnHPChange?.Invoke(MaxHP, currentHP);
        if (IsDead)
        {
            IDeathHandler death = GetComponent<IDeathHandler>();
            death.HandleDeath(attacker);
        }
    }
}