using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField]
    private Slider masterSlider;

    [SerializeField]
    private Slider bgmSlider;

    [SerializeField]
    private Slider sfxSlider;

    [SerializeField]
    private Slider voiceSlider;

    [SerializeField]
    private Slider uiSlider;

    [Header("Mouse")]
    [SerializeField]
    private Slider sensitivitySlider;

    private void OnEnable()
    {
        RefreshUI();
    }

    private void Awake()
    {
        masterSlider.onValueChanged.AddListener(SetMaster);

        bgmSlider.onValueChanged.AddListener(SetBGM);

        sfxSlider.onValueChanged.AddListener(SetSFX);

        voiceSlider.onValueChanged.AddListener(SetVoice);

        uiSlider.onValueChanged.AddListener(SetUI);

        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(SetMaster);

        bgmSlider.onValueChanged.RemoveListener(SetBGM);

        sfxSlider.onValueChanged.RemoveListener(SetSFX);

        voiceSlider.onValueChanged.RemoveListener(SetVoice);

        uiSlider.onValueChanged.RemoveListener(SetUI);

        sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);
    }

    private void RefreshUI()
    {
        masterSlider.SetValueWithoutNotify(
            SettingsData.MasterVolume);

        bgmSlider.SetValueWithoutNotify(
            SettingsData.BGMVolume);

        sfxSlider.SetValueWithoutNotify(
            SettingsData.SFXVolume);

        voiceSlider.SetValueWithoutNotify(
            SettingsData.VoiceVolume);

        uiSlider.SetValueWithoutNotify(
            SettingsData.UIVolume);

        sensitivitySlider.SetValueWithoutNotify(
            SettingsData.MouseSensitivity);
    }

    public void SetMaster(float value)
    {
        SettingsData.MasterVolume = value;

        SettingsData.Save();

        RefreshPlayingAudio();
    }

    public void SetBGM(float value)
    {
        SettingsData.BGMVolume = value;

        SettingsData.Save();

        RefreshPlayingAudio();
    }

    public void SetSFX(float value)
    {
        SettingsData.SFXVolume = value;

        SettingsData.Save();
    }

    public void SetVoice(float value)
    {
        SettingsData.VoiceVolume = value;

        SettingsData.Save();

        RefreshPlayingAudio();
    }

    public void SetUI(float value)
    {
        SettingsData.UIVolume = value;

        SettingsData.Save();
    }

    public void SetSensitivity(float value)
    {
        SettingsData.MouseSensitivity = value;

        SettingsData.Save();
    }

    private void RefreshPlayingAudio()
    {
        SoundManager soundManager =
            SoundManager.Instance;

        if (soundManager == null)
            return;

        soundManager.RefreshVolume();
    }
}