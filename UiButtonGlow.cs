using UnityEngine;
using UnityEngine.UI;

public class UIButtonGlow : MonoBehaviour
{
    public Image image;
    public Color normalColor;
    public Color hoverColor;

    public void OnHoverEnter()
    {
        image.color = hoverColor;
    }

    public void OnHoverExit()
    {
        image.color = normalColor;
    }
}