using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manager管理规则
// PanelView负责ui的收集和绑定
// ItemView负责单个数据的展示


/// <summary>
/// 槽位管理器
/// 负责接收逃逸成功的方块数据，将其转化为UI槽位弹药，并向战斗系统提供弹药读取、消耗接口
/// </summary>
public class SlotManager : Singleton<SlotManager>
{
    #region Inspector Configuration
    [Header("Slot Settings")]
    [SerializeField] private int maxSlotCount = 8;

    [Header("Optional UI Bindings")]
    [SerializeField] private Transform slotRoot; // 用于放置SlotItemView的父物体

    #endregion

    #region Runtime Data
    private readonly List<SlotData> _slots = new List<SlotData>();
    private readonly List<SlotItemView> _slotViews = new List<SlotItemView>();

    public int MaxSlotCount => maxSlotCount;

    #endregion

    
#region Lifecycle
    protected override void Awake()
    {
        base.Awake();

        InitSlots();
        AutoBindViews();
        RefreshAllViews();
    }

    private void OnEnable()
    {
        EventManager.AddListener<BlockData>(EventID.OnBlockEscapeSuccess, OnBlockEscapeSuccess);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<BlockData>(EventID.OnBlockEscapeSuccess, OnBlockEscapeSuccess);
    }


#endregion

#region Initialization

    /// <summary>
    /// 初始化固定数量的槽位数据。
    /// </summary>
    private void InitSlots()
    {
        _slots.Clear();

        for (int i = 0; i < maxSlotCount; i++)
        {
            SlotData slotData = new SlotData();
            slotData.Clear();
            _slots.Add(slotData);
        }
    }

    /// <summary>
    /// 从指定根节点下自动绑定 SlotItemView。
    /// </summary>
    private void AutoBindViews()
    {
        _slotViews.Clear();

        if (slotRoot == null)
        {
            return;
        }

        SlotItemView[] views = slotRoot.GetComponentsInChildren<SlotItemView>(true);

        for (int i = 0; i < views.Length && i < maxSlotCount; i++)
        {
            _slotViews.Add(views[i]);
        }
    }

    /// <summary>
    /// 外部手动绑定槽位 UI。
    /// 适合后面由 SlotPanelView 初始化完成后，把 7 个 SlotItemView 传进来。
    /// </summary>
    public void BindViews(IList<SlotItemView> views)
    {
        _slotViews.Clear();

        if (views == null)
        {
            RefreshAllViews();
            return;
        }

        for (int i = 0; i < views.Count && i < maxSlotCount; i++)
        {
            if (views[i] != null)
            {
                _slotViews.Add(views[i]);
            }
        }

        RefreshAllViews();
    }

#endregion

#region 弹药逻辑

    /// <summary>
    /// 尝试加入新的弹药
    /// 每个方块占用一个槽位，StrengthCount 表示弹药数量
    /// </summary>
    /// <param name="colorType"></param>
    /// <param name="strengthCount"></param>
    /// <returns></returns>
    public bool TryAddAmmo(BlockType colorType, int strengthCount)
    {
        if(colorType == BlockType.None || strengthCount <= 0)
        {
            Debug.LogWarning($"尝试添加无效弹药：颜色={colorType}, 强度={strengthCount}");
            return false;
        }

        int emptyIndex = FindFirstEmptySlotIndex();
        
        //代表已满
        if(emptyIndex < 0) return false;

        _slots[emptyIndex].Load(colorType, strengthCount);
        RefreshSlotView(emptyIndex);

        //通知战斗系统 有新的弹药入槽位
        EventManager.Broadcast(EventID.OnBlockSlotted);
        return true;
    }

    // 获取第一顺位弹药
    public bool TryGetFirstAmmo(out int slotIndex, out SlotData slotData)
    {
        for(int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsEmpty && _slots[i].StrengthCount > 0)
            {
                slotIndex = i;
                slotData = _slots[i];
                return true;
            }
        }

