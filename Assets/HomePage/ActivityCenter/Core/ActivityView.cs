using UnityEngine;
using UnityEngine.UI;
using System;

namespace ActivityCenter.Core
{
    public class ActivityView : MonoBehaviour
    {
        [Header("Activity Buttons")]
        public Button signinButton;
        public Button turntableButton;
        public Button taskButton;
        public Button expeditionButton;
        public Button videoPrivilegesButton;
        public Button grandTreasuryButton;
        public Button swimmingCompetitionButton;

        // 暴露给Manager的点击事件
        public Action OnSignInClicked;
        public Action OnTurntableClicked;
        public Action OnTaskClicked;
        public Action OnExpeditionClicked;
        public Action OnVideoPrivilegesClicked;
        public Action OnGrandTreasuryClicked;
        public Action OnSwimmingCompetitionClicked;
        // ... 之后可以按需补充其他活动的Action

        private void Awake()
        {
            // 绑定按钮事件
            signinButton.onClick.AddListener(() => OnSignInClicked?.Invoke());
            turntableButton.onClick.AddListener(() => OnTurntableClicked?.Invoke());
            taskButton.onClick.AddListener(() => OnTaskClicked?.Invoke());
            expeditionButton.onClick.AddListener(() => OnExpeditionClicked?.Invoke());
            videoPrivilegesButton.onClick.AddListener(() => OnVideoPrivilegesClicked?.Invoke());
            grandTreasuryButton.onClick.AddListener(() => OnGrandTreasuryClicked?.Invoke());
            swimmingCompetitionButton.onClick.AddListener(() => OnSwimmingCompetitionClicked?.Invoke());
            // ... 绑定其他按钮
        }
    }
}