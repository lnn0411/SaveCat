using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ActivityCenter.Task
{
    public class TaskView : MonoBehaviour
    {
        [Header("UI References")]
        public Button closeButton;
        public TMP_Text starCountText;
        public TMP_Text timerNumText;
        
        // 新增：广告按钮引用
        public Button advertisingButton; 

        [Header("Prizes (0:First, 1:Second, 2:Third)")]
        public Button[] prizeButtons; 
        public Image[] prizeRequireIcons;
        public TMP_Text[] prizeRequireTexts; 

        [Header("Scroll View")]
        public Transform contentTransform;

        public Action OnCloseClicked;
        public Action<int> OnPrizeClicked;
        // 新增：广告按钮点击事件
        public Action OnAdvertisingClicked; 

        private void Awake()
        {
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            
            // 绑定广告按钮点击事件
            if (advertisingButton != null)
            {
                advertisingButton.onClick.AddListener(() => OnAdvertisingClicked?.Invoke());
            }
            
            for (int i = 0; i < prizeButtons.Length; i++)
            {
                int index = i; 
                prizeButtons[i].onClick.AddListener(() => OnPrizeClicked?.Invoke(index));
            }
        }

        public void RefreshUI(TaskData data)
        {
            starCountText.text = data.CurrentStars.ToString();

            // 新增：根据数据状态控制广告按钮显隐
            if (advertisingButton != null)
            {
                advertisingButton.gameObject.SetActive(!data.HasWatchedAd);
            }

            for (int i = 0; i < prizeButtons.Length; i++)
            {
                int state = data.PrizeStates[i];
                bool isCollected = state == 2;

                prizeRequireTexts[i].gameObject.SetActive(!isCollected);
                if (i < prizeRequireIcons.Length && prizeRequireIcons[i] != null)
                {
                    prizeRequireIcons[i].gameObject.SetActive(!isCollected);
                }

                if (!isCollected)
                {
                    prizeRequireTexts[i].text = data.PrizeRequires[i].ToString();
                }

                if (state == 0)      // 未达标
                {
                    prizeButtons[i].interactable = false;
                }
                else if (state == 1) // 可领取
                {
                    prizeButtons[i].interactable = true;
                }
                else if (state == 2) // 已领取
                {
                    prizeButtons[i].interactable = false;
                }
            }
        }

        public void UpdateTimerDisplay(string timeStr)
        {
            timerNumText.text = timeStr;
        }
    }
}