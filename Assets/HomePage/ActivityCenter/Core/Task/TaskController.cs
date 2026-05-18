using UnityEngine;
using ActivityCenter.Core;

namespace ActivityCenter.Task
{
    public class TaskController : IActivityController
    {
        private TaskData _data;
        private TaskView _view;
        private GameObject _uiInstance;
        private GameObject _taskItemPrefab;

        public bool NeedUpdate => _uiInstance != null; 

        public void Open()
        {
            Debug.Log("打开任务模块");
            if (_uiInstance != null) return; 

            _data = new TaskData();
            _data.OnDataUpdated += UpdateView;

            GameObject mainPrefab = Resources.Load<GameObject>("Prefab/Activity/Task/Task Canvas");
            _taskItemPrefab = Resources.Load<GameObject>("Prefab/Activity/Task/TaskItem/TaskItem");
    
            if (mainPrefab == null || _taskItemPrefab == null)
            {
                Debug.LogError("Task 模块预制体加载失败，请检查路径！");
                return;
            }

            _uiInstance = Object.Instantiate(mainPrefab);
            _view = _uiInstance.GetComponent<TaskView>();

            _view.OnCloseClicked += Close;
            _view.OnPrizeClicked += HandlePrizeClick;
            
            // 新增：监听 View 层的广告点击事件
            _view.OnAdvertisingClicked += HandleAdvertisingClick; 

            GenerateTaskItems();
            UpdateView();
        }

        public void Close()
        {
            if (_uiInstance != null)
            {
                Object.Destroy(_uiInstance);
                _uiInstance = null;
                _view = null;

                if (_data != null)
                {
                    _data.OnDataUpdated -= UpdateView;
                    _data = null;
                }
            }
        }

        public void OnUpdate()
        {
            if (_data == null || _view == null) return;

            System.TimeSpan ts = _data.ActivityEndTime - System.DateTime.Now;
            if (ts.TotalSeconds > 0)
            {
                _view.UpdateTimerDisplay($"{ts.Days}D {ts.Hours}H");
            }
            else
            {
                _view.UpdateTimerDisplay("0D 0H");
            }
        }

        private void GenerateTaskItems()
        {
            foreach (Transform child in _view.contentTransform)
            {
                Object.Destroy(child.gameObject);
            }

            foreach (var itemData in _data.TaskList)
            {
                GameObject go = Object.Instantiate(_taskItemPrefab, _view.contentTransform);
                TaskItemView itemView = go.GetComponent<TaskItemView>();
                
                itemView.OnClaimRewardEvent += HandleTaskRewardClaimed;
                itemView.Setup(itemData);
            }
        }

        private void HandleTaskRewardClaimed(TaskItemData taskItem)
        {
            taskItem.state = TaskState.Collected;
            _data.AddStarsFromTask(taskItem.rewardStars);
        }

        private void HandlePrizeClick(int index)
        {
            _data.ClaimPrize(index);
        }

        // 新增：处理广告点击逻辑
        private void HandleAdvertisingClick()
        {
            Debug.Log("[TaskController] 准备播放广告...");
            
            // TODO: 这里应接入真正的广告 SDK 逻辑
            bool isAdPlaySuccess = true; // 模拟广告播放成功
            
            if (isAdPlaySuccess)
            {
                Debug.Log("[TaskController] 广告播放完毕，发放 5 颗星星奖励！");
                // 通知 Model 数据已看广告并加星，Model 会自动触发 UI 刷新
                _data.WatchAdComplete(5); 
            }
        }

        private void UpdateView()
        {
            if (_view != null && _data != null)
            {
                _view.RefreshUI(_data);
            }
        }
    }
}