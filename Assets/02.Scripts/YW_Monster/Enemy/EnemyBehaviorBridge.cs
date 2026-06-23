using Fusion;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviorBridge : NetworkBehaviour, ISlowable
{
    [SerializeField] EnemySO dataSO;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float proxyPositionLerp = 20f;
    [SerializeField] private float proxyRotationLerp = 20f;

    private NavMeshAgent navMeshAgent;
    private BehaviorGraphAgent behaviorGraphAgent;
    private bool? lastCanRunAi;
    private bool proxyTransformInitialized;
    private TickTimer slowTimer;
    private float slowPercent;

    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }

    public EnemySO Config => dataSO;
    public Transform[] PatrolPoints => patrolPoints;

    public bool HasConfig => dataSO != null;
    public bool HasPatrolPoints => patrolPoints != null && patrolPoints.Length > 0;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();

        TryPopulatePatrolPointsFromScene();
    }
    public void Init(Transform[] patrolPoints)
    {
        this.patrolPoints = patrolPoints;
    }
    public override void Spawned()
    {
        ApplyAuthorityState();
        UpdateSlow();

        if (Object == null || Object.HasStateAuthority)
        {
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }
    }

    public override void FixedUpdateNetwork()
    {

        ApplyAuthorityState();
        UpdateSlow();

        if (Object == null || Object.HasStateAuthority)
        {
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }
    }

    public override void Render()
    {
        if (Object == null || Object.HasStateAuthority)
        {
            return;
        }

        if (!proxyTransformInitialized)
        {
            transform.SetPositionAndRotation(NetworkPosition, NetworkRotation);
            proxyTransformInitialized = true;
            return;
        }

        float positionT = 1f - Mathf.Exp(-proxyPositionLerp * Time.deltaTime);
        float rotationT = 1f - Mathf.Exp(-proxyRotationLerp * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, NetworkPosition, positionT);
        transform.rotation = Quaternion.Slerp(transform.rotation, NetworkRotation, rotationT);
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

            if (canRunAi)
            {
                ApplyConfigToAgent();
            }
        }
    }

    private void ApplyConfigToAgent()
    {
        if (navMeshAgent == null || dataSO == null)
        {
            return;
        }

        ApplyAgentSpeed();
    }

    private void TryPopulatePatrolPointsFromScene()
    {
        if (HasPatrolPoints)
        {
            return;
        }

        // Auto-populate patrol points from the scene when none are assigned.
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


    private void UpdateSlow()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        if (!slowTimer.IsRunning || !slowTimer.Expired(Runner))
            return;

        slowTimer = TickTimer.None;
        slowPercent = 0f;
        ApplyAgentSpeed();
    }

    private void ApplyAgentSpeed()
    {
        if (navMeshAgent == null || dataSO == null)
            return;

        navMeshAgent.speed = dataSO.chaseSpeed * (1f - slowPercent);
    }

    public void ApplySlow(float percent, float duration)
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        if (duration <= 0f)
            return;

        slowPercent = Mathf.Clamp01(percent);
        slowTimer = TickTimer.CreateFromSeconds(Runner, duration);
        ApplyAgentSpeed();
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
    public GameObject GetEffect(EffectType effectType)
    {
        if (dataSO.attackEffects.Length == 0 || (int)effectType > dataSO.attackEffects.Length) return null;

        return dataSO.attackEffects[(int)effectType];
    }
}
