using Fusion;
using UnityEngine;

public class DataBaseObject : NetworkBehaviour
{
    private NetworkTask networkTask;

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
                $"DATABASE NetworkTask MISSING | Object={name}");
        }
    }

    public void Interact(Player player)
    {
        if (player == null ||
            !player.HasInputAuthority)
        {
            return;
        }
        if (player.IsGhost)
        {
            Debug.Log(
                $"LAN INTERACTION BLOCKED | {player.Name} is a ghost.");

            return;
        }
        if (networkTask == null)
        {
            FindNetworkTask();
        }

        if (networkTask == null)
        {
            Debug.LogError(
                "DATABASE INTERACTION FAILED | NetworkTask is null");

            return;
        }

        if (networkTask.IsCompleted)
        {
            Debug.LogWarning(
                "DATABASE TASK ALREADY COMPLETED");

            return;
        }

        if (DatabaseManager.Instance == null)
        {
            Debug.LogError(
                "DatabaseManager.Instance is null");

            return;
        }

        DatabaseManager.Instance
            .OpenDatabase(networkTask);
    }
}