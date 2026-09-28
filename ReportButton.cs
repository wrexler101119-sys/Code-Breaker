using UnityEngine;

public class ReportButton : MonoBehaviour
{
    public void OnReport()
    {
        Player player =
            GAMEUIMANAGER.Singleton.LocalPlayer;

        if (player == null)
            return;

        ReachDetector reach =
            player.GetComponentInChildren<ReachDetector>();

        if (reach == null)
            return;

        DeadBody body =
            reach.CurrentDeadBody;

        if (body == null)
            return;

        Debug.Log("BODY REPORTED");

        body.Report(player);
    }
}