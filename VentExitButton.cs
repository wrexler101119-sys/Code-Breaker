using UnityEngine;

public class VentExitButton : MonoBehaviour
{
    public void ExitVent()
    {
        Player player = GAMEUIMANAGER.Singleton.LocalPlayer;

        if (player == null)
            return;

        player.ExitVent();

        GAMEUIMANAGER.Singleton.ShowVentButton(false);
    }
}