using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CraftingStation : PanelBase, InteractableUI
{
    public static bool IsCraftingOpen { get; private set; }
    public static UnityEvent onCraftingOpened = new();
    public static UnityEvent onCraftingClosed = new();

    [Header("UI")]
    [SerializeField] private CanvasGroup craftingCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private NotebookController notebookController;
    [SerializeField] private CraftingBeakerController beakerController;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Recipes")]
#if UNITY_EDITOR
    [SerializeField, Tooltip("Used for quick testing when opening the crafting station.")]
    private RecipeData testRecipe;
#endif

    [SerializeField] private RecipeData[] availableRecipes;

    public RecipeData[] AvailableRecipes => availableRecipes;

    private bool isAnimating;
    private bool justOpened;

    private InventoryManager inventory;
    private RecipeManager recipeManager;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        inventory = InventoryManager.Instance;
        recipeManager = FindAnyObjectByType<RecipeManager>();

        if (notebookController == null)
            notebookController = FindAnyObjectByType<NotebookController>();

        if (beakerController == null)
            beakerController = FindAnyObjectByType<CraftingBeakerController>();

        beakerController?.SetCraftingStation(this);
    }

    private void Start()
    {
        // Awake order between scene objects is not guaranteed.
        inventory ??= InventoryManager.Instance;
        recipeManager ??= FindAnyObjectByType<RecipeManager>();

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
        if (justOpened)
        {
            justOpened = false;
            return;
        }

        if (isOpen && !isAnimating && Input.GetKeyDown(closeKey))
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

    /// <summary>
    /// Attempts a complete craft directly from inventory. This is useful for
    /// non-beaker stations and guarantees that a result is never granted unless
    /// every ingredient was consumed.
    /// </summary>
    public bool TryCraft(RecipeData recipe)
    {
        inventory ??= InventoryManager.Instance;
        if (inventory == null || !TryConsumeIngredients(recipe))
        {
            Debug.Log("Missing or invalid ingredients.");
            return false;
        }

        CompleteCraft(recipe);
        return true;
    }

    // Kept for existing UnityEvents and any existing callers.
    public void Craft(RecipeData recipe) => TryCraft(recipe);

    /// <summary>
    /// Adds the result after CraftingBeakerController has already transferred every
    /// ingredient out of inventory. Do not use this as a general crafting entry point.
    /// </summary>
    public void CompleteCraft(RecipeData recipe)
    {
        inventory ??= InventoryManager.Instance;
        if (recipe == null || recipe.result == null || inventory == null) return;

        inventory.AddItem(recipe.result, recipe.resultAmount);
        Debug.Log($"Crafted {recipe.result.itemName} x{recipe.resultAmount}");

        if (recipeManager != null)
            recipeManager.DiscoverRecipe(recipe);
        else
            Debug.LogWarning("RecipeManager not found in scene. Recipe could not be discovered.");
    }

    /// <summary>Loads a configured recipe into the beaker UI.</summary>
    public void SelectRecipe(RecipeData recipe)
    {
        if (recipe == null || beakerController == null) return;
        beakerController.LoadRecipe(recipe);
    }

    private bool TryConsumeIngredients(RecipeData recipe)
    {
        if (recipe == null || recipe.result == null || recipe.resultAmount < 1 ||
            recipe.ingredients == null || recipe.ingredients.Length == 0)
            return false;

        // Aggregate first: a malformed recipe may contain the same item twice.
        // Checking each entry independently would allow a partial deduction.
        var totals = new System.Collections.Generic.Dictionary<ItemData, int>();
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            if (ingredient == null || ingredient.item == null || ingredient.amount < 1)
                return false;

            totals[ingredient.item] = totals.TryGetValue(ingredient.item, out int amount)
                ? amount + ingredient.amount
                : ingredient.amount;
        }

        foreach (var requirement in totals)
            if (!inventory.HasItem(requirement.Key, requirement.Value))
                return false;

        foreach (var requirement in totals)
            if (!inventory.RemoveItem(requirement.Key, requirement.Value))
                return false;

        return true;
    }

    private IEnumerator FadeIn()
    {
        isAnimating = true;
        justOpened = true;

        base.SetOpen(true);
        IsCraftingOpen = true;
        onCraftingOpened?.Invoke();

        craftingCanvasGroup.gameObject.SetActive(true);

        if (notebookController != null)
        {
            notebookController.OpenToTab(NotebookController.InventoryTabIndex);
        }

        if (beakerController != null)
        {
#if UNITY_EDITOR
            if (testRecipe != null)
                SelectRecipe(testRecipe);
            else
#endif
            if (availableRecipes != null && availableRecipes.Length > 0)
                SelectRecipe(availableRecipes[0]);
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            craftingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

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

        base.SetOpen(false);
        IsCraftingOpen = false;
        onCraftingClosed?.Invoke();

        craftingCanvasGroup.interactable = false;
        craftingCanvasGroup.blocksRaycasts = false;

        if (notebookController != null)
        {
            notebookController.Close();
        }

        if (beakerController != null)
        {
            beakerController.CancelAndRestore();
            beakerController.LoadRecipe(null);
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            craftingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            yield return null;
        }

        craftingCanvasGroup.alpha = 0f;
        craftingCanvasGroup.gameObject.SetActive(false);

        isAnimating = false;
    }

#if UNITY_EDITOR
    public void CraftTestRecipe()
    {
        Craft(testRecipe);
    }
#endif
}
