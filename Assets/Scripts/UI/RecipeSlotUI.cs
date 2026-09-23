using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Text;

public class RecipeSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI recipeFormulaText; // Single text element for the whole line

    [Header("Formatting")]
    [SerializeField] private string ingredientSeparator = " + ";
    [SerializeField] private string resultSeparator = " -> ";

    [Header("Selection")]
    [SerializeField] private Color selectedColor = new(1f, 0.86f, 0.38f, 1f);

    private RecipeData recipe;
    private Color defaultColor = Color.white;
    private static RecipeSlotUI selectedSlot;

    public void Setup(RecipeData recipe)
    {
        if (recipe == null || recipe.result == null || recipe.ingredients == null) return;

        this.recipe = recipe;

        if (recipeFormulaText != null)
        {
            defaultColor = recipeFormulaText.color;
            recipeFormulaText.color = selectedSlot == this ? selectedColor : defaultColor;
        }

        StringBuilder formula = new();

        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            var ingredient = recipe.ingredients[i];
            if (ingredient.item == null) continue;

            formula.Append($"{ingredient.item.itemName} x {ingredient.amount}");

            if (i < recipe.ingredients.Length - 1)
            {
                formula.Append(ingredientSeparator);
            }
        }

        formula.Append($"{resultSeparator}{recipe.result.itemName} x {recipe.resultAmount}");

        if (recipeFormulaText != null)
        {
            recipeFormulaText.text = formula.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || recipe == null) return;

        var station = FindAnyObjectByType<CraftingStation>();
        if (station == null) return;

        selectedSlot?.SetSelected(false);
        selectedSlot = this;
        SetSelected(true);
        station.SelectRecipe(recipe);
    }

    private void OnDisable()
    {
        if (selectedSlot == this)
            selectedSlot = null;
    }

    private void SetSelected(bool isSelected)
    {
        if (recipeFormulaText != null)
            recipeFormulaText.color = isSelected ? selectedColor : defaultColor;
    }
}
