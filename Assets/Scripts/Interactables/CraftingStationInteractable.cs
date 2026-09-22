using UnityEngine;

public class CraftingStationInteractable : MonoBehaviour, InteractableUI
{
    [SerializeField] private CraftingStation craftingStation;

    private void Awake()
    {
        if (craftingStation == null)
            craftingStation = FindAnyObjectByType<CraftingStation>();
    }

    public void Interact()
    {
        if (craftingStation == null)
        {
            Debug.LogError("CraftingStationInteractable: no CraftingStation assigned or found in the scene.", this);
            return;
        }

        craftingStation.Interact();
    }
}