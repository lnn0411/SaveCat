using System;
using UnityEngine;

/// <summary>
/// 全局数据模型，负责管理玩家的核心变量、数据持久化以及数据变更的事件分发。
/// </summary>
public class GlobalDataModel
{
    // ======================== 单例模式 ========================
    private static GlobalDataModel _instance;
    public static GlobalDataModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GlobalDataModel();
                _instance.LoadData(); // 首次调用时自动加载本地数据
            }
            return _instance;
        }
    }

    // 私有构造函数，防止外部意外实例化
    private GlobalDataModel() { }

    // ======================== 数据字段 ========================
    private int _coins;
    private int _health;
    private int _maxHealth = 100;
    private int _currentLevel;

    // ======================== 事件委托 (View 层监听) ========================
    // 当数值发生变化时触发，UI 脚本可以订阅这些事件来自动刷新画面
    public event Action<int> OnCoinsChanged;
    public event Action<int, int> OnHealthChanged; // 传递 (当前血量, 最大血量)
    public event Action<int> OnLevelChanged;

    // ======================== 属性 (数据封装与验证) ========================
    public int Coins
    {
        get => _coins;
        set
        {
            // 防止无效赋值
            if (_coins != value)
            {
                _coins = Mathf.Max(0, value); // 金币不能为负数
                OnCoinsChanged?.Invoke(_coins); // 触发金币更新事件
            }
        }
    }

    public int MaxHealth
    {
        get => _maxHealth;
        // 如果游戏中有升级增加生命上限的机制，可以在这里添加 set 并在改变时触发 OnHealthChanged
    }

    public int Health
    {
        get => _health;
        set
        {
            if (_health != value)
            {
                _health = Mathf.Clamp(value, 0, _maxHealth); // 限制血量在 0 到 MaxHealth 之间
                OnHealthChanged?.Invoke(_health, _maxHealth);
            }
        }
    }

    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            if (_currentLevel != value)
            {
                _currentLevel = Mathf.Max(1, value); // 关卡最低为 1
                OnLevelChanged?.Invoke(_currentLevel);
            }
        }
    }

    // ======================== 数据持久化 (存档/读档) ========================
    
    /// <summary>
    /// 保存当前数据到本地
    /// </summary>
    public void SaveData()
    {
        PlayerPrefs.SetInt("Player_Coins", _coins);
        PlayerPrefs.SetInt("Player_Health", _health);
        PlayerPrefs.SetInt("Player_Level", _currentLevel);
        PlayerPrefs.Save();
        
        Debug.Log("[GlobalDataModel] 游戏数据已保存。");
    }

    /// <summary>
    /// 从本地加载数据
    /// </summary>
    public void LoadData()
    {
        _coins = PlayerPrefs.GetInt("Player_Coins", 0);
        _maxHealth = 100; // 默认最大血量
        _health = PlayerPrefs.GetInt("Player_Health", _maxHealth);
        _currentLevel = PlayerPrefs.GetInt("Player_Level", 1);
    }

    /// <summary>
    /// 重置所有数据（用于重新开始游戏或测试）
    /// </summary>
    public void ResetData()
    {
        Coins = 0;
        Health = _maxHealth;
        CurrentLevel = 1;
        SaveData();
        
        Debug.Log("[GlobalDataModel] 游戏数据已重置。");
    }
}