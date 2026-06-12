using Fusion;

public class EnemyAttackRange : EnemyAttack
{
    public override void OnEffect(EffectType effect)
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        base.OnEffect(effect);

        RPC_ProjectileEffect(effect);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ProjectileEffect(EffectType effect)
    {
        if (defaultAttackPrefab == null || strongAttackPrefab == null) return;
        EnemyAttackBase attackEffect = null;
        switch (effect)
        {
            case EffectType.DefaultAttack: attackEffect = Instantiate(defaultAttackPrefab, attackPos.position, transform.rotation, attackPos); attackEffect.Init(this); break;
            case EffectType.StrongAttack: attackEffect = Instantiate(strongAttackPrefab, attackPos.position, transform.rotation, attackPos); attackEffect.Init(this); break;
        }
    }
}
