using UnityEngine;

[System.Serializable]
public class Ingredient
{
    public ItemData item;
    [Min(1)]
    public int amount = 1;
}
