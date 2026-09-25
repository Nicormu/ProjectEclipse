using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BeakerFillVisual : MonoBehaviour
{
    [System.Serializable]
    public class FillLevel
    {
        public Sprite[] frames;
    }

    [Header("Fill Levels (index 0 = empty, last index = full)")]
    [SerializeField] private FillLevel[] levels;

    [Header("Ready State")]
    [Tooltip("Frames shown when the beaker is full and the recipe can be crafted.")]
    [SerializeField] private Sprite[] readyFrames;

    [Header("Animation")]
    [Tooltip("Fill units per second the displayed level catches up to the target (1 = empty-to-full in 1 second).")]
    [SerializeField] private float fillSpeed = 2f;
    [Tooltip("Frames per second of the loop inside each level.")]
    [SerializeField] private float framesPerSecond = 8f;

    private Image beakerImage;
    private float targetFill;
    private float displayedFill;
    private bool isReady;

    private void Awake()
    {
        beakerImage = GetComponent<Image>();
        if (levels == null || levels.Length == 0 || levels[0].frames == null || levels[0].frames.Length == 0)
        {
            Debug.LogError("BeakerFillVisual: levels[0] (empty beaker) has no frames assigned.", this);
            enabled = false;
            return;
        }
        beakerImage.sprite = levels[0].frames[0];
        displayedFill = 0f;
        targetFill = 0f;
    }

    private void Update()
    {
        ApplyFrame(targetFill);
    }

    /// <param name="normalizedFill">0 = empty, 1 = full.</param>
    public void SetFill(float normalizedFill, bool instant = false)
    {
        targetFill = Mathf.Clamp01(normalizedFill);
        ApplyFrame(targetFill);
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
    }

    private int GetLevelIndex(float t)
    {
        int last = levels.Length - 1;

        if (t <= 0f) return 0;
        if (t >= 0.999f) return last;
        if (last < 2) return 0; // only empty and full levels exist

        int middle = last - 1; // number of intermediate levels
        return Mathf.Clamp(Mathf.CeilToInt(t * middle - 0.0001f), 1, middle);
    }

    private void ApplyFrame(float t)
    {
        if (levels == null || levels.Length == 0 || beakerImage == null) return;

        int level = GetLevelIndex(t);
        Sprite[] frames = levels[level].frames;

        if (isReady && readyFrames != null && readyFrames.Length > 0 && Mathf.Approximately(t, 1f))
            frames = readyFrames;

        if (frames == null || frames.Length == 0) return;

        int frame = (int)(Time.time * framesPerSecond) % frames.Length;
        if (beakerImage.sprite != frames[frame])
            beakerImage.sprite = frames[frame];
    }
}