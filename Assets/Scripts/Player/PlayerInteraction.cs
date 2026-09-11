using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Player UI")]
    [Tooltip("Drag the 'E' Canvas completely here")]
    [SerializeField] private CanvasGroup interactPromptGroup;
    [Tooltip("How fast the prompt fades in and out")]
    [SerializeField] private float fadeSpeed = 5f;

    private InteractableUI currentInteractable;
    // Growable array to handle scenes with many interactables within range.
    // Starts small and grows as needed instead of silently dropping excess colliders.
    private Collider2D[] _hitColliders;
    // Pre-computed layer bits that match the interactable layer mask (modern replacement for ContactFilter2D).
    private readonly HashSet<int> _interactableLayerBits = new();

    private bool _anyPanelOpen;

    private void Start()
    {
        // Initial buffer sized to a reasonable default; grows on overflow.
        _hitColliders = new Collider2D[32];

        // Pre-compute which layer bits match the interactable layer mask.
        for (int i = 0; i < 32; i++)
        {
            if ((interactableLayer.value & (1 << i)) != 0)
                _interactableLayerBits.Add(i);
        }

        if (interactPromptGroup != null)
        {
            interactPromptGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        PanelManager.OnOpenPanelCountChanged += HandlePanelCountChanged;
    }

    private void OnDisable()
    {
        PanelManager.OnOpenPanelCountChanged -= HandlePanelCountChanged;
    }

    private void HandlePanelCountChanged(int openCount)
    {
        _anyPanelOpen = openCount > 0;
    }

    private void Update()
    {
        if (_anyPanelOpen)
        {
            currentInteractable = null;
            UpdateUI();
            return;
        }

        DetectInteractable();
        UpdateUI();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact();
        }
    }

    private void DetectInteractable()
    {
        // Use OverlapCircleAll (modern API) and filter by interactable layer manually.
        Collider2D[] allHits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);

        // Grow the buffer if we ran out of space in a previous frame; retry so no interactables are silently dropped.
        int filteredCount = 0;
        for (int i = 0; i < allHits.Length; i++)
        {
            if (_interactableLayerBits.Contains(allHits[i].gameObject.layer))
            {
                if (filteredCount < _hitColliders.Length)
                    _hitColliders[filteredCount] = allHits[i];
                filteredCount++;
            }
        }

        // Grow buffer and retry if some valid colliders were dropped.
        while (filteredCount > _hitColliders.Length)
        {
            int newCapacity = Mathf.Max(_hitColliders.Length * 2, 64);
            _hitColliders = new Collider2D[newCapacity];
            filteredCount = 0;
            for (int i = 0; i < allHits.Length; i++)
            {
                if (_interactableLayerBits.Contains(allHits[i].gameObject.layer))
                {
                    if (filteredCount < _hitColliders.Length)
                        _hitColliders[filteredCount] = allHits[i];
                    filteredCount++;
                }
            }
        }

        currentInteractable = null;
        float closestDistanceSqr = float.MaxValue;
        var processedInteractables = new HashSet<InteractableUI>();

        for (int i = 0; i < filteredCount; i++)
        {
            InteractableUI interactable = _hitColliders[i].GetComponentInParent<InteractableUI>();

            if (interactable == null || !processedInteractables.Add(interactable))
            {
                continue;
            }

            float distanceSqr = (_hitColliders[i].transform.position - transform.position).sqrMagnitude;
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
            float targetAlpha = currentInteractable != null ? 1f : 0f;

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
