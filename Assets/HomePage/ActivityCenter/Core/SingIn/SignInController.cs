using System;
using UnityEngine;

/// <summary>
/// SignInController 负责连接 SignInView 与 SignInData，接收 UI 事件并驱动数据更新。
/// </summary>
public class SignInController : MonoBehaviour
{
    [Header("MVC Components")]
    [SerializeField] private SignInData signInData;
    [SerializeField] private SignInView signInView;

    private void Awake()
    {
        if (signInData == null)
        {
            signInData = GetComponent<SignInData>();
        }

        if (signInView == null)
        {
            signInView = GetComponentInChildren<SignInView>(true);
        }
    }

    private void Start()
    {
        if (signInData == null || signInView == null)
        {
            Debug.LogWarning("[SignInController] SignInData or SignInView is not assigned.");
            return;
        }

        signInData.OnDataChanged += RefreshView;
        signInView.BindController(this);
        signInView.Initialize();
        RefreshView();
    }

    private void OnDestroy()
    {
        if (signInData != null)
        {
            signInData.OnDataChanged -= RefreshView;
        }
    }

    public void OnDayItemClicked(int dayIndex)
    {
        if (dayIndex == signInData.NextDayIndex && !signInData.HasSignedToday)
        {
            if (signInData.TrySignInToday())
            {
                signInView.ShowFeedback("签到成功，明日继续加油！");
            }
            else
            {
                signInView.ShowFeedback("签到失败，请重试。");
            }
            RefreshView();
        }
        else
        {
            signInView.ShowFeedback("只能签到今天的日期！");
        }
    }

    public void OnCloseButtonClicked()
    {
        if (ActivityManager.Instance != null)
        {
            ActivityManager.Instance.CloseSignIn();
        }
        else
        {
            signInView.Hide();
        }
    }

    private void RefreshView()
    {
        signInView.Refresh(signInData);
    }
}
