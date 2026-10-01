using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button buyButton;

    public ItemData Item { get; private set; }

    public void Setup(ItemData item, Action<ItemData> onBuy)
    {
        Item = item;

        if (nameText != null) nameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (priceText != null) priceText.text = $"${item.buyPrice}";
        if (iconImage != null) iconImage.sprite = item.itemIcon;

        buyButton.onClick.AddListener(() => onBuy(item));
    }

    public void SetAffordable(bool affordable)
    {
        buyButton.interactable = affordable;
    }
}