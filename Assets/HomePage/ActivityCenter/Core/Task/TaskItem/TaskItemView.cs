using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 1. 定义任务种类枚举
public enum TaskType
{
    LoginDays,       // 登录游戏x天
    CollectWool,     // 收集X个毛线
    ReviveCount,     // 复活X次
    ClearGetCoins,   // 通关获得X个金币
    UseItemA,        // 使用X次A道具
    RouletteSpin     // 参与X次转盘活动
}

// 2. 定义任务状态枚举
public enum TaskState
{
    InProgress,      // 进行中（未完成）
    CanClaim,        // 已完成，待领取星星
    Collected        // 已领取（绿勾状态）
}

// 3. 任务数据模型 (Model)
[Serializable]
public class TaskItemData
{
    public string taskId;
    public TaskType type;
    public int targetValue;    // 目标值 (即需求中的 X)
    public int currentValue;   // 当前进度
    public int rewardStars;    // 完成后可获得的星星数量
    public TaskState state;

    // 预留的位置：任务成功的条件判断
    public bool CheckIfCompleted()
    {
        // 如果当前进度大于等于目标值，说明任务已达标
        return currentValue >= targetValue;
    }
}

// 4. 任务预制体视图逻辑 (View)
public class TaskItemView : MonoBehaviour
{
    [Header("UI 节点绑定")]
    public Button rewardButton;          // Button 节点
    public Image buttonImage;            // Button 节点上的 Image 组件
    public TMP_Text rewardText;          // Button 下的 Text (TMP)
    public Image iconImage;              // Icon 节点
    public TMP_Text informationText;     // Schedule/Information 节点

    [Header("图片资源配置")]
    public Sprite starSprite;            // 星星图片 (未领取时显示)
    public Sprite checkmarkSprite;       // 绿色打勾图片 (已领取后显示)
    
    [Tooltip("请按 TaskType 枚举的顺序拖入对应的任务图标")]
    public Sprite[] taskIcons;           

    // 当前绑定的数据
    private TaskItemData currentTaskData;
    
    // 预留给 TaskController 监听的领奖事件
    public Action<TaskItemData> OnClaimRewardEvent;

    /// <summary>
    /// 供外部 TaskController 调用，用于初始化或刷新此任务条
    /// </summary>
    public void Setup(TaskItemData data)
    {
        currentTaskData = data;
        
        // 重新绑定按钮事件，防止对象池复用时产生重复监听
        rewardButton.onClick.RemoveAllListeners();
        rewardButton.onClick.AddListener(OnRewardButtonClicked);

        RefreshUI();
    }

    /// <summary>
    /// 根据当前数据刷新 UI 显示
    /// </summary>
    public void RefreshUI()
    {
        if (currentTaskData == null) return;

        // 1. 设置任务要求文本 (Information)
        informationText.text = GenerateTaskDescription();

        // 2. 根据任务种类设置 Icon
        int iconIndex = (int)currentTaskData.type;
        if (iconIndex >= 0 && iconIndex < taskIcons.Length)
        {
            iconImage.sprite = taskIcons[iconIndex];
        }

        // 3. 每次刷新时，调用预留的条件判断
        if (currentTaskData.state == TaskState.InProgress && currentTaskData.CheckIfCompleted())
        {
            currentTaskData.state = TaskState.CanClaim;
        }

        // 4. 根据任务状态更新 Button 和 Text
        switch (currentTaskData.state)
        {
            case TaskState.InProgress:
                // 未完成：显示星星，显示数字，按钮不可点击
                buttonImage.sprite = starSprite;
                rewardText.text = currentTaskData.rewardStars.ToString();
                rewardButton.interactable = false; 
                break;
            
            case TaskState.CanClaim:
                // 已完成待领取：显示星星，显示数字，按钮可点击
                buttonImage.sprite = starSprite;
                rewardText.text = currentTaskData.rewardStars.ToString();
                rewardButton.interactable = true;
                break;

            case TaskState.Collected:
                // 已完成且已收集：显示绿勾，隐藏数字，按钮不可点击
                buttonImage.sprite = checkmarkSprite;
                rewardText.text = ""; 
                rewardButton.interactable = false; 
                break;
        }
    }

    /// <summary>
    /// 按钮点击事件处理
    /// </summary>
    private void OnRewardButtonClicked()
    {
        if (currentTaskData.state == TaskState.CanClaim)
        {
            // 将领奖逻辑抛给 Controller 处理（例如增加玩家星星总数、播放特效等）
            OnClaimRewardEvent?.Invoke(currentTaskData);
            
            // 假设领取成功，将状态更新为已收集并刷新 UI
            currentTaskData.state = TaskState.Collected;
            RefreshUI();
        }
    }

    /// <summary>
    /// 辅助方法：根据任务类型和进度动态生成任务要求文本
    /// </summary>
    private string GenerateTaskDescription()
    {
        string progressStr = $"({currentTaskData.currentValue}/{currentTaskData.targetValue})";
        
        switch (currentTaskData.type)
        {
            case TaskType.LoginDays: 
                return $"登录游戏{currentTaskData.targetValue}天 {progressStr}";
            case TaskType.CollectWool: 
                return $"收集{currentTaskData.targetValue}个毛线 {progressStr}";
            case TaskType.ReviveCount: 
                return $"复活{currentTaskData.targetValue}次 {progressStr}";
            case TaskType.ClearGetCoins: 
                return $"通关获得{currentTaskData.targetValue}个金币 {progressStr}";
            case TaskType.UseItemA: 
                return $"使用{currentTaskData.targetValue}次A道具 {progressStr}";
            case TaskType.RouletteSpin: 
                return $"参与{currentTaskData.targetValue}次转盘活动 {progressStr}";
            default: 
                return "未知任务";
        }
    }
}