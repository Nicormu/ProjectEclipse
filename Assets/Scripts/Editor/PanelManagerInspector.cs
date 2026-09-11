using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor helper for PanelManager. Auto-creates and configures a PanelManager instance in the current scene,
/// placing it inside a dedicated persistent game object that survives scene loads via DontDestroyOnLoad.
/// </summary>
public static class PanelManagerInspector
{
    [MenuItem("GameObject/UI/Panel Manager", false, 1)]
    private static void AddPanelManagerToScene()
    {
        var existing = GameObject.FindObjectOfType<PanelManager>();
        if (existing != null)
        {
            Debug.LogWarning("A PanelManager already exists in the scene.", existing.gameObject);
            return;
        }

        var go = new GameObject("PanelManager");
        go.AddComponent<PanelManager>();
        Undo.RegisterCreatedObjectUndo(go, "Add Panel Manager");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/UI/Panel Manager", true)]
    private static bool ValidatePanelManagerMenu()
    {
        return GameObject.FindObjectOfType<PanelManager>() == null;
    }
}
