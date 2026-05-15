using ActivityCenter.Core;
using UnityEngine;
using System;

namespace ActivityCenter.VideoPrivileges
{
    public class VideoPrivilegesController : IActivityController
    {
        private VideoPrivilegesData _data;
        private VideoPrivilegesView _view;
        private GameObject _uiInstance;

        // 优化项：记录上一次更新的秒数，避免每帧都在格式化字符串
        private int _lastUpdateSecond = -1;

        // 核心动态轮询标识：只有当 UI 实例存在（即面板打开时），才向 Manager 申请 Update 资源
        public bool NeedUpdate => _uiInstance != null;

        public void Open()
        {
            if (_uiInstance != null) return;

            _data = new VideoPrivilegesData();
            _data.OnDataUpdated += UpdateView;

            GameObject prefab = Resources.Load<GameObject>("Prefab/Activity/VideoPrivileges/VideoPrivileges Canvas");
            if (prefab == null)
            {
                Debug.LogError("VideoPrivileges Canvas 预制体加载失败！");
                return;
            }

            _uiInstance = UnityEngine.Object.Instantiate(prefab);
            _view = _uiInstance.GetComponent<VideoPrivilegesView>();

            _view.OnCloseClicked += Close;
            _view.OnRewardClicked += HandleRewardClick;

            UpdateView();
            
            // 强制刷新一次时间，避免打开瞬间显示默认文本
            _lastUpdateSecond = -1; 
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
            if (_data.IsAllRewardsClaimed()) return;

            Debug.Log("正在呼起视频广告 SDK...");
            
            // TODO: 对接真实的广告SDK逻辑
            bool adWatchSuccess = true; // 模拟看完广告
            
            if (adWatchSuccess)
            {
                Debug.Log($"观看成功，获得第 {_data.CurrentRewardStage + 1} 阶段奖励！");
                _data.AdvanceRewardStage();
                // TODO: 在这里执行发放对应道具的逻辑
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
            if (_view == null) return;

            DateTime now = DateTime.Now;
            
            // 只有当秒数发生变化时，才重新计算和刷新 UI，极大节省性能
            if (now.Second != _lastUpdateSecond)
            {
                _lastUpdateSecond = now.Second;

                // 计算今天剩余时间 (明天凌晨零点 - 当前时间)
                DateTime midnight = now.Date.AddDays(1);
                TimeSpan timeRemaining = midnight - now;

                // 格式化为 HH:MM:SS
                string timeString = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                    (int)timeRemaining.TotalHours, 
                    timeRemaining.Minutes, 
                    timeRemaining.Seconds);

                _view.UpdateTimerText(timeString);
            }
        }
    }
}