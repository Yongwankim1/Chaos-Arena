using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitRealSeconds", story: "[Self] waits [SecondsToWait] real seconds", category: "Action/Timing", id: "8a3df6070a27412d8eeb1307a635cfcb")]
public partial class WaitRealSecondsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> SecondsToWait;

    private double endTime;

    protected override Status OnStart()
    {
        endTime = Time.realtimeSinceStartupAsDouble + Mathf.Max(0f, SecondsToWait?.Value ?? 0f);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Time.realtimeSinceStartupAsDouble >= endTime ? Status.Success : Status.Running;
    }
}
