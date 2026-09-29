using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class WorldUnlock
    {
        public string label; // solo para identificarlo en el inspector
        public GameObject prefab;
        public Transform spawnPoint;
        public int price;
        public bool isPurchased;
    }

    [SerializeField] private List<WorldUnlock> worldUnlocks = new();

    private void Start()
    {
        foreach (var unlock in worldUnlocks)
        {
            if (unlock.isPurchased && unlock.prefab != null && unlock.spawnPoint != null)
            {
                Instantiate(unlock.prefab, unlock.spawnPoint.position, unlock.spawnPoint.rotation);
            }
        }
    }

    public bool TryPurchaseUnlock(int index)
    {
        if (index < 0 || index >= worldUnlocks.Count)
        {
            Debug.LogWarning($"TryPurchaseUnlock: index {index} fuera de rango.");
            return false;
        }

        var unlock = worldUnlocks[index];
        if (unlock.isPurchased)
        {
            Debug.LogWarning($"TryPurchaseUnlock: '{unlock.label}' ya fue comprado.");
            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("TryPurchaseUnlock: no hay CurrencyManager en la escena.");
            return false;
        }

        if (!CurrencyManager.Instance.Spend(unlock.price))
        {
            Debug.LogWarning($"TryPurchaseUnlock: plata insuficiente (necesita {unlock.price}, tiene {CurrencyManager.Instance.Money}).");
            return false;
        }

        Instantiate(unlock.prefab, unlock.spawnPoint.position, unlock.spawnPoint.rotation);
        unlock.isPurchased = true;
        return true;
    }

    public bool TryPurchaseItem(ItemData item, int amount = 1)
    {
        if (item == null || !item.canBuy) return false;
        if (CurrencyManager.Instance == null || InventoryManager.Instance == null) return false;

        int totalPrice = item.buyPrice * amount;
        if (!CurrencyManager.Instance.Spend(totalPrice)) return false;

        InventoryManager.Instance.AddItem(item, amount);
        return true;
    }

    // Wrappers void de un solo parámetro para poder engancharlos en el OnClick de un Button.
    public void PurchaseUnlock(int index) => TryPurchaseUnlock(index);

    public void PurchaseItem(ItemData item) => TryPurchaseItem(item);
}