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

    private void Awake()
    {
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
        Debug.Log("OnBeginDrag called on " + gameObject.name);

        if (Item == null || rootCanvas == null || !CraftingStation.IsCraftingOpen)
        {
            eventData.pointerDrag = null;
            return;
        }
        

        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;

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

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
        if (dragIcon == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        dragIcon.anchoredPosition = localPoint;
    }
}