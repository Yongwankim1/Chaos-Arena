using Fusion;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDeath : NetworkBehaviour, IDeathHandler
{
    [SerializeField] BehaviorGraphAgent graph;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Collider bodyCollider;

    [SerializeField] NetworkMecanimAnimator animator;
    [SerializeField] string DieParam = "Die";
    void Awake()
    {
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(graph == null) graph = GetComponent<BehaviorGraphAgent>();
        if(bodyCollider == null) bodyCollider = GetComponent<Collider>();
        if(animator == null) animator = GetComponent<NetworkMecanimAnimator>();
    }

    public void HandleDeath(IAttacker _)
    {
        graph.enabled = false;
        agent.isStopped = true;
        bodyCollider.enabled = false;
        RPC_Die();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Die()
    {
        animator.SetTrigger(DieParam, false);
        Invoke("SetActiveFalse", 5f);
    }

    private void SetActiveFalse() => gameObject.SetActive(false);

}
