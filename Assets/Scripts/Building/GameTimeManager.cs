using System;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    private DateTime currentGameTime;
    public DateTime CurrentTime => currentGameTime;

    public event System.Action<DateTime> OnTimeChanged;

    [SerializeField] private float secondsPerGameDay = 60f; // 1 real minute = 1 game day (for testing)
    public float SecondsPerGameDay => secondsPerGameDay;
    private float dayTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Initialize to a reasonable start date
        currentGameTime = new DateTime(2024, 1, 1, 6, 0, 0); // 6 AM on Jan 1
    }

    private void Update()
    {
        dayTimer += Time.deltaTime;

        if (dayTimer >= secondsPerGameDay)
        {
            dayTimer -= secondsPerGameDay;
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        currentGameTime = currentGameTime.AddDays(1);
        OnTimeChanged?.Invoke(currentGameTime);
    }

    public void SetTime(DateTime newTime)
    {
        currentGameTime = newTime;
        OnTimeChanged?.Invoke(currentGameTime);
    }

    public void AdvanceDays(int days)
    {
        currentGameTime = currentGameTime.AddDays(days);
        OnTimeChanged?.Invoke(currentGameTime);
    }

    public string GetTimeString()
        => currentGameTime.ToString("yyyy-MM-dd HH:mm");
}
