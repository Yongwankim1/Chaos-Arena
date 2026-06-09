using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopAgent", story: "Stop [Self]", category: "Action", id: "6e991fb7e6b58aab78339b29ce67f864")]
public partial class StopAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

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
            Debug.LogWarning("[EnemyAI][Unknown] StopAgentAction FAILED | Self is null (fallback failed)");
            return Status.Failure;
        }
        UnityEngine.AI.NavMeshAgent agent = Self.Value.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null || !agent.isOnNavMesh)
        {
            return Status.Failure;
        }
        agent.isStopped = true;
        agent.ResetPath();
        return Status.Success;
    }
}

