using System;
using UnityEngine;

/// <summary>
/// SignInData 负责签到状态、签到规则和本地持久化。
/// </summary>
public class SignInData : MonoBehaviour
{
    public const int TotalDays = 7;

    [Header("SignIn State")]
    [SerializeField] private int currentDayIndex = 0;
    [SerializeField] private bool[] signedDays = new bool[TotalDays];

    private const string LastSignDateKey = "SignIn_LastDate";
    private const string CurrentDayIndexKey = "SignIn_CurrentDayIndex";

    public event Action OnDataChanged;

    public bool HasSignedToday { get; private set; }
    public int NextDayIndex => Mathf.Clamp(currentDayIndex, 0, TotalDays - 1);
    public int SignedCount { get; private set; }
    public bool[] SignedDays => signedDays;

    private string lastSignDate;
    private string TodayString => DateTime.Now.ToString("yyyyMMdd");

    private void Awake()
    {
        Load();
    }

    private void Load()
    {
        lastSignDate = PlayerPrefs.GetString(LastSignDateKey, string.Empty);
        currentDayIndex = PlayerPrefs.GetInt(CurrentDayIndexKey, 0);

        if (signedDays == null || signedDays.Length != TotalDays)
        {
            signedDays = new bool[TotalDays];
        }

        HasSignedToday = lastSignDate == TodayString;
        SignedCount = 0;

        for (int i = 0; i < TotalDays; i++)
        {
            if (signedDays[i])
            {
                SignedCount++;
            }
        }

        if (currentDayIndex > TotalDays)
        {
            currentDayIndex = TotalDays;
        }
    }

    private void Save()
    {
        PlayerPrefs.SetString(LastSignDateKey, lastSignDate);
        PlayerPrefs.SetInt(CurrentDayIndexKey, currentDayIndex);
        PlayerPrefs.Save();
    }

    public bool TrySignInToday()
    {
        if (HasSignedToday)
        {
            return false;
        }

        lastSignDate = TodayString;
        HasSignedToday = true;

        if (currentDayIndex < TotalDays)
        {
            signedDays[currentDayIndex] = true;
            SignedCount++;
        }

        currentDayIndex = Mathf.Min(currentDayIndex + 1, TotalDays);
        Save();
        OnDataChanged?.Invoke();
        return true;
    }

    public string GetDayRewardText(int dayIndex)
    {
        int rewardAmount = 50 + dayIndex * 10;
        return $"第{dayIndex + 1}天：{rewardAmount}金币";
    }

    public SignStatus GetSignStatus(int dayIndex)
    {
        if (dayIndex < 0 || dayIndex >= TotalDays)
        {
            return SignStatus.Pending;
        }

        if (signedDays[dayIndex])
        {
            return SignStatus.Signed;
        }

        if (dayIndex == NextDayIndex && !HasSignedToday)
        {
            return SignStatus.Today;
        }

        return SignStatus.Pending;
    }
}

public enum SignStatus
{
    Signed,
    Today,
    Pending
}
