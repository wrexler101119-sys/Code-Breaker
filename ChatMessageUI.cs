using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    private LayoutElement layoutElement;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
    }

    public void Setup(string playerName, string message)
    {
        messageText.text = playerName + ": " + message;

        Canvas.ForceUpdateCanvases();

        float height =
            messageText.preferredHeight + 10f;

        layoutElement.preferredHeight =
            Mathf.Max(30f, height);
    }
}