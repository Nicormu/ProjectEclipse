using UnityEngine;
using System.Collections.Generic;
using System;

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
        DontDestroyOnLoad(gameObject);
    }

    private readonly List<InventorySlot> _inventory = new();

    public void RemoveItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return;

        if (!HasItem(item, amount))
        {
            Debug.LogWarning($"Not enough {item.itemName} to remove.");
            return;
        }

        for (int i = _inventory.Count - 1; i >= 0; i--)
        {
            if (_inventory[i].Item == item)
            {
                if (_inventory[i].Amount >= amount)
                {
                    _inventory[i].Amount -= amount;
                    break;
                }
                else
                {
                    amount -= _inventory[i].Amount;
                    _inventory[i].Amount = 0; // Will be cleaned up below
                }
            }
        }

        // Clean up dead slots with zero amount to prevent accumulation
        _inventory.RemoveAll(slot => slot.Amount <= 0);
    }

    public void AddItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return;

        if (item.stackable)
        {
            foreach (var slot in _inventory)
            {
                if (slot.Item == item && slot.Amount < item.maxStack)
                {
                    int spaceLeft = item.maxStack - slot.Amount;
                    if (amount <= spaceLeft)
                    {
                        slot.Amount += amount;
                        return;
                    }
                    else
                    {
                        slot.Amount += spaceLeft;
                        amount -= spaceLeft;
                    }
                }
            }
        }

        while (amount > 0)
        {
            int amountToAdd = item.stackable ? Mathf.Min(amount, item.maxStack) : 1;
            _inventory.Add(new InventorySlot(item, amountToAdd));
            amount -= amountToAdd;
        }
    }

    // Track items that are committed to beaker slots (not yet crafted).
    // HasItem includes these so they can't be used elsewhere.
    private readonly Dictionary<ItemData, int> _committedItems = new();

    /// <summary>
    /// Track that an item has been placed into a beaker slot (counted toward crafting).
    /// Does NOT remove from inventory — FinalizeCraft handles inventory deduction after verification.
    /// </summary>
    public void CommitItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return;
        _committedItems[item] = _committedItems.GetValueOrDefault(item, 0) + amount;
    }

    /// <summary>
    /// Release committed items back to active inventory availability (undo tracking).
    /// </summary>
    public void ReleaseItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return;
        if (_committedItems.TryGetValue(item, out var current))
        {
            _committedItems[item] = Mathf.Max(0, current - amount);
            if (_committedItems[item] <= 0)
                _committedItems.Remove(item);
        }
    }

    /// <summary>Returns early when item is null or amount is non-positive.</summary>
    private static bool ValidateItem(ItemData item, int amount) => !(item == null || amount <= 0);

    public bool HasItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return false;

        // Count only uncommitted items: committed items have been moved to a beaker slot
        // and are tracked separately via _committedItems. Subtracting them ensures we report
        // only the quantity available for other uses.
        int totalOwned = 0;
        foreach (var slot in _inventory)
        {
            if (slot.Item == item) totalOwned += slot.Amount;
        }
        int committed = _committedItems.GetValueOrDefault(item, 0);
        return Math.Max(0, totalOwned - committed) >= amount;
    }

    public void ClearCommittedItems()
    {
        _committedItems.Clear();
    }

    public IReadOnlyList<InventorySlot> GetInventory()
    {
        return _inventory;
    }
}
