using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public event Action InventoryChanged;

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

    /// <summary>
    /// Removes an exact amount only when the inventory owns all of it.
    /// Returning the result lets transactional systems, such as crafting, avoid
    /// changing their own state when the inventory could not be charged.
    /// </summary>
    public bool RemoveItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return false;

        if (!HasItem(item, amount))
        {
            Debug.LogWarning($"Not enough {item.itemName} to remove.");
            return false;
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
        InventoryChanged?.Invoke();
        return true;
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
                        InventoryChanged?.Invoke();
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

        InventoryChanged?.Invoke();
    }

    /// <summary>Returns early when item is null or amount is non-positive.</summary>
    private static bool ValidateItem(ItemData item, int amount) => !(item == null || amount <= 0);

    public bool HasItem(ItemData item, int amount)
    {
        if (!ValidateItem(item, amount)) return false;

        int totalOwned = 0;
        foreach (var slot in _inventory)
        {
            if (slot.Item == item) totalOwned += slot.Amount;
        }
        return totalOwned >= amount;
    }

    public IReadOnlyList<InventorySlot> GetInventory()
    {
        return _inventory;
    }
}
