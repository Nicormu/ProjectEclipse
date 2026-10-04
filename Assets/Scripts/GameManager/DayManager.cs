using UnityEngine;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }
    public event Action<int> NewDayStarted;
    public event Action TimeChanged;

    [Header("Time")]
    [SerializeField] private float startHour = 8f;
    [SerializeField] private float closingHour = 20f;
    [Tooltip("Minutos del juego que pasan por cada minuto real.")]
    [SerializeField] private float inGameMinutesPerRealMinute = 20f;

    private float _minutes;
    private int _lastWholeMinute;

    public int Day { get; private set; } = 1;
    public int Hour => (int)(_minutes / 60f);
    public int Minute => (int)_minutes % 60;
    public bool IsClosed => _minutes >= closingHour * 60f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _minutes = startHour * 60f;
        _lastWholeMinute = (int)_minutes;
    }

    private void Update()
    {
        if (IsClosed) return;

        _minutes += inGameMinutesPerRealMinute / 60f * Time.deltaTime;
        _minutes = Mathf.Min(_minutes, closingHour * 60f);

        if ((int)_minutes != _lastWholeMinute)
        {
            _lastWholeMinute = (int)_minutes;
            TimeChanged?.Invoke();
        }
    }

    public void Sleep()
    {
        Day++;
        _minutes = startHour * 60f;
        _lastWholeMinute = (int)_minutes;

        NewDayStarted?.Invoke(Day);
        TimeChanged?.Invoke();
    }
}