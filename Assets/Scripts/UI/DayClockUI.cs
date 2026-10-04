using UnityEngine;
using TMPro;

public class DayClockUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clockText;

    private DayManager days;

    private void Start()
    {
        days = DayManager.Instance;
        if (days == null)
        {
            Debug.LogError("DayClockUI: no hay DayManager en la escena.", this);
            return;
        }

        days.TimeChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (days != null)
            days.TimeChanged -= Refresh;
    }

    private void Refresh()
    {
        if (clockText == null) return;
        clockText.text = $"Dia {days.Day}  {days.Hour:00}:{days.Minute:00}";
    }
}