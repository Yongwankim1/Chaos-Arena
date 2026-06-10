using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance
    {
        get;
        private set;
    }

    [SerializeField]
    private List<Transform> blueSpawnPoints =
        new List<Transform>();

    [SerializeField]
    private List<Transform> redSpawnPoints =
        new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSpawnPosition(
        TeamType team)
    {
        List<Transform> targetList =
            team == TeamType.Blue
            ? blueSpawnPoints
            : redSpawnPoints;

        if (targetList.Count == 0)
        {
            Debug.LogError(
                $"No Spawn Point : {team}");

            return Vector3.zero;
        }

        int randomIndex =
            Random.Range(
                0,
                targetList.Count);

        return targetList[randomIndex].position;
    }
}