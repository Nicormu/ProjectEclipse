using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingBeakerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CraftingStation craftingStation;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject ingredientSlotPrefab;
    [SerializeField] private BeakerFillVisual beakerVisual;

    [Header("Events")]
    public UnityEvent onRecipeReady;
    public UnityEvent onRecipeNotReady;

    private RecipeData currentRecipe;
    private readonly List<IngredientSlotUI> activeSlots = new();
    private int totalRequiredVolume;
    private int totalFilledVolume;

    public float FillRatio => totalRequiredVolume > 0 ? (float)totalFilledVolume / totalRequiredVolume : 0f;

    public bool IsRecipeReady => currentRecipe != null && totalFilledVolume >= totalRequiredVolume && totalRequiredVolume > 0;

    public void LoadRecipe(RecipeData recipe)
    {
        ClearSlots();
        currentRecipe = recipe;
        totalRequiredVolume = 0;
        totalFilledVolume = 0;

        UpdateBeakerVisual(instant: true);

        if (recipe == null || recipe.ingredients == null) return;

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            GameObject slotObject = Instantiate(ingredientSlotPrefab, slotsParent);

            if (slotObject.TryGetComponent<IngredientSlotUI>(out var slotUI))
            {
                slotUI.Setup(ingredient, this);
                activeSlots.Add(slotUI);
                totalRequiredVolume += ingredient.amount;
            }
        }

        onRecipeNotReady?.Invoke();
    }

    /// <summary>
    /// Called by IngredientSlotUI when a valid item is dropped.
    /// Commits inventory, updates beaker visuals, returns true if slot was filled.
    /// Excess items (dragged beyond spaceLeft) are automatically returned to inventory.
    /// Items are consumed from inventory immediately and tracked via CommitItem so they
    /// cannot be used elsewhere until finalized or released.
    /// </summary>
    public bool RequestFill(IngredientSlotUI slot, int draggedAmount)
    {
        if (slot == null || slot.RequiredItem == null) return false;
        if (slot.IsFull || currentRecipe == null) return false;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in scene.", this);
            return false;
        }

        int spaceLeft = slot.RequiredAmount - slot.FilledAmount;
        int amountToAdd = Mathf.Min(draggedAmount, spaceLeft);
        if (amountToAdd <= 0) return false;

        int excess = draggedAmount - amountToAdd;

        // Remove all dragged items from inventory (they move into the beaker system)
        InventoryManager.Instance.RemoveItem(slot.RequiredItem, draggedAmount);
        // Commit only what was actually filled (tracked toward recipe completion)
        InventoryManager.Instance.CommitItem(slot.RequiredItem, amountToAdd);

        // Update slot visual state
        slot.AddFilled(amountToAdd);

        // Return excess items to inventory
        if (excess > 0)
            InventoryManager.Instance.AddItem(slot.RequiredItem, excess);

        // Update global beaker volume tracking
        totalFilledVolume += amountToAdd;

        // Smoothly update the main beaker visual
        beakerVisual?.SetFill(FillRatio);

        if (IsRecipeReady)
            onRecipeReady?.Invoke();

        return true;
    }

    /// <summary>
    /// Called when crafting starts — consumes remaining uncommitted ingredients from inventory.
    /// CanCraft was already verified (via corrected HasItem), so this always succeeds.
    /// Committed items tracked in InventoryManager represent what's in beaker slots;
    /// excess committed amounts are implicitly released via ClearCommittedItems.
    /// </summary>
    public bool FinalizeCraft()
    {
        if (currentRecipe == null) return false;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in scene.", this);
            return false;
        }

        foreach (Ingredient ingredient in currentRecipe.ingredients)
        {
            // Amount already in beaker slots for this ingredient type
            int inBeaker = 0;
            foreach (var slot in activeSlots)
                if (slot.RequiredItem == ingredient.item)
                    inBeaker += slot.FilledAmount;

            // Remove only the portion not yet in any beaker slot
            int neededFromInventory = Mathf.Max(0, ingredient.amount - inBeaker);
            if (neededFromInventory > 0)
                InventoryManager.Instance.RemoveItem(ingredient.item, neededFromInventory);
        }

        // Commit items are released (items in beaker slots become part of the craft result)
        InventoryManager.Instance.ClearCommittedItems();

        return true;
    }

    /// <summary>
    /// Called when crafting is cancelled — returns all beaker items back to inventory.
    /// </summary>
    public void CancelAndRestore()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in scene.", this);
            return;
        }

        foreach (var slot in activeSlots)
        {
            if (slot.FilledAmount > 0)
            {
                InventoryManager.Instance.ReleaseItem(slot.RequiredItem, slot.FilledAmount);
            }
            slot.ResetSlot();
        }

        InventoryManager.Instance.ClearCommittedItems();
        totalFilledVolume = 0;

        // Reset beaker visual so the fill animation doesn't drift toward a stale target after cancel
        if (beakerVisual != null)
            beakerVisual.SetFill(0f, instant: true);
    }

    public void Craft()
    {
        if (currentRecipe == null || craftingStation == null) return;

        // Consume remaining uncommitted ingredients from inventory.
        // CanCraft was verified before this call, so sufficient items exist.
        FinalizeCraft();

        // Add result to inventory and discover recipe
        craftingStation.Craft(currentRecipe);

        // Reset slots and visual
        foreach (var slot in activeSlots)
            slot.ResetSlot();

        totalFilledVolume = 0;
        beakerVisual?.SetFill(0f, instant: true);

        onRecipeNotReady?.Invoke();
    }

    private void ClearSlots()
    {
        // Restore any remaining items before destroying slots
        foreach (var slot in activeSlots)
        {
            if (slot.FilledAmount > 0 && InventoryManager.Instance != null)
                InventoryManager.Instance.ReleaseItem(slot.RequiredItem, slot.FilledAmount);
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearCommittedItems();

        foreach (var slot in activeSlots)
            if (slot != null && slot.gameObject != null) Destroy(slot.gameObject);

        activeSlots.Clear();
    }

    public void UpdateBeakerVisual(bool instant = false)
    {
        if (beakerVisual == null) return;
        beakerVisual.SetFill(FillRatio, instant);
    }
}
