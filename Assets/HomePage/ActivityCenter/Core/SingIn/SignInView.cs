using UnityEngine;
using UnityEngine.UI;
using System;

namespace ActivityCenter.SignIn
{
    public class SignInView : MonoBehaviour
    {
        [Header("UI References")]
        public Button closeButton;
        public Button oneMoreAgainButton;
        public Button[] dayButtons; // 请在Inspector中按DayItem_1到7的顺序拖入
        
        // 供Controller监听的UI事件
        public Action OnCloseClicked;
        public Action<int> OnDayClicked;
        public Action OnOneMoreAgainClicked;

        private void Awake()
        {
            // 绑定基础按钮事件
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            oneMoreAgainButton.onClick.AddListener(() => OnOneMoreAgainClicked?.Invoke());

            // 绑定7天签到按钮事件
            for (int i = 0; i < dayButtons.Length; i++)
            {
                int index = i; // 捕获局部变量以供闭包使用
                dayButtons[i].onClick.AddListener(() => OnDayClicked?.Invoke(index));
            }
        }

        // 唯一的刷新入口，根据传入的最新数据刷新UI状态
        public void RefreshUI(SignInData data)
        {
            for (int i = 0; i < dayButtons.Length; i++)
            {
                bool isSigned = data.SignedInDays[i];
                // 仅当是今天且今天未签到时，对应的按钮才可交互
                dayButtons[i].interactable = (i == data.CurrentDayIndex) && !data.HasSignedInToday && !isSigned;
                
                // TODO: 可以在这里添加打勾图标的显示/隐藏、已签到遮罩等表现逻辑
            }

            // 如果今天已经签到，隐藏双倍广告按钮
            oneMoreAgainButton.gameObject.SetActive(!data.HasSignedInToday);
        }
    }
}