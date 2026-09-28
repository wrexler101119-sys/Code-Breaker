using Fusion;
using TMPro;
using UnityEngine;

public class GlobalTaskTracker : NetworkBehaviour
{
    public static GlobalTaskTracker Instance
    {
        get;
        private set;
    }

    [Networked, OnChangedRender(
        nameof(OnCompletedTasksChanged))]
    public int CompletedTasks
    {
        get;
        private set;
    }

    [SerializeField] private int maxTasks = 6;

    public int MaxTasks => maxTasks;

    public bool AllTasksCompleted =>
        maxTasks > 0 &&
        CompletedTasks >= maxTasks;

    private TextMeshProUGUI taskText;

    public override void Spawned()
    {
        Instance = this;

        if (HasStateAuthority)
        {
            CompletedTasks = 0;
        }

        FindTaskText();
        UpdateUI();
    }

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddTask()
    {
        if (!HasStateAuthority)
            return;

        if (CompletedTasks >= maxTasks)
            return;

        CompletedTasks++;

        Debug.Log(
            $"GLOBAL TASK ADDED | " +
            $"Completed={CompletedTasks}/{maxTasks}");

        UpdateUI();

        if (AllTasksCompleted &&
            GameLogic.Instance != null)
        {
            GameLogic.Instance
                .RequestWinnerCheck();
        }
    }

    public void RemoveTask()
    {
        if (!HasStateAuthority)
            return;

        if (CompletedTasks <= 0)
            return;

        CompletedTasks--;

        Debug.Log(
            $"GLOBAL TASK REMOVED | " +
            $"Completed={CompletedTasks}/{maxTasks}");

        UpdateUI();
    }

    

    private void OnCompletedTasksChanged()
    {
        UpdateUI();
    }

    private void FindTaskText()
    {
        if (taskText != null)
            return;

        GameObject textObject =
            GameObject.Find("TaskText");

        if (textObject != null)
        {
            taskText =
                textObject.GetComponent<TextMeshProUGUI>();
        }
    }

    private void UpdateUI()
    {
        if (taskText == null)
        {
            FindTaskText();
        }

        if (taskText != null)
        {
            taskText.text =
                $"Tasks: {CompletedTasks}/{maxTasks}";
        }
    }
    public void ResetTracker()
    {
        if (!HasStateAuthority)
            return;

        CompletedTasks = 0;

        UpdateUI();

        Debug.Log(
            $"GLOBAL TASK TRACKER RESET | " +
            $"Completed=0/{MaxTasks}");
    }
}