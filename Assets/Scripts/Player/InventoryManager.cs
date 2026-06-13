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

    private readonly Dictionary<ItemData, int> _inventory = new();

    public bool HasItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

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
        if (_inventory[item] <= 0)
        {
            _inventory.Remove(item);
        }
    }

    public void AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return;

        if (_inventory.ContainsKey(item))
        {
            _inventory[item] += amount;
        }
        else
        {
            _inventory.Add(item, amount);
        }

        //Debug.Log($"Added {amount}x {item.itemName}");
        //PrintInventory();
    }   

    public void PrintInventory()
    {
        Debug.Log("=== Inventory ===");

        if (_inventory.Count == 0)
        {
            Debug.Log("Inventory is empty.");
            return;
        }

        foreach (var item in _inventory)
        {
            Debug.Log($"{item.Key.itemName}: {item.Value}");
        }
    }

    public IReadOnlyDictionary<ItemData, int> GetInventory()
    {
        return _inventory;
    }
}