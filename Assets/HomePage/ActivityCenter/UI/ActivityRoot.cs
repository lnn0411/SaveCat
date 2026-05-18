using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ActivityRoot 管理活动中心主界面，统一调度各活动的打开和关闭。
/// </summary>
public class ActivityRoot : MonoBehaviour
{
    private void Start()
    {
        GameObject activityGo = new GameObject("ActivityRoot");
        // 挂载核心Manager
        activityGo.AddComponent<ActivityCenter.Core.ActivityManager>();
    }
}


