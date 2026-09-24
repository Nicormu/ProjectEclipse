using UnityEngine;
using System.Linq;

[CreateAssetMenu(
    fileName = "New Recipe",
    menuName = "Crafting/Recipe"
)]
public class RecipeData : ScriptableObject
{
    [Header("Ingredients")]
    public Ingredient[] ingredients;

    [Header("Result")]
    public ItemData result;
    [Min(1)]
    public int resultAmount = 1;

    private void OnValidate()
    {
        if (ingredients == null || ingredients.Length == 0)
            Debug.LogWarning($"Recipe '{name}' must have at least one ingredient.", this);

        if (result == null)
            Debug.LogWarning($"Recipe '{name}' has no result item assigned.", this);

        // Check for duplicate ingredients (same item referenced twice)
        if (ingredients != null && ingredients.Length > 0)
        {
            bool hasCoreIngredient = false;
            foreach (var ing in ingredients)
            {
                if (ing.item != null && ing.item.isCoreIngredient)
                {
                    hasCoreIngredient = true;
                    break;
                }
            }
            if (!hasCoreIngredient)
                Debug.LogWarning($"Recipe '{name}' no incluye un ingrediente central (Astromyces).", this);
        }
    }
}
