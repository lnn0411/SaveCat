using ActivityCenter.Core;
using UnityEngine;

namespace ActivityCenter.Turntable
{
    public class TurntableController : IActivityController
    {
        private TurntableData _data;
        private TurntableView _view;
        private GameObject _uiInstance;

        // --- 旋转动画相关变量 ---
        private bool _isSpinning = false;
        private float _spinTimer = 0f;
        private float _spinDuration = 3.0f; // 旋转持续时间
        private float _startAngle = 0f;
        private float _targetAngle = 0f;
        private int _currentPrizeIndex = 0;
        
        // 【核心】动态控制是否需要轮询：只有转盘在转时，才需要执行 OnUpdate
        public bool NeedUpdate => _isSpinning;

        public void Open()
        {
            if (_uiInstance != null) return;

            _data = new TurntableData();
            _data.OnDataUpdated += UpdateView;

            GameObject prefab = Resources.Load<GameObject>("Prefab/Activity/Turntable/Turntable Canvas");
            if (prefab == null) return;

            _uiInstance = Object.Instantiate(prefab);
            _view = _uiInstance.GetComponent<TurntableView>();

            _view.OnCloseClicked += Close;
            _view.OnDrawClicked += HandleDrawClick;
            _view.OnAdDrawClicked += HandleAdDrawClick;

            UpdateView();
        }

        public void Close()
        {
            if (_uiInstance != null)
            {
                Object.Destroy(_uiInstance);
                _uiInstance = null;
                _view = null;
                _isSpinning = false; // 强制停止动画，退出轮询

                if (_data != null)
                {
                    _data.OnDataUpdated -= UpdateView;
                    _data = null;
                }
            }
        }

        // 这是被 ActivityManager 集中调用的 Update
        public void OnUpdate()
        {
            if (!_isSpinning || _view == null) return;

            _spinTimer += Time.deltaTime;
            float t = _spinTimer / _spinDuration;

            // 使用类似 EaseOut (减速) 的缓动曲线：1 - (1-t)^3
            float easeT = 1f - Mathf.Pow(1f - t, 3);
            
            // 计算当前角度并应用
            float currentAngle = Mathf.Lerp(_startAngle, _targetAngle, easeT);
            _view.SetTurntableRotation(currentAngle);

            // 动画结束
            if (_spinTimer >= _spinDuration)
            {
                _isSpinning = false;
                _view.SetTurntableRotation(_targetAngle);
                
                // 记录实际结束角度，下一次从当前显示角度继续
                _startAngle = NormalizeAngle(_targetAngle);
                
                OnSpinFinished();
            }
        }

        private void HandleDrawClick()
        {
            if (_isSpinning) return;

            if (_data.CanFreeDraw)
            {
                _data.RecordFreeDraw();
                StartSpin();
            }
            else
            {
                // 假设你的全局数据模型存在
                if (GlobalDataModel.Instance.Coins >= TurntableData.DrawCost)
                {
                    GlobalDataModel.Instance.Coins -= TurntableData.DrawCost;
                    _data.RecordPaidDraw();
                    StartSpin();
                }
                else
                {
                    Debug.Log("金币不足！");
                    // TODO: 可以弹出提示UI
                }
            }
        }

        private void HandleAdDrawClick()
        {
            if (_isSpinning) return;

            if (_data.CanAdDraw)
            {
                Debug.Log("播放广告中...");
                bool isAdSuccess = true; // 模拟广告播放成功
                
                if (isAdSuccess)
                {
                    _data.RecordAdDraw();
                    StartSpin();
                }
            }
        }

        private void StartSpin()
        {
            _isSpinning = true;
            _spinTimer = 0f;

            // 每次抽奖前从当前显示角度开始，而不是重置到初始角度
            _startAngle = GetCurrentTurntableAngle();

            UpdateView(); // 刷新UI，禁用按钮

            // 1. 随机决定要中哪个奖品 (1 到 6)
            _currentPrizeIndex = Random.Range(1, 7);
            
            // 2. 计算目标角度
            // 1号在顶部，2号在最右，依旧顺时针递增
            int baseSpins = 5;
            float prizeAngleOffset = GetPrizeAngleOffset(_currentPrizeIndex);

            // 计算从当前起点到目标位置需要补偿的角度，保证每次旋转都从上一次的结束位置开始
            float extraRotation = (prizeAngleOffset - _startAngle + 360f) % 360f;
            _targetAngle = _startAngle + baseSpins * 360f + extraRotation;

            Debug.Log($"开始抽奖！目标奖品: {_currentPrizeIndex}");
        }

        private void OnSpinFinished()
        {
            UpdateView(); // 恢复按钮交互
            Debug.Log($"抽奖结束，发放奖励！当前奖品: {_currentPrizeIndex}");
            // TODO: 根据 _currentPrizeIndex 发放奖励
        }

        private float GetCurrentTurntableAngle()
        {
            if (_view != null && _view.turntableRect != null)
            {
                return NormalizeAngle(_view.turntableRect.localEulerAngles.z);
            }
            return NormalizeAngle(_startAngle);
        }

        private static float GetPrizeAngleOffset(int prizeIndex)
        {
            switch (prizeIndex)
            {
                case 1: return 30f;   // 顶部
                case 2: return 90f;  // 右侧
                case 3: return 150f;
                case 4: return 210f;
                case 5: return 270f;
                case 6: return 330f;
                default: return 0f;
            }
        }

        private static float NormalizeAngle(float angle)
        {
            float normalized = angle % 360f;
            if (normalized < 0f)
            {
                normalized += 360f;
            }
            return normalized;
        }

        private void UpdateView()
        {
            if (_view != null && _data != null)
            {
                _view.RefreshUI(_data, _isSpinning);
            }
        }
    }
}