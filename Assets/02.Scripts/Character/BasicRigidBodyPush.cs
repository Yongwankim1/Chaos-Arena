using UnityEngine;

public class BasicRigidBodyPush : MonoBehaviour
{
    public LayerMask pushLayers;
    public bool canPush = true;
    [Range(0.5f, 5f)] public float strength = 1.1f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!canPush) return;

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
            return;

        Vector3 forceDirection = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        body.AddForce(forceDirection * strength, ForceMode.Impulse);
    }
}