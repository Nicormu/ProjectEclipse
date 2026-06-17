using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData Item { get; private set; }
    public int Amount { get; set; } 

    public InventorySlot(ItemData item, int amount)
    {
        this.Item = item;
        this.Amount = amount;
    }
}