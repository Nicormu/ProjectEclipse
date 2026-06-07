using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Using a dictionary to store items and their counts.
    private readonly Dictionary<ItemData, int> _inventory = new();

    public bool HasItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        // Check if the inventory has the item and if the quantity is sufficient.
        return _inventory.TryGetValue(item, out int currentAmount) && currentAmount >= amount;
    }

    public void RemoveItem(ItemData item, int amount)
    {
        if (!HasItem(item, amount))
        {
            Debug.LogWarning($"Tried to remove {amount} of {item.itemName}, but not enough in inventory.");
            return;
        }

        _inventory[item] -= amount;

        // If the item count drops to 0 or below, remove it from the dictionary.
        if (_inventory[item] <= 0)
        {
            _inventory.Remove(item);
        }
    }

    public void AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return;

        // If we already have the item, add to the count. Otherwise, add the item to the inventory.
        if (_inventory.ContainsKey(item))
        {
            _inventory[item] += amount;
        }
        else
        {
            _inventory.Add(item, amount);
        }
    }
}