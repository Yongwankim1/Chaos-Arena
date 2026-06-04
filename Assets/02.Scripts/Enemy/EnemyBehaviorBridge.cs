using Fusion;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviorBridge : NetworkBehaviour
{
    [SerializeField] EnemySO dataSD;
    [SerializeField] private Transform[] patrolPoints;

    private NavMeshAgent navMeshAgent;
    private BehaviorGraphAgent behaviorGraphAgent;
    private bool? lastCanRunAi;

    public EnemySO Config => dataSD;
    public Transform[] PatrolPoints => patrolPoints;

    public bool HasConfig => dataSD != null;
    public bool HasPatrolPoints => patrolPoints != null && patrolPoints.Length > 0;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();

        TryPopulatePatrolPointsFromScene();
    }

    public override void Spawned()
    {
        ApplyAuthorityState();
    }

    public override void FixedUpdateNetwork()
    {
        ApplyAuthorityState();
    }

    private void ApplyAuthorityState()
    {
        bool canRunAi = Object == null || Object.HasStateAuthority;
        if (lastCanRunAi == canRunAi)
        {
            return;
        }

        lastCanRunAi = canRunAi;

        if (behaviorGraphAgent != null)
        {
            behaviorGraphAgent.enabled = canRunAi;
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = canRunAi;
        }
    }

    private void TryPopulatePatrolPointsFromScene()
    {
        if (HasPatrolPoints)
        {
            return;
        }

        // 수동 할당이 비어 있으면 scene의 PatrolPoint 루트를 기준으로 자동 수집한다.
        GameObject patrolRootObject = GameObject.Find("PatrolPoint");
        if (patrolRootObject == null)
        {
            return;
        }

        Transform patrolRoot = patrolRootObject.transform;
        if (patrolRoot.childCount <= 0)
        {
            return;
        }

        Transform[] discoveredPoints = new Transform[patrolRoot.childCount];
        for (int childIndex = 0; childIndex < patrolRoot.childCount; childIndex++)
        {
            discoveredPoints[childIndex] = patrolRoot.GetChild(childIndex);
        }

        patrolPoints = discoveredPoints;
    }

    public Vector3 GetPatrolPosition(int index)
    {
        if (!HasPatrolPoints)
            return transform.position;

        int safeIndex = Mathf.Abs(index) % patrolPoints.Length;
        Transform point = patrolPoints[safeIndex];
        return point != null ? point.position : transform.position;
    }

    public List<GameObject> GetPatrolPosition()
    {
        List<GameObject> ret = new List<GameObject>();

        foreach(Transform point in patrolPoints )
        {
            GameObject go = point.gameObject;
            ret.Add( go );
        }
        return ret;
    }
}
