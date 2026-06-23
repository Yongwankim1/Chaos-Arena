using Fusion;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] bool spawnMage = true;
    [SerializeField] bool spawnKnight = true;

    [SerializeField] GameObject enemyMagePrefab;
    [SerializeField] GameObject enemyKnightPrefab;

    [SerializeField] Transform mageSpawnPos;
    [SerializeField] Transform knightSpawnPos;

    private NetworkObject mage;
    private NetworkObject knight;

    [SerializeField] Transform[] magePatrols = new Transform[5];
    [SerializeField] Transform[] knightPatrols = new Transform[5];
    private Vector3 GetSpawnPositionOnNavMesh(Vector3 origin)
    {
        return origin;
    }
    public void Spawn(NetworkRunner runner)
    {
        if (runner == null) return;
        if (!runner.IsServer) return;
        if (!spawnMage && !spawnKnight) return;

        if (spawnMage)
        {
            mage = SpawnEnemy(runner, enemyMagePrefab, mageSpawnPos, magePatrols);
        }

        if (spawnKnight)
        {
            knight = SpawnEnemy(runner, enemyKnightPrefab, knightSpawnPos, knightPatrols);
        }
    }
    private NetworkObject SpawnEnemy(NetworkRunner runner, GameObject prefabObject, Transform spawnPos, Transform[] patrols)
    {
        if (prefabObject == null) return null;
        if (spawnPos == null) return null;

        NetworkObject prefab = prefabObject.GetComponent<NetworkObject>();
        if (prefab == null) return null;

        Vector3 spawnPosition = GetSpawnPositionOnNavMesh(spawnPos.position);
        NetworkObject enemy = runner.Spawn(prefab, spawnPosition, Quaternion.identity);

        SetupSpawnedEnemy(enemy, spawnPosition, patrols);
        return enemy;
    }
    private void SetupSpawnedEnemy(NetworkObject enemy, Vector3 spawnPosition, Transform[] patrols)
    {
        if (enemy == null) return;

        BehaviorGraphAgent behavior = enemy.GetComponent<BehaviorGraphAgent>();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (behavior != null)
        {
            behavior.enabled = false;
        }

        if (agent != null)
        {
            agent.Warp(spawnPosition);

            if (!agent.isOnNavMesh)
            {
                Debug.LogError($"Enemy spawned off NavMesh: {enemy.name}, position: {spawnPosition}");
                return;
            }
        }

        EnemyBehaviorBridge bridge = enemy.GetComponent<EnemyBehaviorBridge>();
        if (bridge != null)
        {
            bridge.Init(patrols);
        }

        if (behavior != null)
        {
            behavior.enabled = true;
        }
    }
    public void ReSpawn(NetworkRunner runner)
    {
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
