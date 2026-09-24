using UnityEngine;
using UnityEngine.EventSystems;

public class BeakerDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private CraftingBeakerController controller;

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI dragged = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<InventorySlotUI>()
            : null;

        if (dragged == null || dragged.Item == null) return;

        controller.RequestFill(dragged.Item, dragged.Amount);
    }
}