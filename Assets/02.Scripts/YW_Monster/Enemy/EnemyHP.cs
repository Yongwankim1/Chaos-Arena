using System;
using Fusion;
using UnityEngine;

public class EnemyHP : NetworkBehaviour, IDamageable, IHasHealth, IHealable
{
    [SerializeField] private GameObject target;
    public GameObject Target => target;
    public bool HasTarget => target != null;
    public int MaxHP = 100;
    [Networked] public int currentHP { get; set; }
    public bool IsDead => currentHP <= 0;

    public event Action<int, int> OnHPChange;

    [Header("Heal")]
    [SerializeField] private float healPercent = 0.2f; // 회복 1회당 최대 체력 기준 회복 비율
    [SerializeField] private float healInterval = 1f;  // 회복 주기(초)
    public float HealPercent => healPercent;
    public float HealInterval => healInterval;

    public override void Spawned()
    {
        if (Object != null && Object.HasStateAuthority && currentHP <= 0)
        {
            currentHP = MaxHP;
            OnHPChange?.Invoke(MaxHP, currentHP);
        }
    }

    public void ClearTarget()
    {
        target = null;
    }

    public void TakeDamage(int damage, IAttacker attacker)
    {
        if (Object != null && !Object.HasStateAuthority) return;
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

    public void Heal(float amount)
    {
        if (Object != null && !Object.HasStateAuthority) return;
        if (IsDead) return;
        if (amount <= 0) return;

        currentHP = Mathf.Min(currentHP + Mathf.RoundToInt(amount), MaxHP);
        OnHPChange?.Invoke(MaxHP, currentHP);
    }

    public void HealByPercent(float percent)
    {
        if (IsDead) return;
        if (percent <= 0) return;

        int healAmount = Mathf.RoundToInt(MaxHP * percent);
        Heal(healAmount);
    }
}
