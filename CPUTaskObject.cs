using Fusion;
using UnityEngine;

public class CPUTaskObject : NetworkBehaviour
{
    private NetworkTask networkTask;

    private void Awake()
    {
        FindTask();
    }

    public override void Spawned()
    {
        FindTask();
    }

    private void FindTask()
    {
        networkTask = GetComponent<NetworkTask>();

        if (networkTask == null)
            networkTask = GetComponentInParent<NetworkTask>();

        if (networkTask == null)
            networkTask = GetComponentInChildren<NetworkTask>(true);

        if (networkTask == null)
            Debug.LogError($"❌ CPU NetworkTask missing on {name}");
    }

    public void Interact(Player player)
    {
        if (player == null || !player.HasInputAuthority)
            return;
        if (player.IsGhost)
        {
            Debug.Log(
                $"LAN INTERACTION BLOCKED | {player.Name} is a ghost.");

            return;
        }
        if (networkTask == null)
            FindTask();

        if (networkTask == null)
        {
            Debug.LogError("❌ CPU NetworkTask is NULL.");
            return;
        }

        if (networkTask.IsCompleted)
        {
            Debug.Log("CPU task already completed.");
            return;
        }

        if (CPUTaskManager.Instance == null)
        {
            Debug.LogError("❌ CPUTaskManager missing.");
            return;
        }

        CPUTaskManager.Instance.OpenTask(networkTask);
    }
}