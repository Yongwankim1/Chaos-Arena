using UnityEngine;

public class BruteBuffEffect : MonoBehaviour
{
    [SerializeField]
    private GameObject effectRoot;

    private ParticleSystem[] _particles;

    private void Awake()
    {
        if (effectRoot == null)
            return;

        _particles =
            effectRoot.GetComponentsInChildren<ParticleSystem>(
                true);

        effectRoot.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        if (effectRoot == null)
            return;

        effectRoot.SetActive(visible);

        if (_particles == null)
            return;

        foreach (ParticleSystem particle in _particles)
        {
            if (particle == null)
                continue;

            if (visible)
            {
                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear);

                particle.Play(true);
            }
            else
            {
                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
