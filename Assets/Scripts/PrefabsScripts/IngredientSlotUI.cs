using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class IngredientSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Display")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Beaker Visual")]
    [SerializeField] private Image beakerImage;
    [SerializeField] private Sprite emptyBeakerSprite;
    [SerializeField] private Sprite[] level1Frames;
    [SerializeField] private Sprite[] level2Frames;
    [SerializeField] private Sprite[] level3Frames;
    [SerializeField] private float beakerFrameRate = 10f;

    [Header("Feedback Hooks (wire animations here later)")]
    public UnityEvent onAccepted;
    public UnityEvent onRejected;

    public ItemData RequiredItem { get; private set; }
    public int RequiredAmount { get; private set; }
    public int FilledAmount { get; private set; }
    public bool IsFull => FilledAmount >= RequiredAmount;

    private CraftingBeakerController controller;
    private Coroutine beakerAnimation;

    public void Setup(Ingredient ingredient, CraftingBeakerController owningController)
    {
        controller = owningController;
        RequiredItem = ingredient.item;
        RequiredAmount = ingredient.amount;
        FilledAmount = 0;

        if (iconImage != null)
            iconImage.sprite = RequiredItem != null ? RequiredItem.itemIcon : null;

        RefreshDisplay();
        PlayBeakerLevel(0);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI dragged = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<InventorySlotUI>()
            : null;

        if (dragged == null || dragged.Item == null)
            return;

        if (controller == null || dragged.Item != RequiredItem || IsFull)
        {
            onRejected?.Invoke();
            return;
        }

        controller.RequestFill(this, dragged.Amount);
    }

    public void AddFilled(int amount)
    {
        FilledAmount = Mathf.Min(RequiredAmount, FilledAmount + amount);
        RefreshDisplay();
        PlayBeakerLevel(FilledAmount);
        onAccepted?.Invoke();
    }

    public void ResetSlot()
    {
        FilledAmount = 0;
        RefreshDisplay();
        PlayBeakerLevel(0);
    }

    private void RefreshDisplay()
    {
        if (amountText != null)
            amountText.text = $"{FilledAmount}/{RequiredAmount}";
    }

    private void PlayBeakerLevel(int level)
    {
        if (beakerImage == null) return;

        if (beakerAnimation != null)
        {
            StopCoroutine(beakerAnimation);
            beakerAnimation = null;
        }

        Sprite[] frames;
        if (level <= 0) frames = null;
        else if (level == 1) frames = level1Frames;
        else if (level == 2) frames = level2Frames;
        else frames = level3Frames;

        if (frames == null || frames.Length == 0)
        {
            beakerImage.sprite = emptyBeakerSprite;
            return;
        }

        beakerAnimation = StartCoroutine(LoopFrames(frames));
    }

    private IEnumerator LoopFrames(Sprite[] frames)
    {
        float delay = 1f / Mathf.Max(1f, beakerFrameRate);
        int i = 0;

        while (true)
        {
            beakerImage.sprite = frames[i];
            i = (i + 1) % frames.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    private void OnDisable()
    {
        if (beakerAnimation != null)
        {
            StopCoroutine(beakerAnimation);
            beakerAnimation = null;
        }
    }
}