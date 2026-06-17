using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [System.Serializable]
    public class UnlockingRecipe
    {
        public RecipeData recipeData;
        public bool isDiscovered;
    }

    [SerializeField] private List<UnlockingRecipe> allRecipes = new();

    public void DiscoverRecipe(RecipeData recipe)
    {
        var target = allRecipes.Find(r => r.recipeData == recipe);
        if (target != null && !target.isDiscovered)
        {
            target.isDiscovered = true;
            Debug.Log($"Unlocked recipe for: {recipe.result.itemName}");
        }
    }

    public List<RecipeData> GetDiscoveredRecipes()
    {
        List<RecipeData> discovered = new();
        foreach (var recipe in allRecipes)
        {
            if (recipe.isDiscovered)
            {
                discovered.Add(recipe.recipeData);
            }
        }
        return discovered;
    }
}