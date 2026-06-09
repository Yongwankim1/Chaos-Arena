using System.Collections;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float velocity = 10f;
    [SerializeField] private Rigidbody rb;

    private Coroutine fireRoutine;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    public void Init(float fireDealy = 0f)
    {

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (fireRoutine != null)
            StopCoroutine(fireRoutine);

        fireRoutine = StartCoroutine(FireDelayRoutine(fireDealy));
    }

    private IEnumerator FireDelayRoutine(float fireDelay)
    {
        yield return new WaitForSeconds(fireDelay);

        rb.linearVelocity = transform.forward * velocity;

        Destroy(gameObject, 3f);
    }
}