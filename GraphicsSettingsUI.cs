using System;
using UnityEngine;
using TMPro;

public class GraphicsSettingsUI : MonoBehaviour
{
    [Header("Framerate")]
    [SerializeField] private TMP_Dropdown framerateDropdown;

    [Header("Graphics Quality")]
    [SerializeField] private TMP_Dropdown graphicsQualityDropdown;

    // CitrioN dropdown:
    // 0 = Platform Default
    // 1 = 30 FPS
    // 2 = 60 FPS
    private readonly int[] framerateValues =
    {
        -1,
        30,
        60
    };

    private const string QualityKey = "Graphics_Quality";
    private const string FramerateKey = "Graphics_Framerate";

    private void Awake()
    {
        SetupFramerate();
        SetupGraphicsQuality();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void Start()
    {
        // Refresh again after the scene/prefabs have finished initializing.
        RefreshUI();
    }

    // ==========================================
    // FRAMERATE
    // ==========================================

    private void SetupFramerate()
    {
        if (framerateDropdown == null)
        {
            Debug.LogError(
                "GRAPHICS UI FAILED | Framerate dropdown missing.");
            return;
        }

        framerateDropdown.onValueChanged.RemoveListener(
            OnFramerateChanged);

        framerateDropdown.onValueChanged.AddListener(
            OnFramerateChanged);
    }

    private void OnFramerateChanged(int index)
    {
        if (index < 0 ||
            index >= framerateValues.Length)
            return;

        int fps = framerateValues[index];

        Application.targetFrameRate = fps;

        PlayerPrefs.SetInt(
            FramerateKey,
            fps);

        PlayerPrefs.Save();

        Debug.Log(
            $"CITRION FRAMERATE CHANGED | " +
            $"Index={index} | FPS={fps}");
    }

    // ==========================================
    // GRAPHICS QUALITY
    // ==========================================

    private void SetupGraphicsQuality()
    {
        if (graphicsQualityDropdown == null)
        {
            Debug.LogError(
                "GRAPHICS UI FAILED | Quality dropdown missing.");
            return;
        }

        graphicsQualityDropdown.onValueChanged.RemoveListener(
            OnGraphicsQualityChanged);

        graphicsQualityDropdown.onValueChanged.AddListener(
            OnGraphicsQualityChanged);
    }

    private void OnGraphicsQualityChanged(int index)
    {
        int count = QualitySettings.names.Length;

        if (count == 0)
            return;

        index = Mathf.Clamp(
            index,
            0,
            count - 1);

        QualitySettings.SetQualityLevel(
            index,
            true);

        PlayerPrefs.SetInt(
            QualityKey,
            index);

        PlayerPrefs.Save();

        Debug.Log(
            $"CITRION GRAPHICS QUALITY CHANGED | " +
            $"Index={index} | " +
            $"Name={QualitySettings.names[index]}");
    }

    // ==========================================
    // REFRESH
    // ==========================================

    public void RefreshUI()
    {
        RefreshFramerate();
        RefreshGraphicsQuality();

        int savedFPS =
            PlayerPrefs.GetInt(
                FramerateKey,
                Application.targetFrameRate);

        int savedQuality =
            PlayerPrefs.GetInt(
                QualityKey,
                QualitySettings.GetQualityLevel());

        Debug.Log(
            $"CITRION GRAPHICS REFRESHED | " +
            $"FPS={savedFPS} | " +
            $"QualityIndex={savedQuality}");
    }

    private void RefreshFramerate()
    {
        if (framerateDropdown == null)
            return;

        int savedFPS =
            PlayerPrefs.GetInt(
                FramerateKey,
                -1);

        int index =
            Array.IndexOf(
                framerateValues,
                savedFPS);

        if (index < 0)
            index = 0;

        framerateDropdown.SetValueWithoutNotify(index);
        framerateDropdown.RefreshShownValue();
    }

    private void RefreshGraphicsQuality()
    {
        if (graphicsQualityDropdown == null)
            return;

        int qualityCount =
            QualitySettings.names.Length;

        if (qualityCount == 0)
            return;

        int savedQuality =
            PlayerPrefs.GetInt(
                QualityKey,
                QualitySettings.GetQualityLevel());

        savedQuality = Mathf.Clamp(
            savedQuality,
            0,
            qualityCount - 1);

        graphicsQualityDropdown.SetValueWithoutNotify(
            savedQuality);

        graphicsQualityDropdown.RefreshShownValue();
    }

    private void OnDestroy()
    {
        if (framerateDropdown != null)
        {
            framerateDropdown.onValueChanged
                .RemoveListener(
                    OnFramerateChanged);
        }

        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.onValueChanged
                .RemoveListener(
                    OnGraphicsQualityChanged);
        }
    }
}