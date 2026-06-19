using System;
using UnityEngine;

public class EnemyHP : MonoBehaviour, IDamageable, IHasHealth
{
    [SerializeField] private GameObject target;
    public GameObject Target => target;
    public bool HasTarget => target != null;
    public int MaxHP = 100;
    public int currentHP = 100;
    public bool IsDead => currentHP <= 0;

    public event Action<int, int> OnHPChange;

    [Header("Heal")]
    [SerializeField] private float healPercent = 0.2f; //초당 회복시킬 퍼센트
    [SerializeField] private float healInterval = 1f;  // 회복 주기(초)
    public float HealPercent => healPercent;
    public float HealInterval => healInterval;
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

    public void GetHPInfo(out float curHP, out float maxHP)
    {
        curHP = currentHP;
        maxHP = MaxHP;
    }
    public GameObject GetDamageableObject()
    {
        return gameObject;
    }
}