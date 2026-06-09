using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour
{
    [SerializeField] protected float velocity = 10f;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected float fireDelay = 0f;
    protected void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    public virtual void Init()
    {
        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

    }
}
