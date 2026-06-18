using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Play(Animator animator, float duration)
    {
        if (animator == null)
            return;

        StartCoroutine(HitStopRoutine(animator, duration));
    }

    private IEnumerator HitStopRoutine(Animator animator, float duration)
    {
        float originSpeed = animator.speed;

        animator.speed = 0f;

        yield return new WaitForSecondsRealtime(duration);

        if (animator != null)
        {
            animator.speed = originSpeed;
        }
    }
}