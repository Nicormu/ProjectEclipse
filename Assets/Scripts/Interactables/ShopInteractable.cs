using UnityEngine;

public class ShopInteractable : MonoBehaviour, InteractableUI
{
    [SerializeField] private ShopPanel shopPanel;

    private void Awake()
    {
        if (shopPanel == null)
            shopPanel = FindAnyObjectByType<ShopPanel>();
    }

    public void Interact()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopInteractable: no ShopPanel assigned or found in the scene.", this);
            return;
        }

        shopPanel.Interact();
    }
}