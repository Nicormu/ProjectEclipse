using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns an invisible full-screen overlay on the root Canvas when any UI panel is open.
/// Clicking on the overlay (i.e. outside all interactive elements) closes everything.
/// </summary>
public class OverlayCloseTrigger : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.4f);

    /// <summary>Explicitly assign the root Canvas in the inspector to avoid ambiguous FindResults.</summary>
    [SerializeField] private Canvas manualCanvas;

    private Image _overlayImage;
    private Canvas _rootCanvas;
    private bool _isActive;
    private int _openPanelCount; // tracks how many panels (notebook + crafting) are open

    /// <summary>Call when a panel opens. Closes everything on the first call if count goes 0→1.</summary>
    public void PanelOpened()
    {
        _openPanelCount++;
        EnsureOverlay();
    }

    /// <summary>Call when a panel closes. Disables overlay when count reaches 0.</summary>
    public void PanelClosed()
    {
        _openPanelCount = Mathf.Max(0, _openPanelCount - 1);

        if (_openPanelCount <= 0)
        {
            DisableOverlay();
        }
    }

    /// <summary>Returns whether any panel is currently open (overlay active).</summary>
    public bool IsActive => _isActive;

    private void EnsureOverlay()
    {
        if (_isActive) return;

        if (_rootCanvas == null)
        {
            // Prefer the explicitly assigned canvas; fall back to FindObjectOfType as a last resort.
            _rootCanvas = manualCanvas ?? Object.FindAnyObjectByType<Canvas>();
            if (_rootCanvas == null) return; // no canvas in scene — fall back to manual behavior
        }

        var overlayGO = new GameObject("OverlayCloseTarget", typeof(RectTransform), typeof(Image));
        overlayGO.transform.SetParent(_rootCanvas.transform, false);
        overlayGO.layer = _rootCanvas.gameObject.layer;

        _overlayImage = overlayGO.GetComponent<Image>();
        _overlayImage.color = overlayColor;
        _overlayImage.raycastTarget = true;

        // Place behind all other UI elements by pushing it to the bottom of the render order.
        overlayGO.transform.SetAsFirstSibling();

        var rect = overlayGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Listen for clicks on the overlay — this fires when user clicks "outside" all interactive elements.
        var button = overlayGO.AddComponent<Button>();
        button.onClick.AddListener(CloseAll);

        _isActive = true;
    }

    private void DisableOverlay()
    {
        if (!_isActive) return;
        _isActive = false;

        if (_overlayImage != null && _overlayImage.gameObject != null)
        {
            Destroy(_overlayImage.gameObject);
            _overlayImage = null;
        }
    }

    private void CloseAll()
    {
        // Close the notebook first so it doesn't steal focus back.
        var notebook = Object.FindAnyObjectByType<NotebookController>();
        if (notebook != null)
        {
            notebook.CloseIfOpen();
        }

        // Then close crafting.
        var crafting = Object.FindAnyObjectByType<CraftingStation>();
        if (crafting != null && crafting.gameObject.activeInHierarchy)
        {
            crafting.Interact(); // toggles off if open
        }

        DisableOverlay();
    }
}
