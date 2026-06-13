using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    public void Interact()
    {
        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
        
        if (inventory == null)
        {
            Debug.LogError("InventoryManager not found.");
            return;
        }

        inventory.AddItem(item, amount);

        Destroy(gameObject);
    }
}