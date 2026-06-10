using UnityEngine;

[RequireComponent(typeof(EnemyBehaviorBridge))]
public class EnemyDrawGizmoDebug : MonoBehaviour
{
    [SerializeField] EnemyBehaviorBridge bridge;
    [SerializeField] bool isDebug = true;
    private void Awake()
    {
        if(bridge == null)
            bridge = GetComponent<EnemyBehaviorBridge>();
    }

    private void OnDrawGizmos()
    {
        if (bridge == null|| !isDebug) return;
        //DetectRange
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bridge.Config.detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bridge.Config.attackRange);
    }
}
