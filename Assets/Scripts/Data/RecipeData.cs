using UnityEngine;

[CreateAssetMenu(
    fileName = "New Recipe",
    menuName = "Crafting/Recipe"
)]
public class RecipeData : ScriptableObject
{
    [Header("Ingredients")]
    public Ingredient[] ingredients;

    [Header("Result")]
    public ItemData result;
    [Min(1)]
    public int resultAmount = 1;
}