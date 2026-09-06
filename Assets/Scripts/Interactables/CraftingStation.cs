using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CraftingStation : MonoBehaviour, InteractableUI
{
    /// <summary>Whether the crafting overlay is currently open. Polling this property is discouraged; use onCraftingOpened/onCraftingClosed events instead.</summary>
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

    [Header("Overlay")]
    [SerializeField] private OverlayCloseTrigger overlayClose;

    private bool isOpen;
    private bool isAnimating;
    private bool justOpened;
    private bool isNotebookOpen; // tracks NotebookController state via events

    private PlayerMovement playerMovement;
    private InventoryManager inventory;
    private RecipeManager recipeManager;

    private void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>(); 
        inventory = InventoryManager.Instance;

        recipeManager = FindAnyObjectByType<RecipeManager>(); 

        if (notebookController == null)
            notebookController = FindAnyObjectByType<NotebookController>();

        if (beakerController == null)
            beakerController = FindAnyObjectByType<CraftingBeakerController>();
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

    private void OnEnable()
    {
        NotebookController.onNotebookOpened.AddListener(OnNotebookOpened);
        NotebookController.onNotebookClosed.AddListener(OnNotebookClosed);
    }

    private void OnDisable()
    {
        NotebookController.onNotebookOpened.RemoveListener(OnNotebookOpened);
        NotebookController.onNotebookClosed.RemoveListener(OnNotebookClosed);
    }

    private void OnNotebookOpened() => isNotebookOpen = true;
    private void OnNotebookClosed() => isNotebookOpen = false;

    private void Update()
    {
        if (justOpened)
        {
            justOpened = false;
            return;
        }

        // Note: closeKey defaults to Escape in the inspector; Tab is not included
        // to avoid conflicts with NotebookController's tab toggle.
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

        if (recipeManager != null)
        {
            recipeManager.DiscoverRecipe(recipe);
        }
        else
        {
            Debug.LogWarning("RecipeManager not found in scene. Recipe could not be discovered.");
        }
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
        justOpened = true;

        IsCraftingOpen = true;
        onCraftingOpened?.Invoke();
        overlayClose?.PanelOpened();

        craftingCanvasGroup.gameObject.SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }

        if (notebookController != null)
        {
            notebookController.OpenToTab(NotebookTab.Inventory);
        }

        if (beakerController != null)
        {
#if UNITY_EDITOR
            beakerController.LoadRecipe(testRecipe);
#endif
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
        overlayClose?.PanelClosed();

        IsCraftingOpen = false;
        onCraftingClosed?.Invoke();

        craftingCanvasGroup.interactable = false;
        craftingCanvasGroup.blocksRaycasts = false;

        if (notebookController != null)
        {
            notebookController.CloseIfOpen();
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

            craftingCanvasGroup.alpha =
                Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            yield return null;
        }

        craftingCanvasGroup.alpha = 0f;
        craftingCanvasGroup.gameObject.SetActive(false);

        isOpen = false;

        if (playerMovement != null && !isNotebookOpen)
        {
            playerMovement.SetMovementEnabled(true);
        }

        isAnimating = false;
    }

#if UNITY_EDITOR
    public void CraftTestRecipe()
    {
        Craft(testRecipe);
    }
#endif
}