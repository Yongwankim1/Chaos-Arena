using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [SerializeField]
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float force)
    {
        if (impulseSource == null)
            return;

        impulseSource.GenerateImpulse(Vector3.forward * force);
    }
}