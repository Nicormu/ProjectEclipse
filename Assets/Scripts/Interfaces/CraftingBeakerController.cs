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

    public bool IsRecipeReady => currentRecipe != null && activeSlots.TrueForAll(s => s.IsFull);

    public void LoadRecipe(RecipeData recipe)
    {
        ClearSlots();
        currentRecipe = recipe;

        UpdateBeakerVisual(instant: true);

        if (recipe == null || recipe.ingredients == null) return;

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            GameObject slotObject = Instantiate(ingredientSlotPrefab, slotsParent);

            if (slotObject.TryGetComponent<IngredientSlotUI>(out var slotUI))
            {
                slotUI.Setup(ingredient, this);
                activeSlots.Add(slotUI);
            }
        }

        onRecipeNotReady?.Invoke();
    }

    public void RequestFill(IngredientSlotUI slot, int draggedAmount)
    {
        int amountToAdd = Mathf.Min(draggedAmount, slot.RequiredAmount - slot.FilledAmount);
        if (amountToAdd <= 0) return;

        slot.AddFilled(amountToAdd);
        UpdateBeakerVisual();

        if (IsRecipeReady)
            onRecipeReady?.Invoke();
    }

    public void Craft()
    {
        if (!IsRecipeReady || craftingStation == null) return;

        craftingStation.Craft(currentRecipe); 
        LoadRecipe(currentRecipe);         
    }

    private void ClearSlots()
    {
        foreach (var slot in activeSlots)
            if (slot != null) Destroy(slot.gameObject);

        activeSlots.Clear();
    }

    private void UpdateBeakerVisual(bool instant = false)
    {
        if (beakerVisual == null) return;

        if (activeSlots.Count == 0)
        {
            beakerVisual.SetFill(0f, instant);
            return;
        }

        int totalRequired = 0;
        int totalFilled = 0;

        foreach (var slot in activeSlots)
        {
            totalRequired += slot.RequiredAmount;
            totalFilled += slot.FilledAmount;
        }

        float ratio = totalRequired > 0 ? (float)totalFilled / totalRequired : 0f;
        beakerVisual.SetFill(ratio, instant);
    }
}