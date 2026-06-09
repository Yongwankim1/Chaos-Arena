using UnityEngine;

public class EnemyHP : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject target;
    public GameObject Target => target;
    public bool HasTarget => target != null;
    public int MaxHP = 100;
    public int currentHP = 100;
    public bool IsDead => currentHP <= 0;

    public void TakeDamage(int damage, IAttacker attacker)
    {
        target = attacker.GetAttacker();
        currentHP = Mathf.Max(currentHP - damage, 0);
        
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
    }
}
