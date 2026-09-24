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

    private IEnumerator FadeIn()
{
        isAnimating = true;
        justOpened = true;

        base.SetOpen(true);
        IsCraftingOpen = true;

        craftingCanvasGroup.gameObject.SetActive(true);

        if (notebookController != null)
        {
            notebookController.OpenToTab(NotebookController.InventoryTabIndex);
        }

        onCraftingOpened?.Invoke();


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
}