using Fusion;
using UnityEngine;

public class Vent : NetworkBehaviour
{
    [Header("Vent Connection")]
    public Vent ConnectedVent;

    [Header("Teleport Destination")]
    public Transform ExitPoint;

    public void Interact(Player player)
    {
        if (player == null)
        {
            Debug.LogError(
                $"VENT INTERACTION FAILED | " +
                $"Player reference is null on {name}.");

            return;
        }

        if (!player.HasInputAuthority)
        {
            Debug.LogWarning(
                $"VENT INTERACTION IGNORED | " +
                $"Player={player.Name} has no Input Authority.");

            return;
        }

        if (Object == null || !Object.IsValid)
        {
            Debug.LogError(
                $"VENT INTERACTION FAILED | " +
                $"{name} has no valid NetworkObject.");

            return;
        }

        if (ConnectedVent == null)
        {
            Debug.LogError(
                $"VENT INTERACTION FAILED | " +
                $"{name} has no ConnectedVent.");

            return;
        }

        if (ConnectedVent.ExitPoint == null)
        {
            Debug.LogError(
                $"VENT INTERACTION FAILED | " +
                $"{ConnectedVent.name} has no ExitPoint.");

            return;
        }

        Debug.Log(
            $"VENT INTERACTED | " +
            $"Player={player.Name} | " +
            $"Vent={name} | " +
            $"HostPlayer=" +
            $"{player.HasInputAuthority && player.HasStateAuthority}");

        player.RequestUseVent(Object.Id);
    }
}