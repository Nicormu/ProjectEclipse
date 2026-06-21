using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class IngredientSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Display")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Feedback Hooks (wire animations here later)")]
    public UnityEvent onAccepted;
    public UnityEvent onRejected;

    public ItemData RequiredItem { get; private set; }
    public int RequiredAmount { get; private set; }
    public int FilledAmount { get; private set; }
    public bool IsFull => FilledAmount >= RequiredAmount;

    private CraftingBeakerController controller;

    public void Setup(Ingredient ingredient, CraftingBeakerController owningController)
    {
        controller = owningController;
        RequiredItem = ingredient.item;
        RequiredAmount = ingredient.amount;
        FilledAmount = 0;

        if (iconImage != null)
            iconImage.sprite = RequiredItem != null ? RequiredItem.itemIcon : null;

        RefreshDisplay();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI dragged = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<InventorySlotUI>()
            : null;

        if (dragged == null || dragged.Item == null)
            return;

        if (controller == null || dragged.Item != RequiredItem || IsFull)
        {
            onRejected?.Invoke();
            return;
        }

        controller.RequestFill(this, dragged.Amount);
    }

    public void AddFilled(int amount)
    {
        FilledAmount = Mathf.Min(RequiredAmount, FilledAmount + amount);
        RefreshDisplay();
        onAccepted?.Invoke();
    }

    public void ResetSlot()
    {
        FilledAmount = 0;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (amountText != null)
            amountText.text = $"{FilledAmount}/{RequiredAmount}";
    }
}