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

    public void SetCraftingStation(CraftingStation station)
    {
        if (station != null)
            craftingStation = station;
    }

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
    /// Transfers as much as the slot can accept from inventory into the beaker.
    /// The beaker is the sole source of truth for transferred ingredients: cancelling
    /// restores them and completing the craft consumes them permanently.
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

    if (!InventoryManager.Instance.RemoveItem(slot.RequiredItem, amountToAdd))
        return false;

    // Update slot visual state
    slot.AddFilled(amountToAdd);

    // Update global beaker volume tracking
    totalFilledVolume += amountToAdd;

    // Smoothly update the main beaker visual
    beakerVisual?.SetFill(FillRatio);

    if (IsRecipeReady)
        onRecipeReady?.Invoke();

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
                InventoryManager.Instance.AddItem(slot.RequiredItem, slot.FilledAmount);
            }
            slot.ResetSlot();
        }

        totalFilledVolume = 0;

        // Reset beaker visual so the fill animation doesn't drift toward a stale target after cancel
        if (beakerVisual != null)
            beakerVisual.SetFill(0f, instant: true);
    }

    public void Craft()
    {
        if (currentRecipe == null || craftingStation == null) return;

        if (IsRecipeReady)
        {
            // RequestFill has already transferred every required item from inventory.
            craftingStation.CompleteCraft(currentRecipe);
        }
        else if (totalFilledVolume == 0)
        {
            // The Craft button also supports the simpler recipe -> craft flow.
            // Do not use it after manual filling has begun: those ingredients are
            // already in the beaker and must be completed or returned on cancel.
            if (!craftingStation.TryCraft(currentRecipe))
            {
                onRecipeNotReady?.Invoke();
                return;
            }
        }
        else
        {
            Debug.Log("Finish filling the beaker or cancel the current mixture.", this);
            onRecipeNotReady?.Invoke();
            return;
        }

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
            {
                InventoryManager.Instance.AddItem(slot.RequiredItem, slot.FilledAmount);
            }
        }

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
