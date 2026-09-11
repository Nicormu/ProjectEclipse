using UnityEngine;
using System.Collections.Generic;

public class RecipeTabContent : MonoBehaviour, INotebookTabContent
{
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private GameObject slotPrefab;

    private List<RecipeData> _discovered = new();
    private bool _discoveryCacheDirty = true;

    public GameObject SlotPrefab => slotPrefab;
    public int ItemCount => _discovered.Count;

    public void Refresh()
    {
        if (_discoveryCacheDirty)
        {
            _discovered.Clear();
            if (recipeManager != null)
            {
                var discovered = recipeManager.GetDiscoveredRecipes();
                for (int i = 0; i < discovered.Count; i++)
                    _discovered.Add(discovered[i]);
            }
            _discoveryCacheDirty = false;
        }
    }

    public bool PopulateSlot(int index, GameObject slotInstance)
    {
        if (recipeManager == null || index >= _discovered.Count) return false;
        if (!slotInstance.TryGetComponent<RecipeSlotUI>(out var recipeUI)) return false;

        recipeUI.Setup(_discovered[index]);
        return true;
    }
}
