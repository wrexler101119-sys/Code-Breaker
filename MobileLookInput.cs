using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLookInput : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public static MobileLookInput Instance;

    private Vector2 lookDelta;
    private Vector2 lastPosition;

    public Vector2 LookDelta => lookDelta;

    private void Awake()
    {
        Instance = this;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        lookDelta += eventData.position - lastPosition;
        lastPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public Vector2 ConsumeLookDelta()
    {
        Vector2 delta = lookDelta;
        lookDelta = Vector2.zero;
        return delta;
    }
}