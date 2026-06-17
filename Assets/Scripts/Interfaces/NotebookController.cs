using UnityEngine;
using System.Collections;

public class NotebookController : MonoBehaviour
{
    public static bool IsNotebookOpen { get; private set; } 

    [Header("UI")]
    [SerializeField] private RectTransform notebookPanel; 
    [SerializeField] private NotebookDisplay notebookDisplay;

    [Header("Tab Buttons")]
    [SerializeField] private UnityEngine.UI.Button inventoryTabButton;
    [SerializeField] private UnityEngine.UI.Button recipesTabButton;
    [SerializeField] private UnityEngine.UI.Button tasksTabButton;

    [Header("Positions")]
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private Vector2 openPosition;

    [Header("Settings")]
    [SerializeField] private float animationTime = 0.25f;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private bool isOpen;
    private Coroutine moveRoutine;

    private readonly KeyCode toggleKey = KeyCode.Tab;

    private void Start()
    {
        if (inventoryTabButton != null)
            inventoryTabButton.onClick.AddListener(() => OnTabButtonClicked(NotebookTab.Inventory));
        
        if (recipesTabButton != null)
            recipesTabButton.onClick.AddListener(() => OnTabButtonClicked(NotebookTab.Recipes));

        if (tasksTabButton != null)
            tasksTabButton.onClick.AddListener(() => OnTabButtonClicked(NotebookTab.Tasks));
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetOpen(!isOpen);
        }
        else if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
        }
    }

    private void OnTabButtonClicked(NotebookTab clickedTab)
    {
        if (!isOpen)
        {
            notebookDisplay.SetTabInstant(clickedTab);
            SetOpen(true);
        }
        else if (isOpen && notebookDisplay.currentTab == clickedTab)
        {
            SetOpen(false);
        }
        else if (isOpen && notebookDisplay.currentTab != clickedTab)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(AnimateTabChange(clickedTab));
        }
    }

    // NEW: The mechanical "Close, Swap, Open" animation
    private IEnumerator AnimateTabChange(NotebookTab newTab)
    {
        yield return StartCoroutine(MovePanelRoutine(closedPosition));

        notebookDisplay.SetTabInstant(newTab);

        yield return StartCoroutine(MovePanelRoutine(openPosition));

        moveRoutine = null;
    }

    public void SetOpen(bool shouldBeOpen)
    {
        if (isOpen == shouldBeOpen) return;

        isOpen = shouldBeOpen;
        IsNotebookOpen = isOpen; 

        if (isOpen)
        {
            notebookDisplay.Refresh();
        }

        if (playerMovement != null) 
        {
            bool canPlayerMove = !isOpen && !CraftingStation.IsCraftingOpen;
            playerMovement.SetMovementEnabled(canPlayerMove);
        }

        Vector2 target = isOpen ? openPosition : closedPosition;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MovePanelRoutine(target));
    }

    private IEnumerator MovePanelRoutine(Vector2 target)
    {
        Vector2 start = notebookPanel.anchoredPosition;
        float t = 0f;
        float rate = 1f / animationTime; 

        while (t < 1f)
        {
            t += Time.deltaTime * rate;
            notebookPanel.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        notebookPanel.anchoredPosition = target;
    }
}