using UnityEngine;

[CreateAssetMenu(fileName = "BuffSO", menuName = "Buff/BuffSO")]
public class BuffSO : ScriptableObject
{
    public float Value;
    public BuffType type;
    public GameObject Effect;
}
