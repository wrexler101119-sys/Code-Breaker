using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError(
                $"BUTTON SOUND FAILED | " +
                $"{name} has no Button component.");

            return;
        }

        button.onClick.RemoveListener(PlayClick);
        button.onClick.AddListener(PlayClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClick);
        }
    }

    private void PlayClick()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick();
        }
        else
        {
            Debug.LogWarning(
                $"BUTTON SOUND | " +
                $"UIAudioManager not found for {name}");
        }
    }
}