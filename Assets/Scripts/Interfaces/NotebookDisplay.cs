using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class NotebookDisplay : MonoBehaviour
{
    [System.Serializable]
    public class NotebookTabEntry
    {
        public string title;
        public TabHoverEffect hoverEffect;
        [Tooltip("Must implement INotebookTabContent. Leave empty for tabs with no list content (e.g. Tasks).")]
        public MonoBehaviour contentProvider;

        public INotebookTabContent Content => contentProvider as INotebookTabContent;
    }

    [Header("Tabs")]
    [SerializeField] private List<NotebookTabEntry> tabs = new();
    public int CurrentTabIndex { get; private set; }
    public int TabCount => tabs.Count;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI pageTitleText;
    [SerializeField] private Transform contentPanel;

    [Header("Pagination Settings")]
    [SerializeField] private int itemsPerPage = 8;
    [SerializeField] private TextMeshProUGUI pageNumberText;

    [Header("Pagination Buttons")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject prevButton;

    private int currentPage = 0;
    private int _lastActiveTabIndex;

    private void OnEnable()
    {
        // Preserve the last active tab instead of forcing reset to index 0.
        if (CurrentTabIndex == 0 && _lastActiveTabIndex != 0)
            CurrentTabIndex = _lastActiveTabIndex;
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

    public void SetTabInstant(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Count) return;

        _lastActiveTabIndex = CurrentTabIndex;
        CurrentTabIndex = tabIndex;
        currentPage = 0;
        UpdateTabVisuals();
        UpdateDisplay();
    }

    private void UpdateTabVisuals()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].hoverEffect?.SetSelected(i == CurrentTabIndex);
        }
    }

    private void UpdateDisplay()
    {
        if (contentPanel == null || tabs.Count == 0 || CurrentTabIndex >= tabs.Count) return;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        var tab = tabs[CurrentTabIndex];

        if (pageTitleText != null)
            pageTitleText.text = tab.title;

        int maxPages;
        var content = tab.Content;

        if (content != null)
        {
            content.Refresh();
            int listCount = content.ItemCount;

            CalculatePagination(listCount, out maxPages);

            int startIndex = currentPage * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            for (int i = startIndex; i < endIndex && i < listCount; i++)
            {
                GameObject newSlot = Instantiate(content.SlotPrefab, contentPanel);
                if (!content.PopulateSlot(i, newSlot))
                {
                    Destroy(newSlot);
                }
            }
        }
        else
        {
            CalculatePagination(0, out maxPages);
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
