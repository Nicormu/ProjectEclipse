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
        var existing = GameObject.FindAnyObjectByType<CraftingStation>();
        if (existing != null)
        {
            Debug.LogWarning("A CraftingStation already exists in the scene.", existing.gameObject);
            return;
        }

        // Parent holds the script and must stay active so Interact()/Update() keep working
        // even while the visible panel is faded out/hidden.
        var go = new GameObject("CraftingStation", typeof(CraftingStation));
        var cs = go.GetComponent<CraftingStation>();

        // Separate child holds the CanvasGroup that actually gets shown/hidden.
        var canvasGO = new GameObject("CraftingCanvas", typeof(CanvasGroup));
        canvasGO.transform.SetParent(go.transform, false);

        var so = new SerializedObject(cs);

        so.FindProperty("craftingCanvasGroup").objectReferenceValue = canvasGO.GetComponent<CanvasGroup>();
        so.FindProperty("notebookController").objectReferenceValue = Object.FindAnyObjectByType<NotebookController>();

        var beakerCtrl = Object.FindAnyObjectByType<CraftingBeakerController>();
        if (beakerCtrl != null)
            so.FindProperty("beakerController").objectReferenceValue = beakerCtrl;

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, "Add Crafting Station");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/UI/Crafting Station", true)]
    private static bool ValidateCraftingStationMenu()
    {
        return GameObject.FindAnyObjectByType<CraftingStation>() == null;
    }
}