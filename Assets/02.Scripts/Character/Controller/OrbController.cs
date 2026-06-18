using UnityEngine;

public class OrbController : MonoBehaviour
{
    [SerializeField] private GameObject[] orbs = new GameObject[3];
    [SerializeField] private Transform targetPos;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private float height = 1.2f;
    [SerializeField] private float orbitSpeed = 180f;
    [SerializeField] private float selfRotateSpeed = 360f;
    [SerializeField] private float orbitTilt = 35f;
    [SerializeField] private bool faceCenter = true;

    private float angle;
    private float selfAngle;
    private static readonly Vector3[] OrbitAxes =
    {
        new Vector3(0.15f, 1f, 0.35f).normalized,
        new Vector3(0.65f, 0.8f, -0.2f).normalized,
        new Vector3(-0.45f, 0.9f, 0.55f).normalized
    };

    private void Update()
    {
        if (targetPos == null || orbs == null || orbs.Length == 0)
            return;

        angle += orbitSpeed * Time.deltaTime;
        selfAngle += selfRotateSpeed * Time.deltaTime;
        UpdateOrbs();
    }

    private void UpdateOrbs()
    {
        int activeOrbCount = 0;

        foreach (GameObject orb in orbs)
        {
            if (orb != null)
                activeOrbCount++;
        }

        if (activeOrbCount == 0)
            return;

        int orbIndex = 0;

        foreach (GameObject orb in orbs)
        {
            if (orb == null)
                continue;

            float orbAngle = angle + (360f / activeOrbCount * orbIndex);
            Vector3 baseOffset = Quaternion.Euler(0f, 360f / activeOrbCount * orbIndex, 0f) * Vector3.forward * radius;
            Vector3 orbitAxis = OrbitAxes[orbIndex % OrbitAxes.Length];
            Quaternion tiltedOrbit = Quaternion.AngleAxis(orbitTilt, targetPos.forward);
            Vector3 offset = Quaternion.AngleAxis(orbAngle, tiltedOrbit * orbitAxis) * baseOffset;
            Vector3 position = targetPos.position + offset;
            position.y += height;

            Quaternion rotation = faceCenter
                ? Quaternion.LookRotation((targetPos.position + Vector3.up * height - position).normalized, Vector3.up)
                : orb.transform.rotation;

            if (selfRotateSpeed != 0f)
                rotation *= Quaternion.Euler(0f, selfAngle, 0f);

            orb.transform.SetPositionAndRotation(position, rotation);
            orbIndex++;
        }
    }
}
