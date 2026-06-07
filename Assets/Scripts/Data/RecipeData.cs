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
    public int resultAmount = 1;
}
