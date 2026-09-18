using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;

    public Vector2 InputDirection { get; private set; }

    [Range(0f, 1f)]
    public float deadZone = 0.15f;

    private float radius;
    private bool isDragging = false;

    private void Start()
    {
        radius = Mathf.Min(
            background.rect.width,
            background.rect.height
        ) / 2f;

        InputDirection = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Just start dragging.
        // The handle stays in the center until the player actually moves.
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Vector2 localPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out localPosition
        );

        // Correct for the background pivot.
        localPosition -= background.rect.center;

        // Keep handle inside joystick.
        localPosition = Vector2.ClampMagnitude(
            localPosition,
            radius
        );

        handle.anchoredPosition = localPosition;

        // No movement if finger is very close to center.
        if (localPosition.magnitude < radius * deadZone)
        {
            InputDirection = Vector2.zero;
        }
        else
        {
            // IMPORTANT:
            // Normalize so movement speed is FULL regardless
            // of how far the joystick is pushed.
            InputDirection = localPosition.normalized;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        handle.anchoredPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }
}