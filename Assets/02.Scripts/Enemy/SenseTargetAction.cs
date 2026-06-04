using System;
using Fusion;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SenseTargetAction", story: "[Self] senses [Target] and updates", category: "Action", id: "bb0d18137790d8b35db6177a1555593a")]
public partial class SenseTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> CanSeeTarget;
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget;
    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPosition;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<bool> HasTarget;
    protected override Status OnUpdate()
    {
        if (Self?.Value == null)
        {
            ClearTarget();
            return Status.Failure;
        }

        NetworkObject networkObject = Self.Value.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.HasStateAuthority)
        {
            ClearTarget();
            return Status.Failure;
        }

        float detectRange = GetDetectRange();
        PlayerController closestTarget = FindClosestAlivePlayer(detectRange);

        if (closestTarget == null)
        {
            ClearTarget();
            return Status.Success;
        }

        Vector3 selfPosition = Self.Value.transform.position;
        Vector3 targetPosition = closestTarget.transform.position;
        float distance = Vector3.Distance(selfPosition, targetPosition);

        SetTarget(closestTarget.gameObject, distance, targetPosition);

        return Status.Success;
    }

    private float GetDetectRange()
    {
        EnemyBehaviorBridge bridge = Self.Value.GetComponent<EnemyBehaviorBridge>();
        if (bridge != null && bridge.HasConfig)
        {
            return bridge.Config.detectRange;
        }

        return Mathf.Infinity;
    }

    private PlayerController FindClosestAlivePlayer(float detectRange)
    {
        PlayerController closestPlayer = null;
        float closestSqrDistance = detectRange * detectRange;
        Vector3 selfPosition = Self.Value.transform.position;
        PlayerController[] players = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in players)
        {
            if (player == null || player.IsDead)
            {
                continue;
            }

            float sqrDistance = (player.transform.position - selfPosition).sqrMagnitude;
            if (sqrDistance > closestSqrDistance)
            {
                continue;
            }

            closestPlayer = player;
            closestSqrDistance = sqrDistance;
        }

        return closestPlayer;
    }

    private void ClearTarget()
    {
        if (Target != null)
        {
            Target.Value = null;
        }

        if (CanSeeTarget != null)
        {
            CanSeeTarget.Value = false;
        }

        if (DistanceToTarget != null)
        {
            DistanceToTarget.Value = Mathf.Infinity;
        }
    }

    private void SetTarget(GameObject target, float distance, Vector3 targetPosition)
    {
        if (Target != null)
        {
            Target.Value = target;
        }

        if (CanSeeTarget != null)
        {
            CanSeeTarget.Value = true;
        }

        if (DistanceToTarget != null)
        {
            DistanceToTarget.Value = distance;
        }

        if (TargetPosition != null)
        {
            TargetPosition.Value = targetPosition;
        }

        if (LastKnownPosition != null)
        {
            LastKnownPosition.Value = targetPosition;
        }

        if (HasLastKnownPosition != null)
        {
            HasLastKnownPosition.Value = true;
        }
    }
}

