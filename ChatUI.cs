using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private ChatMessageUI messagePrefab;

    public ScrollRect scrollRect;
    private void Awake()
    {
        Instance = this;
    }

    public void AddMessage(
        string playerName,
        string message)
    {

        Debug.Log("CHAT RECEIVED: " + playerName + " : " + message);
        ChatMessageUI msg =
            Instantiate(
                messagePrefab,
                content);

        msg.Setup(
            playerName,
            message);


              Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;
    }
}