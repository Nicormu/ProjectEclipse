using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public event Action<int> MoneyChanged;

    [SerializeField] private int startingMoney = 0;

    private int _money;
    public int Money => _money;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _money = startingMoney;
    }

    public bool Spend(int amount)
    {
        if (amount <= 0 || amount > _money) return false;

        _money -= amount;
        MoneyChanged?.Invoke(_money);
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;

        _money += amount;
        MoneyChanged?.Invoke(_money);
    }
}