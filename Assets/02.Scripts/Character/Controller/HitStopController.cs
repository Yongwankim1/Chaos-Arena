using UnityEngine;

public class HitStopController : MonoBehaviour
{
    private Animator _animator;
    private float _remainTime;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_remainTime <= 0f)
            return;

        _remainTime -= Time.unscaledDeltaTime;

        if (_remainTime <= 0f && _animator != null)
        {
            _animator.speed = 1f;
        }
    }

    public void Play(float duration)
    {
        if (_animator == null)
            return;

        if (duration <= _remainTime)
            return;

        _remainTime = duration;
        _animator.speed = 0f;
    }

    private void OnDisable()
    {
        if (_animator != null)
        {
            _animator.speed = 1f;
        }

        _remainTime = 0f;
    }
}