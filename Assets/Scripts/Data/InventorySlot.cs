using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData Item { get; private set; }
    private int _amount;
    /// <summary>Clamped to non-negative. Outside code may write freely — all writes are clamped.</summary>
    public int Amount { get => _amount; set => _amount = Mathf.Max(0, value); }

    public InventorySlot(ItemData item, int amount)
    {
        this.Item = item;
        this.Amount = Mathf.Max(0, amount);
    }
}