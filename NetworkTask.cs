using Fusion;
using UnityEngine;

public class NetworkTask : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnCompletedChanged))]
    public NetworkBool IsCompleted { get; private set; }

    [Header("Objects")]
    [SerializeField] private GameObject interactableObject;
    [SerializeField] private GameObject completedVisual;

    public override void Spawned()
    {
        UpdateVisuals();
    }

    public void TryComplete()
    {
        if (IsCompleted)
            return;

        RPC_Complete();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Complete()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;

        if (GlobalTaskTracker.Instance != null)
        {
            GlobalTaskTracker.Instance.AddTask();
        }

        UpdateVisuals();

        Debug.Log(
            $"TASK COMPLETED: {name} | Completed={IsCompleted}");
    }

    public bool ResetFromSabotage()
    {
        if (!HasStateAuthority)
            return false;

        if (!IsCompleted)
            return false;

        IsCompleted = false;

        UpdateVisuals();

        Debug.Log(
            $"TASK RESET BY SABOTAGE: {name} | Completed={IsCompleted}");

        return true;
    }

    private void OnCompletedChanged()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        bool completed = IsCompleted;

        if (interactableObject != null)
        {
            interactableObject.SetActive(!completed);
        }

        if (completedVisual != null)
        {
            completedVisual.SetActive(completed);
        }

        if (completed && HasInputAuthority)
        {
            if (GAMEUIMANAGER.Singleton != null)
            {
                GAMEUIMANAGER.Singleton.ShowJoystick(true);
            }

            if (MobileControlsManager.Instance != null)
            {
                MobileControlsManager.Instance.SetControlsEnabled(true);
            }
        }

    }
    public void ResetForNewRound()
    {
        if (!HasStateAuthority)
            return;

        IsCompleted = false;

        UpdateVisuals();

        Debug.Log(
            $"TASK RESET FOR NEW ROUND | Task={name}");
    }
}