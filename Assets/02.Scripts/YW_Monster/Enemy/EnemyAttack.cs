using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttack : NetworkBehaviour, IEnemyAttacker
{
    [SerializeField] NetworkMecanimAnimator netAnimator;
    [SerializeField] private string attackParam1 = "Attack";
    [SerializeField] private string attackParam2 = "Attack2";
    [SerializeField] private EnemyBehaviorBridge bridge;
    [SerializeField] Transform attackPos;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] EnemyHP enemyHP;

    EnemyAttackBase defaultAttackPrefab;
    EnemyAttackBase strongAttackPrefab;
    private void Awake()
    {
        if (attackPos == null) attackPos = transform;
        if (netAnimator == null) netAnimator = GetComponent<NetworkMecanimAnimator>();
        if(bridge == null) bridge = GetComponent<EnemyBehaviorBridge>();
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(enemyHP == null) enemyHP = GetComponent<EnemyHP>();
        defaultAttackPrefab = bridge.GetEffect(EffectType.DefaultAttack).GetComponent<EnemyAttackBase>();
        strongAttackPrefab = bridge.GetEffect(EffectType.StrongAttack).GetComponent<EnemyAttackBase>();

    }
    public void DefaultAttack()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        if (netAnimator == null) return;
        agent.isStopped = false;
        AimAtTarget();
        agent.isStopped = true;
        agent.ResetPath();
        RPC_DefaultAttack();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DefaultAttack()
    {
        netAnimator.SetTrigger(attackParam2, true);
        Debug.Log("DefaultAttack animation started");
    }

    public void StrongAttack()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        if (netAnimator == null) return;
        agent.isStopped = false;
        AimAtTarget();
        agent.isStopped = true;
        agent.ResetPath();
        RPC_StrongAttack();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StrongAttack()
    {
        netAnimator.SetTrigger(attackParam1, true);
        Debug.Log("StrongAttack animation started");
    }

    public void OnEffect(EffectType effect)
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        RPC_PlayEffect(effect);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayEffect(EffectType effect)
    {
        // TODO: Play attack effect and sound.
        Debug.Log("Attack effect");
        if (defaultAttackPrefab == null || strongAttackPrefab == null) return;
        EnemyAttackBase attackEffect = null;
        switch (effect)
        {
            case EffectType.DefaultAttack: attackEffect = Instantiate(defaultAttackPrefab, attackPos.position, transform.rotation, attackPos); attackEffect.Init(); break;
            case EffectType.StrongAttack: attackEffect = Instantiate(strongAttackPrefab, attackPos.position, transform.rotation, attackPos); attackEffect.Init(); break;
        }

    }


    public GameObject GetAttacker()
    {
        return gameObject;
    }

    public void AimAtTarget()
    {
        if (enemyHP == null || enemyHP.Target == null)
            return;

        Vector3 targetPosition = enemyHP.Target.transform.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

}
