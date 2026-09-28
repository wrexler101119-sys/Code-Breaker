using UnityEngine;

public class VentButton : MonoBehaviour
{
    public void OnVentPressed()
    {
        Player player = GAMEUIMANAGER.Singleton.LocalPlayer;

        if (player == null)
            return;

        ReachDetector reach =
            player.GetComponentInChildren<ReachDetector>();

        if (reach == null)
            return;

        Vent vent = reach.CurrentVent;

        if (vent == null)
            return;

        vent.Interact(player);
    }
}