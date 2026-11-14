using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private VirginDropPlayerController player;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float radius = 92f;

    public void Configure(VirginDropPlayerController target, RectTransform thumb)
    {
        player = target;
        handle = thumb;
    }

    public void OnPointerDown(PointerEventData eventData) => UpdateInput(eventData);
    public void OnDrag(PointerEventData eventData) => UpdateInput(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        if (player != null) player.SetTouchMoveInput(Vector2.zero);
    }

    private void UpdateInput(PointerEventData eventData)
    {
        var root = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, eventData.position, eventData.pressEventCamera, out var local))
            return;
        var input = Vector2.ClampMagnitude(local / radius, 1f);
        if (handle != null) handle.anchoredPosition = input * radius;
        if (player != null) player.SetTouchMoveInput(input);
    }
}

public sealed class MobileAttackButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private VirginDropPlayerController player;

    public void Configure(VirginDropPlayerController target) => player = target;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (player != null) player.RequestAttack();
    }
}
