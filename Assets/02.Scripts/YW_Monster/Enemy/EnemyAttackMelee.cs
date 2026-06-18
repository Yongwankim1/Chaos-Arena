using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class EnemyAttackMelee : EnemyAttack
{
    [SerializeField] bool isDebug;
    [SerializeField] Vector3 defaultAttackSize = new Vector3(5f, 5f, 5f);
    [SerializeField] Vector3 strongAttackSize = new Vector3(5f, 5f, 5f);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] float strongAttackDamageMultiplier = 1.2f;
    [SerializeField] Transform effectPos;
    [Networked] public NetworkBool IsAttack { get; set; }
    [Networked] private EffectType CurrentAttackEffect { get; set; }

    private readonly HashSet<IDamageable> hitTargets = new();

    [SerializeField]
    private AttackData attackFeedbackData;

    public override void OnEffect(EffectType effect)
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        base.OnEffect(effect);

        if (Object.HasStateAuthority)
        {
            CurrentAttackEffect = effect;
            IsAttack = true;
            hitTargets.Clear();
        }

        RPC_MeleeEffect(effect);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MeleeEffect(EffectType effect)
    {
        Debug.Log("Attack effect");

        EnemyAttackBase prefab = GetAttackPrefab(effect);
        if (prefab == null) return;

        EnemyAttackBase attackEffect = Instantiate(prefab, effectPos.position, transform.rotation, effectPos);

        attackEffect.Init();
    }

    public void MeleeAttackEnd()
    {
        if (Object.HasStateAuthority)
        {
            IsAttack = false;
            hitTargets.Clear();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsAttack) return;

        Vector3 attackSize = GetAttackSize(CurrentAttackEffect);
        Vector3 attackHalfExtents = attackSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            attackPos.position,
            attackHalfExtents,
            attackPos.rotation,
            targetLayer
        );

        int attackDamage = Mathf.RoundToInt(GetAttackDamage(CurrentAttackEffect));

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable == null) continue;
            if (hitTargets.Contains(damageable)) continue;
            Debug.Log(hit.tag);
            hitTargets.Add(damageable);
            damageable.TakeDamage(attackDamage, this);
            HitFeedbackSystem.Apply(this, damageable, attackFeedbackData);
        }
    }

    private Vector3 GetAttackSize(EffectType effect)
    {
        return effect == EffectType.StrongAttack ? strongAttackSize : defaultAttackSize;
    }

    private EnemyAttackBase GetAttackPrefab(EffectType effect)
    {
        return effect == EffectType.StrongAttack ? strongAttackPrefab : defaultAttackPrefab;
    }

    private float GetAttackDamage(EffectType effect)
    {
        return effect == EffectType.StrongAttack
            ? damage * strongAttackDamageMultiplier
            : damage;
    }

    private void OnDrawGizmos()
    {
        if (!isDebug || !attackPos) return;

        Gizmos.matrix = Matrix4x4.TRS(attackPos.position, attackPos.rotation, Vector3.one);

        if (Application.isPlaying && CurrentAttackEffect == EffectType.StrongAttack)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.zero, strongAttackSize);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, defaultAttackSize);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}
