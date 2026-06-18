using Fusion;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
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

        SetupSpawnedEnemy(mage, magePosition, magePatrols);
        SetupSpawnedEnemy(knight, knightPosition, knightPatrols);
    }
    private void SetupSpawnedEnemy(NetworkObject enemy, Vector3 spawnPosition, Transform[] patrols)
    {
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

        enemy.GetComponent<EnemyBehaviorBridge>().Init(patrols);

        if (behavior != null)
        {
            behavior.enabled = true;
        }
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
