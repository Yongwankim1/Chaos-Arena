using UnityEngine;

[System.Serializable]
public class RoundData
{
    public float PreparationTime = 20f;

    public float RoundTime = 300f;
}

[CreateAssetMenu(fileName = "RoundRuleData", menuName = "Game Data/Round Rule Data")]
public class RoundRuleData : ScriptableObject
{
    public RoundData[] Rounds;
}