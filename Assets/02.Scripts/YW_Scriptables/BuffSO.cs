using UnityEngine;

[CreateAssetMenu(fileName = "BuffSO", menuName = "Buff/BuffSO")]
public class BuffSO : ScriptableObject
{
    [SerializeField] private float value;
    [SerializeField] private float duration;
    [SerializeField] private BuffType type;
    [SerializeField] private GameObject effect;
    [SerializeField] private float attackBonusPercent;
    [SerializeField] private float slowPercent;
    [SerializeField] private float slowDuration;
    public float AttackBonusPercent => attackBonusPercent;
    public float SlowPercent => slowPercent;
    public float SlowDuration => slowDuration;

    public float Value => value;
    public float Duration => duration;
    public BuffType Type => type;
    public GameObject Effect => effect;
}
