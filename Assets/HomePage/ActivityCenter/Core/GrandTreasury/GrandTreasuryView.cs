using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ActivityCenter.GrandTreasury
{
    public class GrandTreasuryView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI timerCounter; // 倒计时文本
        public TextMeshProUGUI progressText; // 进度条文本 Text (TMP)
        public Button rewardButton;          // 领奖按钮
        public Button closeButton;           // 关闭按钮

        // 抛出给 Controller 处理的事件
        public Action OnRewardClicked;
        public Action OnCloseClicked;

        private void Awake()
        {
            if (rewardButton != null)
                rewardButton.onClick.AddListener(() => OnRewardClicked?.Invoke());

            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        // 刷新静态 UI 状态
        public void RefreshUI(GrandTreasuryData data)
        {
            if (progressText != null)
            {
                progressText.text = $"{data.CurrentCoins}/{data.MaxCoins}";
            }

            // 表现层逻辑：如果已经领取过了，可以把按钮置灰或隐藏
            if (data.IsRewardClaimed)
            {
                rewardButton.interactable = false;
                // TODO: 可以进一步把按钮上的文字改成“已领取”
            }
        }

        // 独立暴露给 Controller 调用的倒计时刷新接口
        public void UpdateTimerText(string timeString)
        {
            if (timerCounter != null)
            {
                timerCounter.text = timeString;
            }
        }
    }
}