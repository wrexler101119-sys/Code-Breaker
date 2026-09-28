using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FOVSliderDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider == null)
        {
            Debug.LogError(
                $"FOV DISPLAY FAILED | Slider missing on {name}");
        }

        if (valueText == null)
        {
            Debug.LogError(
                $"FOV DISPLAY FAILED | Value Text missing on {name}");
        }
    }

    private void OnEnable()
    {
        if (slider == null)
            return;

        slider.onValueChanged.AddListener(UpdateDisplay);

        // Load the saved FOV into this slider.
        if (CameraFOVManager.Instance != null)
        {
            float savedFOV =
                CameraFOVManager.Instance.GetFOV();

            slider.SetValueWithoutNotify(savedFOV);
            UpdateDisplay(savedFOV);
        }
        else
        {
            UpdateDisplay(slider.value);
        }
    }

    private void OnDisable()
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(UpdateDisplay);
    }

    private void UpdateDisplay(float value)
    {
        if (valueText == null)
            return;

        valueText.text =
            Mathf.RoundToInt(value).ToString();
    }
}