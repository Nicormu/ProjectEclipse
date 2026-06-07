using UnityEngine;
using System.Collections;

public class CraftingStation : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private CanvasGroup craftingCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Recipes")]
    [SerializeField] private RecipeData[] availableRecipes;

    public RecipeData[] AvailableRecipes => availableRecipes;

    private bool isOpen;
    private bool isAnimating;

    private PlayerMovement playerMovement;
    private InventoryManager inventory;

    private void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>(); // This is okay for now, but could also be a singleton.
        inventory = InventoryManager.Instance;
    }

    private void Start()
    {
        if (craftingCanvasGroup == null)
        {
            Debug.LogError("Crafting Canvas Group is not assigned in the inspector!", this);
            return;
        }

        craftingCanvasGroup.alpha = 0f;
        craftingCanvasGroup.interactable = false;
        craftingCanvasGroup.blocksRaycasts = false;
        craftingCanvasGroup.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && !isAnimating && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(FadeOut());
        }
    }

    public void Interact()
    {
        if (isAnimating)
            return;

        if (isOpen)
            StartCoroutine(FadeOut());
        else
            StartCoroutine(FadeIn());
    }

    public void Craft(RecipeData recipe)
    {
        if (inventory == null)
        {
            Debug.LogError("InventoryManager not found.");
            return;
        }

        if (!CanCraft(recipe))
        {
            Debug.Log("Missing ingredients.");
            return;
        }

        ConsumeIngredients(recipe);

        inventory.AddItem(
            recipe.result,
            recipe.resultAmount
        );

        Debug.Log($"Crafted {recipe.result.itemName} x{recipe.resultAmount}");
    }

    private bool CanCraft(RecipeData recipe)
    {
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            if (!inventory.HasItem(
                    ingredient.item,
                    ingredient.amount))
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeIngredients(RecipeData recipe)
    {
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(
                ingredient.item,
                ingredient.amount);
        }
    }

    private IEnumerator FadeIn()
    {
        isAnimating = true;
        isOpen = true;

        craftingCanvasGroup.gameObject.SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            craftingCanvasGroup.alpha =
                Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

            yield return null;
        }

        craftingCanvasGroup.alpha = 1f;
        craftingCanvasGroup.interactable = true;
        craftingCanvasGroup.blocksRaycasts = true;

        isAnimating = false;
    }

    private IEnumerator FadeOut()
    {
        isAnimating = true;

        craftingCanvasGroup.interactable = false;
        craftingCanvasGroup.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            craftingCanvasGroup.alpha =
                Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            yield return null;
        }

        craftingCanvasGroup.alpha = 0f;
        craftingCanvasGroup.gameObject.SetActive(false);

        isOpen = false;

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
        }

        isAnimating = false;
    }
}