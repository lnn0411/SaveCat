using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SignInView 负责显示签到界面和更新 UI 元素。
/// </summary>
public class SignInView : BaseView
{
    [Header("Header")]
    [SerializeField] private TMP_Text titleText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("Day Items")]
    [SerializeField] private List<DayItem> dayItems = new List<DayItem>();

    [Header("Color Settings")]
    [SerializeField] private Color signedColor = new Color(0.22f, 0.78f, 0.35f, 1f);
    [SerializeField] private Color todayColor = new Color(0.96f, 0.78f, 0.18f, 1f);
    [SerializeField] private Color pendingColor = Color.white;

    private SignInController controller;

    [Serializable]
    private class DayItem
    {
        public Button dayButton;
        public TMP_Text dayLabel;
        public TMP_Text rewardLabel;
    }

    public void BindController(SignInController controller)
    {
        this.controller = controller;
    }

    protected override void BindComponents()
    {
        if (titleText == null)
        {
            titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
        }

        if (closeButton == null)
        {
            closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
        }
    }

    protected override void InitLogic()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        // DayItem buttons will be set in Refresh
    }

    protected override void OnHide()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        // Remove DayItem listeners
        foreach (var item in dayItems)
        {
            if (item != null && item.dayButton != null)
            {
                item.dayButton.onClick.RemoveAllListeners();
            }
        }
    }

    public void Refresh(SignInData data)
    {
        if (data == null)
        {
            return;
        }

        titleText.text = "每日签到";

        for (int i = 0; i < dayItems.Count; i++)
        {
            var item = dayItems[i];
            if (item == null || item.dayButton == null)
            {
                continue;
            }

            item.dayLabel.text = $"第{i + 1}天";
            item.rewardLabel.text = data.GetDayRewardText(i);

            SignStatus status = data.GetSignStatus(i);
            switch (status)
            {
                case SignStatus.Signed:
                    item.dayButton.image.color = signedColor;
                    item.dayButton.interactable = false;
                    break;
                case SignStatus.Today:
                    item.dayButton.image.color = todayColor;
                    item.dayButton.interactable = true;
                    int dayIndex = i; // Capture for lambda
                    item.dayButton.onClick.AddListener(() => OnDayItemClicked(dayIndex));
                    break;
                default:
                    item.dayButton.image.color = pendingColor;
                    item.dayButton.interactable = false;
                    break;
            }
        }
    }

    public void ShowFeedback(string message)
    {
        // Reserved for future use, e.g., OneMoreAgain button feedback
        Debug.Log($"[SignInView] Feedback: {message}");
    }

    private void OnDayItemClicked(int dayIndex)
    {
        controller?.OnDayItemClicked(dayIndex);
    }

    private void OnCloseButtonClicked()
    {
        controller?.OnCloseButtonClicked();
    }
}
