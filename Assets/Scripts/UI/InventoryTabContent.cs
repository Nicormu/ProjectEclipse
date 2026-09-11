using UnityEngine;

public class InventoryTabContent : MonoBehaviour, INotebookTabContent
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject slotPrefab;

    public GameObject SlotPrefab => slotPrefab;
    public int ItemCount => inventoryManager != null ? inventoryManager.GetInventory().Count : 0;

    public void Refresh()
    {
        if (inventoryManager == null) return;
    }

    public bool PopulateSlot(int index, GameObject slotInstance)
    {
        if (inventoryManager == null || inventoryManager.GetInventory().Count <= index)
            return false;

        var slot = inventoryManager.GetInventory()[index];
        if (slot.Item == null) return false;

        if (slotInstance.TryGetComponent<InventorySlotUI>(out var slotUI))
        {
            slotUI.Setup(slot.Item, slot.Amount);
        }
        return true;
    }
}
