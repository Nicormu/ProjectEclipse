using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private CurrencyManager currency;

    private void Start()
    {
        currency = CurrencyManager.Instance;
        if (currency == null)
        {
            Debug.LogError("MoneyUI: no hay CurrencyManager en la escena.", this);
            return;
        }

        currency.MoneyChanged += Refresh;
        Refresh(currency.Money);
    }

    private void OnDestroy()
    {
        if (currency != null)
            currency.MoneyChanged -= Refresh;
    }

    private void Refresh(int money)
    {
        if (moneyText != null)
            moneyText.text = $"${money}";
    }
}