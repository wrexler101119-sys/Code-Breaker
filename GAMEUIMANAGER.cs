using Fusion.Menu;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GAMEUIMANAGER : MonoBehaviour
{
    public static GAMEUIMANAGER Singleton;

    [Header("Door UI")]
    [SerializeField] private GameObject doorPanel;
    [SerializeField] private Button doorButton;
    [SerializeField] private TextMeshProUGUI doorButtonText;

    [Header("LAN UI")]
    [SerializeField] private GameObject lanTaskPanel;
    [SerializeField] private GameObject lanPanel;

    [Header("powersource UI")]
    [SerializeField] private GameObject powerTaskPanel;
    [SerializeField] private GameObject powerPanel;

    [Header("Laptop UI")]
    [SerializeField] private GameObject laptopTaskPanel;
    [SerializeField] private GameObject laptopPanel;
    [Header("Laptop Clue UI")]
    [SerializeField] private GameObject laptopClueButton;
    [SerializeField] private GameObject laptopCluePanel;

    [Header("KEYBOARD UI")]
    [SerializeField] private GameObject keyboardTaskPanel;
    [SerializeField] private GameObject keyboardPanel;
    [Header("Keyboard Clue UI")]
    [SerializeField] private GameObject keyboardClueButton;
    [SerializeField] private GameObject keyboardCluePanel;

    [Header("Database UI")]
    [SerializeField] private GameObject databaseTaskPanel;
    [SerializeField] private GameObject databasePanel;

    [Header("Database Clue UI")]
    [SerializeField] private GameObject databaseClueButton;
    [SerializeField] private GameObject databaseCluePanel;

    [Header("CPU UI")]
    [SerializeField] private GameObject cpuTaskPanel;
    [SerializeField] private GameObject cpuPanel;


    [Header("ROLE UI")]
    [SerializeField] private GameObject rolePanel;
    [SerializeField] private TextMeshProUGUI roleText;

    [Header("REPORT UI")]
    [SerializeField] private GameObject reportButton;

    [Header("IMPOSTOR UI BUTTONS")]
    [SerializeField] private GameObject impostorPanel;
    [SerializeField] private GameObject sabotageButton;

    [Header("Joystick UI")]
    [SerializeField] private GameObject JoyStick;

    [SerializeField]
    private GameObject emergencyButtonUI;

    [SerializeField]
    private GameObject winnerPanel;
    [SerializeField] private Color crewmateColor = Color.cyan;
    [SerializeField] private Color impostorColor = Color.red;
    [Header("Winner Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private TMP_Text playAgainButtonText;
    [SerializeField]
    private TMPro.TextMeshProUGUI winnerText;

    [Header("Emergency UI")]
    [SerializeField] private Button emergencyButton;
    [SerializeField] private TMP_Text emergencyButtonText;

    [Header("Vent UI")]
    [SerializeField] private GameObject ventButton;
    [SerializeField] private GameObject exitVentButton;

    [SerializeField] private GameObject ventExitButton;
    [SerializeField]
    private GameObject ghostPanel;

    [SerializeField]
    private Button whistleButton;
    [SerializeField] private TMP_Text whistleButtonText;
    [SerializeField] private Button ghostSound2Button;
    [SerializeField] private TMP_Text ghostSound2ButtonText;

    [SerializeField] private Button ghostSound3Button;
    [SerializeField] private TMP_Text ghostSound3ButtonText;

    private float emergencyCooldownRemaining;
    private bool emergencyCooldownRunning;
    [Header("Kill Cooldown UI")]
    [SerializeField] private Button killButton;
    [SerializeField] private TMP_Text killButtonText;


    private Doors currentDoor;
    private LanCableObject currentLanTask;
    private PowerTaskObject currentPowerTask;
    private LaptopTaskObject currentLaptopTask;
    private KeyboardTaskObject currentKeyboardTask;
    private DataBaseObject currentDatabaseTask;
    private CPUTaskObject currentCpuTask;
    private DeadBody currentDeadBody;
    public Player LocalPlayer;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
    }

    private void Start()
    {
        laptopTaskPanel?.SetActive(false);
        laptopPanel?.SetActive(false);
        laptopClueButton?.SetActive(false);
        laptopCluePanel?.SetActive(false);


        databaseTaskPanel?.SetActive(false);
        databasePanel?.SetActive(false);
        databaseClueButton?.SetActive(false);
        databaseCluePanel?.SetActive(false);

        keyboardTaskPanel?.SetActive(false);
        keyboardPanel?.SetActive(false);
        keyboardClueButton?.SetActive(false);
        keyboardCluePanel?.SetActive(false);

        lanTaskPanel?.SetActive(false);
        lanPanel?.SetActive(false);

        powerTaskPanel?.SetActive(false);
        powerPanel?.SetActive(false);

        cpuTaskPanel?.SetActive(false);
        cpuPanel?.SetActive(false);

        doorPanel?.SetActive(false);
        rolePanel?.SetActive(false);


        impostorPanel?.SetActive(false);
        sabotageButton?.SetActive(false);
        reportButton?.SetActive(false);

        if (reportButton != null)
        {
            Button reportUIButton =
     reportButton.GetComponentInChildren<Button>();

            if (reportUIButton != null)
            {
                reportUIButton.onClick.RemoveAllListeners();
                reportUIButton.onClick.AddListener(
                    OnReportButtonPressed);
            }
            else
            {
                Debug.LogError(
                    "REPORT BUTTON OBJECT HAS NO BUTTON COMPONENT.");
            }

            reportButton.SetActive(false);
        }
        if (doorButton != null)
            doorButton.onClick.AddListener(OnDoorButtonPressed);

        doorButton.onClick.RemoveAllListeners();
        doorButton.onClick.AddListener(OnDoorButtonPressed);
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(
                OnPlayAgainPressed);
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(
                OnExitGamePressed);
        }

        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }
        if (ghostPanel != null)
        {
            ghostPanel.SetActive(false);
        }

        if (whistleButton != null)
        {
            whistleButton.onClick.RemoveAllListeners();
            whistleButton.onClick.AddListener(
                OnWhistlePressed);
        }
        else
        {
            Debug.LogError(
                "GHOST UI FAILED | Whistle Button is not assigned.");
        }
        if (ghostSound2Button != null)
{
    ghostSound2Button.onClick.RemoveAllListeners();

    ghostSound2Button.onClick.AddListener(
        OnGhostSound2Pressed);
}

if (ghostSound3Button != null)
{
    ghostSound3Button.onClick.RemoveAllListeners();

    ghostSound3Button.onClick.AddListener(
        OnGhostSound3Pressed);
}
    }
    private void Update()
    {
        // Always update ghost whistle UI.
        UpdateGhostWhistleUI();
        UpdateKillCooldownUI();

        // Emergency cooldown is separate.
        if (!emergencyCooldownRunning)
            return;

        emergencyCooldownRemaining -= Time.deltaTime;

        if (emergencyCooldownRemaining < 0f)
        {
            emergencyCooldownRemaining = 0f;
        }

        if (emergencyButtonText != null)
        {
            emergencyButtonText.text =
                "" +
                Mathf.CeilToInt(emergencyCooldownRemaining);
        }

        if (emergencyCooldownRemaining <= 0f)
        {
            emergencyCooldownRunning = false;

            if (emergencyButton != null)
            {
                emergencyButton.interactable = true;
            }

            if (emergencyButtonText != null)
            {
                emergencyButtonText.text =
                    "";
            }
        }
    }
    private void UpdateKillCooldownUI()
    {
        if (LocalPlayer == null)
            return;

        // Never access Networked properties before Fusion Spawned().
        if (LocalPlayer.Object == null ||
            !LocalPlayer.Object.IsValid ||
            !LocalPlayer.Object.IsInSimulation)
        {
            return;
        }

        if (LocalPlayer.Role != PlayerRole.Impostor ||
            LocalPlayer.IsGhost ||
            LocalPlayer.IsDead)
        {
            if (killButton != null)
            {
                killButton.gameObject.SetActive(false);
            }

            return;
        }

        if (killButton != null)
        {
            killButton.gameObject.SetActive(true);
        }

        float remaining =
            LocalPlayer.GetKillCooldownRemaining();

        if (killButtonText != null)
        {
            if (remaining > 0f)
            {
                killButtonText.text =
                    $"\n{Mathf.CeilToInt(remaining)}";
            }
            else
            {
                killButtonText.text =
                    "";
            }
        }

        if (killButton != null)
        {
            killButton.interactable =
                LocalPlayer.CanKill();
        }
    }
    // ==================================================
    // LAPTOP CLUE
    // ==================================================

    public void ShowLaptopClueButton(bool show)
    {
        if (laptopClueButton == null)
        {
            Debug.LogError(
                "LAPTOP CLUE BUTTON FAILED | " +
                "laptopClueButton is not assigned.");

            return;
        }

        laptopClueButton.SetActive(show);

        Debug.Log(
            $"LAPTOP CLUE BUTTON ACTIVE = {show}");
    }

    public void OnLaptopClueButtonPressed()
    {
        Debug.Log("OPEN LAPTOP CLUE BUTTON PRESSED");

        if (laptopCluePanel == null)
        {
            Debug.LogError(
                "LAPTOP CLUE PANEL FAILED | " +
                "laptopCluePanel is not assigned.");

            return;
        }

        if (laptopClueButton != null)
        {
            laptopClueButton.SetActive(false);
        }

        laptopCluePanel.SetActive(true);
        laptopCluePanel.transform.SetAsLastSibling();

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(true);
        }

        SetTaskControlsLocked(true);

        Debug.Log(
            "LAPTOP CLUE PANEL OPENED");
    }

    public void CloseLaptopClue()
    {
        if (laptopCluePanel != null)
        {
            laptopCluePanel.SetActive(false);
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(false);
        }

        SetTaskControlsLocked(false);

        Debug.Log(
            "LAPTOP CLUE PANEL CLOSED");
    }
    // ==================================================
    // KEYBOARD CLUE
    // ==================================================

    public void ShowKeyboardClueButton(bool show)
    {
        if (keyboardClueButton == null)
        {
            Debug.LogError(
                "KEYBOARD CLUE BUTTON FAILED | " +
                "keyboardClueButton is not assigned.");

            return;
        }

        keyboardClueButton.SetActive(show);

        Debug.Log(
            $"KEYBOARD CLUE BUTTON ACTIVE = {show}");
    }

    public void OnKeyboardClueButtonPressed()
    {
        Debug.Log("OPEN KEYBOARD CLUE BUTTON PRESSED");

        if (keyboardCluePanel == null)
        {
            Debug.LogError(
                "KEYBOARD CLUE PANEL FAILED | " +
                "keyboardCluePanel is not assigned.");

            return;
        }

        if (keyboardClueButton != null)
        {
            keyboardClueButton.SetActive(false);
        }

        keyboardCluePanel.SetActive(true);
        keyboardCluePanel.transform.SetAsLastSibling();

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(true);
        }

        SetTaskControlsLocked(true);

        Debug.Log(
            "KEYBOARD CLUE PANEL OPENED");
    }

    public void CloseKeyboardClue()
    {
        if (keyboardCluePanel != null)
        {
            keyboardCluePanel.SetActive(false);
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(false);
        }

        SetTaskControlsLocked(false);

        Debug.Log(
            "KEYBOARD CLUE PANEL CLOSED");
    }

    public void ShowDatabaseClueButton(bool show)
    {
        if (databaseClueButton == null)
        {
            Debug.LogError(
                "DATABASE CLUE BUTTON FAILED | " +
                "databaseClueButton is not assigned.");
            return;
        }

        databaseClueButton.SetActive(show);

        Debug.Log(
            $"DATABASE CLUE BUTTON ACTIVE = {show}");
    }
    public void OnDatabaseClueButtonPressed()
    {
        Debug.Log("OPEN CLUE BUTTON PRESSED");

        if (databaseCluePanel == null)
        {
            Debug.LogError(
                "DATABASE CLUE PANEL FAILED | " +
                "databaseCluePanel is not assigned.");
            return;
        }

        if (databaseClueButton != null)
            databaseClueButton.SetActive(false);

        databaseCluePanel.SetActive(true);
        databaseCluePanel.transform.SetAsLastSibling();

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(true);
        }

        SetTaskControlsLocked(true);

        Debug.Log("DATABASE CLUE PANEL OPENED");
    }
    public void CloseDatabaseClue()
    {
        if (databaseCluePanel != null)
            databaseCluePanel.SetActive(false);

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(false);
        }

        SetTaskControlsLocked(false);

        Debug.Log("DATABASE CLUE PANEL CLOSED");
    }
    public void OnWhistlePressed()
    {
        Debug.Log("WHISTLE BUTTON PRESSED");

        if (LocalPlayer == null)
        {
            Debug.LogError(
                "WHISTLE FAILED | LocalPlayer is null.");

            return;
        }

        if (!LocalPlayer.IsGhost)
        {
            Debug.LogWarning(
                "WHISTLE FAILED | LocalPlayer is not a ghost.");

            return;
        }

        LocalPlayer.PlayGhostWhistle();
    }
    public void OnGhostSound2Pressed()
    {
        if (LocalPlayer == null)
            return;

        if (!LocalPlayer.IsGhost)
            return;

        LocalPlayer.PlayGhostSound2();
    }

    public void OnGhostSound3Pressed()
    {
        if (LocalPlayer == null)
            return;

        if (!LocalPlayer.IsGhost)
            return;

        LocalPlayer.PlayGhostSound3();
    }
    private void UpdateGhostWhistleUI()
    {
        if (LocalPlayer == null)
            return;

        if (LocalPlayer.Object == null ||
            !LocalPlayer.Object.IsValid ||
            !LocalPlayer.Object.IsInSimulation)
        {
            return;
        }

        if (!LocalPlayer.IsGhost)
        {
            if (ghostPanel != null)
                ghostPanel.SetActive(false);

            return;
        }

        if (ghostPanel != null)
            ghostPanel.SetActive(true);

        // WHISTLE
        float whistleRemaining =
            LocalPlayer.GetWhistleCooldownRemaining();

        if (whistleButtonText != null)
        {
            whistleButtonText.text =
                whistleRemaining > 0f
                    ? $"WHISTLE\n{Mathf.CeilToInt(whistleRemaining)}"
                    : "WHISTLE";
        }

        if (whistleButton != null)
        {
            whistleButton.interactable =
                whistleRemaining <= 0f;
        }

        // SOUND 2 - SCREAM
        float sound2Remaining =
            LocalPlayer.GetGhostSound2CooldownRemaining();

        if (ghostSound2ButtonText != null)
        {
            ghostSound2ButtonText.text =
                sound2Remaining > 0f
                    ? $"SCREAM\n{Mathf.CeilToInt(sound2Remaining)}"
                    : "SCREAM";
        }

        if (ghostSound2Button != null)
        {
            ghostSound2Button.interactable =
                sound2Remaining <= 0f;
        }


        // SOUND 3 - ANGER
        float sound3Remaining =
            LocalPlayer.GetGhostSound3CooldownRemaining();

        if (ghostSound3ButtonText != null)
        {
            ghostSound3ButtonText.text =
                sound3Remaining > 0f
                    ? $"SHOUT\n{Mathf.CeilToInt(sound3Remaining)}"
                    : "SHOUT";
        }

        if (ghostSound3Button != null)
        {
            ghostSound3Button.interactable =
                sound3Remaining <= 0f;
        }
    }
    public void ShowGhostPanel(bool show)
    {
        if (ghostPanel != null)
        {
            ghostPanel.SetActive(show);
        }

        if (!show)
        {
            if (whistleButtonText != null)
            {
                whistleButtonText.text = "WHISTLE";
            }

            if (whistleButton != null)
            {
                whistleButton.interactable = true;
            }
        }

        Debug.Log(
            $"GHOST PANEL ACTIVE={show}");
    }

    private void CloseAllGameplayPanels()
    {
        lanTaskPanel?.SetActive(false);
        lanPanel?.SetActive(false);

        powerTaskPanel?.SetActive(false);
        powerPanel?.SetActive(false);

        laptopTaskPanel?.SetActive(false);
        laptopPanel?.SetActive(false);

        keyboardTaskPanel?.SetActive(false);
        keyboardPanel?.SetActive(false);

        databaseTaskPanel?.SetActive(false);
        databasePanel?.SetActive(false);

        cpuTaskPanel?.SetActive(false);
        cpuPanel?.SetActive(false);

        doorPanel?.SetActive(false);
        rolePanel?.SetActive(false);
        impostorPanel?.SetActive(false);

        sabotageButton?.SetActive(false);
        reportButton?.SetActive(false);

        if (ventButton != null)
            ventButton.SetActive(false);

        if (exitVentButton != null)
            exitVentButton.SetActive(false);

        if (ventExitButton != null)
            ventExitButton.SetActive(false);

        if (emergencyButtonUI != null)
            emergencyButtonUI.SetActive(false);
    }
    public void StartEmergencyCooldown(float seconds)
    {
        emergencyCooldownRemaining = seconds;
        emergencyCooldownRunning = true;

        emergencyButton.interactable = false;
    }
    public void ShowEmergencyButton(bool show)
    {
        if (emergencyButtonUI != null)
        {
            emergencyButtonUI.SetActive(show);
        }
    }

    public void ShowWinner(string text)
    {
        CloseAllGameplayPanels();

        if (winnerText != null)
        {
            winnerText.text = text;

            winnerText.color =
                text.Contains("CREWMATE")
                    ? Color.cyan
                    : Color.red;
        }

        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
            winnerPanel.transform.SetAsLastSibling();
        }

        bool localPlayerIsHost =
            LocalPlayer != null &&
            LocalPlayer.HasStateAuthority;

        // Only the host can restart the room.
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(
                localPlayerIsHost);

            playAgainButton.interactable =
                localPlayerIsHost;
        }

        if (exitGameButton != null)
        {
            exitGameButton.gameObject.SetActive(true);
            exitGameButton.interactable = true;
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(true);
        }

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(false);

            MobileControlsManager.Instance
                .ShowJoystick(false);

            MobileControlsManager.Instance
                .ShowLook(false);
        }

        Debug.Log(
            $"WINNER PANEL OPENED | " +
            $"Winner={text} | " +
            $"LocalHost={localPlayerIsHost}");
    }
    public void OnPlayAgainPressed()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "PLAY AGAIN FAILED | LocalPlayer is null.");

            return;
        }

        if (!LocalPlayer.HasStateAuthority)
        {
            Debug.LogWarning(
                "PLAY AGAIN BLOCKED | Only the host can restart.");

            return;
        }

        if (GameLogic.Instance == null)
        {
            Debug.LogError(
                "PLAY AGAIN FAILED | GameLogic.Instance is null.");

            return;
        }

        if (playAgainButton != null)
        {
            playAgainButton.interactable = false;
        }

        GameLogic.Instance.RequestPlayAgain();

        Debug.Log("HOST PRESSED PLAY AGAIN");
    }

    public void OnExitGamePressed()
    {
        FusionMenuUIGameplay gameplayMenu =
            FindFirstObjectByType<FusionMenuUIGameplay>(
                FindObjectsInactive.Include);

        if (gameplayMenu == null)
        {
            Debug.LogError(
                "EXIT FAILED | FusionMenuUIGameplay was not found.");

            return;
        }

        if (exitGameButton != null)
        {
            exitGameButton.interactable = false;
        }

        gameplayMenu.DisconnectFromGame();

        Debug.Log("EXIT GAME PRESSED");
    }


    public void ResetWinnerUIForNewRound()
    {
        winnerPanel?.SetActive(false);

        ShowGhostPanel(false);

        if (winnerText != null)
        {
            winnerText.text = "";
        }

        if (playAgainButton != null)
        {
            playAgainButton.interactable = true;
        }

        if (exitGameButton != null)
        {
            exitGameButton.interactable = true;
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(false);
        }

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance.SetControlsEnabled(true);
            MobileControlsManager.Instance.ShowJoystick(true);
            MobileControlsManager.Instance.ShowLook(true);
        }

        Debug.Log(
            "WINNER AND GHOST UI CLOSED | WAITING FOR NEW ROUND");
    }
    // ================= DOOR =================
    public void ShowDoorUI(bool show)
    {
        doorPanel?.SetActive(show);

        if (!show)
            currentDoor = null;
    }

    public void SetDoor(Doors door)
    {
        currentDoor = door;

        if (doorButtonText != null)
            doorButtonText.text = "OPEN / CLOSE DOOR";
    }

    public void OnDoorButtonPressed()
    {
        Debug.Log($"DOOR BUTTON PRESSED Frame={Time.frameCount} Time={Time.time}");
        if (LocalPlayer == null)
        {
            Debug.Log("LOCAL PLAYER NULL");
            return;
        }

        if (currentDoor == null)
        {
            Debug.Log("CURRENT DOOR NULL");
            return;
        }

        Debug.Log("PRESSING DOOR BUTTON");

        LocalPlayer.RPC_ToggleDoor(currentDoor.Object);
    }

    // ==================================================
    // SHARED TASK CONTROL METHODS
    // ==================================================

   public void LockTaskControls()
    {
        JoyStick?.SetActive(false);

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(false);
        }
    }

    public void UnlockTaskControls()
    {
        JoyStick?.SetActive(true);

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);
        }
    }

    public void SetTaskControlsLocked(bool locked)
    {
        if (locked)
        {
            LockTaskControls();
        }
        else
        {
            UnlockTaskControls();
        }
    }

    // ==================================================
    // LAN TASK
    // ==================================================

    public void SetLanTask(LanCableObject task)
    {
        currentLanTask = task;
    }

    public void ShowLanUI(bool show)
    {
        if (lanTaskPanel != null)
        {
            lanTaskPanel.SetActive(show);
        }

        if (!show)
        {
            currentLanTask = null;
        }
    }

    public void OnLanButtonPressed()
    {
        OpenCurrentLanTask();
    }

    public void OpenLanTaskPanel()
    {
        OpenCurrentLanTask();
    }

    private void OpenCurrentLanTask()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "LAN OPEN FAILED | LocalPlayer is null.");

            return;
        }

        if (currentLanTask == null)
        {
            Debug.LogError(
                "LAN OPEN FAILED | currentLanTask is null.");

            return;
        }

        currentLanTask.Interact(LocalPlayer);

        // Hide the small LAN interaction button.
        lanTaskPanel?.SetActive(false);

        Debug.Log("LAN INTERACTION BUTTON HIDDEN");
    }

    public void ShowLanTaskPanel()
    {
        lanPanel?.SetActive(true);
        lanTaskPanel?.SetActive(false);

        LockTaskControls();
    }

    public void CloseLanTaskPanel()
    {
        lanPanel?.SetActive(false);

        UnlockTaskControls();

        Debug.Log("LAN TASK CLOSED | PLAYER UNLOCKED");
        InputManager inputManager =
      FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
        }

        ShowJoystick(true);
    }

    // ==================================================
    // POWER TASK
    // ==================================================

    public void SetPowerTask(PowerTaskObject task)
    {
        currentPowerTask = task;

        Debug.Log(
            currentPowerTask != null
                ? $"POWER TASK ASSIGNED | Object={currentPowerTask.name}"
                : "POWER TASK CLEARED");
    }

    public void ShowPowerUI(bool show)
    {
        if (powerTaskPanel != null)
        {
            powerTaskPanel.SetActive(show);
        }

        // Do not clear currentPowerTask here.
        // ReachDetector will clear it when leaving the trigger.
    }

    public void OnPowerButtonPressed()
    {
        OpenCurrentPowerTask();
    }

    public void OpenPowerTaskPanel()
    {
        OpenCurrentPowerTask();
    }

    private void OpenCurrentPowerTask()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "POWER OPEN FAILED | LocalPlayer is null.");

            return;
        }

        if (currentPowerTask == null)
        {
            Debug.LogError(
                "POWER OPEN FAILED | currentPowerTask is null.");

            return;
        }

        PowerTaskObject taskToOpen =
            currentPowerTask;

        // Hide the small interaction button first.
        ShowPowerUI(false);

        // Open the actual puzzle.
        taskToOpen.Interact(LocalPlayer);

        Debug.Log(
            $"POWER TASK OPEN REQUESTED | Object={taskToOpen.name}");
    }

    public void ShowPowerTaskPanel()
    {
        if (powerPanel != null)
        {
            powerPanel.SetActive(true);
        }

        ShowPowerUI(false);

        LockTaskControls();

        Debug.Log("POWER TASK PANEL SHOWN");
    }

    public void ClosePowerTaskPanel()
    {
        if (powerPanel != null)
        {
            powerPanel.SetActive(false);
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
            inputManager.ClearMovementInput();
        }

        SetTaskControlsLocked(false);

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);

            MobileControlsManager.Instance
                .ShowJoystick(true);
        }

        ShowJoystick(true);

        Debug.Log(
            "POWER TASK CLOSED | ROTATION AND MOVEMENT UNLOCKED");
    }

    // ==================================================
    // LAPTOP TASK
    // ==================================================

    public void SetLaptopTask(LaptopTaskObject task)
    {
        currentLaptopTask = task;
    }

    public void ShowLaptopUI(bool show)
    {
        if (laptopTaskPanel != null)
        {
            laptopTaskPanel.SetActive(show);
        }

        if (!show)
        {
            currentLaptopTask = null;
        }
    }

    public void OnLaptopButtonPressed()
    {
        OpenCurrentLaptopTask();
    }

    public void OpenLaptopTaskPanel()
    {
        OpenCurrentLaptopTask();
    }

    private void OpenCurrentLaptopTask()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "LAPTOP OPEN FAILED | LocalPlayer is null.");

            return;
        }

        if (currentLaptopTask == null)
        {
            Debug.LogError(
                "LAPTOP OPEN FAILED | currentLaptopTask is null.");

            return;
        }

        currentLaptopTask.Interact(LocalPlayer);

        laptopTaskPanel?.SetActive(false);

        Debug.Log("LAPTOP INTERACTION BUTTON HIDDEN");
    }

    public void ShowLaptopTaskPanel()
    {
        if (laptopPanel != null)
        {
            laptopPanel.SetActive(true);
        }

        ShowLaptopUI(false);

        LockTaskControls();

        Debug.Log(
            "LAPTOP TASK PANEL SHOWN | PLAYER LOCKED");
    }

    public void CloseLaptopTaskPanel()
    {
        if (laptopPanel != null)
        {
            laptopPanel.SetActive(false);
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
            inputManager.ClearMovementInput();
        }

        SetTaskControlsLocked(false);

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);

            MobileControlsManager.Instance
                .ShowJoystick(true);
        }

        ShowJoystick(true);

       
    }
    public void MarkLaptopDone()
    {
        currentLaptopTask = null;
    }

    // ==================================================
    // KEYBOARD TASK
    // ==================================================

    public void SetKeyboardTask(KeyboardTaskObject task)
    {
        currentKeyboardTask = task;
    }

    public void ShowKeyboardUI(bool show)
    {
        if (keyboardTaskPanel != null)
        {
            keyboardTaskPanel.SetActive(show);
        }

        if (!show)
        {
            currentKeyboardTask = null;
        }
    }

    public void OnKeyboardButtonPressed()
    {
        OpenCurrentKeyboardTask();
    }

    public void OpenKeyboardTaskPanel()
    {
        OpenCurrentKeyboardTask();
    }

    private void OpenCurrentKeyboardTask()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "KEYBOARD OPEN FAILED | LocalPlayer is null.");

            return;
        }

        if (currentKeyboardTask == null)
        {
            Debug.LogError(
                "KEYBOARD OPEN FAILED | currentKeyboardTask is null.");

            return;
        }

        currentKeyboardTask.Interact(LocalPlayer);

        keyboardTaskPanel?.SetActive(false);

        Debug.Log("KEYBOARD INTERACTION BUTTON HIDDEN");
    }

    public void ShowKeyboardTaskPanel()
    {
        keyboardPanel?.SetActive(true);
        keyboardTaskPanel?.SetActive(false);

        LockTaskControls();
    }

    public void CloseKeyboardTaskPanel()
    {
        keyboardPanel?.SetActive(false);

        UnlockTaskControls();

        Debug.Log("KEYBOARD TASK CLOSED | PLAYER UNLOCKED");
        InputManager inputManager =
      FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
        }

        ShowJoystick(true);
    }

    // ==================================================
    // DATABASE TASK
    // ==================================================

    public void SetDatabaseTask(DataBaseObject task)
    {
        currentDatabaseTask = task;

        Debug.Log(
            currentDatabaseTask != null
                ? $"DATABASE TASK ASSIGNED | Object={currentDatabaseTask.name}"
                : "DATABASE TASK CLEARED");
    }

    public void ShowDatabaseUI(bool show)
    {
        if (databaseTaskPanel != null)
        {
            databaseTaskPanel.SetActive(show);
        }

        // Do not clear currentDatabaseTask here.
        // ReachDetector will clear it when the player exits the trigger.
    }

    public void OnDatabaseButtonPressed()
    {
        OpenCurrentDatabaseTask();
    }

    public void OpenDatabaseTaskPanel()
    {
        OpenCurrentDatabaseTask();
    }

    private void OpenCurrentDatabaseTask()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "DATABASE OPEN FAILED | LocalPlayer is null.");

            return;
        }

        if (currentDatabaseTask == null)
        {
            Debug.LogError(
                "DATABASE OPEN FAILED | currentDatabaseTask is null.");

            return;
        }

        DataBaseObject taskToOpen =
            currentDatabaseTask;

        // Hide the small interaction button.
        ShowDatabaseUI(false);

        // Open the actual Database puzzle.
        taskToOpen.Interact(LocalPlayer);

        Debug.Log(
            $"DATABASE TASK OPEN REQUESTED | Object={taskToOpen.name}");
    }

    public void ShowDatabaseTaskPanel()
    {
        if (databasePanel != null)
        {
            databasePanel.SetActive(true);
        }

        ShowDatabaseUI(false);

        LockTaskControls();

        Debug.Log(
            "DATABASE TASK PANEL SHOWN | PLAYER LOCKED");
    }

    public void CloseDatabaseTaskPanel()
    {
        if (databasePanel != null)
        {
            databasePanel.SetActive(false);
        }

        UnlockTaskControls();

        Debug.Log(
            "DATABASE TASK CLOSED | PLAYER UNLOCKED");
    }

    public void MarkDatabaseDone()
    {
        currentDatabaseTask = null;

        ShowDatabaseUI(false);

        Debug.Log("DATABASE TASK MARKED COMPLETE");
    }

    // ==================================================
    // CPU TASK
    // ==================================================
    public void SetCPUTask(CPUTaskObject cpuTask)
    {
        currentCpuTask = cpuTask;

        Debug.Log(
            currentCpuTask != null
                ? $"CPU TASK ASSIGNED | Object={currentCpuTask.name}"
                : "CPU TASK CLEARED");
    }

    public void ShowCPUUI(bool show)
    {
        if (cpuTaskPanel != null)
        {
            cpuTaskPanel.SetActive(show);
        }

        if (!show)
        {
            currentCpuTask = null;
        }
    }

    public void OpenCpuTaskPanel()
    {
        OpenCurrentCpuTask();
    }

    public void OnCPUButtonPressed()
    {
        OpenCurrentCpuTask();
    }

    private void OpenCurrentCpuTask()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "CPU OPEN FAILED | LocalPlayer is null.");

            return;
        }

        if (currentCpuTask == null)
        {
            Debug.LogError(
                "CPU OPEN FAILED | currentCpuTask is null.");

            return;
        }


        currentCpuTask.Interact(LocalPlayer);

        cpuTaskPanel?.SetActive(false);

        Debug.Log("CPU INTERACTION BUTTON HIDDEN");
    }

    public void CloseCpuPanel()
    {
        if (cpuPanel != null)
        {
            cpuPanel.SetActive(false);
        }

        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.SetLookLocked(false);
            inputManager.ClearMovementInput();
        }

        SetTaskControlsLocked(false);

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);

            MobileControlsManager.Instance
                .ShowJoystick(true);
        }

        ShowJoystick(true);

        Debug.Log(
            "POWER TASK CLOSED | ROTATION AND MOVEMENT UNLOCKED");
    }
    // ================= ROLE (FIXED) =================
    public void ShowRole(string role, Color color)
    {
        Debug.Log("ROLE IS: " + role);
        if (rolePanel == null || roleText == null)
            return;

        rolePanel.SetActive(true);

        roleText.text = role;
        roleText.color = color;

        CancelInvoke(nameof(HideRole));
        Invoke(nameof(HideRole), 3f);
    }

    private void HideRole()
    {
        rolePanel?.SetActive(false);

        if (roleText != null)
            roleText.text = "";
    }

    // ================= PLAYER READY FIX =================
    public void DidSetReady()
    {
        Debug.Log("Player is ready!");
    }
    public void showimpostorui (bool show)
    {
       
        
            impostorPanel.SetActive(show);
        
    }
    public void SetImpostorUI(bool show)
    {
        Debug.Log("SET IMPOSTOR UI: " + show);

        if (impostorPanel != null)
        {
            impostorPanel.SetActive(show);

            if (!show)
                sabotageButton?.SetActive(false);
        }
        else
        {
            Debug.LogError("IMPOSTOR PANEL NOT ASSIGNED");
        }
    }
    public void SetDeadBody(DeadBody deadBody)
    {
        currentDeadBody = deadBody;

        Debug.Log(
            currentDeadBody != null
                ? $"CURRENT DEAD BODY SET | {currentDeadBody.name}"
                : "CURRENT DEAD BODY CLEARED");
    }

    public void ShowReportButton(bool show)
    {
        if (reportButton == null)
        {
            Debug.LogError(
                "REPORT BUTTON FAILED | reportButton is null.");

            return;
        }

        reportButton.SetActive(show);

        if (show)
        {
            reportButton.transform.SetAsLastSibling();
        }

        Debug.Log(
            $"REPORT BUTTON ACTIVE={reportButton.activeInHierarchy}");
    }

    public void OnReportButtonPressed()
    {
        if (LocalPlayer == null)
        {
            Debug.LogError(
                "REPORT FAILED | LocalPlayer is null.");

            return;
        }

        if (LocalPlayer.IsDead)
        {
            Debug.LogWarning(
                "REPORT BLOCKED | Dead players cannot report.");

            ShowReportButton(false);
            return;
        }

        if (currentDeadBody == null)
        {
            Debug.LogError(
                "REPORT FAILED | currentDeadBody is null.");

            ShowReportButton(false);
            return;
        }

        DeadBody bodyToReport =
            currentDeadBody;

        currentDeadBody = null;

        ShowReportButton(false);

        bodyToReport.Report(LocalPlayer);

        Debug.Log(
            $"REPORT SENT | Victim={bodyToReport.DeadPlayerName}");
    }
    public void ShowJoystick(bool show)
    {
        if (JoyStick != null)
        {
            JoyStick.SetActive(show);
        }
    }

    public void ShowVentButton(bool show)
    {
        Debug.Log("ShowVentButton = " + show);

        ventButton.SetActive(show);
    }
    public void ShowExitVentButton(bool show)
    {
        exitVentButton.SetActive(show);
        ventButton.SetActive(false);

    }
    public void ShowVentExitButton(bool show)
    {
        if (ventExitButton != null)
            ventExitButton.SetActive(show);
    }

    // ================= SABOTAGE =================
    public void ShowSabotageButton(bool show)
    {
        if (sabotageButton == null)
            return;

        bool canShow =
            show &&
            LocalPlayer != null &&
            LocalPlayer.HasInputAuthority &&
            LocalPlayer.Role == PlayerRole.Impostor &&
            !LocalPlayer.IsDead;

        sabotageButton.SetActive(canShow);
    }

    public void OnSabotageButtonPressed()
    {
        Debug.Log("SABOTAGE BUTTON PRESSED");

        if (LocalPlayer == null)
        {
            Debug.LogError("SABOTAGE FAILED: LocalPlayer is null.");
            return;
        }

        LocalPlayer.SabotageButtonPressed();
    }

}