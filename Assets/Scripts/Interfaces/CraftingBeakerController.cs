using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingBeakerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CraftingStation craftingStation;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject ingredientSlotPrefab;

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
}