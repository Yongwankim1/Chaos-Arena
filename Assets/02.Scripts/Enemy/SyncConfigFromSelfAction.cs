using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SyncConfigFromSelfAction", story: "[Self] updates config", category: "Action", id: "fc86be8da164e4a926968cd95c485ca7")]
public partial class SyncConfigFromSelfAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<float> AttackRange;

    [SerializeReference] public BlackboardVariable<float> DetectRadius;

    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;

    [SerializeReference] public BlackboardVariable<float> PatrolSpeed;

    [SerializeReference] public BlackboardVariable<float> AttackCooldown;

    [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPoints;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null) return Status.Failure;

        EnemyBehaviorBridge bridge = Self.Value.GetComponent<EnemyBehaviorBridge>();

        if(bridge == null || !bridge.HasConfig) return Status.Failure;

        EnemySO config = bridge.Config;

        AttackRange.Value = config.attackRange;
        DetectRadius.Value = config.detectRange;
        ChaseSpeed.Value = config.chaseSpeed;
        PatrolSpeed.Value = config.patrolSpeed;
        AttackCooldown.Value = config.attackCooldown;

        PatrolPoints.Value = bridge.GetPatrolPosition();

        return Status.Success;
    }

}

