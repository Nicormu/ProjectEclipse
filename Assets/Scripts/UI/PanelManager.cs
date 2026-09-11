using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }
    public static event System.Action<int> OnOpenPanelCountChanged;

    private readonly List<IPanel> _openPanels = new();

    public bool AnyPanelOpen => _openPanels.Count > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void NotifyOpened(IPanel panel)
    {
        if (_openPanels.Contains(panel)) return;
        _openPanels.Add(panel);
        OnOpenPanelCountChanged?.Invoke(_openPanels.Count);
    }

    public void NotifyClosed(IPanel panel)
    {
        if (!_openPanels.Remove(panel)) return;
        OnOpenPanelCountChanged?.Invoke(_openPanels.Count);
    }

    public void CloseAll()
    {
        foreach (var panel in _openPanels.ToArray())
        {
            panel.Close();
        }
    }
}
