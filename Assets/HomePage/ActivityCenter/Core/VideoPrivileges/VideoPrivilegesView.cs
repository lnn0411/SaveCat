using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ActivityCenter.VideoPrivileges
{
    public class VideoPrivilegesView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI timerCounter;
        public Button closeButton;
        public Button rewardButton;
        public TextMeshProUGUI rewardButtonText; // 按钮上的文字，方便在看满后修改显示

        [Header("Prize Images (Assign RewardImage 1 to 4)")]
        public Image[] rewardImages; // 请在Inspector中按顺序拖入4个RewardImage

        [Header("Sprite Assets")]
        public Sprite completedSprite;  // 已领取的图片
        public Sprite incompleteSprite; // 未领取的图片

        // 交互事件
        public Action OnCloseClicked;
        public Action OnRewardClicked;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

            if (rewardButton != null)
                rewardButton.onClick.AddListener(() => OnRewardClicked?.Invoke());
        }

        // 刷新奖励状态 UI
        public void RefreshUI(VideoPrivilegesData data)
        {
            for (int i = 0; i < rewardImages.Length; i++)
            {
                // 如果当前索引小于已领取的阶段数，显示已完成图片
                if (i < data.CurrentRewardStage)
                {
                    rewardImages[i].sprite = completedSprite;
                }
                else
                {
                    rewardImages[i].sprite = incompleteSprite;
                }
            }

            // 更新按钮状态
            if (data.IsAllRewardsClaimed())
            {
                rewardButton.interactable = false;
                if (rewardButtonText != null) rewardButtonText.text = "今日已达上限";
            }
            else
            {
                rewardButton.interactable = true;
                if (rewardButtonText != null) rewardButtonText.text = "看视频领奖励";
            }
        }

        // 独立提供给 Controller 频繁刷新的倒计时接口
        public void UpdateTimerText(string timeString)
        {
            if (timerCounter != null)
            {
                timerCounter.text = timeString;
            }
        }
    }
}