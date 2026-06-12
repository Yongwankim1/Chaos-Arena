using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBehaviorBridge))]
public abstract class EnemyAttack : NetworkBehaviour, IEnemyAttacker
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected string attackParam1 = "Attack";
    [SerializeField] protected string attackParam2 = "Attack2";
    [SerializeField] protected EnemyBehaviorBridge bridge;
    [SerializeField] protected Transform attackPos;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected EnemyHP enemyHP;
    [SerializeField] protected EnemyAttackType enemyAttackType;
    [SerializeField] protected float damage;
    [SerializeField] private float targetClearDelay = 5f;

    protected EnemyAttackBase defaultAttackPrefab;
    protected EnemyAttackBase strongAttackPrefab;

    private float lastAttackTime = -1f;

    private void Awake()
    {
        if (attackPos == null) attackPos = transform;
        if (animator == null) animator = GetComponent<Animator>();
        if (bridge == null) bridge = GetComponent<EnemyBehaviorBridge>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (enemyHP == null) enemyHP = GetComponent<EnemyHP>();
        defaultAttackPrefab = bridge.GetEffect(EffectType.DefaultAttack).GetComponent<EnemyAttackBase>();
        strongAttackPrefab = bridge.GetEffect(EffectType.StrongAttack).GetComponent<EnemyAttackBase>();
        damage = bridge.Config.attackDamage;
    }

    public override void Render()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        if (enemyHP == null || !enemyHP.HasTarget)
            return;

        if (lastAttackTime < 0f)
        {
            lastAttackTime = Time.time;
            return;
        }

        if (Time.time - lastAttackTime < targetClearDelay)
            return;

        enemyHP.ClearTarget();
        lastAttackTime = -1f;
    }

    void Start()
    {
        if (bridge != null) enemyAttackType = bridge.Config.attackType;
    }

    public void DefaultAttack()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        if (animator == null) return;

        lastAttackTime = Time.time;
        agent.isStopped = false;
        AimAtTarget();
        agent.isStopped = true;
        agent.ResetPath();
        RPC_DefaultAttack();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DefaultAttack()
    {
        animator.SetTrigger(attackParam2);
        Debug.Log("DefaultAttack animation started");
    }

    public void StrongAttack()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        if (animator == null) return;

        lastAttackTime = Time.time;
        agent.isStopped = false;
        AimAtTarget();
        agent.isStopped = true;
        agent.ResetPath();
        RPC_StrongAttack();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AimAtRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StrongAttack()
    {
        animator.SetTrigger(attackParam1);
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

        RPC_AimAtRotation(Quaternion.LookRotation(direction.normalized));
    }
}
