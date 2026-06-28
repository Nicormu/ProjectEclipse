using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    // Public method to allow UI elements (like Text or Buttons) to trigger drag via event triggers
    public void StartDrag()
    {
        if (!isDragging && Item != null && rootCanvas != null && CraftingStation.IsCraftingOpen)
        {
            BeginDragInternal();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        if (!isDragging && Item != null && rootCanvas != null && CraftingStation.IsCraftingOpen)
        {
            BeginDragInternal();
        }
    }

    private void BeginDragInternal()
    {
        isDragging = true;
        canvasGroup.alpha = 0.5f;
        
        // Delay blocking raycasts slightly to ensure the drag icon spawns correctly
        StartCoroutine(DelayRaycastBlock());
        
        if (iconImage != null && iconImage.sprite != null)
        {
            GameObject ghost = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghost.transform.SetParent(rootCanvas.transform, false);
            ghost.transform.SetAsLastSibling();

            Image ghostImage = ghost.GetComponent<Image>();
            ghostImage.sprite = iconImage.sprite;
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

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        dragIcon.anchoredPosition = localPoint;
    }
}
