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
    [Header("Activity Buttons")]
    [SerializeField] private Button signinButton;
    [SerializeField] private Button turntableButton;
    [SerializeField] private Button taskButton;
    [SerializeField] private Button expeditionButton;
    [SerializeField] private Button videoPrivilegesButton;
    [SerializeField] private Button grandTreasuryButton;
    [SerializeField] private Button swimmingCompetitionButton;
    [SerializeField] private Button closeButton;

    private Dictionary<Button, ActivityType> buttonActivityMap = new Dictionary<Button, ActivityType>();
    private Dictionary<Button, UnityAction> buttonListeners = new Dictionary<Button, UnityAction>();
    private bool childActivityOpen = false;

    private void Awake()
    {
        // 移除 AutoBindButtons()，因为 Activity Canvas 在 Start() 中加载
    }

    private void Start()
    {
        EnsureActivityCanvasLoaded();
        AutoBindButtons();
        // 创建 ActivityManager 单例实例，如果不存在
        if (ActivityManager.Instance == null)
        {
            GameObject obj = new GameObject("ActivityManager");
            ActivityManager manager = obj.AddComponent<ActivityManager>();
            DontDestroyOnLoad(obj); // 确保跨场景存在
        }
        // 验证初始化
        if (ActivityManager.Instance == null)
        {
            Debug.LogError("[ActivityRoot] Failed to initialize ActivityManager singleton");
        }
        RegisterButtonListeners();
        SubscribeActivityManagerEvents();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
        UnsubscribeActivityManagerEvents();
    }

    private void EnsureActivityCanvasLoaded()
    {
        // 如果 Activity Canvas 已经存在，则无需再次加载
        GameObject existingActivityCanvas = GameObject.Find("Activity Canvas");
        if (existingActivityCanvas != null)
        {
            Debug.Log("[ActivityRoot] Activity Canvas already exists in scene");
            return;
        }

        GameObject activityCanvasPrefab = Resources.Load<GameObject>("Prefab/Activity/Activity Canvas");
        if (activityCanvasPrefab == null)
        {
            Debug.LogError("[ActivityRoot] Failed to load Activity Canvas prefab from Resources/Prefab/Activity/Activity Canvas");
            return;
        }

        Transform parent = transform.root;
        GameObject instance = Instantiate(activityCanvasPrefab, parent);
        instance.name = "Activity Canvas";
        Debug.Log("[ActivityRoot] Activity Canvas loaded successfully");
    }

    private void AutoBindButtons()
    {
        Transform activityCanvas = GameObject.Find("Activity Canvas")?.transform;
        if (activityCanvas == null)
        {
            Debug.LogWarning("[ActivityRoot] Activity Canvas not found, cannot bind buttons");
            return;
        }

        if (signinButton == null)
            signinButton = FindButtonRecursive(activityCanvas, "SigninButton");
        if (turntableButton == null)
            turntableButton = FindButtonRecursive(activityCanvas, "TurntableButton");
        if (taskButton == null)
            taskButton = FindButtonRecursive(activityCanvas, "TaskButton");
        if (expeditionButton == null)
            expeditionButton = FindButtonRecursive(activityCanvas, "ExpeditionButton");
        if (videoPrivilegesButton == null)
            videoPrivilegesButton = FindButtonRecursive(activityCanvas, "VideoPrivilegesButton");
        if (grandTreasuryButton == null)
            grandTreasuryButton = FindButtonRecursive(activityCanvas, "GrandTreasuryButton");
        if (swimmingCompetitionButton == null)
            swimmingCompetitionButton = FindButtonRecursive(activityCanvas, "SwimmingCompetitionButton");
        if (closeButton == null)
            closeButton = FindButtonRecursive(activityCanvas, "CloseButton");

        if (signinButton == null) Debug.LogWarning("[ActivityRoot] SigninButton not found in Activity Canvas");
        if (turntableButton == null) Debug.LogWarning("[ActivityRoot] TurntableButton not found in Activity Canvas");
        if (taskButton == null) Debug.LogWarning("[ActivityRoot] TaskButton not found in Activity Canvas");
        if (expeditionButton == null) Debug.LogWarning("[ActivityRoot] ExpeditionButton not found in Activity Canvas");
        if (videoPrivilegesButton == null) Debug.LogWarning("[ActivityRoot] VideoPrivilegesButton not found in Activity Canvas");
        if (grandTreasuryButton == null) Debug.LogWarning("[ActivityRoot] GrandTreasuryButton not found in Activity Canvas");
        if (swimmingCompetitionButton == null) Debug.LogWarning("[ActivityRoot] SwimmingCompetitionButton not found in Activity Canvas");
        if (closeButton == null) Debug.LogWarning("[ActivityRoot] CloseButton not found in Activity Canvas");
    }

    private Button FindButtonRecursive(Transform parent, string targetName)
    {
        if (parent == null)
            return null;

        if (parent.name == targetName)
            return parent.GetComponent<Button>();

        for (int i = 0; i < parent.childCount; i++)
        {
            Button button = FindButtonRecursive(parent.GetChild(i), targetName);
            if (button != null)
                return button;
        }

        return null;
    }

    private void RegisterButtonListeners()
    {
        RegisterButton(signinButton, ActivityType.SignIn);
        RegisterButton(turntableButton, ActivityType.Turntable);
        RegisterButton(taskButton, ActivityType.Task);
        RegisterButton(expeditionButton, ActivityType.Expedition);
        RegisterButton(videoPrivilegesButton, ActivityType.VideoPrivileges);
        RegisterButton(grandTreasuryButton, ActivityType.GrandTreasury);
        RegisterButton(swimmingCompetitionButton, ActivityType.SwimmingCompetition);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void RegisterButton(Button button, ActivityType activityType)
    {
        if (button == null) return;
        UnityAction action = () => OnActivityButtonClicked(activityType);
        button.onClick.AddListener(action);
        buttonActivityMap[button] = activityType;
        buttonListeners[button] = action;
    }

    private void UnregisterButtonListeners()
    {
        foreach (var kvp in buttonListeners)
        {
            if (kvp.Key != null)
            {
                kvp.Key.onClick.RemoveListener(kvp.Value);
            }
        }
        buttonListeners.Clear();
        buttonActivityMap.Clear();

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    private void OnActivityButtonClicked(ActivityType activityType)
    {
        if (ActivityManager.Instance == null)
        {
            Debug.LogError("ActivityManager is not initialized");
            return;
        }

        switch (activityType)
        {
            case ActivityType.SignIn:
                Transform signInParent = GameObject.Find("Activity Canvas")?.transform ?? transform.root;
                if (ActivityManager.Instance.OpenSignIn(signInParent) != null)
                {
                    SetMainButtonsInteractable(false);
                    childActivityOpen = true;
                }
                break;
            case ActivityType.Turntable:
                Debug.Log("[ActivityRoot] Turntable activity not implemented yet");
                break;
            case ActivityType.Task:
                Debug.Log("[ActivityRoot] Task activity not implemented yet");
                break;
            case ActivityType.Expedition:
                Debug.Log("[ActivityRoot] Expedition activity not implemented yet");
                break;
            case ActivityType.VideoPrivileges:
                Debug.Log("[ActivityRoot] VideoPrivileges activity not implemented yet");
                break;
            case ActivityType.GrandTreasury:
                Debug.Log("[ActivityRoot] GrandTreasury activity not implemented yet");
                break;
            case ActivityType.SwimmingCompetition:
                Debug.Log("[ActivityRoot] SwimmingCompetition activity not implemented yet");
                break;
        }
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void SubscribeActivityManagerEvents()
    {
        if (ActivityManager.Instance != null)
        {
            ActivityManager.Instance.OnActivityOpened += OnActivityOpened;
            ActivityManager.Instance.OnActivityClosed += OnActivityClosed;
        }
    }

    private void UnsubscribeActivityManagerEvents()
    {
        if (ActivityManager.Instance != null)
        {
            ActivityManager.Instance.OnActivityOpened -= OnActivityOpened;
            ActivityManager.Instance.OnActivityClosed -= OnActivityClosed;
        }
    }

    private void OnActivityOpened(string prefabName)
    {
        SetMainButtonsInteractable(false);
        childActivityOpen = true;
    }

    private void OnActivityClosed(string prefabName)
    {
        SetMainButtonsInteractable(true);
        childActivityOpen = false;
    }

    private void SetMainButtonsInteractable(bool interactable)
    {
        if (signinButton != null) signinButton.interactable = interactable;
        if (turntableButton != null) turntableButton.interactable = interactable;
        if (taskButton != null) taskButton.interactable = interactable;
        if (expeditionButton != null) expeditionButton.interactable = interactable;
        if (videoPrivilegesButton != null) videoPrivilegesButton.interactable = interactable;
        if (grandTreasuryButton != null) grandTreasuryButton.interactable = interactable;
        if (swimmingCompetitionButton != null) swimmingCompetitionButton.interactable = interactable;
        if (closeButton != null) closeButton.interactable = interactable;
    }
}

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
