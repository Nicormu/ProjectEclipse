using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CloseButtonVisibility : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 5f;

    private CanvasGroup canvasGroup;
    private bool anyPanelOpen;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        anyPanelOpen = PanelManager.Instance != null && PanelManager.Instance.AnyPanelOpen;
        canvasGroup.alpha = anyPanelOpen ? 1f : 0f;

        PanelManager.OnOpenPanelCountChanged += HandleOpenPanelCountChanged;
    }

    private void OnDisable()
    {
        PanelManager.OnOpenPanelCountChanged -= HandleOpenPanelCountChanged;
    }

    private void HandleOpenPanelCountChanged(int openCount)
    {
        anyPanelOpen = openCount > 0;
    }

    private void Update()
    {
        float targetAlpha = anyPanelOpen ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        canvasGroup.interactable = anyPanelOpen;
        canvasGroup.blocksRaycasts = anyPanelOpen;
    }
}