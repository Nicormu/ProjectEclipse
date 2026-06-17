using UnityEngine;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(ItemData item, int amount)
    {
        nameText.text = item.itemName;
        
        if (descriptionText != null) 
        {
            descriptionText.text = item.description; 
        }

        amountText.text = $"x{amount}";
    }
}