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

    public ItemData Item { get; private set; }
    public int Amount { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private int originalSiblingIndex;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    public void Setup(ItemData item, int amount)
    {
        Item = item;
        Amount = amount;

        nameText.text = item.itemName;

        if (descriptionText != null)
            descriptionText.text = item.description;

        if (iconImage != null)
            iconImage.sprite = item.itemIcon;

        amountText.text = $"x{amount}";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Item == null || rootCanvas == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}