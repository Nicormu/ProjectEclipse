using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to a raycastable child of an InventorySlotUI (text, image, etc.). Unity routes
/// the drag lifecycle to the first raycast hit, so this forwards that lifecycle to the
/// owning inventory slot without turning ordinary clicks into drag attempts.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlotDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = GetComponentInParent<InventorySlotUI>();
        slot?.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) => GetComponentInParent<InventorySlotUI>()?.OnDrag(eventData);

    public void OnEndDrag(PointerEventData eventData) => GetComponentInParent<InventorySlotUI>()?.OnEndDrag(eventData);
}
