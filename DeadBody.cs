using Fusion;
using UnityEngine;

public class DeadBody : NetworkBehaviour
{
    [Header("Dead Body Color")]
    [SerializeField] private MeshRenderer bodyRenderer;
    [SerializeField] private MeshRenderer rightArmRenderer;
    [SerializeField] private MeshRenderer leftArmRenderer;

    [Networked, OnChangedRender(nameof(OnPlayerColorChanged))]
    public int PlayerColorIndex { get; set; }

    [Networked]
    public NetworkBool IsReported { get; set; }

    [Networked]
    public string DeadPlayerName { get; set; }

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

    public override void Spawned()
    {
        ApplyPlayerColor();

        Debug.Log(
            $"DEAD BODY SPAWNED | " +
            $"Player={DeadPlayerName} | " +
            $"ColorIndex={PlayerColorIndex}");
    }

    public void InitializeDeadBody(
        string playerName,
        int colorIndex)
    {
        if (!HasStateAuthority)
        {
            Debug.LogError(
                "DEAD BODY INITIALIZATION FAILED | " +
                "No State Authority.");

            return;
        }

        DeadPlayerName = playerName;

        PlayerColorIndex = Mathf.Clamp(
            colorIndex,
            0,
            PlayerColors.Length - 1);

        IsReported = false;

        ApplyPlayerColor();

        Debug.Log(
            $"DEAD BODY INITIALIZED | " +
            $"Player={DeadPlayerName} | " +
            $"ColorIndex={PlayerColorIndex}");
    }

    private void OnPlayerColorChanged()
    {
        ApplyPlayerColor();
    }

    private void ApplyPlayerColor()
    {
        int safeIndex = Mathf.Clamp(
            PlayerColorIndex,
            0,
            PlayerColors.Length - 1);

        Color selectedColor =
            PlayerColors[safeIndex];

        ApplyColorToRenderer(
            bodyRenderer,
            selectedColor);

        ApplyColorToRenderer(
            rightArmRenderer,
            selectedColor);

        ApplyColorToRenderer(
            leftArmRenderer,
            selectedColor);

        Debug.Log(
            $"DEAD BODY COLOR APPLIED | " +
            $"Player={DeadPlayerName} | " +
            $"ColorIndex={safeIndex}");
    }

    private void ApplyColorToRenderer(
        MeshRenderer targetRenderer,
        Color selectedColor)
    {
        if (targetRenderer == null)
            return;

        MaterialPropertyBlock propertyBlock =
            new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(
            propertyBlock);

        propertyBlock.SetColor(
            "_BaseColor",
            selectedColor);

        propertyBlock.SetColor(
            "_Color",
            selectedColor);

        targetRenderer.SetPropertyBlock(
            propertyBlock);
    }

    public void Report(Player reporter)
    {
        if (reporter == null)
            return;

        if (!reporter.HasInputAuthority)
            return;

        RPC_ReportBody(reporter.Name);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ReportBody(string reporterName)
    {
        if (IsReported)
            return;

        IsReported = true;

        if (MeetingManager.Instance != null)
        {
            MeetingManager.Instance.RPC_ReportBody(
                reporterName,
                DeadPlayerName);
        }
        else
        {
            Debug.LogError(
                "REPORT FAILED | MeetingManager.Instance is null.");

            IsReported = false;
            return;
        }

        Runner.Despawn(Object);
    }
}