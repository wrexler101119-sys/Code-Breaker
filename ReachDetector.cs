using UnityEngine;
using UnityEngine.EventSystems;

public class ReachDetector : MonoBehaviour
{
    private Doors currentDoor;
    public Doors CurrentDoor => currentDoor;

    private LanCableObject currentLanTask;
    private PowerTaskObject currentPowerTask;
    private LaptopTaskObject currentLaptopTask;
    private KeyboardTaskObject currentKeyboardTask;
    private DataBaseObject currentDatabaseTask;
    private CPUTaskObject currentCPUTask;
    private DatabaseClue currentDatabaseClue;
    private KeyboardClue currentKeyboardClue;
    private LaptopClue currentLaptopClue;

    private NetworkTask currentSabotageTask;
    public NetworkTask CurrentSabotageTask => currentSabotageTask;

    private DeadBody currentDeadBody;
    public DeadBody CurrentDeadBody => currentDeadBody;

    private EmergencyButton currentEmergencyButton;

    private InteractableHighlight currentHighlight;
    private int currentHighlightColliderCount;
    public EmergencyButton CurrentEmergencyButton => currentEmergencyButton;
  
    private Player player;

    private Vent currentVent;
    public Vent CurrentVent => currentVent;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }
    private NetworkTask GetNetworkTask(
    Component taskObject)
    {
        if (taskObject == null)
            return null;

        NetworkTask task =
            taskObject.GetComponent<NetworkTask>();

        if (task == null)
        {
            task =
                taskObject.GetComponentInParent<NetworkTask>();
        }

        return task;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (player == null ||
            !player.HasInputAuthority)
        {
            return;
        }
        // ==================================================
        // LAPTOP CLUE
        // ==================================================

        LaptopClue laptopClue =
            other.GetComponentInParent<LaptopClue>();

        if (laptopClue != null)
        {
            currentLaptopClue = laptopClue;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowLaptopClueButton(true);
            }

            Debug.Log(
                $"LAPTOP CLUE IN REACH | " +
                $"Object={laptopClue.name}");
        }

        // ==================================================
        // DATABASE CLUE
        // ==================================================
        DatabaseClue databaseClue =
            other.GetComponentInParent<DatabaseClue>();

        if (databaseClue != null)
        {
            currentDatabaseClue = databaseClue;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowDatabaseClueButton(true);
            }

            Debug.Log(
                $"DATABASE CLUE IN REACH | " +
                $"Object={databaseClue.name}");
        }
        // ==================================================
        // KEYBOARD CLUE
        // ==================================================

        KeyboardClue keyboardClue =
            other.GetComponentInParent<KeyboardClue>();

        if (keyboardClue != null)
        {
            currentKeyboardClue = keyboardClue;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowKeyboardClueButton(true);
            }

            Debug.Log(
                $"KEYBOARD CLUE IN REACH | " +
                $"Object={keyboardClue.name}");
        }
        // =========================
        // DEAD BODY
        // =========================

        DeadBody detectedBody =
        other.GetComponentInParent<DeadBody>();

    if (detectedBody != null)
    {
        Debug.Log(
            $"DEAD BODY TRIGGER ENTER | " +
            $"Collider={other.name} | " +
            $"Body={detectedBody.name} | " +
            $"Victim={detectedBody.DeadPlayerName}");

        if (player == null)
        {
            player = GetComponentInParent<Player>();
        }

        // Dead players cannot report bodies.
        if (player != null && player.IsDead)
        {
            Debug.Log(
                "REPORT BUTTON BLOCKED | Local player is dead.");

            return;
        }

        currentDeadBody = detectedBody;

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton.SetDeadBody(
                detectedBody);

            GAMEUIMANAGER.Singleton.ShowReportButton(
                true);

            Debug.Log("REPORT BUTTON SHOWN");
        }
        else
        {
            Debug.LogError(
                "REPORT BUTTON FAILED | " +
                "GAMEUIMANAGER.Singleton is null.");
        }

        return;
    
        }
        // =========================
        // SABOTAGE TARGET
        // =========================

        SabotageTarget sabotageTarget =
            other.GetComponent<SabotageTarget>();

        if (sabotageTarget == null)
        {
            sabotageTarget =
                other.GetComponentInParent<SabotageTarget>();
        }

        if (sabotageTarget != null &&
            player.Role == PlayerRole.Impostor &&
            !player.IsDead)
        {
            NetworkTask sabotageTask =
                sabotageTarget.Task;

            if (sabotageTask != null &&
                sabotageTask.IsCompleted)
            {
                currentSabotageTask =
                    sabotageTask;

                if (GAMEUIMANAGER.Singleton != null)
                {
                    GAMEUIMANAGER.Singleton
                        .ShowSabotageButton(true);
                }

                Debug.Log(
                    $"SABOTAGE TARGET ENTERED: " +
                    $"{sabotageTask.name}");
            }
        }

        // =========================
        // HINGED DOOR
        // =========================

        Doors door =
            other.GetComponent<Doors>();

        if (door == null)
        {
            door =
                other.GetComponentInParent<Doors>();
        }

        if (door != null)
        {
            currentDoor = door;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .SetDoor(door);

                GAMEUIMANAGER.Singleton
                    .ShowDoorUI(true);
            }
        }

        // =========================
        // SLIDING DOOR
        // =========================

        SlidingDoor slidingDoor =
            other.GetComponent<SlidingDoor>();

        if (slidingDoor == null)
        {
            slidingDoor =
                other.GetComponentInParent<SlidingDoor>();
        }

        if (slidingDoor != null)
        {
            slidingDoor.Open();
        }
        // ==================================================
        // LAN TASK
        // ==================================================
        SetHighlightTarget(other);
        LanCableObject lan =
            other.GetComponentInParent<LanCableObject>();

        if (lan != null)
        {
            NetworkTask task = GetNetworkTask(lan);

            if (task != null &&
                !task.IsCompleted)
            {
                currentLanTask = lan;

                GAMEUIMANAGER.Singleton?
                    .SetLanTask(lan);

                GAMEUIMANAGER.Singleton?
                    .ShowLanUI(true);

                Debug.Log(
                    $"LAN TASK IN REACH | Task={task.name}");
            }
        }

        // ==================================================
        // POWER TASK
        // ==================================================
        SetHighlightTarget(other);
        PowerTaskObject power =
            other.GetComponentInParent<PowerTaskObject>();

        if (power != null)
        {
            NetworkTask task = GetNetworkTask(power);

            if (task != null &&
                !task.IsCompleted)
            {
                currentPowerTask = power;

                GAMEUIMANAGER.Singleton?
                    .SetPowerTask(power);

                GAMEUIMANAGER.Singleton?
                    .ShowPowerUI(true);

                Debug.Log(
                    $"POWER TASK IN REACH | Task={task.name}");
            }
        }

        // ==================================================
        // LAPTOP TASK
        // ==================================================
        SetHighlightTarget(other);
        LaptopTaskObject laptop =
            other.GetComponentInParent<LaptopTaskObject>();

        if (laptop != null)
        {
            NetworkTask task = GetNetworkTask(laptop);

            if (task != null &&
                !task.IsCompleted)
            {
                currentLaptopTask = laptop;

                GAMEUIMANAGER.Singleton?
                    .SetLaptopTask(laptop);

                GAMEUIMANAGER.Singleton?
                    .ShowLaptopUI(true);

                Debug.Log(
                    $"LAPTOP TASK IN REACH | Task={task.name}");
            }
        }

        // ==================================================
        // KEYBOARD TASK
        // ==================================================
        SetHighlightTarget(other);
        KeyboardTaskObject keyboard =
            other.GetComponentInParent<KeyboardTaskObject>();

        if (keyboard != null)
        {
            NetworkTask task = GetNetworkTask(keyboard);

            if (task != null &&
                !task.IsCompleted)
            {
                currentKeyboardTask = keyboard;

                GAMEUIMANAGER.Singleton?
                    .SetKeyboardTask(keyboard);

                GAMEUIMANAGER.Singleton?
                    .ShowKeyboardUI(true);

                Debug.Log(
                    $"KEYBOARD TASK IN REACH | Task={task.name}");
            }
        }

        // ==================================================
        // DATABASE TASK
        // ==================================================
        SetHighlightTarget(other);
        DataBaseObject database =
            other.GetComponentInParent<DataBaseObject>();

        if (database != null)
        {
            NetworkTask task = GetNetworkTask(database);

            if (task != null &&
                !task.IsCompleted)
            {
                currentDatabaseTask = database;

                GAMEUIMANAGER.Singleton?
                    .SetDatabaseTask(database);

                GAMEUIMANAGER.Singleton?
                    .ShowDatabaseUI(true);

                Debug.Log(
                    $"DATABASE TASK IN REACH | Task={task.name}");
            }
        }

        // ==================================================
        // CPU TASK
        // ==================================================
        SetHighlightTarget(other);
        CPUTaskObject cpuTask =
      other.GetComponent<CPUTaskObject>();

        if (cpuTask == null)
        {
            cpuTask =
                other.GetComponentInParent<CPUTaskObject>();
        }

        if (cpuTask == null)
        {
            cpuTask =
                other.GetComponentInChildren<CPUTaskObject>(true);
        }

        if (cpuTask != null)
        {
            NetworkTask task =
                cpuTask.GetComponent<NetworkTask>();

            if (task == null)
            {
                task =
                    cpuTask.GetComponentInParent<NetworkTask>();
            }

            if (task != null && !task.IsCompleted)
            {
                currentCPUTask = cpuTask;

                GAMEUIMANAGER.Singleton
                    .SetCPUTask(currentCPUTask);

                GAMEUIMANAGER.Singleton
                    .ShowCPUUI(true);

                Debug.Log(
                    $"CPU ENTERED REACH | Object={cpuTask.name}");
            }
        }

        // =========================
        // EMERGENCY BUTTON
        // =========================
        SetHighlightTarget(other);
        EmergencyButton emergency =
            other.GetComponent<EmergencyButton>();

        if (emergency == null)
        {
            emergency =
                other.GetComponentInParent<EmergencyButton>();
        }

        if (emergency != null &&
            !player.IsDead)
        {
            currentEmergencyButton =
                emergency;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowEmergencyButton(true);
            }
        }

        // =========================
        // VENT
        // =========================
        SetHighlightTarget(other);
        Vent vent =
            other.GetComponent<Vent>();

        if (vent == null)
        {
            vent =
                other.GetComponentInParent<Vent>();
        }

        if (vent != null &&
            player.Role == PlayerRole.Impostor &&
            !player.IsDead)
        {
            currentVent = vent;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowVentButton(true);
            }

            Debug.Log(
                $"VENT IN REACH: {vent.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null ||
            !player.HasInputAuthority)
        {
            return;
        }
        // ==================================================
        // LAPTOP CLUE EXIT
        // ==================================================

        LaptopClue laptopClue =
            other.GetComponentInParent<LaptopClue>();

        if (laptopClue != null &&
            laptopClue == currentLaptopClue)
        {
            currentLaptopClue = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowLaptopClueButton(false);
            }

            Debug.Log(
                $"LAPTOP CLUE LEFT REACH | " +
                $"Object={laptopClue.name}");
        }
        // ==================================================
        // DATABASE CLUE EXIT
        // ==================================================
        DatabaseClue databaseClue =
            other.GetComponentInParent<DatabaseClue>();

        if (databaseClue != null &&
            databaseClue == currentDatabaseClue)
        {
            currentDatabaseClue = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowDatabaseClueButton(false);
            }

            Debug.Log(
                $"DATABASE CLUE LEFT REACH | " +
                $"Object={databaseClue.name}");
        }
        // ==================================================
        // KEYBOARD CLUE EXIT
        // ==================================================

        KeyboardClue keyboardClue =
            other.GetComponentInParent<KeyboardClue>();

        if (keyboardClue != null &&
            keyboardClue == currentKeyboardClue)
        {
            currentKeyboardClue = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowKeyboardClueButton(false);
            }

            Debug.Log(
                $"KEYBOARD CLUE LEFT REACH | " +
                $"Object={keyboardClue.name}");
        }
        // =========================
        // SABOTAGE TARGET
        // =========================

        SabotageTarget sabotageTarget =
            other.GetComponent<SabotageTarget>();

        if (sabotageTarget == null)
        {
            sabotageTarget =
                other.GetComponentInParent<SabotageTarget>();
        }

        if (sabotageTarget != null &&
            sabotageTarget.Task == currentSabotageTask)
        {
            ClearCurrentSabotageTarget();
        }

        // =========================
        // DEAD BODY
        // =========================

        DeadBody detectedBody =
            other.GetComponentInParent<DeadBody>();

        if (detectedBody != null &&
            currentDeadBody == detectedBody)
        {
            Debug.Log(
                $"DEAD BODY TRIGGER EXIT | " +
                $"Collider={other.name} | " +
                $"Body={detectedBody.name}");

            currentDeadBody = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton.SetDeadBody(
                    null);

                GAMEUIMANAGER.Singleton.ShowReportButton(
                    false);
            }

            return;
        }

        // =========================
        // ROTATING DOOR
        // =========================

        Doors door =
            other.GetComponent<Doors>();

        if (door == null)
        {
            door =
                other.GetComponentInParent<Doors>();
        }

        if (door != null &&
            door == currentDoor)
        {
            currentDoor = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowDoorUI(false);
            }
        }

        // ==================================================
        // LAN TASK EXIT
        // ==================================================
        ClearHighlightTarget(other);
        LanCableObject lan =
            other.GetComponentInParent<LanCableObject>();

        if (lan != null &&
            lan == currentLanTask)
        {
            currentLanTask = null;

            GAMEUIMANAGER.Singleton?
                .SetLanTask(null);

            GAMEUIMANAGER.Singleton?
                .ShowLanUI(false);

            Debug.Log(
                $"LAN TASK LEFT REACH | Task={lan.name}");
        }

        // ==================================================
        // POWER TASK EXIT
        // ==================================================
        ClearHighlightTarget(other);
        PowerTaskObject power =
            other.GetComponentInParent<PowerTaskObject>();

        if (power != null &&
            power == currentPowerTask)
        {
            currentPowerTask = null;

            GAMEUIMANAGER.Singleton?
                .SetPowerTask(null);

            GAMEUIMANAGER.Singleton?
                .ShowPowerUI(false);

            Debug.Log(
                $"POWER TASK LEFT REACH | Task={power.name}");
        }

        // ==================================================
        // LAPTOP TASK EXIT
        // ==================================================
        ClearHighlightTarget(other);
        LaptopTaskObject laptop =
            other.GetComponentInParent<LaptopTaskObject>();

        if (laptop != null &&
            laptop == currentLaptopTask)
        {
            currentLaptopTask = null;

            GAMEUIMANAGER.Singleton?
                .SetLaptopTask(null);

            GAMEUIMANAGER.Singleton?
                .ShowLaptopUI(false);

            Debug.Log(
                $"LAPTOP TASK LEFT REACH | Task={laptop.name}");
        }

        // ==================================================
        // KEYBOARD TASK EXIT
        // ==================================================
        ClearHighlightTarget(other);
        KeyboardTaskObject keyboard =
            other.GetComponentInParent<KeyboardTaskObject>();

        if (keyboard != null &&
            keyboard == currentKeyboardTask)
        {
            currentKeyboardTask = null;

            GAMEUIMANAGER.Singleton?
                .SetKeyboardTask(null);

            GAMEUIMANAGER.Singleton?
                .ShowKeyboardUI(false);

            Debug.Log(
                $"KEYBOARD TASK LEFT REACH | Task={keyboard.name}");
        }

        // ==================================================
        // DATABASE TASK EXIT
        // ==================================================
        ClearHighlightTarget(other);
        DataBaseObject database =
            other.GetComponentInParent<DataBaseObject>();

        if (database != null &&
            database == currentDatabaseTask)
        {
            currentDatabaseTask = null;

            GAMEUIMANAGER.Singleton?
                .SetDatabaseTask(null);

            GAMEUIMANAGER.Singleton?
                .ShowDatabaseUI(false);

            Debug.Log(
                $"DATABASE TASK LEFT REACH | Task={database.name}");
        }

        // ==================================================
        // CPU TASK EXIT
        // ==================================================
        ClearHighlightTarget(other);
        CPUTaskObject cpuTask =
     other.GetComponent<CPUTaskObject>();

        if (cpuTask == null)
        {
            cpuTask =
                other.GetComponentInParent<CPUTaskObject>();
        }

        if (cpuTask == null)
        {
            cpuTask =
                other.GetComponentInChildren<CPUTaskObject>(true);
        }

        if (cpuTask != null &&
            cpuTask == currentCPUTask)
        {
            currentCPUTask = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .SetCPUTask(null);

                GAMEUIMANAGER.Singleton
                    .ShowCPUUI(false);
            }

            Debug.Log("CPU EXITED REACH");
        }

        // =========================
        // EMERGENCY BUTTON
        // =========================
        ClearHighlightTarget(other);
        EmergencyButton emergency =
            other.GetComponent<EmergencyButton>();

        if (emergency == null)
        {
            emergency =
                other.GetComponentInParent<EmergencyButton>();
        }

        if (emergency != null &&
            emergency == currentEmergencyButton)
        {
            currentEmergencyButton = null;

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowEmergencyButton(false);
            }

            if (MobileControlsManager.Instance != null)
            {
                MobileControlsManager.Instance
                    .SetControlsEnabled(true);
            }
        }

        // =========================
        // VENT
        // =========================
        ClearHighlightTarget(other);
        Vent vent =
            other.GetComponent<Vent>();

        if (vent == null)
        {
            vent =
                other.GetComponentInParent<Vent>();
        }

        if (vent != null &&
            vent == currentVent)
        {
            // Keep the vent reference while the player is inside.
            if (!player.IsInVent)
            {
                currentVent = null;

                if (GAMEUIMANAGER.Singleton != null)
                {
                    GAMEUIMANAGER.Singleton
                        .ShowVentButton(false);
                }
            }
        }
    }

    private void Update()
    {
        if (player == null || !player.HasInputAuthority)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }
    }

    private void HandleTap(Vector2 screenPos)
    {
        Ray ray =
            Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            5f))
        {
            Debug.Log("HIT: " + hit.collider.name);

            PowerTaskObject power =
                hit.collider
                    .GetComponentInParent<PowerTaskObject>();

            if (power != null)
            {
                power.Interact(player);
                return;
            }

            LanCableObject lan =
                hit.collider
                    .GetComponentInParent<LanCableObject>();

            if (lan != null)
            {
                lan.Interact(player);
                return;
            }

            LaptopTaskObject laptop =
                hit.collider
                    .GetComponentInParent<LaptopTaskObject>();

            if (laptop != null)
            {
                laptop.Interact(player);
                return;
            }

            KeyboardTaskObject keyboard =
                hit.collider
                    .GetComponentInParent<KeyboardTaskObject>();

            if (keyboard != null)
            {
                keyboard.Interact(player);
                return;
            }

    
            DataBaseObject database =
                hit.collider
                    .GetComponentInParent<DataBaseObject>();

            if (database != null)
            {
                database.Interact(player);
                return;
            }

            CPUTaskObject cpu =
                hit.collider
                    .GetComponentInParent<CPUTaskObject>();

            if (cpu != null)
            {
                cpu.Interact(player);
                return;
            }
        }
    }

    public void ClearCurrentSabotageTarget()
    {
        currentSabotageTask = null;

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton
                .ShowSabotageButton(false);
        }
    }

    public void ClearCurrentVent()
    {
        currentVent = null;

        GAMEUIMANAGER.Singleton
            .ShowVentButton(false);
    }
    private void SetHighlightTarget(Collider other)
    {
        if (other == null)
            return;

        InteractableHighlight newHighlight =
            other.GetComponentInParent<InteractableHighlight>();

        if (newHighlight == null)
        {
            newHighlight =
                other.GetComponentInChildren<InteractableHighlight>(true);
        }

        if (newHighlight == null)
            return;

        if (currentHighlight == newHighlight)
        {
            currentHighlightColliderCount++;
            return;
        }

        ClearCurrentHighlight();

        currentHighlight = newHighlight;
        currentHighlightColliderCount = 1;

        currentHighlight.SetHighlight(true);

        Debug.Log(
            $"HIGHLIGHT ON | Object={currentHighlight.name}");
    }

    private void ClearHighlightTarget(Collider other)
    {
        if (other == null || currentHighlight == null)
            return;

        InteractableHighlight leavingHighlight =
            other.GetComponentInParent<InteractableHighlight>();

        if (leavingHighlight == null)
        {
            leavingHighlight =
                other.GetComponentInChildren<InteractableHighlight>(true);
        }

        if (leavingHighlight != currentHighlight)
            return;

        currentHighlightColliderCount--;

        if (currentHighlightColliderCount <= 0)
        {
            ClearCurrentHighlight();
        }
    }

    private void ClearCurrentHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);

            Debug.Log(
                $"HIGHLIGHT OFF | Object={currentHighlight.name}");
        }

        currentHighlight = null;
        currentHighlightColliderCount = 0;
    }

    private void OnDisable()
    {
        ClearCurrentHighlight();
    }
}