        slotIndex = -1;
        slotData = null;
        return false;
    }

    /// <summary>
    /// 当前是否还有空槽位。
    /// </summary>
    public bool HasFreeSlot()
    {
        return FindFirstEmptySlotIndex() >= 0;
    }

    /// <summary>
    /// 获取第一个空槽位索引
    /// 用来决定飞向哪个UI槽位
    /// </summary>
    /// <param name="emptySlotIndex"></param>
    /// <returns></returns>
    public bool TryGetFirstEmptySlotIndex(out int emptySlotIndex)
    {
        emptySlotIndex = FindFirstEmptySlotIndex();
        return emptySlotIndex >= 0;
    }

    /// <summary>
    /// 收集当前所有可用弹药槽位
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public int CollecAvailableAmmoSlots(List<int>result)
    {
        if(result == null) return 0;
        result.Clear();

        for(int i = 0; i < _slots.Count; i++)
        {
            SlotData slotData = _slots[i];
            if(slotData == null) continue;
            if(!slotData.IsEmpty && slotData.StrengthCount > 0)
            {
                result.Add(i);
            }
        }
        return result.Count;
    }

    /// <summary>
    /// 获取指定槽位的弹药数据
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="slotData"></param>
    /// <returns></returns>
    public bool TryGetAmmoAt(int slotIndex, out SlotData slotData)
    {
        if(slotIndex < 0 || slotIndex >= _slots.Count) 
        {
            slotData = null;
            return false;
        }

        slotData = _slots[slotIndex];
        if(slotData == null || slotData.IsEmpty || slotData.StrengthCount <= 0)
        {
            slotData = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 获取指定槽位对应的UI表现对象
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public SlotItemView GetSlotView(int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= _slotViews.Count) return null;
        return _slotViews[slotIndex];
    }
    
    /// <summary>
    /// 消耗指定槽位一点威力
    /// 威力为0时，清空槽位数据
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool TryConsumeAmmoAt(int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= _slots.Count) return false;

        SlotData slotData = _slots[slotIndex];

        if(slotData.IsEmpty || slotData.StrengthCount <= 0) return false;

        slotData.StrengthCount--;
        if(slotData.StrengthCount <= 0)
        {
            slotData.Clear();
            RefreshAllViews();
        }
        else
        {
            RefreshSlotView(slotIndex);
        }
        return true;
    }

    /// <summary>
    /// 清楚所有数据
    /// </summary>
    public void ClearAllSlots()
    {
        for(int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Clear();
        }
        RefreshAllViews();
    }

    /// <summary>
    /// 获取第一个空槽位索引
    /// </summary>
    private int FindFirstEmptySlotIndex()
    {
        for(int i = 0; i < _slots.Count; i++)
        {
            if(_slots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1;
    }

#endregion


#region  刷新逻辑

    /// <summary>
    /// 刷新所有槽位UI表现
    /// </summary>
    private void RefreshAllViews()
    {
        for(int i = 0; i < _slotViews.Count; i++)
        {
            RefreshSlotView(i);
        }
    }

    private void RefreshSlotView(int index)
    {
        if(index < 0 || index >= _slotViews.Count) return;

        if(_slotViews[index] == null) return;
        _slotViews[index].Refresh(_slots[index]);
    }
#endregion


#region 事件

    /// <summary>
    /// 方块逃逸成功后 添加弹药
    /// </summary>
    /// <param name="blockData"></param>
    private void OnBlockEscapeSuccess(BlockData blockData)
    {
        if(blockData == null) return;

        bool success = TryAddAmmo(blockData.Type, blockData.Length);

        if(!success)
        {
            Debug.Log($"弹药已满，无法添加新弹药：颜色={blockData.Type}, 强度={blockData.Length}");
            // 槽位满了 发出失败提示
            return;
        }
    }

#endregion
}
