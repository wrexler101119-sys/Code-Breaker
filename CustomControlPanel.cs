using UnityEngine;

public class CustomControlsPanel : MonoBehaviour
{
    public static CustomControlsPanel Instance;

    [Header("Panels")]
    [SerializeField] private GameObject controlsPanel;

    private void Awake()
    {
        Instance = this;

        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
            Debug.Log("CUSTOM CONTROLS PANEL OPENED");
        }
    }

    public void CloseControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
            Debug.Log("CUSTOM CONTROLS PANEL CLOSED");
        }
    }

    public void ToggleControls()
    {
        if (controlsPanel == null)
            return;

        controlsPanel.SetActive(!controlsPanel.activeSelf);
    }
}