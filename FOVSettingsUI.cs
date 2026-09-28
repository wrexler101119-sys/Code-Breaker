using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FOVSettingsUI : MonoBehaviour
{
    [Header("FOV")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovText;

    private const float MinFOV = 60f;
    private const float MaxFOV = 90f;
    private const float DefaultFOV = 75f;

    private void Awake()
    {
        if (fovSlider == null)
        {
            fovSlider = GetComponent<Slider>();
        }

        if (fovSlider == null)
        {
            Debug.LogError(
                $"FOV UI FAILED | Slider missing on {name}");
            return;
        }

        fovSlider.minValue = MinFOV;
        fovSlider.maxValue = MaxFOV;
        fovSlider.wholeNumbers = true;

        fovSlider.onValueChanged.RemoveListener(OnFOVChanged);
        fovSlider.onValueChanged.AddListener(OnFOVChanged);
    }

    private void OnEnable()
    {
        RefreshFOVUI();
    }

    private void RefreshFOVUI()
    {
        float value = DefaultFOV;

        if (CameraFOVManager.Instance != null)
        {
            value = CameraFOVManager.Instance.GetFOV();
        }
        else
        {
            value = PlayerPrefs.GetFloat(
                "Camera_FieldOfView",
                DefaultFOV);
        }

        value = Mathf.Clamp(
            value,
            MinFOV,
            MaxFOV);

        fovSlider.SetValueWithoutNotify(value);

        UpdateFOVText(value);

        Debug.Log(
            $"FOV UI REFRESHED | FOV={value:F0}");
    }

    private void OnFOVChanged(float value)
    {
        value = Mathf.Clamp(
            value,
            MinFOV,
            MaxFOV);

        if (CameraFOVManager.Instance != null)
        {
            CameraFOVManager.Instance.SetFOV(value);
        }
        else
        {
            Debug.LogWarning(
                "FOV UI | CameraFOVManager.Instance is null.");
        }

        UpdateFOVText(value);

        Debug.Log(
            $"FOV UI CHANGED | FOV={value:F0}");
    }

    private void UpdateFOVText(float value)
    {
        if (fovText == null)
            return;

        fovText.text =
            Mathf.RoundToInt(value).ToString();
    }

    private void OnDestroy()
    {
        if (fovSlider != null)
        {
            fovSlider.onValueChanged
                .RemoveListener(OnFOVChanged);
        }
    }
}