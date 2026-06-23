using System.Collections;
using System;
using Fusion;
using UnityEngine;

public class NetFadeInFadeOut : NetworkBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private float defaultFadeDuration = 0.5f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (cg == null)
        {
            cg = GetComponent<CanvasGroup>();
        }
    }

    private void OnValidate()
    {
        if (cg == null)
        {
            cg = GetComponent<CanvasGroup>();
        }
    }

    public void FadeIn()
    {
        FadeIn(defaultFadeDuration);
    }

    public void FadeIn(float duration)
    {
        RPC_PlayFade(0f, 1f, duration, true, false);
    }

    public void FadeOut()
    {
        FadeOut(defaultFadeDuration);
    }

    public void FadeOut(float duration)
    {
        RPC_PlayFade(1f, 0f, duration, false, true);
    }

    public void FadeOutIn()
    {
        FadeOutIn(defaultFadeDuration, defaultFadeDuration);
    }

    public void FadeOutIn(float fadeOutDuration, float fadeInDuration)
    {
        RPC_PlayFadeOutIn(fadeOutDuration, fadeInDuration);
    }

    public void LocalFadeIn()
    {
        LocalFadeIn(defaultFadeDuration);
    }

    public void LocalFadeIn(float duration, Action onComplete = null)
    {
        StartFade(0f, 1f, duration, true, false, onComplete);
    }

    public void LocalFadeOut()
    {
        LocalFadeOut(defaultFadeDuration);
    }

    public void LocalFadeOut(float duration, Action onComplete = null)
    {
        StartFade(1f, 0f, duration, false, true, onComplete);
    }

    public void LocalFadeOutIn()
    {
        LocalFadeOutIn(defaultFadeDuration, defaultFadeDuration);
    }

    public void LocalFadeOutIn(float fadeOutDuration, float fadeInDuration, Action onComplete = null)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        cg.gameObject.SetActive(true);

        fadeRoutine = StartCoroutine(
            FadeOutInRoutine(fadeOutDuration, fadeInDuration, onComplete));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayFade(float from, float to, float duration, bool interactable, bool disableOnComplete)
    {
        StartFade(from, to, duration, interactable, disableOnComplete, null);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayFadeOutIn(float fadeOutDuration, float fadeInDuration)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        cg.gameObject.SetActive(true);

        fadeRoutine = StartCoroutine(
            FadeOutInRoutine(fadeOutDuration, fadeInDuration, null));
    }

    private void StartFade(float from, float to, float duration, bool interactable, bool disableOnComplete, Action onComplete)
    {
        cg.gameObject.SetActive(true);

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine( from, to, duration, interactable, disableOnComplete, onComplete));
    }

    private IEnumerator FadeOutInRoutine( float fadeOutDuration, float fadeInDuration, Action onComplete)
    {
        yield return FadeRoutine(1f, 0f, fadeOutDuration, false, false, null);
        yield return FadeRoutine(0f, 1f, fadeInDuration, true, false, null);

        fadeRoutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator FadeRoutine( float from, float to, float duration, bool interactable, bool disableOnComplete, Action onComplete)
    {
        if (cg == null)
            yield break;

        cg.gameObject.SetActive(true);

        duration = Mathf.Max(0f, duration);

        cg.alpha = from;
        cg.blocksRaycasts = true;
        cg.interactable = false;

        if (duration <= 0f)
        {
            cg.alpha = to;
            cg.blocksRaycasts = interactable;
            cg.interactable = interactable;
            gameObject.SetActive(!disableOnComplete);
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        cg.alpha = to;
        cg.blocksRaycasts = interactable;
        cg.interactable = interactable;
        cg.gameObject.SetActive(!disableOnComplete);
        onComplete?.Invoke();
    }
}
