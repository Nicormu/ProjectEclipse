using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RecipeManager : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnValidate()
    {
        _cacheDirty = true;
    }
#endif

    [System.Serializable]
    public class UnlockingRecipe
    {
        public RecipeData recipeData;
        public bool isDiscovered;
    }

    [SerializeField] private List<UnlockingRecipe> allRecipes = new();
    private List<RecipeData> _discoveredCache;
    private bool _cacheDirty = true;

    public void DiscoverRecipe(RecipeData recipe)
    {
        var target = allRecipes.Find(r => r.recipeData == recipe);
        if (target != null && !target.isDiscovered)
        {
            target.isDiscovered = true;
            Debug.Log($"Unlocked recipe for: {recipe.result.itemName}");
            _cacheDirty = true;
        }
    }

    private void RefreshCache()
    {
        if (!_cacheDirty) return;

        _discoveredCache = new List<RecipeData>();
        foreach (var recipe in allRecipes)
        {
            if (recipe.isDiscovered)
                _discoveredCache.Add(recipe.recipeData);
        }
        _cacheDirty = false;
    }

    public IReadOnlyList<RecipeData> GetDiscoveredRecipes()
    {
        RefreshCache();
        return _discoveredCache;
    }
}
