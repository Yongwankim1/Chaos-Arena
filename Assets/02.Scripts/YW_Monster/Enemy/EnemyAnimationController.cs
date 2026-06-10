using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : NetworkBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] private string movingParam = "Moving";
    [SerializeField] private string velocityXParam = "Velocity X";
    [SerializeField] private string velocityZParam = "Velocity Z";
    [SerializeField] private float movingThreshold = 0.05f;

    [SerializeField] 

    int movingHash;
    int velocityXHash;
    int velocityZHash;

    private void Awake()
    {
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(animator == null) animator = GetComponent<Animator>();

        movingHash = Animator.StringToHash(movingParam);
        velocityXHash = Animator.StringToHash(velocityXParam);
        velocityZHash = Animator.StringToHash(velocityZParam);
    }

    public override void FixedUpdateNetwork()
    {
        if (animator == null || agent == null) return;
        if (Object != null && !Object.HasStateAuthority) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        Vector3 planarVelocity = agent.velocity;
        planarVelocity.y = 0f;
        Vector3 localVelocity = transform.InverseTransformDirection(planarVelocity);

        float normalizedX = Mathf.Clamp(localVelocity.x / Mathf.Max(0.01f,agent.speed), -1f, 1f);
        float normalizedZ = Mathf.Clamp(localVelocity.z / Mathf.Max(0.01f,agent.speed), -1f, 1f);

        bool isMoving = !agent.isStopped && planarVelocity.sqrMagnitude >= movingThreshold * movingThreshold;

        animator.SetBool(movingHash, isMoving);
        animator.SetFloat(velocityXHash, normalizedX);
        animator.SetFloat(velocityZHash, normalizedZ);
    }
}
