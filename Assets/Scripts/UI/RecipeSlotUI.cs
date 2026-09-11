using UnityEngine;
using TMPro;
using System.Text;

public class RecipeSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI recipeFormulaText; // Single text element for the whole line

    [Header("Formatting")]
    [SerializeField] private string ingredientSeparator = " + ";
    [SerializeField] private string resultSeparator = " -> ";

    public void Setup(RecipeData recipe)
    {
        if (recipe == null || recipe.result == null || recipe.ingredients == null) return;

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
}
