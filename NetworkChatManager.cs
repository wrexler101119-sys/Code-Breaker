using Fusion;
using TMPro;
using UnityEngine;

public class NetworkChatManager : NetworkBehaviour
{
    public static NetworkChatManager Instance;

    [SerializeField] private TMP_InputField chatInput;

    private void Awake()
    {
        Instance = this;
    }

    public void SendMessage()
    {
        Player player =
            GAMEUIMANAGER.Singleton.LocalPlayer;

        if (player == null)
            return;

        if (player.IsDead)
            return;

        string text = chatInput.text.Trim();

        if (string.IsNullOrEmpty(text))
            return;

        RPC_SendChat(
            player.Name,
            text);

        chatInput.text = "";
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SendChat(
        string playerName,
        string message)
    {
        ChatUI.Instance.AddMessage(
            playerName,
            message);
    }
}