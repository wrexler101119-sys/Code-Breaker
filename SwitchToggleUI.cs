using UnityEngine;
using UnityEngine.UI;
using System;

public class SwitchToggleUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

   
    public bool IsOn { get; private set; }

    public Action<bool> OnToggle; // 🔥 notify manager

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Toggle);

        button.transition = Selectable.Transition.None;

        SetState(false);
    }

    public void Toggle()
    {
        SetState(!IsOn);
    }

    public void SetState(bool value)
    {
        IsOn = value;

        if (targetImage != null)
            targetImage.sprite = IsOn ? onSprite : offSprite;

        OnToggle?.Invoke(IsOn);
    }
}