using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TargetIsDead", story: "[Self] clears [Target] if dead and updates [HasTarget]", category: "Action/AI", id: "2f184f935313803e138aeaacbcdf4c5f")]
public partial class TargetIsDeadAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> HasTarget;

    protected override Status OnUpdate()
    {
        if (Target?.Value == null)
        {
            return Status.Success;
        }

        PlayerCharacter character = Target.Value.GetComponentInParent<PlayerCharacter>();
        if (character == null)
        {
            return Status.Success;
        }

        if (character.IsDead)
        {
            Target.Value = null;
            SetHasTarget(false);

            EnemyHP enemyHP = Self?.Value != null
                ? Self.Value.GetComponent<EnemyHP>()
                : null;

            enemyHP?.ClearTarget();
        }

        return Status.Success;
    }

    private void SetHasTarget(bool value)
    {
        if (HasTarget != null)
        {
            HasTarget.Value = value;
        }
    }
}

