using Fusion;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDeath : NetworkBehaviour, IDeathHandler
{
    [SerializeField] BehaviorGraphAgent graph;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Collider bodyCollider;

    [SerializeField] Animator animator;
    [SerializeField] string DieParam = "Die";
    [SerializeField] BuffType buffType;
    void Awake()
    {
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(graph == null) graph = GetComponent<BehaviorGraphAgent>();
        if(bodyCollider == null) bodyCollider = GetComponent<Collider>();
        if(animator == null) animator = GetComponent<Animator>();
    }

    public void HandleDeath(IAttacker attacker)
    {
        if (!Object.HasStateAuthority)
            return;

        GameObject attackerObject = attacker.GetAttacker();
        if (attackerObject != null)
        {
            Buff buff = attackerObject.GetComponent<Buff>();
            if (buff != null)
            {
                buff.AddBuff(buffType); // 원하는 버프 타입
            }
        }

        RPC_Die();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Die()
    {
        if (graph != null) graph.enabled = false;
        if (agent != null) agent.isStopped = true;
        if (bodyCollider != null) bodyCollider.enabled = false;

        if (animator != null)
            animator.SetTrigger(DieParam);

        Invoke(nameof(SetActiveFalse), 5f);
    }

    private void SetActiveFalse() => gameObject.SetActive(false);

}
