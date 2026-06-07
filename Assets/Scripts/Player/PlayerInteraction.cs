using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;
    
    [Header("Player UI")]
    [Tooltip("Drag the 'E' Canvas completely here")]
    [SerializeField] private CanvasGroup interactPromptGroup;
    [Tooltip("How fast the prompt fades in and out")]
    [SerializeField] private float fadeSpeed = 5f; // Added a speed control

    private IInteractable currentInteractable;
    private readonly Collider2D[] hitColliders = new Collider2D[10];
    private ContactFilter2D contactFilter; 

    private void Start()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(interactableLayer);
        contactFilter.useLayerMask = true;
        
        // Ensure the UI starts completely invisible when the game begins
        if (interactPromptGroup != null)
        {
            interactPromptGroup.alpha = 0f;
        }
    }

    void Update()
    {
        DetectInteractable();
        UpdateUI();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void DetectInteractable()
    {
        int numColliders = Physics2D.OverlapCircle(transform.position, interactionRadius, contactFilter, hitColliders);
        
        currentInteractable = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < numColliders; i++)
        {
            IInteractable interactable = hitColliders[i].GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float distanceSqr = (hitColliders[i].transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                currentInteractable = interactable;
            }
        }
    }

    private void UpdateUI()
    {
        if (interactPromptGroup != null)
        {
            // 1. Determine where the alpha SHOULD be (1 if near an object, 0 if not)
            float targetAlpha = currentInteractable != null ? 1f : 0f;
            
            // 2. Smoothly transition the current alpha toward the target alpha
            interactPromptGroup.alpha = Mathf.MoveTowards(
                interactPromptGroup.alpha, 
                targetAlpha, 
                Time.deltaTime * fadeSpeed
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius); 
    }
}