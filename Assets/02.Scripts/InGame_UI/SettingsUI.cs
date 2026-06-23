using TMPro;
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

    [Header("Input")]
    [SerializeField] private TMP_InputField masterInput;
    [SerializeField] private TMP_InputField bgmInput;
    [SerializeField] private TMP_InputField sfxInput;
    [SerializeField] private TMP_InputField voiceInput;
    [SerializeField] private TMP_InputField uiInput;


    [Header("Mouse")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInput;

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

        masterInput.onValueChanged.AddListener(SetMasterInput);
        bgmInput.onValueChanged.AddListener(SetBGMInput);
        sfxInput.onValueChanged.AddListener(SetSFXInput);
        voiceInput.onValueChanged.AddListener(SetVoiceInput);
        uiInput.onValueChanged.AddListener(SetUIInput);
        sensitivityInput.onValueChanged.AddListener(SetSensitivityInput);
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(SetMaster);

        bgmSlider.onValueChanged.RemoveListener(SetBGM);

        sfxSlider.onValueChanged.RemoveListener(SetSFX);

        voiceSlider.onValueChanged.RemoveListener(SetVoice);

        uiSlider.onValueChanged.RemoveListener(SetUI);

        sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);

        masterInput.onValueChanged.RemoveListener(SetMasterInput);
        bgmInput.onValueChanged.RemoveListener(SetBGMInput);
        sfxInput.onValueChanged.RemoveListener(SetSFXInput);
        voiceInput.onValueChanged.RemoveListener(SetVoiceInput);
        uiInput.onValueChanged.RemoveListener(SetUIInput);
        sensitivityInput.onValueChanged.RemoveListener(SetSensitivityInput);
    }
    private void LateUpdate()
    {
        ClampInput(masterInput);
        ClampInput(bgmInput);
        ClampInput(sfxInput);
        ClampInput(voiceInput);
        ClampInput(uiInput);
        ClampInput(sensitivityInput);
    }

    private void RefreshUI()
    {
        masterSlider.SetValueWithoutNotify(SettingsData.MasterVolume);
        bgmSlider.SetValueWithoutNotify(SettingsData.BGMVolume);
        sfxSlider.SetValueWithoutNotify(SettingsData.SFXVolume);
        voiceSlider.SetValueWithoutNotify(SettingsData.VoiceVolume);
        uiSlider.SetValueWithoutNotify(SettingsData.UIVolume);
        float sensitivityPercent =Mathf.InverseLerp(0.5f,3f,SettingsData.MouseSensitivity);

        sensitivitySlider.SetValueWithoutNotify(sensitivityPercent);


        masterInput.SetTextWithoutNotify(
            Mathf.RoundToInt(SettingsData.MasterVolume * 100f).ToString());

        bgmInput.SetTextWithoutNotify(
            Mathf.RoundToInt(SettingsData.BGMVolume * 100f).ToString());

        sfxInput.SetTextWithoutNotify(
            Mathf.RoundToInt(SettingsData.SFXVolume * 100f).ToString());

        voiceInput.SetTextWithoutNotify(
            Mathf.RoundToInt(SettingsData.VoiceVolume * 100f).ToString());

        uiInput.SetTextWithoutNotify(
            Mathf.RoundToInt(SettingsData.UIVolume * 100f).ToString());

        sensitivityInput.SetTextWithoutNotify(Mathf.RoundToInt(sensitivityPercent * 100f).ToString());
    }

    public void SetMaster(float value)
    {
        SettingsData.MasterVolume = value;

        masterInput.SetTextWithoutNotify(
            Mathf.RoundToInt(value * 100f).ToString());

        SettingsData.Save();

        RefreshPlayingAudio();
    }

    public void SetBGM(float value)
    {
        SettingsData.BGMVolume = value;

        bgmInput.SetTextWithoutNotify(
            Mathf.RoundToInt(value * 100f).ToString());

        SettingsData.Save();

        RefreshPlayingAudio();
    }

    public void SetSFX(float value)
    {
        SettingsData.SFXVolume = value;

        sfxInput.SetTextWithoutNotify(
            Mathf.RoundToInt(value * 100f).ToString());

        SettingsData.Save();
    }

    public void SetVoice(float value)
    {
        SettingsData.VoiceVolume = value;

        voiceInput.SetTextWithoutNotify(
            Mathf.RoundToInt(value * 100f).ToString());

        SettingsData.Save();

        RefreshPlayingAudio();
    }

    public void SetUI(float value)
    {
        SettingsData.UIVolume = value;

        uiInput.SetTextWithoutNotify(
            Mathf.RoundToInt(value * 100f).ToString());

        SettingsData.Save();
    }

    public void SetSensitivity(float sliderValue)
    {
        SettingsData.MouseSensitivity =
            Mathf.Lerp(
                0.5f,
                3f,
                sliderValue);

        sensitivityInput.SetTextWithoutNotify(
            Mathf.RoundToInt(
                sliderValue * 100f).ToString());

        SettingsData.Save();
    }
    private void SetMasterInput(string value)
    {
        UpdateInput(value, masterSlider, v =>
        {
            SettingsData.MasterVolume = v;
            RefreshPlayingAudio();
        });
    }

    private void SetBGMInput(string value)
    {
        UpdateInput(value, bgmSlider, v =>
        {
            SettingsData.BGMVolume = v;
            RefreshPlayingAudio();
        });
    }

    private void SetSFXInput(string value)
    {
        UpdateInput(value, sfxSlider, v =>
        {
            SettingsData.SFXVolume = v;
        });
    }

    private void SetVoiceInput(string value)
    {
        UpdateInput(value, voiceSlider, v =>
        {
            SettingsData.VoiceVolume = v;
            RefreshPlayingAudio();
        });
    }

    private void SetUIInput(string value)
    {
        UpdateInput(value, uiSlider, v =>
        {
            SettingsData.UIVolume = v;
        });
    }

    private void SetSensitivityInput(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (!int.TryParse(value, out int percent))
            return;

        percent = Mathf.Clamp(percent, 0, 100);

        sensitivityInput.SetTextWithoutNotify(
            percent.ToString());

        float sliderValue =
            percent / 100f;

        sensitivitySlider.SetValueWithoutNotify(
            sliderValue);

        SettingsData.MouseSensitivity =
            Mathf.Lerp(
                0.5f,
                3f,
                sliderValue);

        SettingsData.Save();
    }

    private void ClampInput(TMP_InputField input)
    {
        if (string.IsNullOrEmpty(input.text))
            return;

        if (!int.TryParse(input.text, out int value))
            return;

        value = Mathf.Clamp(value, 0, 100);

        if (input.text != value.ToString())
        {
            input.SetTextWithoutNotify(value.ToString());
        }
    }
    private void UpdateInput(
     string text,
     Slider slider,
     System.Action<float> onChanged)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!int.TryParse(text, out int percent))
            return;

        percent = Mathf.Clamp(percent, 0, 100);

        float value = percent / 100f;

        slider.SetValueWithoutNotify(value);

        onChanged?.Invoke(value);

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

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}