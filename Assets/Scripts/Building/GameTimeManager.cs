using System;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    [SerializeField] private float secondsPerGameDay = 60f;
    public float SecondsPerGameDay => secondsPerGameDay;

    private DateTime currentGameTime;
    public DateTime CurrentTime => currentGameTime;

    public int    CurrentHour   => currentGameTime.Hour;
    public int    CurrentMinute => currentGameTime.Minute;
    public int    CurrentYear   => currentGameTime.Year;
    public Season CurrentSeason => SeasonFor(currentGameTime);

    // Fires whenever the game hour changes
    public event Action<DateTime> OnHourChanged;

    // Fires whenever the calendar day changes (use this for daily resets, need decay etc.)
    public event Action<DateTime> OnDayChanged;

    // Fires when the season changes
    public event Action<Season> OnSeasonChanged;

    // Fires when the year rolls over
    public event Action<int> OnYearChanged;

    private Season lastSeason;
    private int    lastHour;
    private int    lastDay;
    private int    lastYear;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        currentGameTime = new DateTime(2024, 3, 1, 7, 0, 0); // Spring, 7am
        CacheState();
    }

    private void Update()
    {
        // Advance the clock: (deltaTime / secondsPerGameDay) fraction of a day in game minutes
        float gameMinutes = (Time.deltaTime / secondsPerGameDay) * 24f * 60f;
        currentGameTime = currentGameTime.AddMinutes(gameMinutes);

        CheckBoundaries();
    }

    private void CheckBoundaries()
    {
        if (currentGameTime.Hour != lastHour)
        {
            lastHour = currentGameTime.Hour;
            OnHourChanged?.Invoke(currentGameTime);
        }

        if (currentGameTime.Day != lastDay)
        {
            lastDay = currentGameTime.Day;
            OnDayChanged?.Invoke(currentGameTime);
        }

        Season season = SeasonFor(currentGameTime);
        if (season != lastSeason)
        {
            lastSeason = season;
            OnSeasonChanged?.Invoke(season);
        }

        if (currentGameTime.Year != lastYear)
        {
            lastYear = currentGameTime.Year;
            OnYearChanged?.Invoke(currentGameTime.Year);
        }
    }

    public static Season SeasonFor(DateTime date) => date.Month switch
    {
        3 or 4 or 5   => Season.Spring,
        6 or 7 or 8   => Season.Summer,
        9 or 10 or 11 => Season.Autumn,
        _              => Season.Winter
    };

    public void SetTime(DateTime newTime)
    {
        currentGameTime = newTime;
        CacheState();
        OnHourChanged?.Invoke(currentGameTime);
        OnDayChanged?.Invoke(currentGameTime);
        OnSeasonChanged?.Invoke(CurrentSeason);
    }

    public void AdvanceDays(int days)
    {
        currentGameTime = currentGameTime.AddDays(days);
        CacheState();
        OnDayChanged?.Invoke(currentGameTime);
    }

    public string GetTimeString()
        => currentGameTime.ToString("ddd dd MMM yyyy  HH:mm");

    private void CacheState()
    {
        lastHour   = currentGameTime.Hour;
        lastDay    = currentGameTime.Day;
        lastSeason = SeasonFor(currentGameTime);
        lastYear   = currentGameTime.Year;
    }
}
