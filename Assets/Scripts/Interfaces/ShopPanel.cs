using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPanel : PanelBase
{
    public static bool IsShopOpen { get; private set; }

    [Header("Reactivos por categoría")]
    [SerializeField] private List<ItemData> basicos = new();
    [SerializeField] private List<ItemData> especializados = new();
    [SerializeField] private List<ItemData> volatiles = new();

    [Header("UI")]
    [Tooltip("Hijo con todo lo visual. NO puede ser el mismo objeto que tiene este script.")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private ShopSlotUI slotPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Botones de categoría (orden: básicos, especializados, volátiles)")]
    [SerializeField] private List<Button> categoryButtons = new();

    [Header("Referencias")]
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    private readonly List<ShopSlotUI> _slots = new();
    private CurrencyManager _currency;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (shopManager == null)
            shopManager = FindAnyObjectByType<ShopManager>();
    }

    private void Start()
    {
        for (int i = 0; i < categoryButtons.Count; i++)
        {
            int index = i; // captura para el closure
            if (categoryButtons[i] != null)
                categoryButtons[i].onClick.AddListener(() => ShowCategory(index));
        }

        panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey))
            SetOpen(false);
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrency();
    }

    public void Interact()
    {
        SetOpen(!isOpen);
    }

    protected override void SetOpen(bool shouldBeOpen)
    {
        if (isOpen == shouldBeOpen) return;

        base.SetOpen(shouldBeOpen);
        IsShopOpen = isOpen;
        panelRoot.SetActive(isOpen);

        if (isOpen)
        {
            SubscribeToCurrency();
            ShowCategory(0);
        }
        else
        {
            UnsubscribeFromCurrency();
        }
    }

    private void SubscribeToCurrency()
    {
        _currency = CurrencyManager.Instance;
        if (_currency == null)
        {
            Debug.LogWarning("ShopPanel: no hay CurrencyManager en la escena.", this);
            return;
        }

        _currency.MoneyChanged += HandleMoneyChanged;
        HandleMoneyChanged(_currency.Money);
    }

    private void UnsubscribeFromCurrency()
    {
        if (_currency == null) return;
        _currency.MoneyChanged -= HandleMoneyChanged;
        _currency = null;
    }

    private void HandleMoneyChanged(int money)
    {
        if (moneyText != null)
            moneyText.text = $"${money}";

        foreach (var slot in _slots)
            slot.SetAffordable(money >= slot.Item.buyPrice);
    }

    private List<ItemData> GetCategory(int index)
    {
        return index switch
        {
            0 => basicos,
            1 => especializados,
            2 => volatiles,
            _ => null
        };
    }

    private void ShowCategory(int index)
    {
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);
        _slots.Clear();

        var items = GetCategory(index);
        if (items == null) return;

        foreach (var item in items)
        {
            if (item == null || !item.canBuy) continue;

            var slot = Instantiate(slotPrefab, contentPanel);
            slot.Setup(item, OnBuyClicked);
            _slots.Add(slot);
        }

        if (_currency != null)
            HandleMoneyChanged(_currency.Money);
    }

    private void OnBuyClicked(ItemData item)
    {
        if (shopManager == null) return;
        shopManager.TryPurchaseItem(item);
        // No hace falta refrescar nada acá: Spend() dispara MoneyChanged.
    }
}