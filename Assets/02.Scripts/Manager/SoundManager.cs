using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("Pool")]
    [SerializeField] private int poolSize = 20;

    private readonly Queue<AudioSource> sourcePool = new();

    private readonly Dictionary<SoundEntry, AudioClip> clipCache = new();

    private readonly Dictionary<SoundEntry, Task<AudioClip>> loadingCache = new();

    private SoundEntry currentBGM;
    private SoundEntry currentVoice;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CreatePool();
        SettingsData.Load();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new($"Audio_{i}");

            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();

            source.playOnAwake = false;

            sourcePool.Enqueue(source);
        }
    }

    private AudioSource GetSource()
    {
        if (sourcePool.Count > 0)
        {
            return sourcePool.Dequeue();
        }

        GameObject obj = new("Audio_Dynamic");

        obj.transform.SetParent(transform);

        AudioSource source = obj.AddComponent<AudioSource>();

        source.playOnAwake = false;

        return source;
    }

    private void ReturnSource(AudioSource source)
    {
        source.Stop();

        source.clip = null;

        source.transform.position = Vector3.zero;

        source.transform.SetParent(transform);

        sourcePool.Enqueue(source);
    }

    private async Task<AudioClip> LoadClip(SoundEntry entry)
    {
        return await entry.Clip
            .LoadAssetAsync<AudioClip>()
            .Task;
    }

    private async Task<AudioClip> GetClip(SoundEntry entry)
    {
        if (entry == null)
            return null;

        if (clipCache.TryGetValue(entry, out AudioClip cached))
        {
            return cached;
        }

        if (loadingCache.TryGetValue(entry, out Task<AudioClip> loadingTask))
        {
            return await loadingTask;
        }

        Task<AudioClip> task = LoadClip(entry);

        loadingCache.Add(entry, task);

        AudioClip clip = await task;

        loadingCache.Remove(entry);

        if (clip != null)
        {
            clipCache[entry] = clip;
        }

        return clip;
    }

    public async void PlayBGM(SoundEntry entry)
    {
        if (entry == null)
            return;

        if (currentBGM == entry)
            return;

        currentBGM = entry;

        AudioClip clip = await GetClip(entry);

        if (clip == null)
            return;

        bgmSource.clip = clip;

        bgmSource.volume = entry.Volume * SettingsData.MasterVolume * SettingsData.BGMVolume;

        bgmSource.loop = true;

        bgmSource.Play();
    }

    public void StopBGM()
    {
        currentBGM = null;

        bgmSource.Stop();
    }

    public async void PlayVoice(SoundEntry entry)
    {
        currentVoice = entry;

        AudioClip clip = await GetClip(entry);

        if (clip == null)
            return;

        voiceSource.Stop();

        voiceSource.clip = clip;

        voiceSource.volume = entry.Volume * SettingsData.MasterVolume * SettingsData.VoiceVolume;

        voiceSource.Play();
    }

    public async void Play2D(SoundEntry entry)
    {
        AudioClip clip = await GetClip(entry);

        if (clip == null)
            return;

        AudioSource source = GetSource();

        source.spatialBlend = 0f;

        source.clip = clip;

        source.volume = entry.Volume * SettingsData.MasterVolume * SettingsData.SFXVolume;

        source.Play();

        StartCoroutine(ReturnRoutine(source));
    }

    public async void Play3D(SoundEntry entry, Vector3 position)
    {
        AudioClip clip = await GetClip(entry);

        if (clip == null)
            return;

        AudioSource source = GetSource();

        source.transform.position = position;

        source.spatialBlend = 1f;

        source.minDistance = entry.MinDistance;

        source.maxDistance = entry.MaxDistance;

        source.rolloffMode = AudioRolloffMode.Linear;

        source.clip = clip;

        source.volume = entry.Volume * SettingsData.MasterVolume * SettingsData.SFXVolume;

        source.Play();

        StartCoroutine(ReturnRoutine(source));
    }

    public async void PlayAttached3D(SoundEntry entry,Transform target)
    {
        AudioClip clip = await GetClip(entry);

        if (clip == null)
            return;

        AudioSource source = GetSource();

        source.transform.SetParent(target);

        source.transform.localPosition = Vector3.zero;

        source.spatialBlend = 1f;

        source.minDistance = entry.MinDistance;

        source.maxDistance = entry.MaxDistance;

        source.rolloffMode = AudioRolloffMode.Linear;

        source.clip = clip;

        source.volume = entry.Volume * SettingsData.MasterVolume * SettingsData.SFXVolume;

        source.Play();

        StartCoroutine(ReturnAttachedRoutine(source));
    }

    public async void PlayUI(SoundEntry entry)
    {
        AudioClip clip = await GetClip(entry);

        if (clip == null)
            return;

        AudioSource source = GetSource();

        source.spatialBlend = 0f;

        source.clip = clip;

        source.volume =
            entry.Volume *
            SettingsData.MasterVolume *
            SettingsData.UIVolume;

        source.Play();

        StartCoroutine(ReturnRoutine(source));
    }

    private IEnumerator ReturnRoutine(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        ReturnSource(source);
    }

    private IEnumerator ReturnAttachedRoutine(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        ReturnSource(source);
    }
    public void RefreshVolume()
    {
        if (bgmSource != null &&
            currentBGM != null)
        {
            bgmSource.volume =
                currentBGM.Volume *
                SettingsData.MasterVolume *
                SettingsData.BGMVolume;
        }

        if (voiceSource != null &&
            currentVoice != null)
        {
            voiceSource.volume =
                currentVoice.Volume *
                SettingsData.MasterVolume *
                SettingsData.VoiceVolume;
        }
    }
}