using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] GameObject enemyMagePrefab;
    [SerializeField] GameObject enemyKnightPrefab;

    [SerializeField] Transform mageSpawnPos;
    [SerializeField] Transform knightSpawnPos;

    [SerializeField] private float navMeshSearchRadius = 5f;

    private NetworkObject mage;
    private NetworkObject knight;

    [SerializeField] Transform[] magePatrols = new Transform[5];
    [SerializeField] Transform[] knightPatrols = new Transform[5];
    private Vector3 GetSpawnPositionOnNavMesh(Vector3 origin)
    {
        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return origin;
    }
    public void Spawn(NetworkRunner runner)
    {
        if (enemyMagePrefab == null) return;
        if (enemyKnightPrefab == null) return;
        if (runner == null) return;
        if (!runner.IsServer) return;

        NetworkObject magePrefab = enemyMagePrefab.GetComponent<NetworkObject>();
        NetworkObject knightPrefab = enemyKnightPrefab.GetComponent<NetworkObject>();

        if (magePrefab == null) return;
        if (knightPrefab == null) return;

        Vector3 magePosition = GetSpawnPositionOnNavMesh(mageSpawnPos.position);
        Vector3 knightPosition = GetSpawnPositionOnNavMesh(knightSpawnPos.position);

        mage = runner.Spawn(magePrefab, magePosition, Quaternion.identity);
        knight = runner.Spawn(knightPrefab, knightPosition, Quaternion.identity);

        mage.GetComponent<EnemyBehaviorBridge>().Init(magePatrols);
        knight.GetComponent<EnemyBehaviorBridge>().Init(knightPatrols);
    }

    public void ReSpawn(NetworkRunner runner)
    {
        if (enemyMagePrefab == null) return;
        if (enemyKnightPrefab == null) return;
        if (runner == null) return;
        if (!runner.IsServer) return;

        if (mage != null && mage.IsValid)
        {
            runner.Despawn(mage);
        }

        if (knight != null && knight.IsValid)
        {
            runner.Despawn(knight);
        }

        mage = null;
        knight = null;

        Spawn(runner);
    }
 }
