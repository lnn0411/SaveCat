using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能系统管理器
/// </summary>
public class SkillManager : Singleton<SkillManager>
{
    //每种技能库存
    private readonly Dictionary<SkillType,int> _skillCounts = new Dictionary<SkillType, int>();
    //当前正在等待玩家点目标的技能
    private SkillType _pendingTargetSkill = SkillType.None;
    //对外暴露
    public SkillType PendingTargetSkill => _pendingTargetSkill;

    protected override void Awake()
    {
        base.Awake();
        InitSkillCounts();
    }
#region 初始化
    //初始化技能次数
    private void InitSkillCounts()
    {
        _skillCounts.Clear();

    }
#endregion

#region 对外接口
    /// <summary>
    /// 发放技能次数
    /// 对外接口
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public void GrantSkillUse(SkillType type, int amount)
    {
        if(type == SkillType.None || amount <= 0) return;
        if(!_skillCounts.ContainsKey(type))
        {
            _skillCounts[type] = 0;
        }
        _skillCounts[type] += amount;

        //通知UI刷新技能数量
        EventManager.Broadcast(EventID.OnSkillInventoryChanged);
    }

    /// <summary>
    /// 查询某个技能当前剩余次数
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetSkillCount(SkillType type)
    {
        if(!_skillCounts.TryGetValue(type,out int count)) return 0;
        return count;
    }

    /// <summary>
    /// 点击技能按钮时调用
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool TryUseSkill(SkillType type)
    {
        if(type == SkillType.None) return false;

        //技能次数不足 不能使用
        if(GetSkillCount(type) <= 0)
        {
            EventManager.Broadcast(EventID.OnSkillUseFailed, type);
            return false;
        }
        switch(type)
        {
            case SkillType.UnlockSlot:
                return TryUseUnlockSlot();
            case SkillType.TapKill:
                return EnterTargetMode(SkillType.TapKill);
            case SkillType.DragonRecolor:
                return TryUseDragonRecolor();

            default:
                return false;

        }
    }

    // 针对等待技能     点到方块后调用
    public bool TryHandleBlockClick(BlockView blockView)
    {
        //当前没有等待技能
        if(_pendingTargetSkill == SkillType.None) return false;
        if(blockView == null || blockView.Data == null) return false;
        //等待技能分发
        switch(_pendingTargetSkill)
        {
            case SkillType.TapKill:
                return TryExecuteTapKill(blockView);
            default:
                return false;
        }
    }
#endregion


#region 技能逻辑

    /// <summary>
    /// 解锁槽位技能
    /// </summary>
    /// <returns></returns>
    private bool TryUseUnlockSlot()
    {
        Debug.Log("尝试使用解锁槽位技能");
        EventManager.Broadcast(EventID.OnSkillUseFailed, SkillType.UnlockSlot);
        return false;
    }

    /// <summary>
    /// 龙身变色技能
    /// </summary>
    /// <returns></returns>
    private bool TryUseDragonRecolor()
    {
        Debug.Log("尝试使用龙变色技能");
        EventManager.Broadcast(EventID.OnSkillUseFailed, SkillType.DragonRecolor);
        return false;
    }

    /// <summary>
    /// 执行 点啥消啥技能
    /// </summary>
    /// <param name="blockView"></param>
    /// <returns></returns>
    private bool TryExecuteTapKill(BlockView blockView)
    {
        Debug.Log($"尝试对方块 {blockView.Data.Id} 使用点杀技能");
        ExitTargetMode();
        ConsumeSkill(SkillType.TapKill);
        return true;
    }
    /// <summary>
    /// 进入到等待阶段 适用于 技能需要点击目标方块
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool EnterTargetMode(SkillType type)
    {
        _pendingTargetSkill = type;
        //事件分为 当前有技能进入选中状态
        EventManager.Broadcast(EventID.OnSkillTargetModeChanged, _pendingTargetSkill);
        return true;
    }

    /// <summary>
    /// 退出等待点击目标状态
    /// </summary>
    private void ExitTargetMode()
    {
        _pendingTargetSkill = SkillType.None;
        //事件分发
        EventManager.Broadcast(EventID.OnSkillTargetModeChanged, _pendingTargetSkill);
    }

    /// <summary>
    /// 消耗一次技能
    /// </summary>
    /// <param name="type"></param>
    private void ConsumeSkill(SkillType type)
    {
        if(!_skillCounts.ContainsKey(type))
        {
            return;
        }

        _skillCounts[type] = Math.Max(0, _skillCounts[type] - 1);
        //事件分发
        EventManager.Broadcast(EventID.OnSkillInventoryChanged);

    }
#endregion
}
