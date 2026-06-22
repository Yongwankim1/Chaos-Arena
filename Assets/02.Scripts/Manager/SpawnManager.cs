using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance
    {
        get;
        private set;
    }

    [SerializeField] private List<Transform> blueSpawnPoints = new List<Transform>();

    [SerializeField] private List<Transform> redSpawnPoints = new List<Transform>();

    private int _blueSpawnIndex;
    private int _redSpawnIndex;

    private void Awake()
    {
        Instance = this;
    }
    public void ResetSpawnIndex()
    {
        _blueSpawnIndex = 0;
        _redSpawnIndex = 0;
    }

    public Vector3 GetSpawnPosition(TeamType team)
    {
        List<Transform> targetList = team == TeamType.Blue ? blueSpawnPoints : redSpawnPoints;

        if (targetList.Count == 0)
        {
            Debug.LogError($"No Spawn Point : {team}");

            return Vector3.zero;
        }

        int spawnIndex;

        if (team == TeamType.Blue)
        {
            spawnIndex = Mathf.Clamp(_blueSpawnIndex, 0, targetList.Count - 1);

            _blueSpawnIndex++;
        }
        else
        {
            spawnIndex = Mathf.Clamp(_redSpawnIndex, 0, targetList.Count - 1);

            _redSpawnIndex++;
        }

        return targetList[spawnIndex].position;
    }
    public Vector3 GetSpawnPosition(TeamType team, int slotIndex)
    {
        List<Transform> targetList = team == TeamType.Blue ? blueSpawnPoints : redSpawnPoints;

        if (targetList.Count == 0)
        {
            Debug.LogError($"No Spawn Point : {team}");

            return Vector3.zero;
        }

        slotIndex = Mathf.Clamp(slotIndex, 0, targetList.Count - 1);

        return targetList[slotIndex].position;
    }
}