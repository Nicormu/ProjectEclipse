using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns an invisible full-screen overlay on the root Canvas whenever any registered
/// panel is open (tracked via PanelManager). Clicking it closes every open panel.
/// </summary>
public class OverlayCloseTrigger : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.4f);
    [SerializeField] private Canvas manualCanvas;

    private Image _overlayImage;
    private Canvas _rootCanvas;
    private bool _isActive;

    public bool IsActive => _isActive;

    private void OnEnable()
    {
        PanelManager.OnOpenPanelCountChanged += HandleOpenPanelCountChanged;
    }

    private void OnDisable()
    {
        PanelManager.OnOpenPanelCountChanged -= HandleOpenPanelCountChanged;
    }

    private void HandleOpenPanelCountChanged(int openCount)
    {
        if (openCount > 0)
            EnsureOverlay();
        else
            DisableOverlay();
    }

    private void EnsureOverlay()
    {
        if (_isActive) return;

        if (_rootCanvas == null)
        {
            _rootCanvas = manualCanvas != null ? manualCanvas : Object.FindAnyObjectByType<Canvas>();
            if (_rootCanvas == null) return;
        }

        var overlayGO = new GameObject("OverlayCloseTarget", typeof(RectTransform), typeof(Image));
        overlayGO.transform.SetParent(_rootCanvas.transform, false);
        overlayGO.layer = _rootCanvas.gameObject.layer;

        _overlayImage = overlayGO.GetComponent<Image>();
        _overlayImage.color = overlayColor;
        _overlayImage.raycastTarget = true;

        overlayGO.transform.SetAsFirstSibling();

        var rect = overlayGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

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
        PanelManager.Instance?.CloseAll();
    }
}
