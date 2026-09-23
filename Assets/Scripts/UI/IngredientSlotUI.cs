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

    [Header("Progress Feedback")]
    [SerializeField] private Color filledTint = new(0.65f, 1f, 0.72f, 1f);

    [Header("Feedback Hooks")]
    public UnityEvent onAccepted;
    public UnityEvent onRejected;

    public ItemData RequiredItem { get; private set; }
    public int RequiredAmount { get; private set; }
    public int FilledAmount { get; private set; }
    public bool IsFull => FilledAmount >= RequiredAmount;

    private CraftingBeakerController controller;
    private Color iconBaseColor = Color.white;

    public void Setup(Ingredient ingredient, CraftingBeakerController owningController)
    {
        controller = owningController;
        RequiredItem = ingredient.item;
        RequiredAmount = ingredient.amount;
        FilledAmount = 0;

        if (iconImage != null)
        {
            iconImage.sprite = RequiredItem != null ? RequiredItem.itemIcon : null;
            iconBaseColor = iconImage.color;
        }

        RefreshDisplay();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI dragged = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<InventorySlotUI>()
            : null;

        if (dragged == null || dragged.Item == null)
            return;

        // Reject wrong item type or already-full slot
        if (controller == null || dragged.Item != RequiredItem || IsFull)
        {
            onRejected?.Invoke();
            return;
        }

        if (!controller.RequestFill(this, dragged.Amount))
            onRejected?.Invoke();
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
        if (iconImage != null)
        {
            float progress = RequiredAmount > 0 ? (float)FilledAmount / RequiredAmount : 0f;
            iconImage.color = Color.Lerp(iconBaseColor, filledTint, progress);
        }
    }
}
