using UnityEngine;

public class ItemPickup : MonoBehaviour, InteractableUI
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    public void Interact()
    {
        if (item == null)
        {
            Debug.LogWarning($"ItemPickup on '{gameObject.name}' has no item assigned. Destroying.", this);
            Destroy(gameObject);
            return;
        }

        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogError("InventoryManager not found.", this);
            return;
        }

        inventory.AddItem(item, amount);

        Destroy(gameObject);
    }
}