using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public void SetName(string playerName)
    {
        if (nameText != null)
        {
            nameText.text = playerName;
        }
    }

    public void SetColor(Color color)
    {
        if (nameText != null)
        {
            nameText.color = color;
        }
    }

    public void Show(bool show)
    {
        if (nameText != null)
        {
            nameText.gameObject.SetActive(show);
        }
    }
}