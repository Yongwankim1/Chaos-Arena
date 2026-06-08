using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NonStop", story: "[Self] Non Stop", category: "Action/AI", id: "6b703b84f2c23c7c45379c9eef628d85")]
public partial class NonStopAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<GameObject> Target;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(Self?.Value == null || Target?.Value == null) return Status.Failure;
        NavMeshAgent agent = Self?.Value.GetComponent<NavMeshAgent>();
        if(agent == null ) return Status.Failure;

        agent.isStopped = false;
        Transform self = Self?.Value.transform;
        self.LookAt(Target?.Value.transform);
        agent.ResetPath();

        return Status.Success;
    }
}

