using Fusion;
using UnityEngine;

public class EmergencyButton : NetworkBehaviour
{
    public void Press(Player player)
    {
        if (player == null)
            return;

        if (player.IsDead)
            return;

        RPC_Press(player.Name);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Press(string playerName)
    {
        MeetingManager.Instance.RPC_ReportBody(
            playerName,
            "Emergency Meeting");
    }
}