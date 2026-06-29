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

    private readonly List<InventorySlot> _inventory = new(); 

    public bool HasItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        int totalOwned = 0;
        foreach (var slot in _inventory)
        {
            if (slot.Item == item) totalOwned += slot.Amount;
        }

        return totalOwned >= amount;
    }

    public void RemoveItem(ItemData item, int amount)
    {
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
                    _inventory[i].Amount = 0; // Keep the empty entry in the journal
                }
            }
        }
    }

    public void AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return;

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

    public IReadOnlyList<InventorySlot> GetInventory()
    {
        return _inventory;
    }
}