using UnityEngine;

public class SettingsOpener : MonoBehaviour
{
    public GameObject settingsCanvas;

    public void OnSettingsPressed()
    {
        settingsCanvas.SetActive(true);
    }
}