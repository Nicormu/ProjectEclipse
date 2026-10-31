using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private SettingsPanel settingsPanel;

    public void Play() => SceneManager.LoadScene(gameSceneName);

    public void OpenSettings() => settingsPanel.Open();

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}