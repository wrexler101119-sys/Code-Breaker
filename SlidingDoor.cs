using Fusion;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SlidingDoor : NetworkBehaviour
{
    [SerializeField] private Transform doorVisual;

    [Header("Slide Settings")]
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Auto Close")]
    [SerializeField] private float autoCloseDelay = 3f;

    [Header("Sound")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;

    [Networked, OnChangedRender(nameof(OnDoorChanged))]
    private bool isOpen { get; set; }

    private Vector3 closedPos;
    private Vector3 openPos;

    private float currentT;
    private float targetT;
    private float velocity;

    private float closeTimer;

    public override void Spawned()
    {
        audioSource = GetComponent<AudioSource>();

        // initialize state on server
        if (HasStateAuthority)
            isOpen = false;

        closedPos = doorVisual.localPosition;
        openPos = closedPos + doorVisual.right * slideDistance;

        currentT = targetT = isOpen ? 1f : 0f;

        ApplyPosition();
    }

    // ✅ Called by ANY player (client or host)
    public void Open()
    {
        RPC_Open();
    }

    // ✅ Always handled by server/state authority
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Open()
    {
        isOpen = true;
        closeTimer = autoCloseDelay;
    }

    private void OnDoorChanged()
    {
        targetT = isOpen ? 1f : 0f;

        if (audioSource != null)
        {
            if (isOpen && openSound != null)
                audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null)
                audioSource.PlayOneShot(closeSound);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (isOpen)
        {
            closeTimer -= Runner.DeltaTime;

            if (closeTimer <= 0f)
            {
                isOpen = false;
            }
        }
    }

    public override void Render()
    {
        currentT = Mathf.SmoothDamp(
            currentT,
            targetT,
            ref velocity,
            smoothTime
        );

        ApplyPosition();
    }

    private void ApplyPosition()
    {
        if (doorVisual == null) return;

        doorVisual.localPosition = Vector3.Lerp(
            closedPos,
            openPos,
            currentT
        );
    }
}