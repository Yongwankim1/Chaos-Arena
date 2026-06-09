using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class EnemyAttackBehavior : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private NetworkObject networkObject;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(agent == null) agent = animator.GetComponent<NavMeshAgent>();
        if(networkObject == null) networkObject = animator.GetComponent<NetworkObject>();


    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetStopped(true);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetStopped(false);
    }

    private void SetStopped(bool stopped)
    {
        if (networkObject != null && !networkObject.HasStateAuthority) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        agent.isStopped = stopped;

        if (stopped)
        {
            agent.velocity = Vector3.zero;
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
