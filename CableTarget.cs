using UnityEngine;
using UnityEngine.EventSystems;

public class CableTarget :
    MonoBehaviour,
    IDropHandler
{
    [Header("Target")]
    public int slotID;

    [HideInInspector]
    public CableDrag currentCable;

    public void OnDrop(
        PointerEventData eventData)
    {
        Debug.Log(
            $"CABLE DROP DETECTED | " +
            $"Target={name} | " +
            $"SlotID={slotID}");

        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning(
                $"CABLE DROP FAILED | " +
                $"Target={name} | " +
                $"pointerDrag is NULL");

            return;
        }

        CableDrag dragged =
            eventData.pointerDrag
                .GetComponent<CableDrag>();

        if (dragged == null)
        {
            dragged =
                eventData.pointerDrag
                    .GetComponentInParent<CableDrag>();
        }

        if (dragged == null)
        {
            Debug.LogWarning(
                $"CABLE DROP FAILED | " +
                $"Target={name} | " +
                $"No CableDrag found.");

            return;
        }

        Debug.Log(
            $"CABLE DROP FOUND | " +
            $"Cable={dragged.name} | " +
            $"CableID={dragged.cableID} | " +
            $"Slot={name} | " +
            $"SlotID={slotID}");

        // ---------------------------------------------------------
        // DON'T ALLOW TWO CABLES IN ONE SLOT
        // ---------------------------------------------------------

        if (currentCable != null &&
            currentCable != dragged)
        {
            Debug.Log(
                $"CABLE DROP REJECTED | " +
                $"Slot={name} already contains " +
                $"{currentCable.name}");

            return;
        }

        // ---------------------------------------------------------
        // SNAP CABLE
        // ---------------------------------------------------------

        dragged.SnapTo(this);

        eventData.Use();
    }

    public void ClearSlot()
    {
        if (currentCable != null)
        {
            currentCable.currentSlot = null;
        }

        currentCable = null;
    }
}