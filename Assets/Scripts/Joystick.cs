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
    public float deadZone = 0f;

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

        localPosition -= background.rect.center;

        localPosition = Vector2.ClampMagnitude(
            localPosition,
            radius
        );

        handle.anchoredPosition = localPosition;

        float distance = localPosition.magnitude / radius;

        if (distance < deadZone)
        {
            InputDirection = Vector2.zero;
        }
        else
        {
            // Keep the joystick magnitude.
            // This allows Idle → Walk → Run.
            InputDirection = localPosition / radius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        handle.anchoredPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }
}