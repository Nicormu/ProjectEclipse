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

    [Header("Inventory")]
    public bool stackable = true;
    [Min(1)]
    public int maxStack = 99;
}