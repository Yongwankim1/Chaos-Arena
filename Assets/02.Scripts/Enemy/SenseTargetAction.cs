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

        // TODO: 데미지/피격 코드에서 나를 때린 플레이어를 Target.Value에 넣어준다.
        // 이 액션은 더 이상 가까운 플레이어를 먼저 찾지 않고, 피격으로 지정된 Target만 감지한다.
        GameObject target = Self?.Value.GetComponent<EnemyHP>().Target;

        if (target != null) Target.Value = target;

        PlayerController hitAttackerTarget = Target?.Value != null
            ? Target.Value.GetComponent<PlayerController>()
            : null;

        if (hitAttackerTarget == null || hitAttackerTarget.IsDead)
        {
            ClearTarget();
            return Status.Success;
        }

        Vector3 selfPosition = Self.Value.transform.position;
        Vector3 targetPosition = hitAttackerTarget.transform.position;
        float distance = Vector3.Distance(selfPosition, targetPosition);

        if (distance > detectRange)
        {
            ClearTarget();
            return Status.Success;
        }

        SetTarget(hitAttackerTarget.gameObject, distance, targetPosition);

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

        if (HasTarget != null)
        {
            HasTarget.Value = false;
        }

        if (HasLastKnownPosition != null)
        {
            HasLastKnownPosition.Value = false;
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

        if (HasTarget != null)
        {
            HasTarget.Value = true;
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

