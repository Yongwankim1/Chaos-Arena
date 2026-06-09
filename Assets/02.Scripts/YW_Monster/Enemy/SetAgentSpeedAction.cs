using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetAgentSpeedAction", story: "Set [Self] agent speed to [Speed]", category: "Action", id: "9f8d0ae00ff648ff9e588170e0bdffe2")]
public partial class SetAgentSpeedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Speed;

    // 실행 중인 노드의 소유 GameObject를 사용해 개체별 Self를 정확히 복구한다.
    private GameObject ResolveFallbackSelf()
    {
        return GameObject;
    }

    protected override Status OnUpdate()
    {
        if (Self != null && Self.Value == null)
        {
            Self.Value = ResolveFallbackSelf();
        }

        if (Self?.Value == null)
        {
            Debug.LogWarning("[EnemyAI][Unknown] SetAgentSpeedAction FAILED | Self is null (fallback failed)");
            return Status.Failure;
        }

        NavMeshAgent agent = Self.Value.GetComponent<NavMeshAgent>();
        if (agent == null || !agent.isOnNavMesh)
        {
            return Status.Failure;
        }
        agent.isStopped = false;
        agent.speed = Speed.Value;

        return Status.Success;
    }
}

