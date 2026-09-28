using UnityEngine;
using UnityEngine.EventSystems;

public class CableDrag :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Cable")]
    public int cableID;

    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPos;
    private bool initialized;

    public CableTarget currentSlot;

    private void Awake()
    {
        InitializeCable();
    }

    private void Start()
    {
        InitializeCable();
    }

    private void InitializeCable()
    {
        if (initialized)
            return;

        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (rect == null)
        {
            Debug.LogError(
                $"CABLE INITIALIZATION FAILED | " +
                $"RectTransform missing on {name}");

            return;
        }

        if (canvas == null)
        {
            Debug.LogError(
                $"CABLE INITIALIZATION FAILED | " +
                $"Canvas missing for {name}");

            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogError(
                $"CABLE INITIALIZATION FAILED | " +
                $"CanvasGroup missing on {name}");

            return;
        }

        startPos = rect.anchoredPosition;

        initialized = true;

        Debug.Log(
            $"CABLE INITIALIZED | " +
            $"Cable={name} | " +
            $"ID={cableID} | " +
            $"Start={startPos}");
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        InitializeCable();

        if (!initialized)
            return;

        canvasGroup.blocksRaycasts = false;

        if (currentSlot != null)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }

        Debug.Log(
            $"CABLE DRAG START | " +
            $"Cable={name} | ID={cableID}");
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        InitializeCable();

        if (!initialized)
            return;

        rect.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        InitializeCable();

        if (!initialized)
            return;

        canvasGroup.blocksRaycasts = true;

        if (currentSlot == null)
        {
            rect.anchoredPosition = startPos;

            Debug.Log(
                $"CABLE RETURNED TO START | " +
                $"Cable={name}");
        }
        else
        {
            Debug.Log(
                $"CABLE DROP SUCCESS | " +
                $"Cable={name} | " +
                $"Slot={currentSlot.name}");
        }
    }

    public void SnapTo(CableTarget slot)
    {
        InitializeCable();

        if (!initialized || slot == null)
            return;

        RectTransform slotRect =
            slot.GetComponent<RectTransform>();

        if (slotRect == null)
        {
            Debug.LogError(
                $"CABLE SNAP FAILED | " +
                $"Slot={slot.name} has no RectTransform.");

            return;
        }

        if (currentSlot != null &&
            currentSlot != slot)
        {
            currentSlot.ClearSlot();
        }

        currentSlot = slot;
        slot.currentCable = this;

        // Make sure all UI RectTransforms are up to date.
        Canvas.ForceUpdateCanvases();

        // =========================================================
        // GET THE CENTER OF THE SLOT
        // =========================================================

        Vector3 slotCenter =
            slotRect.TransformPoint(
                slotRect.rect.center);

        // =========================================================
        // GET THE CENTER OF THE CABLE
        // =========================================================

        Vector3 cableCenter =
            rect.TransformPoint(
                rect.rect.center);

        // =========================================================
        // MOVE THE CABLE CENTER TO THE SLOT CENTER
        // =========================================================

        Vector3 offset =
            slotCenter - cableCenter;

        rect.position += offset;

        Debug.Log(
            $"CABLE CENTER SNAPPED | " +
            $"Cable={name} | " +
            $"CableID={cableID} | " +
            $"Slot={slot.name} | " +
            $"SlotID={slot.slotID}");
    }

    public void ResetCable()
    {
        InitializeCable();

        if (!initialized)
            return;

        if (currentSlot != null)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }

        rect.anchoredPosition = startPos;

        canvasGroup.blocksRaycasts = true;

        Debug.Log(
            $"CABLE RESET | " +
            $"Cable={name}");
    }
}