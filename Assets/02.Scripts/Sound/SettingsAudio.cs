using UnityEngine;
using UnityEngine.Audio;

public class SettingsAudio : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mixer;

    private float ToDb(float value)
    {
        return Mathf.Log10(value) * 20f;
    }

    public void SetMaster(float value)
    {
        mixer.SetFloat(
            "MasterVolume",
            ToDb(value));

        PlayerPrefs.SetFloat(
            "MasterVolume",
            value);
    }

    public void SetBGM(float value)
    {
        mixer.SetFloat(
            "BGMVolume",
            ToDb(value));

        PlayerPrefs.SetFloat(
            "BGMVolume",
            value);
    }

    public void SetSFX(float value)
    {
        mixer.SetFloat(
            "SFXVolume",
            ToDb(value));

        PlayerPrefs.SetFloat(
            "SFXVolume",
            value);
    }

    public void SetVoice(float value)
    {
        mixer.SetFloat(
            "VoiceVolume",
            ToDb(value));

        PlayerPrefs.SetFloat(
            "VoiceVolume",
            value);
    }

    public void SetUI(float value)
    {
        mixer.SetFloat(
            "UIVolume",
            ToDb(value));

        PlayerPrefs.SetFloat(
            "UIVolume",
            value);
    }
}