using Fusion;
using UnityEngine;

public class PowerTaskObject : NetworkBehaviour
{
    private NetworkTask networkTask;

    public NetworkTask Task => networkTask;

    private void Awake()
    {
        FindNetworkTask();
    }

    public override void Spawned()
    {
        FindNetworkTask();
    }

    private void FindNetworkTask()
    {
        networkTask =
            GetComponent<NetworkTask>();

        if (networkTask == null)
        {
            networkTask =
                GetComponentInParent<NetworkTask>();
        }

        if (networkTask == null)
        {
            networkTask =
                GetComponentInChildren<NetworkTask>(true);
        }

        if (networkTask == null)
        {
            Debug.LogError(
                $"POWER NETWORK TASK MISSING | Object={name}");
        }
        else
        {
            Debug.Log(
                $"POWER NETWORK TASK FOUND | Task={networkTask.name}");
        }
    }

    public void Interact(Player player)
    {
        if (player == null)
        {
            Debug.LogError(
                "POWER INTERACTION FAILED | Player is null.");

            return;
        }
        if (player.IsGhost)
        {
            Debug.Log(
                $"LAN INTERACTION BLOCKED | {player.Name} is a ghost.");

            return;
        }
        if (!player.HasInputAuthority)
            return;

        if (networkTask == null)
        {
            FindNetworkTask();
        }

        if (networkTask == null)
        {
            Debug.LogError(
                "POWER INTERACTION FAILED | NetworkTask is null.");

            return;
        }

        if (networkTask.IsCompleted)
        {
            Debug.LogWarning(
                $"POWER TASK ALREADY COMPLETED | " +
                $"Task={networkTask.name}");

            return;
        }

        if (PowerTaskManager.Instance == null)
        {
            Debug.LogError(
                "POWER INTERACTION FAILED | " +
                "PowerTaskManager.Instance is null.");

            return;
        }

        Debug.Log(
            $"OPENING POWER TASK | Task={networkTask.name}");

        PowerTaskManager.Instance
            .OpenTask(networkTask);
    }
}