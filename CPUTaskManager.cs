using System.Collections;
using TMPro;
using UnityEngine;

public class CPUTaskManager : MonoBehaviour
{
    public static CPUTaskManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject taskUI;

    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField[] answerInputs;

    [Header("Correct Answers")]
    [SerializeField] private string[] correctAnswers;

    private NetworkTask currentTask;
    private bool isChecking;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenTask(NetworkTask task)
    {
        if (task == null)
        {
            Debug.LogError("CPU Task cannot open. NetworkTask is null.");
            return;
        }

        if (task.IsCompleted)
        {
            Debug.Log("CPU task already completed.");
            return;
        }

        currentTask = task;
        isChecking = false;

        ResetInputs();

        if (feedbackText != null)
            feedbackText.text = "";

        if (taskUI != null)
            taskUI.SetActive(true);

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton.ShowCPUUI(false);
            GAMEUIMANAGER.Singleton.LockTaskControls();
        }
        LockPlayerControls();
        Debug.Log($"CPU Task Opened | {task.name}");
    }

    public void SubmitAnswers()
    {
        if (isChecking || currentTask == null)
            return;

        isChecking = true;

        bool allCorrect = true;

        for (int i = 0; i < correctAnswers.Length; i++)
        {
            string answer =
                answerInputs[i].text.Trim();

            string correct =
                correctAnswers[i].Trim();

            if (!string.Equals(
                answer,
                correct,
                System.StringComparison.OrdinalIgnoreCase))
            {
                allCorrect = false;
                break;
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                allCorrect
                    ? "SYSTEM UNIT FIXED"
                    : "WRONG ANSWER";
        }

        if (allCorrect)
            StartCoroutine(CompleteRoutine());
        else
            StartCoroutine(FailRoutine());
    }

    private IEnumerator CompleteRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (currentTask != null)
            currentTask.TryComplete();

        CloseTask();
    }

    private IEnumerator FailRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (feedbackText != null)
            feedbackText.text = "";

        isChecking = false;
    }

    public void ExitTask()
    {
        StopAllCoroutines();

        CloseTask();
    }

    private void CloseTask()
    {
        if (taskUI != null)
            taskUI.SetActive(false);

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton.ShowCPUUI(false);
            GAMEUIMANAGER.Singleton.CloseCpuPanel();
        }

        ResetInputs();

        if (feedbackText != null)
            feedbackText.text = "";

        currentTask = null;
        isChecking = false;

        Debug.Log("CPU Task Closed");
    }

    private void ResetInputs()
    {
        if (answerInputs == null)
            return;

        foreach (TMP_InputField input in answerInputs)
        {
            if (input != null)
                input.text = "";
        }
    }
    private void LockPlayerControls()
    {
        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(true);
        }

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton
                .SetTaskControlsLocked(true);
        }

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(false);
        }

        Debug.Log("CPU PLAYER CONTROLS LOCKED");
    }

    private void UnlockPlayerControls()
    {
        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
        }

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton
                .SetTaskControlsLocked(false);
        }

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);
        }

        Debug.Log("LAN PLAYER CONTROLS UNLOCKED");
    }

}