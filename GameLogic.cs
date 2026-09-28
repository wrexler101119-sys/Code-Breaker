using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Waiting,
    Playing,
    Ended
}

public enum WinningTeam
{
    None,
    Crewmates,
    Impostors
}

public class GameLogic :
    NetworkBehaviour,
    IPlayerJoined,
    IPlayerLeft
{
    public static GameLogic Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private NetworkPrefabRef globalTaskTrackerPrefab;

    [Header("Task Spawner")]
    [SerializeField] private Spawnerobject taskSpawner;

    [Header("Spawn")]
    [SerializeField] private Transform spawnpoint;
    [SerializeField] private Transform spawnpointPivot;

    [Header("Lobby Circle")]
    [SerializeField] private float lobbyRadius = 2.5f;
    [SerializeField] private float lobbyStartAngle = 0f;

    [Header("Gameplay Circle")]
    [SerializeField] private float meetingRadius = 2.5f;

    [Networked]
    public int RequiredPlayersToStart { get; private set; }

    [Networked]
    public NetworkBool GameEnded { get; private set; }

    [Networked]
    public WinningTeam WinningTeam { get; private set; }

    [Networked, OnChangedRender(nameof(GameStateChanged))]
    public GameState State { get; private set; }

    [Networked, Capacity(12)]
    private NetworkDictionary<PlayerRef, Player> Players => default;

    private Coroutine winnerCoroutine;
    private bool pendingPlayAgainReset;

    // =====================================================
    // LOBBY SLOT SYSTEM
    // =====================================================

    // Stores which circle slot belongs to each player.
    // Only the Host/State Authority uses this dictionary.
    private readonly Dictionary<PlayerRef, int> lobbySlots =
        new Dictionary<PlayerRef, int>();

    // Maximum lobby slots.
    private const int MaxLobbySlots = 12;

    // =====================================================
    // SPAWNED
    // =====================================================

    public override void Spawned()
    {
        Instance = this;

        Runner.SetIsSimulated(Object, true);

        if (HasStateAuthority)
        {
            GameEnded = false;
            WinningTeam = WinningTeam.None;
            State = GameState.Waiting;

            RequiredPlayersToStart =
                Mathf.Clamp(
                    Runner.SessionInfo.MaxPlayers,
                    4,
                    12);

            if (GlobalTaskTracker.Instance == null)
            {
                Runner.Spawn(
                    globalTaskTrackerPrefab,
                    Vector3.zero,
                    Quaternion.identity);
            }

            Debug.Log(
                $"LOBBY PLAYER REQUIREMENT | " +
                $"Required={RequiredPlayersToStart}");
        }

        RefreshWaitingUI();
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

    // =====================================================
    // NETWORK UPDATE
    // =====================================================

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        // Play Again reset
        if (pendingPlayAgainReset)
        {
            pendingPlayAgainReset = false;

            ResetMatchToWaiting();
            return;
        }

        if (Players.Count == 0)
            return;

        switch (State)
        {
            case GameState.Waiting:
                CheckForGameStart();
                break;

            case GameState.Playing:
                CheckForWinner();
                break;

            case GameState.Ended:
                break;
        }
    }

    // =====================================================
    // NETWORK INFORMATION
    // =====================================================

    public bool IsNetworkReady
    {
        get
        {
            return Object != null &&
                   Object.IsValid &&
                   Object.IsInSimulation;
        }
    }

    public int CurrentPlayerCount
    {
        get
        {
            if (!IsNetworkReady)
                return 0;

            return Players.Count;
        }
    }

    public int RequiredPlayerCount
    {
        get
        {
            if (!IsNetworkReady)
                return 0;

            return RequiredPlayersToStart;
        }
    }

    public int GetRequiredPlayerCount()
    {
        return RequiredPlayersToStart;
    }

    // =====================================================
    // GAME START
    // =====================================================

    private void CheckForGameStart()
    {
        if (Players.Count < RequiredPlayersToStart)
            return;

        bool allPlayersReady = true;

        foreach (var pair in Players)
        {
            Player player = pair.Value;

            if (player == null ||
                !player.IsReady)
            {
                allPlayersReady = false;
                break;
            }
        }

        if (!allPlayersReady)
            return;

        StartMatch();
    }

    private void StartMatch()
    {
        if (!HasStateAuthority)
            return;

        GameEnded = false;
        WinningTeam = WinningTeam.None;
        State = GameState.Playing;

        if (GlobalTaskTracker.Instance != null)
        {
            GlobalTaskTracker.Instance.ResetTracker();
        }

        AssignRoles();

        // Move players to gameplay circle.
        PreparePlayers();

        foreach (var pair in Players)
        {
            Player player = pair.Value;

            if (player == null)
                continue;

            player.RPC_ShowRole(player.Role);
        }

        Debug.Log(
            $"GAME STARTED | " +
            $"Players={Players.Count} | " +
            $"Impostors={CalculateImpostorCount(Players.Count)}");
    }

    // =====================================================
    // ROLE ASSIGNMENT
    // =====================================================

    private void AssignRoles()
    {
        if (!HasStateAuthority)
            return;

        List<Player> playerList =
            new List<Player>();

        foreach (var pair in Players)
        {
            if (pair.Value != null)
            {
                playerList.Add(pair.Value);
            }
        }

        if (playerList.Count == 0)
        {
            Debug.LogError(
                "ROLE ASSIGNMENT FAILED | No players found.");

            return;
        }

        // Shuffle players.
        for (int i = playerList.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            Player temporary =
                playerList[i];

            playerList[i] =
                playerList[randomIndex];

            playerList[randomIndex] =
                temporary;
        }

        int requiredImpostors =
            CalculateImpostorCount(playerList.Count);

        int assignedImpostors = 0;

        for (int i = 0; i < playerList.Count; i++)
        {
            Player player =
                playerList[i];

            if (i < requiredImpostors)
            {
                player.Role =
                    PlayerRole.Impostor;

                assignedImpostors++;
            }
            else
            {
                player.Role =
                    PlayerRole.Crewmate;
            }

            Debug.Log(
                $"ROLE ASSIGNED BY HOST | " +
                $"Player={player.Name} | " +
                $"Authority={player.Object.InputAuthority} | " +
                $"Role={player.Role}");
        }

        // Safety correction.
        if (assignedImpostors == 0)
        {
            Player fallbackPlayer =
                playerList[
                    Random.Range(
                        0,
                        playerList.Count)];

            fallbackPlayer.Role =
                PlayerRole.Impostor;

            assignedImpostors = 1;

            Debug.LogError(
                $"NO IMPOSTOR WAS ASSIGNED | " +
                $"Forced {fallbackPlayer.Name} to Impostor.");
        }

        Debug.Log(
            $"ROLE ASSIGNMENT COMPLETE | " +
            $"Players={playerList.Count} | " +
            $"RequiredImpostors={requiredImpostors} | " +
            $"AssignedImpostors={assignedImpostors}");
    }

    private int CalculateImpostorCount(int playerCount)
    {
        if (playerCount <= 0)
            return 0;

        return Mathf.Clamp(
            playerCount / 4,
            1,
            3);
    }

    // =====================================================
    // WINNER CHECKING
    // =====================================================

    public void RequestPlayAgain()
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning(
                "PLAY AGAIN REJECTED | No State Authority.");

            return;
        }

        if (!GameEnded ||
            State != GameState.Ended)
        {
            Debug.LogWarning(
                "PLAY AGAIN REJECTED | Game has not ended.");

            return;
        }

        pendingPlayAgainReset = true;

        Debug.Log(
            "PLAY AGAIN REQUEST ACCEPTED | " +
            "Reset queued for Fusion FixedUpdateNetwork.");
    }

    private void CheckForWinner()
    {
        if (GameEnded)
            return;

        if (MeetingManager.Instance != null &&
            MeetingManager.Instance.Object != null &&
            MeetingManager.Instance.Object.IsValid &&
            MeetingManager.Instance.MeetingActive)
        {
            return;
        }

        int livingImpostors = 0;
        int livingCrewmates = 0;

        foreach (
            KeyValuePair<PlayerRef, Player> pair
            in Players)
        {
            Player player =
                pair.Value;

            if (player == null ||
                player.IsDead)
            {
                continue;
            }

            if (player.Role ==
                PlayerRole.Impostor)
            {
                livingImpostors++;
            }
            else
            {
                livingCrewmates++;
            }
        }

        Debug.Log(
            $"WIN CHECK | " +
            $"ImpostorsAlive={livingImpostors} | " +
            $"CrewmatesAlive={livingCrewmates} | " +
            $"Tasks=" +
            $"{GetCompletedTaskCount()}/" +
            $"{GetMaximumTaskCount()}");

        if (GlobalTaskTracker.Instance != null &&
            GlobalTaskTracker.Instance.AllTasksCompleted)
        {
            EndGame(WinningTeam.Crewmates);
            return;
        }

        if (livingImpostors == 0)
        {
            EndGame(WinningTeam.Crewmates);
            return;
        }

        if (livingImpostors >= livingCrewmates)
        {
            EndGame(WinningTeam.Impostors);
        }
    }

    public void RequestWinnerCheck()
    {
        if (!HasStateAuthority)
            return;

        if (State != GameState.Playing ||
            GameEnded)
        {
            return;
        }

        CheckForWinner();
    }

    private void EndGame(
        WinningTeam winningTeam)
    {
        if (!HasStateAuthority)
            return;

        if (GameEnded)
            return;

        GameEnded = true;
        WinningTeam = winningTeam;
        State = GameState.Ended;

        Debug.Log(
            $"GAME ENDED | Winner={winningTeam}");

        if (winnerCoroutine != null)
        {
            StopCoroutine(winnerCoroutine);
        }

        winnerCoroutine =
            StartCoroutine(
                ShowWinnerDelayed(winningTeam));
    }

    private IEnumerator ShowWinnerDelayed(
        WinningTeam winningTeam)
    {
        yield return new WaitForSeconds(2f);

        string message =
            winningTeam ==
            WinningTeam.Crewmates
                ? "TECHNICIAN WIN"
                : "HACKER WIN";

        RPC_ShowWinner(message);

        winnerCoroutine = null;
    }

    [Rpc(
        RpcSources.StateAuthority,
        RpcTargets.All)]
    private void RPC_ShowWinner(
        string winnerMessage)
    {
        Debug.Log(
            $"SHOWING WINNER | {winnerMessage}");

        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton
                .ShowWinner(
                    winnerMessage);
        }
        else
        {
            Debug.LogError(
                "WINNER UI FAILED | " +
                "GAMEUIMANAGER.Singleton is null.");
        }
    }

    // =====================================================
    // PLAY AGAIN / ROUND RESET
    // =====================================================

    private void ResetMatchToWaiting()
    {
        if (!HasStateAuthority)
            return;

        Debug.Log(
            "RESETTING MATCH TO WAITING");

        if (winnerCoroutine != null)
        {
            StopCoroutine(
                winnerCoroutine);

            winnerCoroutine = null;
        }

        // Remove dead bodies.
        RemoveAllDeadBodies();

        // Reset doors.
        ResetAllDoors();

        // Reset task progress.
        if (GlobalTaskTracker.Instance != null)
        {
            GlobalTaskTracker.Instance
                .ResetTracker();
        }

        // Despawn old tasks and
        // spawn fresh tasks at random locations.
        if (taskSpawner != null)
        {
            taskSpawner
                .RespawnAllRandomly();

            Debug.Log(
                "PLAY AGAIN | " +
                "TASKS RESPAWNED AT NEW RANDOM LOCATIONS.");
        }
        else
        {
            Debug.LogError(
                "PLAY AGAIN TASK RESPAWN FAILED | " +
                "Spawnerobject is not assigned.");
        }

        // Reset every player.
        foreach (var pair in Players)
        {
            Player player =
                pair.Value;

            if (player == null)
                continue;

            player.ResetForNewRound();
        }

        // Move all existing players
        // back into their lobby slots.
        PrepareLobbyPlayers();

        GameEnded = false;
        WinningTeam = WinningTeam.None;
        State = GameState.Waiting;

        RPC_ResetRoundUI();

        Debug.Log(
            $"MATCH RETURNED TO WAITING | " +
            $"Players={Players.Count}");
    }

    // =====================================================
    // DEAD BODIES
    // =====================================================

    private void RemoveAllDeadBodies()
    {
        DeadBody[] deadBodies =
            FindObjectsByType<DeadBody>(
                FindObjectsSortMode.None);

        int removedCount = 0;

        foreach (
            DeadBody deadBody
            in deadBodies)
        {
            if (deadBody == null ||
                deadBody.Object == null ||
                !deadBody.Object.IsValid)
            {
                continue;
            }

            Runner.Despawn(
                deadBody.Object);

            removedCount++;
        }

        Debug.Log(
            $"DEAD BODIES REMOVED | Count={removedCount}");
    }

    // =====================================================
    // UI RESET
    // =====================================================

    [Rpc(
        RpcSources.StateAuthority,
        RpcTargets.All)]
    private void RPC_ResetRoundUI()
    {
        if (GAMEUIMANAGER.Singleton != null)
        {
            GAMEUIMANAGER.Singleton
                .ResetWinnerUIForNewRound();
        }

        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.SetWaitUI(
                GameState.Waiting,
                null);

            UIManager.Singleton
                .ShowReadyButton();
        }

        Debug.Log(
            "ROUND UI RESET");
    }

    // =====================================================
    // LOBBY SLOT FUNCTIONS
    // =====================================================

    private int FindAvailableLobbySlot()
    {
        for (int i = 0;
             i < MaxLobbySlots;
             i++)
        {
            bool slotUsed = false;

            foreach (int existingSlot
                     in lobbySlots.Values)
            {
                if (existingSlot == i)
                {
                    slotUsed = true;
                    break;
                }
            }

            if (!slotUsed)
            {
                return i;
            }
        }

        Debug.LogWarning(
            "NO FREE LOBBY SLOT FOUND.");

        return 0;
    }

    private Vector3 GetLobbySlotPosition(
        int slotIndex)
    {
        float angleStep =
            360f / RequiredPlayersToStart;

        float angle =
            lobbyStartAngle +
            angleStep * slotIndex;

        float radians =
            angle * Mathf.Deg2Rad;

        return
            spawnpoint.position +
            new Vector3(
                Mathf.Cos(radians),
                0f,
                Mathf.Sin(radians)
            ) * lobbyRadius;
    }

    private Quaternion GetLobbySlotRotation(
        Vector3 position)
    {
        Vector3 lookDirection =
            spawnpoint.position -
            position;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude >
            0.001f)
        {
            return Quaternion.LookRotation(
                lookDirection,
                Vector3.up);
        }

        return spawnpoint.rotation;
    }

    // =====================================================
    // PLACE ONE NEW PLAYER
    // =====================================================

    private void PlaceNewLobbyPlayer(
        Player player,
        int slotIndex)
    {
        if (!HasStateAuthority)
            return;

        if (player == null)
            return;

        if (spawnpoint == null)
        {
            Debug.LogError(
                "LOBBY PLAYER POSITION FAILED | " +
                "spawnpoint is not assigned.");

            return;
        }

        Vector3 position =
            GetLobbySlotPosition(
                slotIndex);

        Quaternion rotation =
            GetLobbySlotRotation(
                position);

        player.Teleport(
            position,
            rotation);

        Debug.Log(
            $"NEW PLAYER PLACED IN LOBBY | " +
            $"Player={player.Name} | " +
            $"Slot={slotIndex} | " +
            $"Position={position}");
    }

    // =====================================================
    // PREPARE ALL LOBBY PLAYERS
    // ONLY USED ON PLAY AGAIN
    // =====================================================

    private void PrepareLobbyPlayers()
    {
        if (!HasStateAuthority)
            return;

        if (spawnpoint == null)
        {
            Debug.LogError(
                "LOBBY PREPARE FAILED | " +
                "spawnpoint is not assigned.");

            return;
        }

        foreach (
            KeyValuePair<PlayerRef, Player> pair
            in Players)
        {
            PlayerRef playerRef =
                pair.Key;

            Player player =
                pair.Value;

            if (player == null)
                continue;

            // Give missing players a slot.
            if (!lobbySlots.ContainsKey(
                playerRef))
            {
                int newSlot =
                    FindAvailableLobbySlot();

                lobbySlots[playerRef] =
                    newSlot;
            }

            int slot =
                lobbySlots[playerRef];

            Vector3 position =
                GetLobbySlotPosition(
                    slot);

            Quaternion rotation =
                GetLobbySlotRotation(
                    position);

            player.Teleport(
                position,
                rotation);

            Debug.Log(
                $"LOBBY PLAYER POSITION RESET | " +
                $"Player={player.Name} | " +
                $"Slot={slot} | " +
                $"Position={position}");
        }

        Debug.Log(
            $"LOBBY CIRCLE READY | " +
            $"Players={Players.Count}");
    }

    // =====================================================
    // GAMEPLAY CIRCLE
    // =====================================================

    private void PreparePlayers()
    {
        if (!HasStateAuthority)
            return;

        int playerCount =
            Players.Count;

        if (playerCount == 0)
        {
            Debug.LogWarning(
                "PREPARE PLAYERS FAILED | No players.");

            return;
        }

        if (spawnpointPivot == null)
        {
            Debug.LogError(
                "PREPARE PLAYERS FAILED | " +
                "spawnpointPivot is not assigned.");

            return;
        }

        float angleStep =
            360f / playerCount;

        float startAngle =
            Random.Range(
                0f,
                360f);

        int index = 0;

        foreach (
            KeyValuePair<PlayerRef, Player> pair
            in Players)
        {
            Player player =
                pair.Value;

            if (player == null)
                continue;

            float angle =
                startAngle +
                angleStep * index;

            float radians =
                angle * Mathf.Deg2Rad;

            Vector3 position =
                spawnpointPivot.position +
                new Vector3(
                    Mathf.Cos(radians),
                    0f,
                    Mathf.Sin(radians)
                ) * meetingRadius;

            Vector3 lookDirection =
                spawnpointPivot.position -
                position;

            lookDirection.y = 0f;

            Quaternion rotation =
                lookDirection.sqrMagnitude >
                0.001f
                    ? Quaternion.LookRotation(
                        lookDirection,
                        Vector3.up)
                    : spawnpointPivot.rotation;

            player.Teleport(
                position,
                rotation);

            Debug.Log(
                $"GAMEPLAY PLAYER POSITION | " +
                $"Player={player.Name} | " +
                $"Position={position} | " +
                $"Rotation={rotation.eulerAngles}");

            index++;
        }

        Debug.Log(
            $"GAMEPLAY CIRCLE COMPLETE | " +
            $"Players={playerCount}");
    }

    // =====================================================
    // DOOR RESET
    // =====================================================

    private void ResetAllDoors()
    {
        if (!HasStateAuthority)
            return;

        Doors[] doors =
            FindObjectsByType<Doors>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        int resetCount = 0;

        foreach (Doors door in doors)
        {
            if (door == null)
                continue;

            door.ResetForNewRound();

            resetCount++;
        }

        Debug.Log(
            $"ALL DOORS RESET | " +
            $"Count={resetCount}");
    }

    // =====================================================
    // PLAYER JOIN / LEAVE
    // =====================================================

    void IPlayerJoined.PlayerJoined(
        PlayerRef playerRef)
    {
        if (!HasStateAuthority)
            return;

        if (spawnpoint == null)
        {
            Debug.LogError(
                "PLAYER SPAWN FAILED | " +
                "spawnpoint is not assigned.");

            return;
        }

        // Assign a permanent lobby slot
        // for this player.
        int lobbySlot =
            FindAvailableLobbySlot();

        lobbySlots[playerRef] =
            lobbySlot;

        int uniqueColorIndex =
            FindAvailablePlayerColor();

        // Spawn player initially at the center.
        NetworkObject playerObject =
            Runner.Spawn(
                playerPrefab,
                spawnpoint.position,
                spawnpoint.rotation,
                playerRef);

        Player player =
            playerObject.GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError(
                "PLAYER SPAWN FAILED | " +
                "Player component missing.");

            lobbySlots.Remove(
                playerRef);

            Runner.Despawn(
                playerObject);

            return;
        }

        player.SetPlayerColorIndex(
            uniqueColorIndex);

        Players.Add(
            playerRef,
            player);

        Debug.Log(
            $"PLAYER JOINED | " +
            $"Player={playerRef} | " +
            $"ColorIndex={uniqueColorIndex} | " +
            $"LobbySlot={lobbySlot} | " +
            $"Count={Players.Count}");

        // IMPORTANT:
        // ONLY place the NEW player.
        // Existing players are NOT moved.
        if (State == GameState.Waiting)
        {
            PlaceNewLobbyPlayer(
                player,
                lobbySlot);
        }
    }

    private int FindAvailablePlayerColor()
    {
        bool[] usedColors =
            new bool[
                Player.AvailableColorCount];

        foreach (var pair in Players)
        {
            Player existingPlayer =
                pair.Value;

            if (existingPlayer == null)
                continue;

            int index =
                existingPlayer.PlayerColorIndex;

            if (index >= 0 &&
                index < usedColors.Length)
            {
                usedColors[index] = true;
            }
        }

        for (
            int i = 0;
            i < usedColors.Length;
            i++)
        {
            if (!usedColors[i])
            {
                return i;
            }
        }

        int fallbackIndex =
            Random.Range(
                0,
                Player.AvailableColorCount);

        Debug.LogWarning(
            $"ALL PLAYER COLORS ARE USED | " +
            $"FallbackColor={fallbackIndex}");

        return fallbackIndex;
    }

    void IPlayerLeft.PlayerLeft(
        PlayerRef playerRef)
    {
        if (!HasStateAuthority)
            return;

        if (Players.TryGet(
            playerRef,
            out Player player))
        {
            Players.Remove(
                playerRef);

            if (player != null &&
                player.Object != null &&
                player.Object.IsValid)
            {
                Runner.Despawn(
                    player.Object);
            }
        }

        // Free the lobby slot.
        if (lobbySlots.ContainsKey(
            playerRef))
        {
            int oldSlot =
                lobbySlots[playerRef];

            lobbySlots.Remove(
                playerRef);

            Debug.Log(
                $"LOBBY SLOT RELEASED | " +
                $"Player={playerRef} | " +
                $"Slot={oldSlot}");
        }

        Debug.Log(
            $"PLAYER LEFT | " +
            $"Player={playerRef} | " +
            $"Remaining={Players.Count}");

        // IMPORTANT:
        // Do NOT rearrange the remaining players.

        if (State == GameState.Playing &&
            !GameEnded)
        {
            CheckForWinner();
        }
    }

    // =====================================================
    // UI
    // =====================================================

    private void GameStateChanged()
    {
        RefreshWaitingUI();
    }

    private void RefreshWaitingUI()
    {
        if (UIManager.Singleton == null)
            return;

        UIManager.Singleton.SetWaitUI(
            State,
            null);
    }

    // =====================================================
    // PUBLIC INFORMATION
    // =====================================================

    public int GetPlayerCount()
    {
        if (!IsNetworkReady)
            return 0;

        return Players.Count;
    }

    public int GetLivingImpostorCount()
    {
        int count = 0;

        foreach (
            KeyValuePair<PlayerRef, Player> pair
            in Players)
        {
            Player player =
                pair.Value;

            if (player != null &&
                !player.IsDead &&
                player.Role ==
                PlayerRole.Impostor)
            {
                count++;
            }
        }

        return count;
    }

    public int GetLivingCrewmateCount()
    {
        int count = 0;

        foreach (
            KeyValuePair<PlayerRef, Player> pair
            in Players)
        {
            Player player =
                pair.Value;

            if (player != null &&
                !player.IsDead &&
                player.Role ==
                PlayerRole.Crewmate)
            {
                count++;
            }
        }

        return count;
    }

    private int GetCompletedTaskCount()
    {
        return GlobalTaskTracker.Instance != null
            ? GlobalTaskTracker.Instance.CompletedTasks
            : 0;
    }

    private int GetMaximumTaskCount()
    {
        return GlobalTaskTracker.Instance != null
            ? GlobalTaskTracker.Instance.MaxTasks
            : 0;
    }
}