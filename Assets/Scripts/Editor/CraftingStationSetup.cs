using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor helper for CraftingStation. Provides a menu item to auto-configure a crafting station
/// by finding nearby NotebookController, InventoryManager, and RecipeManager instances,
/// and creating the required UI components if they don't exist.
/// </summary>
public static class CraftingStationSetup
{
    [MenuItem("GameObject/UI/Crafting Station", false, 2)]
    private static void AddCraftingStationToScene()
    {
        var existing = GameObject.FindObjectOfType<CraftingStation>();
        if (existing != null)
        {
            Debug.LogWarning("A CraftingStation already exists in the scene.", existing.gameObject);
            return;
        }

        // Create a parent group for all crafting-related objects
        var go = new GameObject("CraftingStation", typeof(CanvasGroup), typeof(CraftingStation));
        var cs = go.GetComponent<CraftingStation>();

        // Auto-find managers if they exist in the scene
        cs.notebookController = Object.FindObjectOfType<NotebookController>();
        var beakerCtrl = Object.FindObjectOfType<CraftingBeakerController>();
        if (beakerCtrl != null)
            cs.beakerController = beakerCtrl;

        Undo.RegisterCreatedObjectUndo(go, "Add Crafting Station");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/UI/Crafting Station", true)]
    private static bool ValidateCraftingStationMenu()
    {
        return GameObject.FindObjectOfType<CraftingStation>() == null;
    }
}
