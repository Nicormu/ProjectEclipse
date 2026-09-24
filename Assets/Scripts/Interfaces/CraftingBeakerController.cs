using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingBeakerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CraftingStation craftingStation;
    [SerializeField] private BeakerFillVisual beakerVisual;

    [Header("Events")]
    public UnityEvent onRecipeReady;
    public UnityEvent onRecipeNotReady;

    private RecipeData currentRecipe;
    private readonly Dictionary<ItemData, int> requiredAmounts = new();
    private readonly Dictionary<ItemData, int> filledAmounts = new();
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
        requiredAmounts.Clear();
        filledAmounts.Clear();
        currentRecipe = recipe;
        totalRequiredVolume = 0;
        totalFilledVolume = 0;

        UpdateBeakerVisual(instant: true);

        if (recipe == null || recipe.ingredients == null) return;

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            if (ingredient.item == null) continue;

            requiredAmounts[ingredient.item] = requiredAmounts.TryGetValue(ingredient.item, out int existing)
                ? existing + ingredient.amount
                : ingredient.amount;
            filledAmounts[ingredient.item] = 0;
            totalRequiredVolume += ingredient.amount;
        }

        onRecipeNotReady?.Invoke();
    }

    /// <summary>
    /// Called when an inventory item is dropped on the beaker. Transfers as much as the
    /// recipe still needs of that item from inventory into the beaker.
    /// </summary>
    public bool RequestFill(ItemData item, int draggedAmount)
    {
        if (item == null || currentRecipe == null) return false;
        if (!requiredAmounts.TryGetValue(item, out int required)) return false;

        int filled = filledAmounts[item];
        int spaceLeft = required - filled;
        if (spaceLeft <= 0) return false;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in scene.", this);
            return false;
        }

        int amountToAdd = Mathf.Min(draggedAmount, spaceLeft);
        if (!InventoryManager.Instance.RemoveItem(item, amountToAdd))
            return false;

        filledAmounts[item] = filled + amountToAdd;
        totalFilledVolume += amountToAdd;

        beakerVisual?.SetFill(FillRatio);

        if (IsRecipeReady)
            onRecipeReady?.Invoke();

        return true;
    }

    /// <summary>Returns every item currently in the beaker back to inventory.</summary>
    public void CancelAndRestore()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in scene.", this);
            return;
        }

        foreach (var kvp in filledAmounts)
        {
            if (kvp.Value > 0)
                InventoryManager.Instance.AddItem(kvp.Key, kvp.Value);
        }

        foreach (var item in new List<ItemData>(filledAmounts.Keys))
            filledAmounts[item] = 0;

        totalFilledVolume = 0;

        if (beakerVisual != null)
            beakerVisual.SetFill(0f, instant: true);
    }

    public void Craft()
    {
        if (currentRecipe == null || craftingStation == null || !IsRecipeReady)
        {
            Debug.Log("Finish filling the beaker to craft.", this);
            onRecipeNotReady?.Invoke();
            return;
        }

        craftingStation.CompleteCraft(currentRecipe);

        foreach (var item in new List<ItemData>(filledAmounts.Keys))
            filledAmounts[item] = 0;

        totalFilledVolume = 0;
        beakerVisual?.SetFill(0f, instant: true);

        onRecipeNotReady?.Invoke();
    }

    public void UpdateBeakerVisual(bool instant = false)
    {
        if (beakerVisual == null) return;
        beakerVisual.SetFill(FillRatio, instant);
    }
}