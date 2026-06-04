using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Fusion;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackTrigger", story: "[Self] OnAttack", category: "Action/AI", id: "df4c53580741014cbb6a8c43f5e31374")]
public partial class AttackTriggerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private IAttacker attacker;
    private NetworkObject networkObject;

    protected override Status OnStart()
    {
        if (Self?.Value == null)
            return Status.Failure;

        networkObject = Self.Value.GetComponentInParent<NetworkObject>();

        if (networkObject == null)
            return Status.Failure;

        if (!networkObject.HasStateAuthority)
            return Status.Failure;

        attacker = Self.Value.GetComponent<IAttacker>();

        if (attacker == null)
            return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self?.Value == null)
            return Status.Failure;

        if (networkObject == null)
            return Status.Failure;

        if (!networkObject.HasStateAuthority)
            return Status.Failure;

        if (attacker == null)
            return Status.Failure;

        attacker.Attack();

        return Status.Success;
    }

    protected override void OnEnd()
    {
        attacker = null;
        networkObject = null;
    }
}

