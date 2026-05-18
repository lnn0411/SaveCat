using UnityEngine;
using ActivityCenter.Core;

namespace ActivityCenter.SignIn
{
    public class SignInController : IActivityController
    {
        private SignInData _data;
        private SignInView _view;
        private GameObject _uiInstance;

        public bool NeedUpdate => false;
        // 打开界面的入口
        public void Open()
        {
            if (_uiInstance != null) return; // 防止重复打开

            // 1. 初始化Model
            _data = new SignInData();
            _data.OnDataUpdated += UpdateView; // 监听数据变化

            // 2. 加载并实例化View (注意：Resources.Load 不需要文件后缀和 Assets/Resources/ 前缀)
            GameObject prefab = Resources.Load<GameObject>("Prefab/Activity/SignIn/SignIn Canvas");
            if (prefab == null)
            {
                Debug.LogError("SignIn Canvas Prefab 路径错误或未找到！");
                return;
            }
            
            _uiInstance = Object.Instantiate(prefab);
            _view = _uiInstance.GetComponent<SignInView>();

            // 3. 绑定View层的用户输入事件
            _view.OnCloseClicked += Close;
            _view.OnDayClicked += HandleDayClick;
            _view.OnOneMoreAgainClicked += HandleOneMoreAgainClick;

            // 4. 初次展示时刷新一次UI
            UpdateView();
        }

        public void OnUpdate()
        {
        }
        // 关闭界面的入口
        public void Close()
        {
            if (_uiInstance != null)
            {
                Object.Destroy(_uiInstance);
                _uiInstance = null;
                _view = null;
                
                // 移除数据监听，防止内存泄漏
                if (_data != null)
                {
                    _data.OnDataUpdated -= UpdateView;
                    _data = null; 
                }
            }
        }

        private void HandleDayClick(int dayIndex)
        {
            // 普通签到
            _data.ExecuteSignIn(dayIndex, isDoubleReward: false);
        }

        private void HandleOneMoreAgainClick()
        {
            // TODO: 在这里接入广告SDK逻辑
            Debug.Log("播放广告中...");
            bool isAdSuccess = true; // 模拟广告播放成功
            
            if (isAdSuccess)
            {
                // 双倍签到
                _data.ExecuteSignIn(_data.CurrentDayIndex, isDoubleReward: true);
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