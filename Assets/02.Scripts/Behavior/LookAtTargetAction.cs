using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LookAtTarget", story: "[Self] Look at [Target]", category: "Action/AI", id: "08efe40680b2baf19c49361a0d29c65d")]
public partial class LookAtTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> TurnSpeed;
    [SerializeReference] public BlackboardVariable<float> AngleTolerance;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || Target?.Value == null)
        {
            return Status.Failure;
        }

        Transform selfTransform = Self.Value.transform;
        Vector3 targetPosition = Target.Value.transform.position;
        targetPosition.y = selfTransform.position.y;

        Vector3 direction = targetPosition - selfTransform.position;
        if (direction.sqrMagnitude <= 0.001f)
        {
            return Status.Success;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        float turnSpeed = TurnSpeed?.Value > 0f ? TurnSpeed.Value : 720f;
        float angleTolerance = AngleTolerance?.Value > 0f ? AngleTolerance.Value : 3f;

        selfTransform.rotation = Quaternion.RotateTowards(
            selfTransform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);

        float angle = Quaternion.Angle(selfTransform.rotation, targetRotation);
        return angle <= angleTolerance ? Status.Success : Status.Running;
    }

}

