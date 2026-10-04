using UnityEngine;

public class Bed : MonoBehaviour, InteractableUI
{
    [SerializeField] private SleepConfirmPanel confirmPanel;

    private void Awake()
    {
        if (confirmPanel == null)
            confirmPanel = FindAnyObjectByType<SleepConfirmPanel>();
    }

    public void Interact()
    {
        if (confirmPanel == null)
        {
            Debug.LogError("Bed: no hay SleepConfirmPanel asignado ni en la escena.", this);
            return;
        }

        confirmPanel.Open();
    }
}