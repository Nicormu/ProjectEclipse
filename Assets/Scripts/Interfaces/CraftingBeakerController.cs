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

    public void RequestFill(IngredientSlotUI slot, int draggedAmount)
    {
        if (slot.IsFull || currentRecipe == null) return;

        int spaceLeft = slot.RequiredAmount - slot.FilledAmount;
        int amountToAdd = Mathf.Min(draggedAmount, spaceLeft);
        
        if (amountToAdd <= 0) return;

        // Update slot state
        slot.AddFilled(amountToAdd);
        
        // Update global beaker volume tracking
        totalFilledVolume += amountToAdd;
        
        // Smoothly update the main beaker visual
        float fillRatio = totalRequiredVolume > 0 ? (float)totalFilledVolume / totalRequiredVolume : 0f;
        beakerVisual?.SetFill(fillRatio);

        if (IsRecipeReady)
            onRecipeReady?.Invoke();
    }

    public void Craft()
    {
        if (!IsRecipeReady || craftingStation == null) return;

        craftingStation.Craft(currentRecipe); 
        
        // Reset slots and visual after crafting
        foreach (var slot in activeSlots)
            slot.ResetSlot();
            
        totalFilledVolume = 0;
        beakerVisual?.SetFill(0f, instant: true);
        
        onRecipeNotReady?.Invoke();
    }

    private void ClearSlots()
    {
        foreach (var slot in activeSlots)
            if (slot != null && slot.gameObject != null) Destroy(slot.gameObject);

        activeSlots.Clear();
    }

    public void UpdateBeakerVisual(bool instant = false)
    {
        if (beakerVisual == null) return;
        
        float fillRatio = totalRequiredVolume > 0 ? (float)totalFilledVolume / totalRequiredVolume : 0f;
        beakerVisual.SetFill(fillRatio, instant);
    }
}
