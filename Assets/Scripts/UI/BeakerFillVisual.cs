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

    [Header("Cross-fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.25f;

    private const int ReadyState = -1;

    private Image beakerImage;
    private float targetFill;
    private float displayedFill;
    private bool isReady;

    private int currentState;
    private float stateStartTime;
    private float fadeStartTime = -1f;

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

        currentState = 0;
        stateStartTime = Time.time;

        if (fadeImage != null)
            SetFadeAlpha(0f);
    }

    private void Update()
    {
        ApplyFrame(targetFill, false);
    }

    /// <param name="normalizedFill">0 = empty, 1 = full.</param>
    public void SetFill(float normalizedFill, bool instant = false)
    {
        targetFill = Mathf.Clamp01(normalizedFill);
        ApplyFrame(targetFill, instant);
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
        ApplyFrame(targetFill, false);
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

    private void ApplyFrame(float t, bool instant)
    {
        if (levels == null || levels.Length == 0 || beakerImage == null) return;

        int level = GetLevelIndex(t);
        bool showReady = isReady && readyFrames != null && readyFrames.Length > 0 && Mathf.Approximately(t, 1f);
        int newState = showReady ? ReadyState : level;

        if (newState != currentState)
        {
            if (!instant && fadeImage != null)
            {
                fadeImage.sprite = beakerImage.sprite;
                SetFadeAlpha(1f);
                fadeStartTime = Time.time;
            }

            currentState = newState;
            stateStartTime = Time.time;
        }

        if (instant)
        {
            fadeStartTime = -1f;
            SetFadeAlpha(0f);
        }

        Sprite[] frames = showReady ? readyFrames : levels[level].frames;
        if (frames == null || frames.Length == 0) return;

        int frame = (int)((Time.time - stateStartTime) * framesPerSecond) % frames.Length;
        if (beakerImage.sprite != frames[frame])
            beakerImage.sprite = frames[frame];

        UpdateFade();
    }

    private void UpdateFade()
    {
        if (fadeImage == null || fadeStartTime < 0f) return;

        float elapsed = Time.time - fadeStartTime;
        if (elapsed >= fadeDuration)
        {
            SetFadeAlpha(0f);
            fadeStartTime = -1f;
            return;
        }

        SetFadeAlpha(1f - elapsed / fadeDuration);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}