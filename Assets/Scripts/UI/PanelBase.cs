using UnityEngine;

public abstract class PanelBase : MonoBehaviour, IPanel
{
    [SerializeField] protected PlayerMovement playerMovement;

    protected bool isOpen;
    public bool IsOpen => isOpen;

    public void Close()
    {
        if (isOpen) SetOpen(false);
    }

    protected virtual void SetOpen(bool shouldBeOpen)
    {
        if (isOpen == shouldBeOpen) return;

        isOpen = shouldBeOpen;

        if (isOpen)
            PanelManager.Instance?.NotifyOpened(this);
        else
            PanelManager.Instance?.NotifyClosed(this);

        UpdatePlayerMovement();
    }

    private void UpdatePlayerMovement()
    {
        if (playerMovement == null) return;
        bool anyOpen = PanelManager.Instance != null && PanelManager.Instance.AnyPanelOpen;
        playerMovement.SetMovementEnabled(!anyOpen);
    }
}
