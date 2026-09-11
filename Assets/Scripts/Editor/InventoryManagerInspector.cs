using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor helper for InventoryManager. Auto-creates and configures an InventoryManager instance in the current scene.
/// </summary>
public static class InventoryManagerInspector
{
    [MenuItem("GameObject/Game Manager/Inventory Manager", false, 1)]
    private static void AddInventoryManagerToScene()
    {
        var existing = GameObject.FindObjectOfType<InventoryManager>();
        if (existing != null)
        {
            Debug.LogWarning("An InventoryManager already exists in the scene.", existing.gameObject);
            return;
        }

        var go = new GameObject("InventoryManager");
        go.AddComponent<InventoryManager>();
        Undo.RegisterCreatedObjectUndo(go, "Add Inventory Manager");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/Game Manager/Inventory Manager", true)]
    private static bool ValidateInventoryManagerMenu()
    {
        return GameObject.FindObjectOfType<InventoryManager>() == null;
    }
}
