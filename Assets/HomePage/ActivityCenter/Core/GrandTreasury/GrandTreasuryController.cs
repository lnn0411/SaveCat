using ActivityCenter.Core;
using UnityEngine;
using System;

namespace ActivityCenter.GrandTreasury
{
    public class GrandTreasuryController : IActivityController
    {
        private GrandTreasuryData _data;
        private GrandTreasuryView _view;
        private GameObject _uiInstance;

        // 用于倒计时优化的缓存变量，记录上一次刷新的小时数
        private int _lastUpdateHour = -1;

        // 只有当 UI 面板打开时，才需要 ActivityManager 轮询本模块
        public bool NeedUpdate => _uiInstance != null;

        public void Open()
        {
            if (_uiInstance != null) return;

            // 1. 初始化数据与事件绑定
            _data = new GrandTreasuryData();
            _data.OnDataUpdated += UpdateView;

            // 2. 加载 UI 预制体
            GameObject prefab = Resources.Load<GameObject>("Prefab/Activity/GrandTreasury/GrandTreasury Canvas");
            if (prefab == null)
            {
                Debug.LogError("GrandTreasury Canvas 预制体加载失败，请检查路径！");
                return;
            }

            _uiInstance = UnityEngine.Object.Instantiate(prefab);
            _view = _uiInstance.GetComponent<GrandTreasuryView>();

            // 3. 绑定 UI 操作事件
            _view.OnCloseClicked += Close;
            _view.OnRewardClicked += HandleRewardClick;

            // 4. 初次打开时强制刷新一遍界面和时间
            UpdateView();
            _lastUpdateHour = -1; 
            OnUpdate(); 
        }

        public void Close()
        {
            if (_uiInstance != null)
            {
                UnityEngine.Object.Destroy(_uiInstance);
                _uiInstance = null;
                _view = null;

                if (_data != null)
                {
                    _data.OnDataUpdated -= UpdateView;
                    _data = null;
                }
            }
        }

        private void HandleRewardClick()
        {
            // 在 Controller 中处理业务逻辑判断
            if (_data.CanClaimReward())
            {
                Debug.Log("条件满足，成功领取大金库奖励！");
                _data.ClaimReward();
                
                // TODO: 在此处调用系统发奖 API 发放虚拟货币
            }
            else
            {
                // 如果不能领取，分类给玩家提示
                if (_data.IsRewardClaimed)
                {
                    Debug.Log("提示：奖励已经领取过了。");
                }
                else
                {
                    Debug.Log($"提示：金币收集进度不足，还差 {_data.MaxCoins - _data.CurrentCoins} 个！");
                }
            }
        }

        private void UpdateView()
        {
            if (_view != null && _data != null)
            {
                _view.RefreshUI(_data);
            }
        }

        // --- Manager 集中调用的轮询方法 ---
        public void OnUpdate()
        {
            if (_view == null || _data == null) return;

            DateTime now = DateTime.Now;

            // 优化：仅当时间的小时发生变化时（比如从 14:59 变成 15:00），才重新计算 UI
            if (now.Hour != _lastUpdateHour)
            {
                _lastUpdateHour = now.Hour;

                TimeSpan timeRemaining = _data.EndTime - now;

                if (timeRemaining.TotalSeconds <= 0)
                {
                    // 活动已结束
                    _view.UpdateTimerText("0D 0H");
                }
                else
                {
                    // 格式化为 XD YH (X天 Y小时)
                    _view.UpdateTimerText($"{timeRemaining.Days}D {timeRemaining.Hours}H");
                }
            }
        }
    }
}