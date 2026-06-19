using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "HealWhenNoTarget", story: "[Self] heals when no target", category: "Action", id: "d4b6e3d3f9194af295e2a7e88f93af10")]
public partial class HealWhenNoTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> HasTarget;

    private float nextHealTime;

    protected override Status OnStart()
    {
        nextHealTime = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self?.Value == null)
        {
            return Status.Failure;
        }

        EnemyHP enemyHP = Self.Value.GetComponent<EnemyHP>();
        if (enemyHP == null || enemyHP.IsDead)
        {
            return Status.Failure;
        }

        if ((HasTarget != null && HasTarget.Value) || enemyHP.currentHP >= enemyHP.MaxHP)
        {
            nextHealTime = Time.time + enemyHP.HealInterval;
            return Status.Running;
        }

        if (nextHealTime <= 0f)
        {
            nextHealTime = Time.time + enemyHP.HealInterval;
            return Status.Running;
        }

        if (Time.time >= nextHealTime)
        {
            enemyHP.HealByPercent(enemyHP.HealPercent);
            nextHealTime = Time.time + enemyHP.HealInterval;
        }

        return Status.Running;
    }
}
