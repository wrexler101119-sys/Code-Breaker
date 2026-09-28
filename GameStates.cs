using Fusion;
using UnityEngine;

public class GameStates : NetworkBehaviour
{
    public static GameStates Instance;

    [Networked]
    public int CompletedTasks { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddTask()
    {
        if (!HasStateAuthority)
            return;

        CompletedTasks++;

        Debug.Log("GLOBAL TASKS: " + CompletedTasks);
    }
}