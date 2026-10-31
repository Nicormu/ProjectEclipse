using UnityEngine;

public class ShopInteractable : MonoBehaviour, InteractableUI
{
    [SerializeField] private ShopPanel shopPanel;

    [SerializeField] private string prompt = "Open shop";
    public string InteractionPrompt => prompt;

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