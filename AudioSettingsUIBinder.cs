using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUIBinder : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;

    [Header("Reset")]
    [SerializeField] private Button resetToDefaultButton;

    private void Start()
    {
        BindSliders();
        BindResetButton();
        LoadSliderValues();
    }

    // ==========================================
    // BIND SLIDERS
    // ==========================================

    private void BindSliders()
    {
        if (masterSlider != null)
        {
            // Do NOT RemoveAllListeners().
            // CitrioN may have its own listeners.
            masterSlider.onValueChanged.AddListener(
                OnMasterChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(
                OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(
                OnSFXChanged);
        }

        if (ambientSlider != null)
        {
            ambientSlider.onValueChanged.AddListener(
                OnAmbientChanged);
        }

        Debug.Log(
            "AUDIO SETTINGS SLIDERS BOUND");
    }

    // ==========================================
    // RESET BUTTON
    // ==========================================

    private void BindResetButton()
    {
        if (resetToDefaultButton == null)
        {
            Debug.LogWarning(
                "AUDIO RESET | Reset button is not assigned.");

            return;
        }

        // Keep CitrioN's existing Reset listener.
        // We only add our audio reset alongside it.
        resetToDefaultButton.onClick.AddListener(
            OnResetAudioPressed);

        Debug.Log(
            "AUDIO RESET BUTTON BOUND");
    }

    // ==========================================
    // LOAD SAVED VALUES INTO UI
    // ==========================================

    private void LoadSliderValues()
    {
        if (AudioSettingsManager.Instance == null)
        {
            Debug.LogError(
                "AUDIO UI FAILED | " +
                "AudioSettingsManager.Instance is null.");

            return;
        }

        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(
                AudioSettingsManager.Instance
                    .GetMasterVolume());
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                AudioSettingsManager.Instance
                    .GetMusicVolume());
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(
                AudioSettingsManager.Instance
                    .GetSFXVolume());
        }

        if (ambientSlider != null)
        {
            ambientSlider.SetValueWithoutNotify(
                AudioSettingsManager.Instance
                    .GetAmbientVolume());
        }

        Debug.Log(
            $"AUDIO UI LOADED | " +
            $"Master={AudioSettingsManager.Instance.GetMasterVolume() * 100f:F0}% | " +
            $"Music={AudioSettingsManager.Instance.GetMusicVolume() * 100f:F0}% | " +
            $"SFX={AudioSettingsManager.Instance.GetSFXVolume() * 100f:F0}% | " +
            $"Ambient={AudioSettingsManager.Instance.GetAmbientVolume() * 100f:F0}%");
    }

    // ==========================================
    // MASTER
    // ==========================================

    private void OnMasterChanged(float value)
    {
        if (AudioSettingsManager.Instance == null)
            return;

        AudioSettingsManager.Instance
            .SetMasterVolume(value);
    }

    // ==========================================
    // MUSIC
    // ==========================================

    private void OnMusicChanged(float value)
    {
        if (AudioSettingsManager.Instance == null)
            return;

        AudioSettingsManager.Instance
            .SetMusicVolume(value);
    }

    // ==========================================
    // SFX
    // ==========================================

    private void OnSFXChanged(float value)
    {
        if (AudioSettingsManager.Instance == null)
            return;

        AudioSettingsManager.Instance
            .SetSFXVolume(value);
    }

    // ==========================================
    // AMBIENT
    // ==========================================

    private void OnAmbientChanged(float value)
    {
        if (AudioSettingsManager.Instance == null)
            return;

        AudioSettingsManager.Instance
            .SetAmbientVolume(value);
    }

    // ==========================================
    // RESET
    // ==========================================

    public void OnResetAudioPressed()
    {
        if (AudioSettingsManager.Instance == null)
        {
            Debug.LogError(
                "AUDIO RESET FAILED | " +
                "AudioSettingsManager.Instance is null.");

            return;
        }

        // Reset AudioMixer + PlayerPrefs.
        AudioSettingsManager.Instance
            .ResetAudioSettings();

        // Update the four sliders.
        // Use .value so CitrioN also refreshes its
        // visible input-field percentage.
        if (masterSlider != null)
            masterSlider.value = 1f;

        if (musicSlider != null)
            musicSlider.value = 1f;

        if (sfxSlider != null)
            sfxSlider.value = 1f;

        if (ambientSlider != null)
            ambientSlider.value = 1f;

        Debug.Log(
            "AUDIO RESET COMPLETE | " +
            "Master=100% | Music=100% | " +
            "SFX=100% | Ambient=100%");
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(
                OnMasterChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(
                OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(
                OnSFXChanged);
        }

        if (ambientSlider != null)
        {
            ambientSlider.onValueChanged.RemoveListener(
                OnAmbientChanged);
        }

        if (resetToDefaultButton != null)
        {
            resetToDefaultButton.onClick.RemoveListener(
                OnResetAudioPressed);
        }
    }
}