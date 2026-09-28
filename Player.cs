using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using TMPro;

public class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer[] modelParts;
    [SerializeField] private KCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private AudioSource source;

    [Header("NameTag")]
    [SerializeField] private NameTag nameTag;
    [SerializeField] private ReachDetector reach;

    [Header("Kill System")]
    [SerializeField] private float killRange = 3f;
    [SerializeField] private float killCooldown = 15f;

    [Header("Death")]
    [SerializeField] private NetworkPrefabRef deadBodyPrefab;



    [SerializeField] private MeshRenderer bodyRenderer;
    [SerializeField] private MeshRenderer leftArmRenderer;
    [SerializeField] private MeshRenderer rightArmRenderer;
  

    [Header("Ghost Sound")]
    [SerializeField]
    private GhostSoundController ghostSoundController;

    [SerializeField]
    private float ghostWhistleCooldown = 10f;
    [SerializeField] private float ghostSound2Cooldown = 10f;
    [SerializeField] private float ghostSound3Cooldown = 10f;
    [Networked]
    private TickTimer GhostWhistleTimer { get; set; }
    [Networked]
    private TickTimer GhostSound2Timer { get; set; }

    [Networked]
    private TickTimer GhostSound3Timer { get; set; }
    [Header("Settings")]
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField]
    private Vector3 jumpImpulse =
        new Vector3(0f, 10f, 0f);
    [Networked, OnChangedRender(nameof(OnPlayerColorChanged))]
    public int PlayerColorIndex { get; private set; }
    [Networked]
    private TickTimer killTimer { get; set; }
    private bool pendingHostVentTeleport;
    private NetworkId pendingHostVentId;
    [Networked, OnChangedRender(nameof(OnGhostChanged))]
    public bool IsGhost { get; private set; }

    [Networked, OnChangedRender(nameof(OnVentChanged))]
    public NetworkBool IsInVent { get; set; }
    private static readonly Color GhostColor =
    new Color32(160, 255, 255, 255);
    [Networked]
    public NetworkId CurrentVentId { get; set; }

    // =========================
    // NETWORKED
    // =========================

    public bool IsLocalDead => IsDead;

    [Networked]
    public string Name { get; private set; }

    [Networked]
    public PlayerRole Role { get; set; }

    [Networked]
    public bool IsDead { get; set; }

    [Networked, OnChangedRender(nameof(OnReadyChanged))]
    public bool IsReady { get; set; }

    [Networked, OnChangedRender(nameof(OnJumped))]
    private int JumpSync { get; set; }

    public static Player Local;

    [Networked]
    private NetworkButtons PreviousButtons { get; set; }

    [Header("Sabotage")]
    [SerializeField] private float sabotageInvisibilitySeconds = 10f;

    [Networked, OnChangedRender(nameof(OnInvisibilityChanged))]
    public NetworkBool IsInvisible { get; private set; }

    [Networked]
    private TickTimer InvisibilityTimer { get; set; }

    private InputManager inputManager;
    private Vector2 baseLookRotation;

    private static readonly Color[] PlayerColors =
{
    new Color32(198, 17, 17, 255),   // Red
    new Color32(19, 46, 210, 255),   // Blue
    new Color32(17, 127, 45, 255),   // Green
    new Color32(246, 246, 87, 255),  // Yellow
    new Color32(239, 125, 13, 255),  // Orange
    new Color32(255, 99, 171, 255),  // Pink
    new Color32(107, 47, 188, 255),  // Purple
    new Color32(56, 255, 221, 255),  // Cyan
    new Color32(80, 239, 57, 255),   // Lime
    new Color32(113, 73, 30, 255),   // Brown
    new Color32(214, 224, 240, 255), // White
    new Color32(63, 71, 78, 255)     // Black
};
    // =========================
    // READY
    // =========================
    private void OnReadyChanged()
    {
        if (HasInputAuthority && IsReady)
        {
            GAMEUIMANAGER.Singleton.DidSetReady();
        }
    }

    // =========================
    // SPAWN
    // =========================
    public override void Spawned()
    {
       

        ApplyPlayerColor();

        if (HasInputAuthority)
        {
            Local = this;

            if (UIManager.Singleton != null)
            {
                UIManager.Singleton.LocalPlayer = this;
                UIManager.Singleton.ShowReadyButton();
            }

            foreach (MeshRenderer renderer in modelParts)
            {
                if (renderer != null)
                {
                    renderer.shadowCastingMode =
                        UnityEngine.Rendering
                            .ShadowCastingMode.ShadowsOnly;
                }
            }

            inputManager =
                Runner.GetComponent<InputManager>();

            if (inputManager != null)
            {
                inputManager.LocalPlayer = this;
            }

            string savedName =
                PlayerPrefs.GetString(
                    "Photon.Menu.Username",
                    "Player");

            RPC_SetPlayerName(savedName);

            if (CameraFollow.Singleton != null)
            {
                CameraFollow.Singleton.SetTarget(
                    camTarget,
                    this);
            }

            if (HasInputAuthority &&
                CameraFOVManager.Instance != null)
            {
                CameraFOVManager.Instance.RefreshAllCameras();
            }

            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton.LocalPlayer = this;
            }

            if (UIManager.Singleton != null)
            {
                UIManager.Singleton.LocalPlayer = this;
            }
        }

        UpdateNameTag();
        RefreshGhostVisibility();


    }
    public void PlayGhostSound2()
    {
        if (!HasInputAuthority)
            return;

        if (!IsGhost)
            return;

        if (!GhostSound2Timer.ExpiredOrNotRunning(Runner))
            return;

        RPC_RequestGhostSound2(1);
    }

    public void PlayGhostSound3()
    {
        if (!HasInputAuthority)
            return;

        if (!IsGhost)
            return;

        if (!GhostSound3Timer.ExpiredOrNotRunning(Runner))
            return;

        RPC_RequestGhostSound3(2);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestGhostSound2(int soundIndex)
    {
        if (!IsGhost)
            return;

        if (!GhostSound2Timer.ExpiredOrNotRunning(Runner))
            return;

        GhostSound2Timer =
            TickTimer.CreateFromSeconds(
                Runner,
                ghostSound2Cooldown);

        RPC_PlayGhostSound(soundIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestGhostSound3(int soundIndex)
    {
        if (!IsGhost)
            return;

        if (!GhostSound3Timer.ExpiredOrNotRunning(Runner))
            return;

        GhostSound3Timer =
            TickTimer.CreateFromSeconds(
                Runner,
                ghostSound3Cooldown);

        RPC_PlayGhostSound(soundIndex);
    }
    public float GetGhostSound2CooldownRemaining()
    {
        if (GhostSound2Timer.ExpiredOrNotRunning(Runner))
            return 0f;

        return GhostSound2Timer.RemainingTime(Runner) ?? 0f;
    }

    public float GetGhostSound3CooldownRemaining()
    {
        if (GhostSound3Timer.ExpiredOrNotRunning(Runner))
            return 0f;

        return GhostSound3Timer.RemainingTime(Runner) ?? 0f;
    }
    public void PlayGhostWhistle()
    {
        Debug.Log(
            $"WHISTLE REQUESTED | " +
            $"Player={Name} | " +
            $"InputAuthority={HasInputAuthority} | " +
            $"Ghost={IsGhost}");

        if (!HasInputAuthority)
        {
            Debug.LogWarning(
                "WHISTLE BLOCKED | Player has no Input Authority.");

            return;
        }

        if (!IsGhost)
        {
            Debug.LogWarning(
                "WHISTLE BLOCKED | Player is not a ghost.");

            return;
        }

        if (!GhostWhistleTimer
            .ExpiredOrNotRunning(Runner))
        {
            Debug.LogWarning(
                $"WHISTLE BLOCKED | Cooldown=" +
                $"{GetWhistleCooldownRemaining():F1}");

            return;
        }

        RPC_RequestGhostSound(0);
    }

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    private void RPC_RequestGhostSound(int soundIndex)
    {
        if (!IsGhost)
        {
            Debug.LogWarning(
                "WHISTLE REJECTED BY HOST | Player is not a ghost.");

            return;
        }

        if (!GhostWhistleTimer
            .ExpiredOrNotRunning(Runner))
        {
            Debug.LogWarning(
                "WHISTLE REJECTED BY HOST | Cooldown active.");

            return;
        }

        GhostWhistleTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                ghostWhistleCooldown);

        RPC_PlayGhostSound(soundIndex);

        Debug.Log(
            $"WHISTLE APPROVED BY HOST | " +
            $"Player={Name} | Index={soundIndex}");
    }

    [Rpc(
        RpcSources.StateAuthority,
        RpcTargets.All)]
    private void RPC_PlayGhostSound(int soundIndex)
    {
        if (ghostSoundController == null)
        {
            Debug.LogError(
                $"GHOST SOUND FAILED | " +
                $"GhostSoundController missing on {Name}");

            return;
        }

        ghostSoundController.PlaySoundLocal(
            soundIndex);
    }

    public float GetWhistleCooldownRemaining()
    {
        if (GhostWhistleTimer
            .ExpiredOrNotRunning(Runner))
        {
            return 0f;
        }

        return GhostWhistleTimer
            .RemainingTime(Runner) ?? 0f;
    }

    public bool CanGhostWhistle()
    {
        return IsGhost &&
               GhostWhistleTimer
                   .ExpiredOrNotRunning(Runner);
    }
   
    private void OnPlayerColorChanged()
    {
        ApplyPlayerColor();
    }

    private void ApplyPlayerColor()
    {
        if (PlayerColors.Length == 0)
            return;

        int safeIndex =
            Mathf.Clamp(
                PlayerColorIndex,
                0,
                PlayerColors.Length - 1);

        Color selectedColor =
            PlayerColors[safeIndex];

        ApplyColorToRenderer(
            bodyRenderer,
            selectedColor);

        ApplyColorToRenderer(
            leftArmRenderer,
            selectedColor);

        ApplyColorToRenderer(
            rightArmRenderer,
            selectedColor);

        Debug.Log(
            $"PLAYER COLOR APPLIED | " +
            $"Player={Name} | " +
            $"Index={safeIndex}");
       
    }
    private void ApplyColorToRenderer(
        MeshRenderer targetRenderer,
        Color color)
    {
        if (targetRenderer == null)
            return;

        MaterialPropertyBlock propertyBlock =
            new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(
            propertyBlock);

        propertyBlock.SetColor(
            "_BaseColor",
            color);

        propertyBlock.SetColor(
            "_Color",
            color);

        targetRenderer.SetPropertyBlock(
            propertyBlock);
    }
    public override void Despawned(
       NetworkRunner runner,
       bool hasState)
    {
        if (Local == this)
        {
            Local = null;
        }

        if (UIManager.Singleton != null &&
            UIManager.Singleton.LocalPlayer == this)
        {
            UIManager.Singleton.LocalPlayer = null;
        }

        if (GAMEUIMANAGER.Singleton != null &&
            GAMEUIMANAGER.Singleton.LocalPlayer == this)
        {
            GAMEUIMANAGER.Singleton.LocalPlayer = null;
            GAMEUIMANAGER.Singleton.ShowGhostPanel(false);
        }
    }

    // =========================
    // FIXED UPDATE
    // =========================
    public override void FixedUpdateNetwork()
    {
        // =========================
        // HOST VENT TELEPORT
        // =========================

        if (HasStateAuthority &&
            pendingHostVentTeleport)
        {
            NetworkId ventId =
                pendingHostVentId;

            pendingHostVentTeleport = false;
            pendingHostVentId = default;

            Debug.Log(
                $"PROCESSING HOST VENT TELEPORT | " +
                $"Player={Name} | " +
                $"VentId={ventId}");

            ExecuteVentTeleport(ventId);
            return;
        }

        // =========================
        // INVISIBILITY TIMER
        // =========================

        if (HasStateAuthority &&
            IsInvisible &&
            InvisibilityTimer.Expired(Runner))
        {
            Debug.Log(
                $"INVISIBILITY TIMER EXPIRED | " +
                $"Player={Name} | " +
                $"InVent={IsInVent} | " +
                $"Dead={IsDead}");

            IsInvisible = false;
            InvisibilityTimer = default;
        }
        // =========================
        // GAME ENDED
        // =========================

        if (GameLogic.Instance != null &&
            GameLogic.Instance.GameEnded)
        {
            kcc.SetInputDirection(Vector3.zero);

            PreviousButtons = default;

            return;
        }
        // =========================
        // MEETING
        // =========================

        if (MeetingManager.Instance != null &&
            MeetingManager.Instance.Object != null &&
            MeetingManager.Instance.Object.IsValid &&
            MeetingManager.Instance.MeetingActive)
        {
            kcc.SetInputDirection(Vector3.zero);
            return;
        }

        // =========================
        // DEAD
        // =========================
     
        // Dead players become ghosts.
        // Ghosts can still move.
        if (IsDead && !IsGhost)
            return;

        // =========================
        // INSIDE VENT
        // =========================

        if (IsInVent)
        {
            kcc.SetInputDirection(Vector3.zero);

            if (GetInput(out NetInput ventInput))
            {
                kcc.AddLookRotation(
                    ventInput.LookDelta * lookSensitivity,
                    -maxPitch,
                    maxPitch);
            }

            return;
        }

        // =========================
        // NORMAL INPUT
        // =========================

        if (GetInput(out NetInput input))
        {
            // LOOK
            kcc.AddLookRotation(
                input.LookDelta * lookSensitivity,
                -maxPitch,
                maxPitch);

            // MOVE
            Vector3 moveDirection =
                kcc.FixedData.TransformRotation *
                input.Direction.X0Y();

            kcc.SetInputDirection(moveDirection);

            // JUMP
            if (input.Buttons.WasPressed(
                PreviousButtons,
                InputButton.Jump))
            {
                if (kcc.FixedData.IsGrounded)
                {
                    kcc.Jump(jumpImpulse);
                    JumpSync++;
                }
            }

            // INTERACT
            if (input.Buttons.WasPressed(
                PreviousButtons,
                InputButton.UseAbility))
            {
                if (reach != null &&
                    reach.CurrentDoor != null)
                {
                    RPC_ToggleDoor(
                        reach.CurrentDoor.Object);
                }

                if (MeetingManager.Instance != null &&
                    MeetingManager.Instance.MeetingActive)
                {
                    return;
                }
            }

            // KILL
            if (input.Buttons.WasPressed(
                PreviousButtons,
                InputButton.Fire))
            {
                if (Role == PlayerRole.Impostor)
                {
                    RPC_RequestKill();
                }
            }

            PreviousButtons = input.Buttons;

            baseLookRotation =
                kcc.GetLookRotation();
        }
    }

    // =========================
    // RENDER
    // =========================
    public override void Render()
    {
        if (kcc.IsPredictingLookRotation)
        {
            Vector2 look = kcc.GetLookRotation();

            look += inputManager.AccumulatedMouseDelta * lookSensitivity;

            kcc.SetLookRotation(look);
        }

        UpdateCamTarget();
        UpdateNameTag();
    }

    // =========================
    // CAMERA
    // =========================
    private void UpdateCamTarget()
    {
        Vector2 look =
            kcc.GetLookRotation();

        camTarget.localRotation =
            Quaternion.Euler(
                look.x,
                0f,
                0f);

        transform.rotation =
            Quaternion.Euler(
                0f,
                look.y,
                0f);
    }

    private void OnJumped()
    {
        if (source != null)
        {
            source.Play();
        }
    }

    // =========================
    // NAME TAG
    // =========================
    private void UpdateNameTag()
    {
        if (nameTag == null)
            return;

        nameTag.SetName(Name);

        TextMeshProUGUI text =
            nameTag.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.color =
                (Role == PlayerRole.Impostor)
                ? Color.red
                : Color.white;
        }

        Canvas canvas =
            nameTag.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.enabled = !HasInputAuthority;
        }
    }
    // =========================
    // DOOR
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ToggleDoor(NetworkObject doorObject)
    {
        if (IsGhost)
            return;
        Debug.Log($"RPC EXECUTED Frame={Time.frameCount}");

        if (doorObject == null)
            return;

        Doors door = doorObject.GetComponent<Doors>();

        if (door == null)
            return;

        door.Toggle();
    }

    // =========================
    // NAME RPC
    // =========================
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string playerName)
    {
        Name = playerName;
    }

    // =========================
    // ROLE UI
    // =========================
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ShowRole(PlayerRole assignedRole)
    {
        Debug.Log(
            $"ROLE UI RECEIVED | " +
            $"Player={Name} | " +
            $"Role={assignedRole}");

        if (GAMEUIMANAGER.Singleton == null)
        {
            Debug.LogError(
                "ROLE UI FAILED | GAMEUIMANAGER.Singleton is null.");

            return;
        }

        if (assignedRole == PlayerRole.Impostor)
        {
            GAMEUIMANAGER.Singleton.ShowRole(
                "HACKER",
                Color.red);

            GAMEUIMANAGER.Singleton.SetImpostorUI(true);
        }
        else
        {
            GAMEUIMANAGER.Singleton.ShowRole(
                "TECHNICIAN",
                Color.green);

            GAMEUIMANAGER.Singleton.SetImpostorUI(false);
        }
    }

    // =========================
    // READY RPC
    // =========================
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
    }
    public void ReadyButtonPressed()
    {
        if (!Object || !Object.IsValid)
        {
            Debug.LogError("Player NetworkObject is not initialized yet.");
            return;
        }

        RPC_SetReady();
    }
    // =========================
    // TELEPORT
    // =========================
    public void Teleport(
        Vector3 position,
        Quaternion rotation)
    {
        kcc.SetPosition(position);

        kcc.SetLookRotation(rotation);

    }

    // =========================
    // KILL
    // =========================
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestKill()
    {
        TryKill();
    }

    private void TryKill()
    {
        if (!HasStateAuthority)
            return;

        if (GameLogic.Instance != null &&
            GameLogic.Instance.GameEnded)
        {
            return;
        }

        if (MeetingManager.Instance != null &&
            MeetingManager.Instance.MeetingActive)
        {
            return;
        }

        if (IsDead || IsGhost)
        {
            Debug.LogWarning(
                "KILL REJECTED | Player is dead or ghost.");

            return;
        }

        if (Role != PlayerRole.Impostor)
        {
            Debug.LogWarning(
                "KILL REJECTED | Player is not an impostor.");

            return;
        }

        if (!killTimer.ExpiredOrNotRunning(Runner))
        {
            Debug.LogWarning(
                "KILL REJECTED | Cooldown is active.");

            return;
        }

        Player target =
            FindClosestPlayer();

        if (target == null)
        {
            Debug.Log(
                "KILL FAILED | No crewmate in range.");

            return;
        }

        if (target.IsDead ||
            target.IsGhost ||
            target.Role == PlayerRole.Impostor)
        {
            return;
        }

        target.RPC_Die();

        killTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                killCooldown);

        Debug.Log(
            $"KILL SUCCESS | " +
            $"Killer={Name} | " +
            $"Target={target.Name} | " +
            $"Cooldown={killCooldown}");
    }
    public void ResetForNewRound()
    {
        if (!HasStateAuthority)
            return;

        IsReady = false;

        // Restore alive state.
        IsDead = false;
        IsGhost = false;

        IsInVent = false;
        CurrentVentId = default;

        IsInvisible = false;
        InvisibilityTimer = default;

        GhostWhistleTimer = default;
        GhostSound2Timer = default;
        GhostSound3Timer = default;
        killTimer = default;

        Role = PlayerRole.Crewmate;

        PreviousButtons = default;

        pendingHostVentTeleport = false;
        pendingHostVentId = default;

        if (kcc != null)
        {
            kcc.SetActive(true);
            kcc.SetInputDirection(Vector3.zero);
        }

        // Restore the real networked player color.
        ApplyPlayerColor();

        // Restore renderers and nametag.
        RefreshPlayerVisibility();

        RPC_ResetGhostStateLocal();

        Debug.Log(
            $"PLAYER RESET FOR NEW ROUND | " +
            $"Player={Name} | " +
            $"Dead={IsDead} | " +
            $"Ghost={IsGhost} | " +
            $"ColorIndex={PlayerColorIndex}");
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetGhostStateLocal()
    {
        // Restore actual player color on every device.
        ApplyPlayerColor();

        foreach (MeshRenderer renderer in modelParts)
        {
            if (renderer == null)
                continue;

            renderer.enabled = true;

            renderer.shadowCastingMode =
                HasInputAuthority
                    ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                    : UnityEngine.Rendering.ShadowCastingMode.On;
        }

        if (nameTag != null)
        {
            nameTag.SetColor(Color.white);

            nameTag.gameObject.SetActive(
                !HasInputAuthority);
        }

        if (HasInputAuthority &&
            GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton.ShowGhostPanel(false);
        }

        RefreshGhostVisibility();

        Debug.Log(
            $"LOCAL GHOST STATE RESET | Player={Name}");
    }

    private Player FindClosestPlayer()
    {
        Player[] players =
            FindObjectsByType<Player>(
                FindObjectsSortMode.None);

        float closestDistance =
            killRange;

        Player closestPlayer = null;

        foreach (Player player in players)
        {
            if (player == null ||
                player == this)
            {
                continue;
            }

            if (player.IsDead ||
                player.IsGhost)
            {
                continue;
            }

            // Impostors cannot kill fellow impostors.
            if (player.Role == PlayerRole.Impostor)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
    public float GetKillCooldownRemaining()
    {
        if (Runner == null ||
            killTimer.ExpiredOrNotRunning(Runner))
        {
            return 0f;
        }

        return killTimer.RemainingTime(Runner) ?? 0f;
    }

    public bool CanKill()
    {
        if (Runner == null)
            return false;

        return
            Role == PlayerRole.Impostor &&
            !IsDead &&
            !IsGhost &&
            killTimer.ExpiredOrNotRunning(Runner);
    }
   
    public void KillButtonPressed()
    {
        if (!HasInputAuthority)
        {
            Debug.LogWarning(
                "KILL BLOCKED | No Input Authority.");

            return;
        }

        if (IsDead || IsGhost)
        {
            Debug.LogWarning(
                "KILL BLOCKED | Player is dead or ghost.");

            return;
        }

        if (Role != PlayerRole.Impostor)
        {
            Debug.LogWarning(
                "KILL BLOCKED | Player is not an impostor.");

            return;
        }

        if (!killTimer.ExpiredOrNotRunning(Runner))
        {
            Debug.LogWarning(
                $"KILL BLOCKED | Cooldown=" +
                $"{GetKillCooldownRemaining():F1}");

            return;
        }

        RPC_RequestKill();
    }

    // =========================
    // DIE
    // =========================
    public const int AvailableColorCount = 12;

    public void SetPlayerColorIndex(int colorIndex)
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning(
                "COLOR ASSIGNMENT BLOCKED | No State Authority.");

            return;
        }

        PlayerColorIndex = Mathf.Clamp(
            colorIndex,
            0,
            AvailableColorCount - 1);

        ApplyPlayerColor();

        Debug.Log(
            $"UNIQUE PLAYER COLOR ASSIGNED | " +
            $"Player={Object.InputAuthority} | " +
            $"ColorIndex={PlayerColorIndex}");
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die()
    {
        IsDead = true;
        IsGhost = true;
        if (HasInputAuthority)
        {
            GAMEUIMANAGER.Singleton.ShowGhostPanel(true);
        }
        BecomeGhostVisual();
        foreach (Player player in
    FindObjectsByType<Player>(
        FindObjectsSortMode.None))
        {
            player.RefreshGhostVisibility();
        }
        // ✅ SPAWN DEAD BODY
        if (Object.HasStateAuthority)
        {
            Runner.Spawn(
        deadBodyPrefab,
        transform.position,
        Quaternion.Euler(90f, 0f, 0f),
        null,
        (runner, obj) =>
        {
            DeadBody deadBody =
                obj.GetComponent<DeadBody>();

            if (deadBody == null)
                return;

            deadBody.InitializeDeadBody(
                Name,
                PlayerColorIndex);

            Debug.Log(
                $"DEAD BODY CREATED | " +
                $"Player={Name} | " +
                $"ColorIndex={PlayerColorIndex}");
        });
        }
        if (HasInputAuthority)
        {
            GAMEUIMANAGER.Singleton.showimpostorui(false);
            GAMEUIMANAGER.Singleton.ShowReportButton(false);
            GAMEUIMANAGER.Singleton.ShowVentButton(false);
            GAMEUIMANAGER.Singleton.ShowEmergencyButton(false);
            GAMEUIMANAGER.Singleton.ShowSabotageButton(false);
        }

    }
    private void BecomeGhostVisual()
    {
        ApplyColorToRenderer(
            bodyRenderer,
            GhostColor);

        ApplyColorToRenderer(
            leftArmRenderer,
            GhostColor);

        ApplyColorToRenderer(
            rightArmRenderer,
            GhostColor);

        if (nameTag != null)
        {
            nameTag.SetColor(Color.gray);
        }

        Debug.Log(
            $"👻 {Name} became a ghost.");
    }
    private void BecomeAliveVisual()
    {
        // Do not use originalPlayerColor.
        // Recalculate from the synchronized color index.
        ApplyPlayerColor();

        foreach (MeshRenderer renderer in modelParts)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        if (nameTag != null)
        {
            nameTag.SetColor(Color.white);
        }

        Debug.Log(
            $"PLAYER COLOR RESTORED | " +
            $"Player={Name} | " +
            $"ColorIndex={PlayerColorIndex}");
    }
    // =========================
    // FORCE KILL
    // =========================
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_KillPlayer(
        NetworkObject targetObject)
    {
        if (targetObject == null)
            return;

        Player target =
            targetObject.GetComponent<Player>();

        if (target == null)
            return;

        if (target.IsDead)
            return;

        target.RPC_Die();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Eject()
    {
        Debug.Log(
            $"VOTED OUT | Player={Name} | " +
            $"Authority={Object.InputAuthority}");

        // ---------------------------------------------------------
        // BECOME DEAD + GHOST
        // ---------------------------------------------------------

        IsDead = true;
        IsGhost = true;

        // ---------------------------------------------------------
        // GHOST VISUAL
        // ---------------------------------------------------------

        BecomeGhostVisual();

        // Refresh ghost visibility for every player.
        foreach (Player player in
            FindObjectsByType<Player>(
                FindObjectsSortMode.None))
        {
            if (player != null)
            {
                player.RefreshGhostVisibility();
            }
        }

        // ---------------------------------------------------------
        // LOCAL ELIMINATED PLAYER
        // ---------------------------------------------------------

        if (HasInputAuthority)
        {
            if (GAMEUIMANAGER.Singleton != null)
            {
                // Show ghost controls.
                GAMEUIMANAGER.Singleton
                    .ShowGhostPanel(true);

                // Remove living-player controls.
                GAMEUIMANAGER.Singleton
                    .showimpostorui(false);

                GAMEUIMANAGER.Singleton
                    .ShowReportButton(false);

                GAMEUIMANAGER.Singleton
                    .ShowVentButton(false);

                GAMEUIMANAGER.Singleton
                    .ShowEmergencyButton(false);

                GAMEUIMANAGER.Singleton
                    .ShowSabotageButton(false);
            }

            // IMPORTANT:
            // Ghost must remain active so it can move.
            if (kcc != null)
            {
                kcc.SetActive(true);
                kcc.SetInputDirection(Vector3.zero);
            }
        }

        // ---------------------------------------------------------
        // DO NOT DISABLE THE WHOLE GAME OBJECT
        // ---------------------------------------------------------

        if (gameObject != null)
        {
            gameObject.SetActive(true);
        }

        Debug.Log(
            $"GHOST EJECTION COMPLETE | " +
            $"Player={Name} | " +
            $"Dead={IsDead} | " +
            $"Ghost={IsGhost}");
    }
    // =========================
    // VENT SYSTEM
    // =========================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestUseVent(NetworkId ventId)
    {
        Debug.Log(
            $"HOST RECEIVED CLIENT VENT REQUEST | " +
            $"Player={Name} | " +
            $"VentId={ventId}");

        ExecuteVentTeleport(ventId);
    }
    public void RequestUseVent(NetworkId ventId)
    {
        if (IsGhost)
            return;
        if (GameLogic.Instance != null &&
    GameLogic.Instance.GameEnded)
        {
            Debug.LogWarning(
                "VENT BLOCKED | Game has ended.");

            return;
        }
        if (!HasInputAuthority)
        {
            Debug.LogWarning(
                $"VENT REQUEST IGNORED | " +
                $"Player={Name} has no Input Authority.");

            return;
        }

        Debug.Log(
            $"VENT REQUEST | " +
            $"Player={Name} | " +
            $"InputAuthority={HasInputAuthority} | " +
            $"StateAuthority={HasStateAuthority} | " +
            $"VentId={ventId}");

        // Host:
        // Queue the teleport so KCC.SetPosition runs during FixedUpdateNetwork.
        if (HasStateAuthority)
        {
            pendingHostVentId = ventId;
            pendingHostVentTeleport = true;

            Debug.Log(
                $"HOST VENT TELEPORT QUEUED | " +
                $"Player={Name} | " +
                $"VentId={ventId}");

            return;
        }

        // Client:
        // Ask State Authority to teleport this player.
        RPC_RequestUseVent(ventId);
    }
    private void ExecuteVentTeleport(NetworkId ventId)
    {
        if (!HasStateAuthority)
        {
            Debug.LogError(
                $"VENT TELEPORT REJECTED | " +
                $"Player={Name} has no State Authority.");

            return;
        }

        if (kcc == null)
        {
            Debug.LogError(
                $"VENT TELEPORT FAILED | " +
                $"Player={Name} has no KCC reference.");

            return;
        }

        if (!Runner.TryFindObject(
            ventId,
            out NetworkObject ventObject))
        {
            Debug.LogError(
                $"VENT TELEPORT FAILED | " +
                $"Vent {ventId} was not found.");

            return;
        }

        Vent sourceVent =
            ventObject.GetComponent<Vent>();

        if (sourceVent == null)
        {
            Debug.LogError(
                $"VENT TELEPORT FAILED | " +
                $"{ventObject.name} has no Vent component.");

            return;
        }

        Vent destinationVent =
            sourceVent.ConnectedVent;

        if (destinationVent == null)
        {
            Debug.LogError(
                $"VENT TELEPORT FAILED | " +
                $"{sourceVent.name} has no ConnectedVent.");

            return;
        }

        if (destinationVent.ExitPoint == null)
        {
            Debug.LogError(
                $"VENT TELEPORT FAILED | " +
                $"{destinationVent.name} has no ExitPoint.");

            return;
        }

        Vector3 destinationPosition =
            destinationVent.ExitPoint.position;

        Debug.Log(
            $"EXECUTING VENT TELEPORT | " +
            $"Player={Name} | " +
            $"From={transform.position} | " +
            $"To={destinationPosition}");

        kcc.SetInputDirection(Vector3.zero);
        kcc.SetPosition(destinationPosition);

        CurrentVentId =
            destinationVent.Object.Id;

        IsInVent = true;

        Debug.Log(
            $"VENT TELEPORT COMPLETED | " +
            $"Player={Name} | " +
            $"Position={transform.position} | " +
            $"InVent={IsInVent}");
    }

    private void OnVentChanged()
    {
        RefreshPlayerVisibility();

        if (HasInputAuthority)
        {
            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton
                    .ShowVentExitButton(IsInVent);
            }

            if (MobileControlsManager.Instance != null)
            {
                MobileControlsManager.Instance
                    .ShowJoystick(!IsInVent);
            }
        }

        Debug.Log(
            $"VENT STATE CHANGED | " +
            $"Player={Name} | " +
            $"Input={HasInputAuthority} | " +
            $"InVent={IsInVent} | " +
            $"Invisible={IsInvisible}");
    }

    public void ExitVent()
    {
        if (!HasInputAuthority)
            return;

        RPC_ExitVent();
    }

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority,
        InvokeLocal = true)]
    private void RPC_ExitVent()
    {
        if (!HasStateAuthority)
            return;

        if (!IsInVent)
            return;

        IsInVent = false;
        CurrentVentId = default;

        Debug.Log(
            $"PLAYER EXITED VENT | Player={Name}");
    }
    private void OnInvisibilityChanged()
    {
        Debug.Log(
            $"INVISIBILITY CHANGED | " +
            $"Player={Name} | " +
            $"Invisible={IsInvisible} | " +
            $"InVent={IsInVent} | " +
            $"Dead={IsDead}");

        RefreshPlayerVisibility();
    }

    private void RefreshPlayerVisibility()
    {
        bool shouldBeVisible =
            !IsDead &&
            !IsInVent &&
            !IsInvisible;

        Debug.Log(
            $"REFRESH VISIBILITY | " +
            $"Player={Name} | " +
            $"Visible={shouldBeVisible} | " +
            $"Invisible={IsInvisible} | " +
            $"InVent={IsInVent} | " +
            $"Dead={IsDead}");

        foreach (MeshRenderer renderer in modelParts)
        {
            if (renderer == null)
                continue;

            renderer.enabled = shouldBeVisible;

            // The local first-person body remains shadow-only.
            // Other players render normally.
            renderer.shadowCastingMode =
                HasInputAuthority
                    ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                    : UnityEngine.Rendering.ShadowCastingMode.On;
        }

        if (nameTag != null)
        {
            nameTag.gameObject.SetActive(
                shouldBeVisible &&
                !HasInputAuthority);
        }
    }
    // =========================
    // SABOTAGE
    // =========================

    public void SabotageButtonPressed()
    {
        if (IsGhost)
            return;

        if (GameLogic.Instance != null &&
    GameLogic.Instance.GameEnded)
        {
            Debug.LogWarning(
                "SABOTAGE BLOCKED | Game has ended.");

            return;
        }
        if (!HasInputAuthority)
        {
            Debug.LogWarning(
                "SABOTAGE FAILED: This is not the local player.");
            return;
        }

        if (Role != PlayerRole.Impostor)
        {
            Debug.LogWarning(
                "SABOTAGE FAILED: Only the Impostor can sabotage.");
            return;
        }

        if (reach == null)
        {
            Debug.LogError(
                "SABOTAGE FAILED: ReachDetector is missing.");
            return;
        }

        NetworkTask targetTask =
            reach.CurrentSabotageTask;

        if (targetTask == null ||
            targetTask.Object == null ||
            !targetTask.Object.IsValid)
        {
            Debug.LogWarning(
                "SABOTAGE FAILED: No completed task is currently in reach.");
            return;
        }

        NetworkId targetTaskId =
            targetTask.Object.Id;

        Debug.Log(
            $"REQUESTING SABOTAGE: {targetTask.name}");

        // Hide the local button immediately. The completed visual will
        // also deactivate after the server approves the sabotage.
        reach.ClearCurrentSabotageTarget();

        RPC_RequestSabotage(targetTaskId);
    }

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    private void RPC_RequestSabotage(
        NetworkId targetTaskId)
    {
        Debug.Log(
            $"SABOTAGE RPC RECEIVED | " +
            $"Role={Role} | " +
            $"Target={targetTaskId}");

        if (Role != PlayerRole.Impostor)
        {
            Debug.LogWarning(
                "SABOTAGE REJECTED: Player is not the Impostor.");
            return;
        }

        if (IsDead)
        {
            Debug.LogWarning(
                "SABOTAGE REJECTED: Dead players cannot sabotage.");
            return;
        }

        if (!Runner.TryFindObject(
            targetTaskId,
            out NetworkObject taskObject))
        {
            Debug.LogWarning(
                "SABOTAGE REJECTED: Task NetworkObject was not found.");
            return;
        }

        NetworkTask task =
            taskObject.GetComponent<NetworkTask>();

        if (task == null)
        {
            Debug.LogWarning(
                "SABOTAGE REJECTED: NetworkTask component is missing.");
            return;
        }

        if (!task.IsCompleted)
        {
            Debug.LogWarning(
                $"SABOTAGE REJECTED: {task.name} is not completed.");
            return;
        }

        bool resetSucceeded =
            task.ResetFromSabotage();

        if (!resetSucceeded)
        {
            Debug.LogWarning(
                $"SABOTAGE FAILED: Could not reset {task.name}.");
            return;
        }

        if (GlobalTaskTracker.Instance != null)
        {
            GlobalTaskTracker.Instance.RemoveTask();
        }
        else
        {
            Debug.LogError(
                "SABOTAGE WARNING: GlobalTaskTracker.Instance is null.");
        }

        IsInvisible = true;

        InvisibilityTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                sabotageInvisibilitySeconds);

        Debug.Log(
            $"SABOTAGE SUCCESS: {task.name} reset. " +
            $"Impostor invisible for {sabotageInvisibilitySeconds} seconds.");
    }
    private void RefreshGhostVisibility()
    {
        if (Player.Local == null)
            return;

        bool localGhost =
            Player.Local.IsGhost;

        bool shouldShow = true;

        // Living players cannot see ghosts
        if (!localGhost && IsGhost)
        {
            shouldShow = false;
        }

        foreach (MeshRenderer renderer in modelParts)
        {
            if (renderer != null)
            {
                renderer.enabled = shouldShow;
            }
        }

        if (nameTag != null)
        {
            nameTag.Show(shouldShow);
        }

        Debug.Log(
            $"GHOST VISIBILITY | " +
            $"{Name} Visible={shouldShow}");
    }
    private void OnGhostChanged()
    {
        RefreshGhostVisibility();
    }
}
