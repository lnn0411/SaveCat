using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace ActivityCenter.Turntable
{
    public class TurntableView : MonoBehaviour
    {
        [Header("Turntable")]
        public RectTransform turntableRect; // 旋转的转盘本体

        [Header("UI Elements")]
        public TMP_Text scheduleText;
        public Button drawButton;
        public Image drawButtonImage; // 用于改变按钮颜色
        public TMP_Text drawButtonText;
        
        public Button adButton;
        public TMP_Text adButtonText;
        
        public Button closeButton;

        // 供Controller监听的事件
        public Action OnDrawClicked;
        public Action OnAdDrawClicked;
        public Action OnCloseClicked;

        private void Awake()
        {
            drawButton.onClick.AddListener(() => OnDrawClicked?.Invoke());
            adButton.onClick.AddListener(() => OnAdDrawClicked?.Invoke());
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        // 刷新界面状态
        public void RefreshUI(TurntableData data, bool isSpinning)
        {
            // 如果正在旋转，禁用所有按钮防止重复点击
            drawButton.interactable = !isSpinning;
            adButton.interactable = !isSpinning && data.CanAdDraw;
            closeButton.interactable = !isSpinning;

            // 进度文本
            if (data.CanFreeDraw)
            {
                scheduleText.text = "您可以免费抽奖啦!";
            }
            else
            {
                scheduleText.text = $"再通过 {data.LevelsUntilNextFreeDraw} 关即可获得免费抽奖!";
            }

            // 抽奖按钮状态改变
            if (data.CanFreeDraw)
            {
                drawButtonImage.color = Color.green;
                drawButtonText.text = "Free";
            }
            else
            {
                drawButtonImage.color = new Color(0.2f, 0.6f, 1f); // 蓝色
                drawButtonText.text = "600";
            }

            // 广告按钮文本
            adButtonText.text = $"看广告抽奖 ({TurntableData.MaxAdDrawsPerDay - data.AdDrawsUsedToday}/{TurntableData.MaxAdDrawsPerDay})";
        }

        // 供Controller每帧调用以旋转UI
        public void SetTurntableRotation(float zAngle)
        {
            if (turntableRect != null)
            {
                turntableRect.localRotation = Quaternion.Euler(0, 0, zAngle);
            }
        }
    }
}