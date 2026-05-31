using System;
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

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

}

