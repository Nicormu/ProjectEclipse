using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SlotDragSource : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"OnPointerDown on {gameObject.name}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"SlotDragSource.OnBeginDrag on {gameObject.name}");
        var slot = GetComponentInParent<InventorySlotUI>();
        Debug.Log($"Found parent InventorySlotUI: {slot}");
        slot?.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) => GetComponentInParent<InventorySlotUI>()?.OnDrag(eventData);

    public void OnEndDrag(PointerEventData eventData) => GetComponentInParent<InventorySlotUI>()?.OnEndDrag(eventData);
}