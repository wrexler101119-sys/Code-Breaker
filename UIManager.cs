using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private LeaderboardItem[] leaderboardItems;
    [SerializeField] private TextMeshProUGUI roleText;

    private GameObject settingsCanvas;
    public Player LocalPlayer;

    private NetworkRunner runner;

    [Header("Lobby")]
    [SerializeField] private GameObject readyButton;

    [SerializeField] private TextMeshProUGUI playerCountText;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject); // persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (readyButton != null)
            readyButton.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ShowReadyButton()
    {
        if (readyButton != null)
            readyButton.SetActive(true);
    }

    // 🔥 FIND SETTINGS UI WHEN GAME SCENE LOADS
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        runner = FindObjectOfType<NetworkRunner>();

        settingsCanvas = GameObject.Find("SettingsMenu_UGUI");

        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(false);
            Debug.Log("✅ Settings Canvas Found");
        }
        else
        {
            Debug.LogWarning("❌ Settings Canvas NOT FOUND");
        }
    }

    private void Update()
    {
        UpdatePlayerCountUI();

        if (LocalPlayer == null)
            return;

        if (runner != null && !runner.IsRunning)
        {
            CloseSettings();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            OnReadyButtonPressed();
        }
        if (LocalPlayer == null)
            return;

        // 🔴 AUTO CLOSE IF DISCONNECTED (CLIENT SAFE)
        if (runner != null && !runner.IsRunning)
        {
            CloseSettings();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnReadyButtonPressed();
        }

    }

    // =========================
    // 🔘 FUSION BUTTON HOOK
    // =========================

    // Called by Fusion SendMessage
    public void OnSettingsPressed()
    {
        Debug.Log("⚙️ Settings Pressed");

        StopPlayerInput();

        OpenSettings();
    }

    public void OnCloseSettingsPressed()
    {
        CloseSettings();

        ResumePlayerInput();

        Debug.Log("⚙️ Settings Closed");
    }

    // =========================
    // SETTINGS CONTROL
    // =========================

    public void OpenSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(true);
            MobileControlsManager.Instance
             .SetControlsEnabled(false);

            MobileControlsManager.Instance
                .ShowJoystick(false);

           
        }
        else
        {
            Debug.LogWarning("Settings Canvas is NULL");
        }
    }

    public void CloseSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(false);
        
        }
    }

    // =========================
    // PLAYER INPUT CONTROL
    // =========================

    private void StopPlayerInput()
    {
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
        }

        Debug.Log("PLAYER INPUT STOPPED");
    }

    private void ResumePlayerInput()
    {
        InputManager inputManager =
            FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.ClearMovementInput();
            inputManager.SetLookLocked(false);
        }

        if (MobileControlsManager.Instance != null)
        {
            MobileControlsManager.Instance
                .SetControlsEnabled(true);

            MobileControlsManager.Instance
                .ShowJoystick(true);
        }

        Debug.Log("PLAYER INPUT RESUMED");
    }
    // =========================
    // ROLE DISPLAY
    // =========================
    private void UpdatePlayerCountUI()
    {
        if (playerCountText == null)
            return;

        GameLogic gameLogic =
            FindFirstObjectByType<GameLogic>();

        if (gameLogic == null ||
            !gameLogic.IsNetworkReady)
        {
            playerCountText.text =
                "Players: --/--";

            return;
        }

        int current =
            gameLogic.CurrentPlayerCount;

        int required =
            gameLogic.RequiredPlayerCount;

        if (required <= 0)
        {
            playerCountText.text =
                "Players: --/--";

            return;
        }

        playerCountText.text =
            $"Players: {current}/{required}";
    }
    public void ShowRole(string role, Color color)
    {
        roleText.text = role;
        roleText.color = color;
        roleText.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideRoleAfterTime());
    }

    private System.Collections.IEnumerator HideRoleAfterTime()
    {
        yield return new WaitForSeconds(3f);
        roleText.gameObject.SetActive(false);
    }

    // =========================
    // GAME STATE UI
    // =========================

    public void DidSetReady()
    {
        instructionText.text = "Waiting for other players...";

        if (readyButton != null)
            readyButton.SetActive(false);
    }

    public void SetWaitUI(GameState newState, Player winner)
    {
        if (newState == GameState.Waiting)
        {
            if (winner == null)
            {
                gameStateText.text = "Waiting to Start";
                instructionText.text = "Press READY when you are ready.";

                if (readyButton != null)
                    readyButton.SetActive(true);
            }
            else
            {
                gameStateText.text = winner.Name + " Wins";
                instructionText.text = "Press READY to play again.";

                if (readyButton != null)
                    readyButton.SetActive(true);
            }
        }
        else
        {
            if (readyButton != null)
                readyButton.SetActive(false);
        }

        gameStateText.enabled = newState == GameState.Waiting;
        instructionText.enabled = newState == GameState.Waiting;
    }

    public void OnReadyButtonPressed()
    {
        if (Player.Local == null)
            return;

        Player.Local.RPC_SetReady();

        if (readyButton != null)
            readyButton.SetActive(false);
    }
    // =========================
    // LEADERBOARD
    // =========================



    public void UpdateLeaderboard(KeyValuePair<PlayerRef, Player>[] players)
    {
        for (int i = 0; i < leaderboardItems.Length; i++)
        {
            LeaderboardItem item = leaderboardItems[i];

            if (i < players.Length)
            {
                item.nameText.text = players[i].Value.Name;
            }
            else
            {
                item.nameText.text = "";
            }
        }
    }

    [Serializable]
    private struct LeaderboardItem
    {
        public TextMeshProUGUI nameText;
    }
}