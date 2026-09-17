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

    private void Start()
    {
        radius = background.sizeDelta.x / 2f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
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
        handle.anchoredPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }
}