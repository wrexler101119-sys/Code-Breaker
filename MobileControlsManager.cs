using UnityEngine;

public class MobileControlsManager : MonoBehaviour
{
    public static MobileControlsManager Instance;

    [Header("Control Panels")]
    [SerializeField] private GameObject joystickPanel;
    [SerializeField] private GameObject lookPanel;

    [Header("Joystick Visual")]
    [Tooltip("Assign the RectTransform containing the joystick background and handle.")]
    [SerializeField] private RectTransform joystickVisual;

    [Tooltip("Assign the CanvasGroup on the joystick visual.")]
    [SerializeField] private CanvasGroup joystickCanvasGroup;

    [Header("Default Slider Percentages")]
    [Range(1f, 100f)]
    [SerializeField] private float defaultJoystickSizePercent = 50f;

    [Range(1f, 100f)]
    [SerializeField] private float defaultJoystickOpacityPercent = 50f;

    private const string JoystickSizeKey =
        "JoystickSizePercent";

    private const string JoystickOpacityKey =
        "JoystickOpacityPercent";

    // The original appearance from the Inspector.
    private Vector3 originalJoystickScale;
    private float originalJoystickOpacity;

    public float CurrentJoystickSizePercent
    {
        get;
        private set;
    }

    public float CurrentJoystickOpacityPercent
    {
        get;
        private set;
    }
    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
    SetControlsEnabled(true);
#elif UNITY_EDITOR
        // Show mobile controls in the Editor for testing.
        SetControlsEnabled(true);
 #else
    SetControlsEnabled(true);
#endif
    }
    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PrepareReferences();
        SaveOriginalAppearance();
        LoadControlSettings();
    }

    private void PrepareReferences()
    {
        if (joystickVisual == null)
        {
            Debug.LogError(
                "MOBILE CONTROLS | Joystick Visual is not assigned.");
        }

        if (joystickCanvasGroup == null &&
            joystickVisual != null)
        {
            joystickCanvasGroup =
                joystickVisual.GetComponent<CanvasGroup>();

            if (joystickCanvasGroup == null)
            {
                joystickCanvasGroup =
                    joystickVisual.gameObject
                        .AddComponent<CanvasGroup>();
            }
        }
    }

    private void SaveOriginalAppearance()
    {
        if (joystickVisual != null)
        {
            originalJoystickScale =
                joystickVisual.localScale;
        }
        else
        {
            originalJoystickScale =
                Vector3.one;
        }

        if (joystickCanvasGroup != null)
        {
            originalJoystickOpacity =
                joystickCanvasGroup.alpha;
        }
        else
        {
            originalJoystickOpacity = 1f;
        }

        Debug.Log(
            $"JOYSTICK ORIGINAL APPEARANCE SAVED | " +
            $"Scale={originalJoystickScale} | " +
            $"Opacity={originalJoystickOpacity:F2}");
    }

    // =========================
    // SHOW / HIDE
    // =========================

    public void SetControlsEnabled(bool enabled)
    {
        if (joystickPanel != null)
            joystickPanel.SetActive(enabled);

        if (lookPanel != null)
            lookPanel.SetActive(enabled);
    }

    public void ShowLook(bool show)
    {
        if (lookPanel != null)
        {
            lookPanel.SetActive(show);
        }
    }

    public void ShowJoystick(bool show)
    {
        if (joystickPanel != null)
        {
            joystickPanel.SetActive(show);
        }
    }

    // =========================
    // JOYSTICK SIZE
    // =========================

    public void SetJoystickSizePercent(float percentage)
    {
        CurrentJoystickSizePercent =
            Mathf.Clamp(percentage, 1f, 100f);

        ApplyJoystickSize();

        PlayerPrefs.SetFloat(
            JoystickSizeKey,
            CurrentJoystickSizePercent);

        PlayerPrefs.Save();

        Debug.Log(
            $"JOYSTICK SIZE SAVED | " +
            $"Percentage={CurrentJoystickSizePercent:F0}%");
    }

    private void ApplyJoystickSize()
    {
        if (joystickVisual == null)
        {
            Debug.LogError(
                "JOYSTICK SIZE FAILED | Joystick Visual is missing.");

            return;
        }

        // 50% means the original Inspector scale.
        float scaleMultiplier =
            CurrentJoystickSizePercent / 50f;

        joystickVisual.localScale =
            originalJoystickScale *
            scaleMultiplier;

        Debug.Log(
            $"JOYSTICK SIZE APPLIED | " +
            $"Percentage={CurrentJoystickSizePercent:F0}% | " +
            $"Multiplier={scaleMultiplier:F2} | " +
            $"Scale={joystickVisual.localScale}");
    }

    // =========================
    // JOYSTICK OPACITY
    // =========================

    public void SetJoystickOpacityPercent(
        float percentage)
    {
        CurrentJoystickOpacityPercent =
            Mathf.Clamp(percentage, 1f, 100f);

        ApplyJoystickOpacity();

        PlayerPrefs.SetFloat(
            JoystickOpacityKey,
            CurrentJoystickOpacityPercent);

        PlayerPrefs.Save();

        Debug.Log(
            $"JOYSTICK OPACITY SAVED | " +
            $"Percentage={CurrentJoystickOpacityPercent:F0}%");
    }

    private void ApplyJoystickOpacity()
    {
        if (joystickCanvasGroup == null)
        {
            Debug.LogError(
                "JOYSTICK OPACITY FAILED | CanvasGroup is missing.");

            return;
        }

        // 50% means the original Inspector opacity.
        float opacityMultiplier =
            CurrentJoystickOpacityPercent / 50f;

        joystickCanvasGroup.alpha =
            Mathf.Clamp01(
                originalJoystickOpacity *
                opacityMultiplier);

        Debug.Log(
            $"JOYSTICK OPACITY APPLIED | " +
            $"Percentage={CurrentJoystickOpacityPercent:F0}% | " +
            $"Alpha={joystickCanvasGroup.alpha:F2}");
    }

    // =========================
    // LOAD / RESET
    // =========================

    private void LoadControlSettings()
    {
        float savedSize =
            PlayerPrefs.GetFloat(
                JoystickSizeKey,
                defaultJoystickSizePercent);

        float savedOpacity =
            PlayerPrefs.GetFloat(
                JoystickOpacityKey,
                defaultJoystickOpacityPercent);

        SetJoystickSizePercent(savedSize);
        SetJoystickOpacityPercent(savedOpacity);

        Debug.Log(
            $"CONTROL SETTINGS LOADED | " +
            $"Size={savedSize:F0}% | " +
            $"Opacity={savedOpacity:F0}%");
    }

    public void ResetControlSettings()
    {
        SetJoystickSizePercent(
            defaultJoystickSizePercent);

        SetJoystickOpacityPercent(
            defaultJoystickOpacityPercent);

        Debug.Log(
            $"CONTROL SETTINGS RESET | " +
            $"Size={defaultJoystickSizePercent:F0}% | " +
            $"Opacity={defaultJoystickOpacityPercent:F0}%");
    }

    public float GetJoystickSizePercent()
    {
        return CurrentJoystickSizePercent;
    }

    public float GetJoystickOpacityPercent()
    {
        return CurrentJoystickOpacityPercent;
    }
}