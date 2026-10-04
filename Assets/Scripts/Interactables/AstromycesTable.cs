using UnityEngine;

public class AstromycesTable : MonoBehaviour, InteractableUI
{
    [System.Serializable]
    public class TableLevel
    {
        [Tooltip("Costo para llegar a este nivel. Se ignora en el nivel 0.")]
        [Min(1)] public int upgradeCost = 50;
        [Min(1)] public int amountPerDay = 3;
        [Min(1)] public int maxStored = 10;
    }

    [SerializeField] private ItemData astromyces;
    [Tooltip("Astromyces que ya hay en la mesa al empezar la partida (dia 1).")]
    [SerializeField, Min(0)] private int startingStored = 3;
    [Tooltip("El nivel 0 es el inicial. Cada nivel siguiente es una mejora.")]
    [SerializeField] private TableLevel[] levels = new TableLevel[1];

    private int levelIndex;
    private int stored;
    private DayManager subscribedDays;

    private bool HasLevels => levels != null && levels.Length > 0;
    public int Level => levelIndex;
    public bool IsMaxLevel => !HasLevels || levelIndex >= levels.Length - 1;
    public int NextUpgradeCost => IsMaxLevel ? 0 : levels[levelIndex + 1].upgradeCost;

    private void Start()
    {
        if (!HasLevels)
        {
            Debug.LogError("AstromycesTable: no hay niveles configurados.", this);
            return;
        }

        stored = Mathf.Min(startingStored, levels[levelIndex].maxStored);

        subscribedDays = DayManager.Instance;
        if (subscribedDays != null)
            subscribedDays.NewDayStarted += OnNewDay;
    }

    private void OnDestroy()
    {
        if (subscribedDays != null)
            subscribedDays.NewDayStarted -= OnNewDay;
    }

    private void OnNewDay(int day)
    {
        if (!HasLevels) return;

        var current = levels[levelIndex];
        stored = Mathf.Min(stored + current.amountPerDay, current.maxStored);
    }

    public void Interact()
    {
        if (stored <= 0) return;

        if (astromyces == null || InventoryManager.Instance == null)
        {
            Debug.LogError("AstromycesTable: falta el item o el InventoryManager.", this);
            return;
        }

        InventoryManager.Instance.AddItem(astromyces, stored);
        stored = 0;
    }

    public bool TryUpgrade()
    {
        if (IsMaxLevel) return false;
        if (CurrencyManager.Instance == null) return false;
        if (!CurrencyManager.Instance.Spend(NextUpgradeCost)) return false;

        levelIndex++;
        return true;
    }

    [ContextMenu("Nuevo dia (test)")]
    private void TestNewDay() => OnNewDay(0);
}