using System.Collections.Generic;
using UnityEngine;
using ActivityCenter.SignIn; // 引入签到模块
using ActivityCenter.Turntable; // 未来引入大转盘等模块
using ActivityCenter.Task; // 引入任务模块
using ActivityCenter.Expedition; // 引入远征模块
using ActivityCenter.VideoPrivileges; // 引入视频权益模块
using ActivityCenter.GrandTreasury; // 引入 grand treasury 模块
// using ActivityCenter.SwimmingCompetition; // 引入游泳比赛模块

namespace ActivityCenter.Core
{
    public enum ActivityType
    {
        SignIn,
        Turntable,
        Task,
        Expedition,
        VideoPrivileges,
        GrandTreasury,
        SwimmingCompetition
    }

    public class ActivityManager : MonoBehaviour
    {
        private GameObject _activityCanvasInstance;
        private ActivityView _activityView;

        // 缓存所有已注册的活动模块
        private Dictionary<ActivityType, IActivityController> _registeredModules = new Dictionary<ActivityType, IActivityController>();

        private void Start()
        {
            // 1. 集中注册所有需要的模块
            RegisterAllModules();
            
            // 2. 初始化主界面
            InitializeActivityMainUI();
        }

        private void RegisterAllModules()
        {
            // 在一开始将所有需要的活动 Controller 实例化并加入字典
            // 因为 Controller 此时只是纯 C# 对象（UI 还没加载），几乎没有内存消耗
            _registeredModules.Add(ActivityType.SignIn, new SignInController());
            _registeredModules.Add(ActivityType.Turntable, new TurntableController());
            // 未来添加新活动只需要在这里加一行即可：
            _registeredModules.Add(ActivityType.Task, new TaskController());
            _registeredModules.Add(ActivityType.Expedition, new ExpeditionController());
            _registeredModules.Add(ActivityType.VideoPrivileges, new VideoPrivilegesController());
            _registeredModules.Add(ActivityType.GrandTreasury, new GrandTreasuryController());
        }

        // --- 核心：集中轮询机制 ---
        private void Update()
        {
            // 遍历所有已注册的模块
            foreach (var module in _registeredModules.Values)
            {
                // 如果模块当前声称自己需要 Update，才执行调用
                if (module.NeedUpdate)
                {
                    module.OnUpdate();
                }
            }
        }

        private void InitializeActivityMainUI()
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Activity/Activity Canvas");
            if (prefab == null) return;

            _activityCanvasInstance = Instantiate(prefab, this.transform); 
            _activityView = _activityCanvasInstance.GetComponent<ActivityView>();

            // 监听主界面的按钮点击，通过类型直接打开
            _activityView.OnSignInClicked += () => OpenActivityModule(ActivityType.SignIn);
            _activityView.OnTurntableClicked += () => OpenActivityModule(ActivityType.Turntable);
            _activityView.OnTaskClicked += () => OpenActivityModule(ActivityType.Task);
             // 未来添加新活动只需要在 ActivityView 添加按钮并监听，然后在这里加一行即可：
            _activityView.OnExpeditionClicked += () => OpenActivityModule(ActivityType.Expedition);
            _activityView.OnVideoPrivilegesClicked += () => OpenActivityModule(ActivityType.VideoPrivileges);
            _activityView.OnGrandTreasuryClicked += () => OpenActivityModule(ActivityType.GrandTreasury);
        }

        private void OpenActivityModule(ActivityType type)
        {
            if (_registeredModules.TryGetValue(type, out IActivityController controller))
            {
                // Debug.Log($"打开活动模块: {type}");
                controller.Open();
            }
            else
            {
                Debug.LogWarning($"[{type}] 尝试打开未注册的活动模块！");
            }
        }

        private void OnDestroy()
        {
            foreach (var controller in _registeredModules.Values)
            {
                controller.Close();
            }
            _registeredModules.Clear();
        }
    }
}