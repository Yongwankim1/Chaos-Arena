using Fusion;
using UnityEngine;

public class MageLaserAttack : NetworkBehaviour
{
    [SerializeField] LineRenderer[] lines = new LineRenderer[2];
    [SerializeField] bool isDebug = true;
    [SerializeField] float attackDistance = 20f;
    [SerializeField] float radius = 1f;
    [SerializeField] int damage = 10;
    [SerializeField] float damageInterval = 0.25f;
    [SerializeField] LayerMask targetMask;

    private IAttacker attacker;
    private TickTimer damageTimer;
    public void Destroy()
    {
        Runner.Despawn(Object);
        Destroy(gameObject);
    }
    public void Init(IAttacker attacker)
    {
        this.attacker = attacker;
    }

    public override void Render()
    {
        UpdateLines();
    }

    private void UpdateLines()
    {
        Vector3 startPoint = transform.position;
        Vector3 endPoint = transform.position + transform.forward * attackDistance;

        foreach (LineRenderer line in lines)
        {
            if (line == null)
                continue;

            line.positionCount = 2;
            line.SetPosition(0, startPoint);
            line.SetPosition(1, endPoint);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!damageTimer.ExpiredOrNotRunning(Runner))
            return;

        DealDamage();

        damageTimer = TickTimer.CreateFromSeconds(Runner, damageInterval);
    }

    private void DealDamage()
    {
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + transform.forward * attackDistance;

        Collider[] hits = Physics.OverlapCapsule(
            point1,
            point2,
            radius,
            targetMask
        );

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if (hit.TryGetComponent(out IAttacker hitAttacker))
            {
                if (hitAttacker == attacker)
                    continue;
            }

            damageable.TakeDamage(damage, attacker);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isDebug) return;

        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + transform.forward * attackDistance;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(point1, radius);
        Gizmos.DrawWireSphere(point2, radius);

        Vector3 right = transform.right * radius;
        Vector3 up = transform.up * radius;

        Gizmos.DrawLine(point1 + right, point2 + right);
        Gizmos.DrawLine(point1 - right, point2 - right);
        Gizmos.DrawLine(point1 + up, point2 + up);
        Gizmos.DrawLine(point1 - up, point2 - up);
    }

}
