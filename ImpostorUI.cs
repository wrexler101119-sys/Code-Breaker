using UnityEngine;

public class ImpostorUI : MonoBehaviour
{
    public void OnKill()
    {
        Debug.Log("KILL BUTTON PRESSED");

        if (GAMEUIMANAGER.Singleton == null)
        {
            Debug.LogError(
                "KILL FAILED | GAMEUIMANAGER.Singleton is null.");

            return;
        }

        Player localPlayer =
            GAMEUIMANAGER.Singleton.LocalPlayer;

        if (localPlayer == null)
        {
            Debug.LogError(
                "KILL FAILED | LocalPlayer is null.");

            return;
        }

        if (localPlayer.IsDead ||
            localPlayer.IsGhost)
        {
            Debug.LogWarning(
                "KILL BLOCKED | Dead or ghost players cannot kill.");

            return;
        }

        if (localPlayer.Role != PlayerRole.Impostor)
        {
            Debug.LogWarning(
                "KILL BLOCKED | Player is not an impostor.");

            return;
        }

        localPlayer.KillButtonPressed();
    }
}