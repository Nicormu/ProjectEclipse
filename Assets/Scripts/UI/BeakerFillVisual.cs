using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BeakerFillVisual : MonoBehaviour
{
    [Header("Fill Frames (index 0 = empty, last index = full)")]
    [SerializeField] private Sprite[] fillFrames;

    [Header("Animation")]
    [Tooltip("Fill units per second the displayed sprite catches up to the target (1 = empty-to-full in 1 second).")]
    [SerializeField] private float fillSpeed = 2f;

    private Image beakerImage;
    private float targetFill;
    private float displayedFill;

    private void Awake()
    {
        beakerImage = GetComponent<Image>();
        if (fillFrames == null || fillFrames.Length == 0)
        {
            Debug.LogError("BeakerFillVisual: fillFrames array is not assigned. No sprites will render.", this);
            enabled = false;
            return;
        }
        if (fillFrames[0] == null)
        {
            Debug.LogError("BeakerFillVisual: fillFrames[0] (empty beaker sprite) is not assigned.", this);
            enabled = false;
            return;
        }
        beakerImage.sprite = fillFrames[0];
        displayedFill = 0f;
        targetFill = 0f;
    }

    private void Update()
    {
        if (Mathf.Approximately(displayedFill, targetFill)) return;

        displayedFill = Mathf.MoveTowards(displayedFill, targetFill, fillSpeed * Time.deltaTime);
        ApplyFrame(displayedFill);
    }

    /// <param name="normalizedFill">0 = empty, 1 = full.</param>
    /// <param name="instant">Skip the animation and snap straight to this fill level.</param>
    public void SetFill(float normalizedFill, bool instant = false)
    {
        targetFill = Mathf.Clamp01(normalizedFill);

        if (instant)
        {
            displayedFill = targetFill;
            ApplyFrame(displayedFill);
        }
    }

    private void ApplyFrame(float t)
    {
        if (fillFrames == null || fillFrames.Length == 0 || beakerImage == null) return;

        int index = Mathf.RoundToInt(t * (fillFrames.Length - 1));
        index = Mathf.Clamp(index, 0, fillFrames.Length - 1);
        
        // Prevent unnecessary sprite assignments to reduce GC overhead
        if (beakerImage.sprite != fillFrames[index])
            beakerImage.sprite = fillFrames[index];
    }
}
