using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Items/Item"
)]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite itemIcon;

    [TextArea]
    public string description;

    [Header("Design")]
    [Tooltip("Marcar en el item Astromyces (u otro ingrediente central obligatorio).")]
    public bool isCoreIngredient;

    [Header("Inventory")]
    public bool stackable = true;

    [Min(1)]
    public int maxStack = 99;

    [Header("Shop")]
    public bool canBuy = true;
    
    [Min(0)]
    public int buyPrice = 10;

    public bool canSell = true;

    [Min(0)]
    public int sellPrice = 5;
}
