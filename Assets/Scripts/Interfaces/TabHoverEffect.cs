using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class TabHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movement")]
    [SerializeField] private Vector2 selectedOffset = new(15f, 0f);
    [SerializeField] private Vector2 hoverOffset = new(40f, 0f);
    [SerializeField] private float slideSpeed = 500f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector2 targetPosition;

    private bool isHovering;
    private bool isSelected;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        targetPosition = originalPosition;
    }

    private void Update()
    {
        if (isHovering)
        {
            targetPosition = originalPosition + hoverOffset;
        }
        else if (isSelected)
        {
            targetPosition = originalPosition + selectedOffset;
        }
        else
        {
            targetPosition = originalPosition;
        }

        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            targetPosition,
            slideSpeed * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}