using UnityEngine;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private InventoryDisplay inventoryDisplay;

    [Header("Positions")]
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private Vector2 openPosition;

    [Header("Settings")]
    [SerializeField] private float animationTime = 0.25f;

    [Header("Player")]
    [SerializeField] private MonoBehaviour playerMovement; // your movement script

    private bool isOpen;
    private Coroutine moveRoutine;

    private readonly KeyCode toggleKey = KeyCode.Tab;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        if (isOpen && Input.anyKeyDown)
        {
            if (!Input.GetKeyDown(toggleKey))
            {
                CloseInventory();
            }
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;

        inventoryDisplay.Refresh();
        playerMovement.enabled = !isOpen;

        Vector2 target = isOpen ? openPosition : closedPosition;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MovePanel(target));
    }

    private void CloseInventory()
    {
        isOpen = false;

        playerMovement.enabled = true;

        Vector2 target = closedPosition;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MovePanel(target));
    }

    private IEnumerator MovePanel(Vector2 target)
    {
        Vector2 start = inventoryPanel.anchoredPosition;
        float t = 0f;

        while (t < animationTime)
        {
            t += Time.deltaTime;

            inventoryPanel.anchoredPosition =
                Vector2.Lerp(start, target, t / animationTime);

            yield return null;
        }

        inventoryPanel.anchoredPosition = target;
        moveRoutine = null;
    }
}