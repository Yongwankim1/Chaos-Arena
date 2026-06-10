using Fusion;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(EnemyBehaviorBridge))]
public abstract class EnemyAttack : NetworkBehaviour, IEnemyAttacker
{
    [SerializeField] protected NetworkMecanimAnimator netAnimator;
    [SerializeField] protected string attackParam1 = "Attack";
    [SerializeField] protected string attackParam2 = "Attack2";
    [SerializeField] protected EnemyBehaviorBridge bridge;
    [SerializeField] protected Transform attackPos;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected EnemyHP enemyHP;
    [SerializeField] protected EnemyAttackType enemyAttackType;
    [SerializeField] protected float damage;


    protected EnemyAttackBase defaultAttackPrefab;
    protected EnemyAttackBase strongAttackPrefab;

    private void Awake()
    {
        if (attackPos == null) attackPos = transform;
        if (netAnimator == null) netAnimator = GetComponent<NetworkMecanimAnimator>();
        if(bridge == null) bridge = GetComponent<EnemyBehaviorBridge>();
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(enemyHP == null) enemyHP = GetComponent<EnemyHP>();
        defaultAttackPrefab = bridge.GetEffect(EffectType.DefaultAttack).GetComponent<EnemyAttackBase>();
        strongAttackPrefab = bridge.GetEffect(EffectType.StrongAttack).GetComponent<EnemyAttackBase>();
        damage = bridge.Config.attackDamage;
    }
    void Start()
    {
        if (bridge != null) enemyAttackType = bridge.Config.attackType;
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

    public virtual void OnEffect(EffectType effect)
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
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
