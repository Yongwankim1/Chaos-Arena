using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "If True", story: "[Value] equal [IsTrue]", category: "Action/if - true", id: "4353c0a5fc4bfb39d5c2c9ff10e96e85")]
public partial class IfTrueAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Value;
    [SerializeReference] public BlackboardVariable<bool> IsTrue;

    protected override Status OnUpdate()
    {
        if (Value != IsTrue) return Status.Failure;

        return Status.Success;
    }
}

