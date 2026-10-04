using UnityEngine;

public class SleepConfirmPanel : PanelBase
{
    [Tooltip("Hijo con todo lo visual. NO puede ser el mismo objeto que tiene este script.")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    private void Start()
    {
        panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey))
            SetOpen(false);
    }

    public void Open() => SetOpen(true);

    public void ConfirmSleep()
    {
        SetOpen(false);

        if (DayManager.Instance != null)
            DayManager.Instance.Sleep();
    }

    protected override void SetOpen(bool shouldBeOpen)
    {
        if (isOpen == shouldBeOpen) return;

        base.SetOpen(shouldBeOpen);
        panelRoot.SetActive(isOpen);
    }
}