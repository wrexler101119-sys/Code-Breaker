using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomControlsSettingsUI : MonoBehaviour
{
    [Header("Unity UI Sliders")]
    [SerializeField] private Slider joystickSizeSlider;
    [SerializeField] private Slider joystickOpacitySlider;

    [Header("Value Text")]
    [SerializeField] private TextMeshProUGUI joystickSizeValueText;
    [SerializeField] private TextMeshProUGUI joystickOpacityValueText;

    private bool initialized;

    private void Awake()
    {
        InitializeSliders();
    }

    private void OnEnable()
    {
        InitializeSliders();

        Canvas.ForceUpdateCanvases();

        RefreshUI();
    }

    private void InitializeSliders()
    {
        if (initialized)
            return;

        ConfigureSlider(joystickSizeSlider);
        ConfigureSlider(joystickOpacitySlider);

        if (joystickSizeSlider != null)
        {
            joystickSizeSlider.onValueChanged
                .RemoveAllListeners();

            joystickSizeSlider.onValueChanged
                .AddListener(OnJoystickSizeChanged);
        }

        if (joystickOpacitySlider != null)
        {
            joystickOpacitySlider.onValueChanged
                .RemoveAllListeners();

            joystickOpacitySlider.onValueChanged
                .AddListener(OnJoystickOpacityChanged);
        }

        initialized = true;
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 1f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;
        slider.direction =
            Slider.Direction.LeftToRight;
        slider.interactable = true;
    }

    private void RefreshUI()
    {
        MobileControlsManager manager =
            MobileControlsManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "CUSTOM CONTROLS FAILED | " +
                "MobileControlsManager.Instance is null.");

            return;
        }

        float sizePercentage =
            manager.GetJoystickSizePercent();

        float opacityPercentage =
            manager.GetJoystickOpacityPercent();

        if (joystickSizeSlider != null)
        {
            joystickSizeSlider.SetValueWithoutNotify(
                sizePercentage);
        }

        if (joystickOpacitySlider != null)
        {
            joystickOpacitySlider.SetValueWithoutNotify(
                opacityPercentage);
        }

        UpdateSizeText(sizePercentage);
        UpdateOpacityText(opacityPercentage);
    }

    private void OnJoystickSizeChanged(
        float percentage)
    {
        MobileControlsManager manager =
            MobileControlsManager.Instance;

        if (manager == null)
            return;

        manager.SetJoystickSizePercent(
            percentage);

        UpdateSizeText(percentage);
    }

    private void OnJoystickOpacityChanged(
        float percentage)
    {
        MobileControlsManager manager =
            MobileControlsManager.Instance;

        if (manager == null)
            return;

        manager.SetJoystickOpacityPercent(
            percentage);

        UpdateOpacityText(percentage);
    }

    private void UpdateSizeText(
        float percentage)
    {
        if (joystickSizeValueText != null)
        {
            joystickSizeValueText.text =
                Mathf.RoundToInt(percentage) +
                "%";
        }
    }

    private void UpdateOpacityText(
        float percentage)
    {
        if (joystickOpacityValueText != null)
        {
            joystickOpacityValueText.text =
                Mathf.RoundToInt(percentage) +
                "%";
        }
    }

    public void ResetControls()
    {
        MobileControlsManager manager =
            MobileControlsManager.Instance;

        if (manager == null)
            return;

        manager.ResetControlSettings();

        RefreshUI();
    }
}