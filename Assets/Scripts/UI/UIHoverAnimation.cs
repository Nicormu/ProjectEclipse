using UnityEngine;

public class UIHoverAnimation : MonoBehaviour
{
    [SerializeField] private float hoverSpeed = 3f;
    [SerializeField] private float hoverAmount = 0.15f;
    
    private Vector3 startPos;

    private void Start()
    {
        // Remember where the UI started so we can bounce relative to this point
        startPos = transform.localPosition;
    }

    private void Update()
    {
        // Use a Sine wave to create a smooth, repeating up-and-down motion
        float newY = startPos.y + (Mathf.Sin(Time.time * hoverSpeed) * hoverAmount);
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}
