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

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found.", this);
            return;
        }

        InventoryManager.Instance.AddItem(item, amount);

        Destroy(gameObject);
    }
}
