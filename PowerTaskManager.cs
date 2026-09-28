using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerTaskManager : MonoBehaviour
{
    public static PowerTaskManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject taskUI;
    [SerializeField] private TextMeshProUGUI indicatorText;

    [Header("Switches")]
    [SerializeField] private SwitchToggleUI[] switches;

    [Header("Lights")]
    [SerializeField] private Image[] lights;

    [Header("Correct Pattern")]
    [SerializeField] private bool[] correctPattern;

    private NetworkTask currentTask;
    private bool isChecking;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("PowerTaskManager initialized.");
    }

    private void Start()
    {
        if (taskUI != null)
        {
            taskUI.SetActive(false);
        }

        ResetPuzzle();
    }

    public void OpenTask(NetworkTask task)
    {
        if (task == null)
        {
            Debug.LogError(
                "POWER OPEN FAILED | NetworkTask is null.");

            return;
        }

        if (task.IsCompleted)
        {
            Debug.LogWarning(
                $"POWER TASK ALREADY COMPLETED | Task={task.name}");

            return;
        }

        if (switches == null ||
            correctPattern == null ||
            switches.Length != correctPattern.Length)
        {
            Debug.LogError(
                "POWER OPEN FAILED | Switch count and " +
                "correct pattern count do not match.");

            return;
        }

        StopActiveCoroutine();

        currentTask = task;
        isChecking = false;

        ResetPuzzle();

        if (taskUI != null)
        {
            taskUI.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "POWER OPEN FAILED | taskUI is not assigned.");
        }

        if (GAMEUIMANAGER.Singleton != null)
        {
            // Hide the small interaction button.
            GAMEUIMANAGER.Singleton.ShowPowerUI(false);

            // Lock camera and movement controls.
            GAMEUIMANAGER.Singleton.LockTaskControls();
        }
        
        
            LockPlayerControls();
        

        Debug.Log(
            $"POWER TASK OPENED | Task={currentTask.name}");
    }

    public void CheckTask()
    {
        if (isChecking)
            return;

        if (currentTask == null)
        {
            Debug.LogError(
                "POWER CHECK FAILED | currentTask is null.");

            return;
        }

        if (switches == null ||
            correctPattern == null ||
            switches.Length != correctPattern.Length)
        {
            Debug.LogError(
                "POWER CHECK FAILED | Switch and pattern " +
                "array lengths do not match.");

            return;
        }

        isChecking = true;

        bool correct = true;

        for (int i = 0; i < correctPattern.Length; i++)
        {
            if (switches[i] == null ||
                switches[i].IsOn != correctPattern[i])
            {
                correct = false;
                break;
            }
        }

        SetLights(correct);

        if (indicatorText != null)
        {
            indicatorText.text =
                correct
                    ? "POWER RESTORED "
                    : "WRONG ";
        }

        if (correct)
        {
            activeCoroutine =
                StartCoroutine(CompleteAfterDelay());
        }
        else
        {
            activeCoroutine =
                StartCoroutine(ResetAfterDelay());
        }
    }

    private IEnumerator CompleteAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        NetworkTask taskToComplete =
            currentTask;

        if (taskToComplete == null)
        {
            Debug.LogError(
                "POWER COMPLETION FAILED | currentTask is null.");

            CloseTask();
            yield break;
        }

        Debug.Log(
            $"POWER TASK COMPLETE | Task={taskToComplete.name}");

        taskToComplete.TryComplete();

        CloseTask();
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        ResetPuzzle();

        isChecking = false;
        activeCoroutine = null;
    }

    public void ExitTask()
    {
        Debug.Log("POWER TASK EXITED");
        StopActiveCoroutine();
        CloseTask();
    }

    private void CloseTask()
    {
        StopActiveCoroutine();

        currentTask = null;
        isChecking = false;

        if (taskUI != null)
        {
            taskUI.SetActive(false);
        }

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton.ShowPowerUI(false);
            GAMEUIMANAGER.Singleton.ClosePowerTaskPanel();
        }
        else
        {
            UnlockPlayerControls();
        }

        Debug.Log(
            "POWER TASK CLOSED | PLAYER CONTROLS RESTORED");
    }

    public void ResetTask()
    {
        ResetPuzzle();
    }

    private void ResetPuzzle()
    {
        isChecking = false;

        if (switches != null)
        {
            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] != null)
                {
                    switches[i].SetState(false);
                }
            }
        }

        if (lights != null)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].color = Color.gray;
                }
            }
        }

        if (indicatorText != null)
        {
            indicatorText.text = "Turn all switches";
        }

        Debug.Log("POWER PUZZLE RESET");
    }

    private void SetLights(bool correct)
    {
        if (lights == null)
            return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].color =
                    correct
                        ? Color.green
                        : Color.red;
            }
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

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(false);

            MobileControlsManager.Instance
                .ShowJoystick(false);
        }
    }

    private void UnlockPlayerControls()
    {
        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
        }

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);

            MobileControlsManager.Instance
                .ShowJoystick(true);
        }
    }

    private void StopActiveCoroutine()
    {
        if (activeCoroutine == null)
            return;

        StopCoroutine(activeCoroutine);
        activeCoroutine = null;
    }
}