using Fusion;
using UnityEngine;

public class Doors : NetworkBehaviour
{
    [SerializeField] private Transform doorPivot;

    [Header("Rotation Settings")]
    [SerializeField] private float startAngle = -90f;
    [SerializeField] private float openOffset = 90f;
    [SerializeField] private float smoothTime = 0.15f;

    [Networked, OnChangedRender(nameof(OnDoorChanged))]
    private bool isOpen { get; set; }

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private float currentT;
    private float targetT;
    private float velocity;

    // =========================================================
    // SPAWN
    // =========================================================

    public override void Spawned()
    {
        closedRotation =
            Quaternion.Euler(
                0f,
                startAngle,
                0f);

        openRotation =
            closedRotation *
            Quaternion.Euler(
                0f,
                openOffset,
                0f);

        // Use the actual networked state when spawning.
        currentT =
            isOpen ? 1f : 0f;

        targetT =
            currentT;

        velocity = 0f;

        ApplyRotation();

        Debug.Log(
            $"DOOR SPAWNED | " +
            $"Name={name} | " +
            $"ID={Object.Id} | " +
            $"StateAuthority={HasStateAuthority} | " +
            $"Open={isOpen}");
    }

    // =========================================================
    // OPEN / CLOSE
    // =========================================================

    public void Toggle()
    {
        Debug.Log(
            $"DOOR TOGGLE REQUEST | " +
            $"Door={name} | " +
            $"Authority={HasStateAuthority}");

        if (!HasStateAuthority)
        {
            Debug.Log(
                $"DOOR TOGGLE REJECTED | " +
                $"{name} has no State Authority.");

            return;
        }

        isOpen = !isOpen;

        targetT =
            isOpen ? 1f : 0f;

        Debug.Log(
            $"DOOR TOGGLED | " +
            $"Door={name} | " +
            $"Open={isOpen}");
    }

    // =========================================================
    // NETWORKED STATE CHANGED
    // =========================================================

    private void OnDoorChanged()
    {
        targetT =
            isOpen ? 1f : 0f;

        Debug.Log(
            $"DOOR NETWORK STATE CHANGED | " +
            $"Door={name} | " +
            $"Open={isOpen}");
    }

    // =========================================================
    // PLAY AGAIN RESET
    // =========================================================

    public void ResetForNewRound()
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning(
                $"DOOR RESET REJECTED | " +
                $"{name} has no State Authority.");

            return;
        }

        // Reset authoritative networked state.
        isOpen = false;

        // Reset visual interpolation immediately.
        targetT = 0f;
        currentT = 0f;
        velocity = 0f;

        ApplyRotation();

        Debug.Log(
            $"DOOR RESET FOR NEW ROUND | " +
            $"Door={name} | " +
            $"Open={isOpen}");
    }

    // =========================================================
    // RENDER
    // =========================================================

    public override void Render()
    {
        currentT =
            Mathf.SmoothDamp(
                currentT,
                targetT,
                ref velocity,
                smoothTime);

        ApplyRotation();
    }

    // =========================================================
    // APPLY ROTATION
    // =========================================================

    private void ApplyRotation()
    {
        if (doorPivot == null)
            return;

        doorPivot.localRotation =
            Quaternion.Lerp(
                closedRotation,
                openRotation,
                currentT);
    }
}