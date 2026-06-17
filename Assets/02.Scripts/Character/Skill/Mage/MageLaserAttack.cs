using Fusion;
using UnityEngine;

public class MageLaserAttack : NetworkBehaviour
{
    [SerializeField] LineRenderer[] lines = new LineRenderer[2];
    [SerializeField] bool isDebug = true;
    [SerializeField] float attackDistance = 20f;
    public void Init(IAttacker attacker)
    {
        
    }


    private void OnDrawGizmos()
    {
        if (!isDebug) return;
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * attackDistance);
    }
}
