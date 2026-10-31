using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPanel : PanelBase
{
    [Tooltip("Hijo con todo lo visual. NO puede ser el mismo objeto que tiene este script.")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GameObject backToMenuButton;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey))
            SetOpen(false);
    }

    public void Open() => SetOpen(true);

    public void BackToMenu()
    {
        PanelManager.Instance?.CloseAll();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnMusicChanged(float value) => AudioManager.Instance?.SetMusicVolume(value);

    private void OnSfxChanged(float value) => AudioManager.Instance?.SetSfxVolume(value);

    private void SyncSliders()
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;

        musicSlider.SetValueWithoutNotify(audio.MusicVolume);
        sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
    }

    protected override void SetOpen(bool shouldBeOpen)
    {
        if (isOpen == shouldBeOpen) return;

        base.SetOpen(shouldBeOpen);
        panelRoot.SetActive(isOpen);

        if (isOpen)
            SyncSliders();
        else
            PlayerPrefs.Save();

        if (backToMenuButton != null)
            backToMenuButton.SetActive(SceneManager.GetActiveScene().name != menuSceneName);
    }
}