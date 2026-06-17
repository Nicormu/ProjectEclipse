using UnityEngine;
using System.Linq; 
using System.Collections.Generic;
using TMPro;

public enum NotebookTab 
{    
    Inventory, 
    Recipes,
    Tasks 
}

public class NotebookDisplay : MonoBehaviour
{
    [Header("Notebook State")]
    public NotebookTab currentTab = NotebookTab.Inventory;
    [SerializeField] private TextMeshProUGUI pageTitleText;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private GameObject recipeSlotPrefab;
    [SerializeField] private Transform contentPanel;

    [Header("Pagination Settings")]
    [SerializeField] private int itemsPerPage = 8;
    [SerializeField] private TextMeshProUGUI pageNumberText; 

    [Header("Tab Hover Effects")]
    [SerializeField] private TabHoverEffect inventoryTabHover;
    [SerializeField] private TabHoverEffect recipesTabHover;
    [SerializeField] private TabHoverEffect tasksTabHover;

    [Header("Pagination Buttons")]
    [SerializeField] private GameObject nextButton; 
    [SerializeField] private GameObject prevButton; 
    
    private int currentPage = 0;

    private void OnEnable()
    {
        currentTab = NotebookTab.Inventory;
        currentPage = 0;
        UpdateTabVisuals();
        UpdateDisplay();
    }

    public void Refresh()
    {
        if (gameObject.activeInHierarchy)
        {
            UpdateDisplay();
        }
    }

    public void SetTabInstant(NotebookTab newTab)
    {
        currentTab = newTab;
        currentPage = 0; 
        UpdateTabVisuals();
        UpdateDisplay();
    }

    private void UpdateTabVisuals()
    {
        if (inventoryTabHover != null) 
            inventoryTabHover.SetSelected(currentTab == NotebookTab.Inventory);
        
        if (recipesTabHover != null) 
            recipesTabHover.SetSelected(currentTab == NotebookTab.Recipes);
            
        if (tasksTabHover != null) 
            tasksTabHover.SetSelected(currentTab == NotebookTab.Tasks);
    }

    private void UpdateDisplay()
    {
        if (slotPrefab == null || contentPanel == null) return;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        int maxPages = 1;
        int listCount = 0;

        if (currentTab == NotebookTab.Inventory)
        {
            if (pageTitleText != null) pageTitleText.text = "INVENTORY";

            if (inventoryManager != null)
            {
                var inventory = inventoryManager.GetInventory();
                var itemList = inventory.ToList();
                listCount = itemList.Count;

                CalculatePagination(listCount, out maxPages);

                int startIndex = currentPage * itemsPerPage;
                int endIndex = startIndex + itemsPerPage;

                for (int i = startIndex; i < endIndex && i < itemList.Count; i++)
                {
                    var slot = itemList[i];
                    if (slot.Item == null) continue; 

                    GameObject newSlot = Instantiate(slotPrefab, contentPanel);
                    if (newSlot.TryGetComponent<InventorySlotUI>(out var slotUI)) 
                    {
                        slotUI.Setup(slot.Item, slot.Amount);
                    }
                }
            }
        }
        else if (currentTab == NotebookTab.Recipes)
        {
            if (pageTitleText != null) pageTitleText.text = "RECIPES";

            if (recipeManager != null && recipeSlotPrefab != null)
        {
            List<RecipeData> discoveredList = recipeManager.GetDiscoveredRecipes();
            listCount = discoveredList.Count;

            CalculatePagination(listCount, out maxPages);

            int startIndex = currentPage * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            for (int i = startIndex; i < endIndex && i < discoveredList.Count; i++)
            {
                RecipeData recipe = discoveredList[i];

                GameObject newSlot = Instantiate(recipeSlotPrefab, contentPanel);
                if (newSlot.TryGetComponent<RecipeSlotUI>(out var recipeUI)) 
                {
                    recipeUI.Setup(recipe); 
                }
            }
        }
    }   
        else if (currentTab == NotebookTab.Tasks)
        {
            if (pageTitleText != null) pageTitleText.text = "TASKS";
            listCount = 0; 
            CalculatePagination(listCount, out maxPages);
        }

        UpdatePaginationUI(maxPages);
    }

    public void NextPage()
    {
        if (nextButton != null && nextButton.activeSelf)
        {
            currentPage++;
            Refresh();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            Refresh();
        }
    }

    private void CalculatePagination(int totalItems, out int maxPages)
    {
        maxPages = Mathf.CeilToInt((float)totalItems / itemsPerPage);
        if (currentPage >= maxPages && maxPages > 0) currentPage = maxPages - 1;
        if (currentPage < 0) currentPage = 0;
    }

    private void UpdatePaginationUI(int maxPages)
    {
        if (pageNumberText != null)
        {
            int displayMaxPages = maxPages > 0 ? maxPages : 1; 
            pageNumberText.text = $"Page {currentPage + 1} / {displayMaxPages}";
        }

        if (prevButton != null) prevButton.SetActive(currentPage > 0); 
        if (nextButton != null) nextButton.SetActive(currentPage < maxPages - 1 && maxPages > 0); 
    }
}