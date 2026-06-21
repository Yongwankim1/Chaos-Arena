using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageVignette : MonoBehaviour
{
    private Volume _volume;
    private Vignette _vignette;

    private float _targetIntensity;
    private float _currentIntensity;

    [SerializeField]
    private float maxIntensity = 0.6f;

    [SerializeField]
    private float fadeSpeed = 2f;

    private void Awake()
    {
        _volume = FindFirstObjectByType<Volume>();

        if (_volume == null)
        {
            Debug.LogError("Volume Not Found");
            enabled = false;
            return;
        }

        if (!_volume.profile.TryGet(out _vignette))
        {
            Debug.LogError("Vignette Not Found");
            enabled = false;
            return;
        }

        _vignette.color.value = Color.red;
        _vignette.intensity.value = 0f;
    }

    private void Update()
    {
        _currentIntensity =Mathf.MoveTowards(_currentIntensity,_targetIntensity,fadeSpeed * Time.deltaTime);

        _targetIntensity = Mathf.MoveTowards(_targetIntensity,0f,fadeSpeed * Time.deltaTime);

        _vignette.intensity.value = _currentIntensity;
    }

    public void TakeDamage()
    {
        _targetIntensity = maxIntensity;
    }
}