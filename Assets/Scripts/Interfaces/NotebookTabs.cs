using UnityEngine;

public class NotebookTabs : MonoBehaviour
{
    [SerializeField] private TabHoverEffect inventoryTab;
    [SerializeField] private TabHoverEffect recipesTab;
    [SerializeField] private TabHoverEffect tasksTab;

    private void Start()
    {
        ShowInventory();
    }

    public void ShowInventory()
    {
        inventoryTab.SetSelected(true);
        tasksTab.SetSelected(false);
        if (recipesTab != null) recipesTab.SetSelected(false); 
    }

    public void ShowRecipes()
    {
        inventoryTab.SetSelected(false);
        tasksTab.SetSelected(false);
        if (recipesTab != null) recipesTab.SetSelected(true);
    }

    public void ShowTasks()
    {
        inventoryTab.SetSelected(false);
        tasksTab.SetSelected(true);
        if (recipesTab != null) recipesTab.SetSelected(false); 
    }
}