using Fusion;
using Fusion.Menu;
using Fusion.Sockets;
using MultiClimb.Menu;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager :
    SimulationBehaviour,
    IBeforeUpdate,
    INetworkRunnerCallbacks
{
    public Player LocalPlayer;

    public Vector2 AccumulatedMouseDelta =>
        accumulatedLookDelta;

    public bool IsLookLocked =>
        lookLocked;

    private NetInput accumulatedInput;
    private Vector2 accumulatedLookDelta;

    private bool resetInput;
    private bool lookLocked;

    void IBeforeUpdate.BeforeUpdate()
    {
        if (resetInput)
        {
            resetInput = false;
            accumulatedInput = default;
        }

        NetworkButtons buttons = default;

        // ========================
        // COMPUTER INPUT
        // ========================

#if UNITY_EDITOR || UNITY_STANDALONE

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            // ========================
            // F1 CAMERA LOOK LOCK
            // ========================

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                ToggleLookLock();
            }

            // ========================
            // CURSOR LOCK
            // ========================

            if (keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame ||
                keyboard.escapeKey.wasPressedThisFrame)
            {
                if (Cursor.lockState ==
                    CursorLockMode.Locked)
                {
                    Cursor.lockState =
                        CursorLockMode.None;

                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState =
                        CursorLockMode.Locked;

                    Cursor.visible = false;
                }
            }
        }

        // ========================
        // MOUSE LOOK
        // ========================

        Mouse mouse = Mouse.current;

        if (!lookLocked && mouse != null)
        {
            Vector2 mouseDelta =
                mouse.delta.ReadValue();

            accumulatedLookDelta +=
                new Vector2(
                    -mouseDelta.y,
                     mouseDelta.x);
        }

#endif

        // ========================
        // MOBILE LOOK
        // ========================

        if (MobileLookInput.Instance != null)
        {
            // Always consume the value so old input
            // does not remain stored while locked.
            Vector2 mobileLook =
                MobileLookInput.Instance
                    .ConsumeLookDelta();

            if (!lookLocked)
            {
                accumulatedLookDelta +=
                    new Vector2(
                        -mobileLook.y,
                         mobileLook.x) * 1.25f;
            }
        }

        // ========================
        // MOVEMENT
        // ========================

        Vector2 moveDirection =
            Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed)
                moveDirection += Vector2.up;

            if (keyboard.sKey.isPressed)
                moveDirection += Vector2.down;

            if (keyboard.aKey.isPressed)
                moveDirection += Vector2.left;

            if (keyboard.dKey.isPressed)
                moveDirection += Vector2.right;

            buttons.Set(
                InputButton.Jump,
                keyboard.spaceKey.isPressed);
        }

#endif

        // ========================
        // MOBILE JOYSTICK
        // ========================

        if (MobileJoystickInput.Instance != null)
        {
            moveDirection +=
                MobileJoystickInput.Instance
                    .MoveInput;
        }

        accumulatedInput.Direction =
            moveDirection;

        accumulatedInput.Buttons =
            buttons;

        if (lookLocked)
        {
            accumulatedLookDelta =
                Vector2.zero;
        }
    }

    // ========================
    // LOOK LOCK
    // ========================

    public void ToggleLookLock()
    {
        SetLookLocked(!lookLocked);
    }

    public void SetLookLocked(bool locked)
    {
        lookLocked = locked;

        // Prevent a sudden camera jump when unlocking.
        accumulatedLookDelta =
            Vector2.zero;

        accumulatedInput.LookDelta =
            Vector2.zero;

        Debug.Log(
            lookLocked
                ? "CAMERA LOOK LOCKED"
                : "CAMERA LOOK UNLOCKED");
    }

    // ========================
    // FUSION INPUT
    // ========================

    void INetworkRunnerCallbacks.OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        accumulatedInput.Direction.Normalize();

        accumulatedInput.LookDelta =
            lookLocked
                ? Vector2.zero
                : accumulatedLookDelta;

        input.Set(accumulatedInput);

        accumulatedLookDelta =
            Vector2.zero;

        resetInput = true;
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        if (player == runner.LocalPlayer)
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }

#endif
    }

    async void INetworkRunnerCallbacks.OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

#endif

        if (shutdownReason ==
            ShutdownReason.DisconnectedByPluginLogic)
        {
            await FindFirstObjectByType
                <MenuConnectionBehaviour>(
                    FindObjectsInactive.Include)
                .DisconnectAsync(
                    ConnectFailReason.Disconnect);

            FindFirstObjectByType
                <FusionMenuUIGameplay>(
                    FindObjectsInactive.Include)
                .Controller
                .Show<FusionMenuUIMain>();
        }
    }
    public void ClearMovementInput()
    {
        accumulatedInput.Direction = Vector2.zero;
        accumulatedLookDelta = Vector2.zero;

        resetInput = true;

        Debug.Log("MOVEMENT INPUT CLEARED");
    }
    // ========================
    // UNUSED CALLBACKS
    // ========================

    void INetworkRunnerCallbacks.OnConnectedToServer(
        NetworkRunner runner)
    {
    }

    void INetworkRunnerCallbacks.OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }

    void INetworkRunnerCallbacks
        .OnCustomAuthenticationResponse(
            NetworkRunner runner,
            Dictionary<string, object> data)
    {
    }

    void INetworkRunnerCallbacks
        .OnDisconnectedFromServer(
            NetworkRunner runner,
            NetDisconnectReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }

    void INetworkRunnerCallbacks.OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks
        .OnReliableDataProgress(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            float progress)
    {
    }

    void INetworkRunnerCallbacks
        .OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            ArraySegment<byte> data)
    {
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(
        NetworkRunner runner)
    {
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(
        NetworkRunner runner)
    {
    }

    void INetworkRunnerCallbacks
        .OnSessionListUpdated(
            NetworkRunner runner,
            List<SessionInfo> sessionList)
    {
    }

    void INetworkRunnerCallbacks
        .OnUserSimulationMessage(
            NetworkRunner runner,
            SimulationMessagePtr message)
    {
    }
}