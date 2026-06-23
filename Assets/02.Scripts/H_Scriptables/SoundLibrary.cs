using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class SoundEntry
{
    public AssetReferenceT<AudioClip> Clip;

    [Range(0f, 1f)]
    public float Volume = 1f;

    public float MinDistance = 3f;

    public float MaxDistance = 30f;
}

#region Category
[Serializable]
public class NarrationCategory
{
    public SoundEntry Welcome;
    public SoundEntry RoundStart;
    public SoundEntry FiveSec;

    public SoundEntry PlayerKill;
    public SoundEntry PlayerDeath;
    public SoundEntry TeamKill;
    public SoundEntry TeamDeath;

    public SoundEntry BlueWin;
    public SoundEntry RedWin;
    public SoundEntry Victory;
    public SoundEntry Defeat;
    public SoundEntry Draw;
}
[Serializable]
public class BGMCategory
{
    public SoundEntry Lobby;
    public SoundEntry Game;
}
[Serializable]
public class UICategory
{
    public SoundEntry Click;
    public SoundEntry Hover;
}

#endregion


[CreateAssetMenu(menuName = "Sound/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Header("Narration")]
    public NarrationCategory Narration;

    [Header("BGM")]
    public BGMCategory BGM;

    [Header("UI")]
    public UICategory UI;
}