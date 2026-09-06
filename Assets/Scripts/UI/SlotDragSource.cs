using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to any child of an InventorySlotUI (text, image, etc.) so that clicking it
/// initiates a drag. Unity's event system routes pointer events only to the first raycast hit,
/// so without this on children, clicks on text never reach the slot's root handler.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlotDragSource : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        var slot = GetComponentInParent<InventorySlotUI>();
        slot?.StartDrag();
    }
}
