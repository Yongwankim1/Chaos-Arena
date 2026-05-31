using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Enemy/EnemySO")]
public class EnemySO : ScriptableObject
{
    public int MaxHP;
    public int Damage;
    public int Defense;

    public GameObject Effect;
}
