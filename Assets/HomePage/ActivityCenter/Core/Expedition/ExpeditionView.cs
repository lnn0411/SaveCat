using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间
using System;

namespace ActivityCenter.Expedition
{
    public class ExpeditionView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI challengeProgressCounter;
        public TextMeshProUGUI participatingPlayersCounter;
        public Button closeButton;
        public Button startButton;

        // 供 Controller 监听的交互事件
        public Action OnCloseClicked;
        public Action OnStartClicked;

        private void Awake()
        {
            // 绑定按钮事件，抛出给 Controller 处理
            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
                
            if (startButton != null)
                startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
        }

        // 提供给 Controller 的 UI 刷新接口
        public void RefreshUI(ExpeditionData data)
        {
            if (challengeProgressCounter != null)
            {
                challengeProgressCounter.text = $"{data.CompletedLevels}/{data.TotalLevels}";
            }

            if (participatingPlayersCounter != null)
            {
                participatingPlayersCounter.text = $"{data.ActivePlayers}/{data.TotalPlayers}";
            }
        }
    }
}