using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera targetCamera;

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        transform.forward = targetCamera.transform.forward;
    }
}
