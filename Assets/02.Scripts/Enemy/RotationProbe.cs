using UnityEngine;

public class RotationProbe : MonoBehaviour
{
    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        Debug.Log($"Update Set: {name} / {transform.eulerAngles}");
    }

    private void LateUpdate()
    {
        Debug.Log($"Late Check: {name} / {transform.eulerAngles}");
    }
}