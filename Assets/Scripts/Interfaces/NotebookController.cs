using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class NotebookController : MonoBehaviour
{
    /// <summary>Whether the notebook overlay is currently open. Polling this property is discouraged; use onNotebookOpened/onNotebookClosed events instead.</summary>
    public static bool IsNotebookOpen { get; private set; }

    public static UnityEvent onNotebookOpened = new();
    public static UnityEvent onNotebookClosed = new();

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

    [Header("Overlay")]
    [SerializeField] private OverlayCloseTrigger overlayClose;

    private bool isOpen;
    private bool isCraftingOpen; // tracks CraftingStation state via events
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

    private void OnEnable()
    {
        CraftingStation.onCraftingOpened.AddListener(OnCraftingOpened);
        CraftingStation.onCraftingClosed.AddListener(OnCraftingClosed);
    }

    private void OnDisable()
    {
        CraftingStation.onCraftingOpened.RemoveListener(OnCraftingOpened);
        CraftingStation.onCraftingClosed.RemoveListener(OnCraftingClosed);
    }

    private void OnCraftingOpened() => isCraftingOpen = true;
    private void OnCraftingClosed() => isCraftingOpen = false;

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

    public void OpenToTab(NotebookTab tab)
    {
        if (!isOpen)
        {
            notebookDisplay.SetTabInstant(tab);
            SetOpen(true);
        }
        else if (notebookDisplay.currentTab != tab)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(AnimateTabChange(tab));
        }
    }

    public void CloseIfOpen()
    {
        if (isOpen)
        {
            SetOpen(false);
        }
    }

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
            onNotebookOpened?.Invoke();
            overlayClose?.PanelOpened(); // count before branch — safe when nothing is actually open
            notebookDisplay.Refresh();
        }
        else
        {
            onNotebookClosed?.Invoke();
            overlayClose?.PanelClosed();
        }

        if (playerMovement != null)
        {
            bool canPlayerMove = !isOpen && !isCraftingOpen;
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