using UnityEngine;
using UnityEngine.AI;

public static class NavMeshReachability
{
    public static bool CanReach(NavMeshAgent agent, Vector3 target, float sampleRadius = 2f)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        return CanReach(agent.transform.position, target, sampleRadius, agent.areaMask);
    }

    public static bool CanReach(Vector3 from, Vector3 target, float sampleRadius = 2f)
    {
        return CanReach(from, target, sampleRadius, NavMesh.AllAreas);
    }

    public static bool CanReach(Vector3 from, Vector3 target, float sampleRadius, int areaMask)
    {
        if (!NavMesh.SamplePosition(from, out var fromHit, sampleRadius, areaMask))
            return false;

        if (!NavMesh.SamplePosition(target, out var targetHit, sampleRadius, areaMask))
            return false;

        var path = new NavMeshPath();

        if (!NavMesh.CalculatePath(fromHit.position, targetHit.position, areaMask, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }
}
