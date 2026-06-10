using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Enemy/EnemySO")]
public class EnemySO : ScriptableObject
{
    public string monsterName;
    public float maxHp;
    public float chaseSpeed;
    public float patrolSpeed;
    public float detectRange;
    public EnemyAttackType attackType;
    public float attackRange;
    public float attackDamage;
    public float defaultAttackCooldown;
    public float StrongAttackCooldown;
    public float StrongAttackMultiplier;

    public GameObject[] attackEffects;
}
