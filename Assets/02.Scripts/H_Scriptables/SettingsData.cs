using UnityEngine;

public static class SettingsData
{
    public static float MasterVolume = 1f;
    public static float BGMVolume = 1f;
    public static float SFXVolume = 1f;
    public static float VoiceVolume = 1f;
    public static float UIVolume = 1f;

    public static float MouseSensitivity = 1f;

    public static void Load()
    {
        MasterVolume =
            PlayerPrefs.GetFloat("MasterVolume", 1f);

        BGMVolume =
            PlayerPrefs.GetFloat("BGMVolume", 1f);

        SFXVolume =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        VoiceVolume =
            PlayerPrefs.GetFloat("VoiceVolume", 1f);

        UIVolume =
            PlayerPrefs.GetFloat("UIVolume", 1f);

        MouseSensitivity =
            PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("BGMVolume", BGMVolume);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        PlayerPrefs.SetFloat("VoiceVolume", VoiceVolume);
        PlayerPrefs.SetFloat("UIVolume", UIVolume);

        PlayerPrefs.SetFloat("MouseSensitivity", MouseSensitivity);

        PlayerPrefs.Save();
    }
}