using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class NotebookController : PanelBase
{
    public const int InventoryTabIndex = 0;

    public static bool IsNotebookOpen { get; private set; }
    public static UnityEvent onNotebookOpened = new();
    public static UnityEvent onNotebookClosed = new();

    [Header("UI")]
    [SerializeField] private RectTransform notebookPanel;
    [SerializeField] private NotebookDisplay notebookDisplay;

    [Header("Tab Buttons")]
    [Tooltip("Order must match the tab list on NotebookDisplay.")]
    [SerializeField] private List<UnityEngine.UI.Button> tabButtons = new();

    [Header("Positions")]
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private Vector2 openPosition;

    [Header("Settings")]
    [SerializeField] private float animationTime = 0.25f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private Coroutine moveRoutine;

    private void Start()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int tabIndex = i; // capture for closure
            if (tabButtons[i] != null)
                tabButtons[i].onClick.AddListener(() => OnTabButtonClicked(tabIndex));
        }
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

    private void OnTabButtonClicked(int tabIndex)
    {
        if (!isOpen)
        {
            notebookDisplay.SetTabInstant(tabIndex);
            SetOpen(true);
        }
        else if (notebookDisplay.CurrentTabIndex == tabIndex)
        {
            SetOpen(false);
        }
        else
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(AnimateTabChange(tabIndex));
        }
    }

    public void OpenToTab(int tabIndex)
    {
        if (!isOpen)
        {
            notebookDisplay.SetTabInstant(tabIndex);
            SetOpen(true);
        }
        else if (notebookDisplay.CurrentTabIndex != tabIndex)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(AnimateTabChange(tabIndex));
        }
    }

    private IEnumerator AnimateTabChange(int newTabIndex)
    {
        yield return StartCoroutine(MovePanelRoutine(closedPosition));

        notebookDisplay.SetTabInstant(newTabIndex);

        yield return StartCoroutine(MovePanelRoutine(openPosition));

        moveRoutine = null;
    }

    protected override void SetOpen(bool shouldBeOpen)
    {
        if (isOpen == shouldBeOpen) return;

        base.SetOpen(shouldBeOpen);
        IsNotebookOpen = isOpen;

        if (isOpen)
        {
            onNotebookOpened?.Invoke();
            notebookDisplay.Refresh();
        }
        else
        {
            onNotebookClosed?.Invoke();
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
