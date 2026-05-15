using ActivityCenter.Core;
using UnityEngine;
// using UnityEngine.SceneManagement; // 如果需要切换场景，请取消注释

namespace ActivityCenter.Expedition
{
    public class ExpeditionController : IActivityController
    {
        private ExpeditionData _data;
        private ExpeditionView _view;
        private GameObject _uiInstance;

        // 远征界面通常是静态展示，不需要每帧轮询更新
        public bool NeedUpdate => false;

        public void OnUpdate()
        {
            // 留空即可，Manager 检查到 NeedUpdate 为 false 时不会调用此方法
        }

        public void Open()
        {
            if (_uiInstance != null) return; // 防止重复打开

            // 1. 初始化 Model 数据
            _data = new ExpeditionData();
            _data.OnDataUpdated += UpdateView;

            // 2. 加载并实例化 View 层预制体
            GameObject prefab = Resources.Load<GameObject>("Prefab/Activity/Expedition/Expedition Canvas");
            if (prefab == null)
            {
                Debug.LogError("Expedition Canvas 预制体路径错误或未找到！");
                return;
            }

            _uiInstance = Object.Instantiate(prefab);
            _view = _uiInstance.GetComponent<ExpeditionView>();

            // 3. 绑定 View 层事件
            _view.OnCloseClicked += Close;
            _view.OnStartClicked += HandleStartClick;

            // 4. 初次打开时刷新一次 UI
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

        private void HandleStartClick()
        {
            Debug.Log("StartButton 被点击，准备转到挑战场景...");
            
            // TODO: 在这里编写转到挑战场景的逻辑
            // 例如：SceneManager.LoadScene("ChallengeSceneName");
            
            // 切换场景前，通常需要关闭当前的活动 UI
            // Close(); 
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