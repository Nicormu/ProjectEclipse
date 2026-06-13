using TMPro;
using UnityEngine;
using System.Text;

public class InventoryDisplay : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private TextMeshProUGUI inventoryText;

    public void Refresh()
    {
        if (inventoryManager == null || inventoryText == null)
        {
            Debug.LogError("InventoryDisplay: Missing references!");
            return;
        }

        var inventory = inventoryManager.GetInventory();

        StringBuilder sb = new();

        foreach (var item in inventory)
        {
            if (item.Key == null) continue;

            sb.AppendLine($"{item.Key.itemName} x{item.Value}");
        }

        inventoryText.text = sb.ToString();
    }
}