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

    private float radius;
    private bool isDragging = false;

    private void Start()
    {
        radius = background.rect.width / 2f;

        InputDirection = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Don't move the joystick when it is only tapped.
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Vector2 position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        position = Vector2.ClampMagnitude(position, radius);

        handle.anchoredPosition = position;

        InputDirection = position / radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        handle.anchoredPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }
}