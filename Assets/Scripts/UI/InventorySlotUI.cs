using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Vector2 dragIconSize = new Vector2(64, 64);

    public ItemData Item { get; private set; }
    public int Amount { get; private set; }

    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private RectTransform dragIcon;
    private bool isDragging = false;

    /// <summary>Returns true when this slot can initiate a drag (has an item, valid canvas, and crafting UI is open).</summary>
    private bool CanStartDrag => Item != null && rootCanvas != null && _craftingOpen;

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

    private bool _craftingOpen;
    private void OnCraftingOpened() => _craftingOpen = true;
    private void OnCraftingClosed() => _craftingOpen = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    public void Setup(ItemData item, int amount)
    {
        Item = item;
        Amount = amount;

        if (nameText != null) nameText.text = item?.itemName ?? string.Empty;
        if (descriptionText != null) descriptionText.text = item?.description ?? string.Empty;
        if (iconImage != null) iconImage.sprite = item?.itemIcon;
        if (amountText != null) amountText.text = $"x{amount}";
    }

    // Public method to allow UI elements (like Text or Buttons) to trigger drag via event triggers.
    // This is the ONLY manual entry point besides OnBeginDrag — OnPointerDown no longer starts drags,
    // since a plain click (no movement) fires PointerDown without ever firing OnBeginDrag/OnEndDrag,
    // which left ghosts stuck and canvasGroup.blocksRaycasts permanently false.
    // Guarded by left mouse button check to prevent right-click drags from non-pointer sources.
    public void StartDrag()
    {
        if (!isDragging && CanStartDrag && Input.GetMouseButton(0))
        {
            BeginDragInternal(null);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!isDragging && CanStartDrag)
        {
            BeginDragInternal(eventData);
        }
    }

    private void BeginDragInternal(PointerEventData eventData = null)
    {
        isDragging = true;
        canvasGroup.alpha = 0.5f;

        // Delay blocking raycasts slightly to ensure the drag icon spawns correctly
        StartCoroutine(DelayRaycastBlock());

        // Always use Item.itemIcon for the ghost — independent of whether iconImage is visible/enabled
        if (Item != null && Item.itemIcon != null)
        {
            GameObject ghost = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghost.transform.SetParent(rootCanvas.transform, false);
            ghost.transform.SetAsLastSibling();

            Image ghostImage = ghost.GetComponent<Image>();
            ghostImage.sprite = Item.itemIcon;
            ghostImage.raycastTarget = false;
            ghostImage.preserveAspect = true;

            dragIcon = ghost.GetComponent<RectTransform>();
            dragIcon.sizeDelta = dragIconSize;

            UpdateDragIconPosition(eventData);
        }
    }

    private System.Collections.IEnumerator DelayRaycastBlock()
    {
        yield return null; // Wait one frame to ensure pointer position is registered
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragInternal();
    }

    // Safety net: if a drag was started via StartDrag() (no PointerEventData) or interrupted in a way
    // that never raised OnEndDrag (focus loss, pointer released outside a draggable, etc.), this ensures
    // the ghost gets destroyed and raycasts/alpha get restored instead of leaving the slot stuck.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            EndDragInternal();
        }
    }

    private void EndDragInternal()
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (dragIcon != null)
        {
            Destroy(dragIcon.gameObject);
            dragIcon = null;
        }
    }

    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (dragIcon == null || rootCanvas == null) return;

        Vector2 localPoint;
        Camera eventCamera = rootCanvas.worldCamera;
        Vector2 screenPos;

        if (eventData != null)
        {
            eventCamera = eventData.pressEventCamera ?? eventCamera;
            screenPos = eventData.position;
        }
        else
        {
            // No PointerEventData available (e.g. called from StartDrag()).
            // Falls back to the mouse position directly — not touch-aware.
            screenPos = Input.mousePosition;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            screenPos,
            eventCamera,
            out localPoint);

        dragIcon.anchoredPosition = localPoint;
    }
}
