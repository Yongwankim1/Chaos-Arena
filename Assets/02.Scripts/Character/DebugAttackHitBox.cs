using UnityEngine;

public class DebugAttackHitBox : MonoBehaviour
{
    [SerializeField] Transform attackPos;
    [SerializeField] float Range;
    [SerializeField] float Radius;
    [SerializeField] bool isDebug;

    private void OnDrawGizmos()
    {
        if (attackPos == null) return;

        Vector3 center = attackPos.position + attackPos.forward * Range * 0.5f;

        Vector3 halfExtents = new Vector3(Radius, 1f, Range * 0.5f);
        Gizmos.color = Color.yellow;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, attackPos.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        Gizmos.matrix = oldMatrix;
    }
}
