using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

// 以颜色为主的攻击系统
public class ColorAttackState
{
    public bool IsActive;
    public float NextAttackTime;
    public float PhaseOffset;
}
/// <summary>
/// 战斗匹配中心。
/// 负责把底槽弹药和龙节段进行颜色匹配，并按固定频率自动攻击。
/// 当前版本先不做弹道表现，只做延迟后的逻辑命中。
/// </summary>
public class MatchManager : Singleton<MatchManager>
{
    #region Inspector Config

    [Header("Attack Timing")]
    [SerializeField] private float firstShotDelay = 0.25f; //第一次攻击的延迟
    [SerializeField] private float colorAttackInterval = 0.8f; //同一颜色的攻击间隔

    // 以颜色为主的攻击状态字典
    private readonly Dictionary<BlockType, ColorAttackState> _colorStates =
        new Dictionary<BlockType, ColorAttackState>();

    [Header("Color length")]
    [SerializeField] private int colorCount = 5; //颜色数量 

    #endregion


    #region Unity Lifecycle

    private void OnEnable()
    {
        EventManager.AddListener<int>(EventID.OnBlockSlotted, OnBlockSlotted);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<int>(EventID.OnBlockSlotted, OnBlockSlotted);
    }

    private void Update()
    {
        if(SlotManager.Instance == null || DragonManager.Instance == null) return;

        for(int i = 0; i < BlockColorUtility.AttackColors.Length; i++)
        {
            TickColor(BlockColorUtility.AttackColors[i]);
        }
        
    }

    #endregion
    #region Initialization
    /// 获取或创建一个颜色状态
    private ColorAttackState GetOrCrateColorState(BlockType color)
    {
        //不存在
        if(!_colorStates.TryGetValue(color, out ColorAttackState state))
        {
            state = new ColorAttackState
            {
                IsActive = false,
                PhaseOffset = GetColorPhaseOffset(color)
            };

            _colorStates[color] = state;
        }
        return state;
    }
    //错峰计算
    private float GetColorPhaseOffset(BlockType color)
    {
        return (int)color * colorAttackInterval / colorCount;
    }


    #endregion

    #region Match Flow
    // 只处理某一种颜色的攻击节奏
    private void TickColor(BlockType color)
    {
        if(!_colorStates.TryGetValue(color, out ColorAttackState state)) return;
        if(!state.IsActive) return;
        //未到时间
        if(Time.time < state.NextAttackTime) return;
        bool didAttack = TryAttackColor(color);
        if(didAttack)
        {
            state.NextAttackTime += colorAttackInterval;
        }
        else
        {
            //没弹药了 或者 没有对应颜色的龙节了 都停掉这个颜色的攻击 等下一次入弹药再激活
            if (!SlotManager.Instance.TryGetBestAmmoSlotByColor(color, out _, out _))
            {
                state.IsActive = false;
            }
            else
            {
                state.NextAttackTime += colorAttackInterval;
            }
        }
    }
    //执行某个颜色的一次攻击
    private bool TryAttackColor(BlockType color)
    {
        // 龙身体是否还有
        if(!DragonManager.Instance.HasAliveSegment()) return false;
        // 龙对应颜色是否存在
        if(!DragonManager.Instance.TryFindFrontMostSegment(color, out DragonSegmentView targetSegment))
        {
            return false;
        }
        // 哪个颜色对应的槽位应该攻击
        if(!SlotManager.Instance.TryGetBestAmmoSlotByColor(color, out int slotIndex, out SlotData slotData))
        {
            return false;
        }

        // 攻击命中
        if(!DragonManager.Instance.TryHitSegment(targetSegment)) return false;
        //消耗对应弹药
        SlotManager.Instance.TryConsumeAmmoAt(slotIndex);
        return true;

    }


    #endregion


    #region Event Handlers

    /// <summary>
    /// 此时动画已经结束 弹药已经入槽，传入的是槽位index,要开打了
    /// </summary>
    private void OnBlockSlotted(int slotIndex)
    {
        if(SlotManager.Instance == null) return;

        // 获取槽位数据
        if(!SlotManager.Instance.TryGetAmmoAt(slotIndex, out SlotData slotData))
        {
            return;
        }
        BlockType color = slotData.ColorType;
        if(color == BlockType.None) return;
        // 获取或创建颜色状态
        ColorAttackState state = GetOrCrateColorState(color);
        // 颜色已经激活了 就不重复激活了
        if(state.IsActive) return;
        state.IsActive = true;

        state.NextAttackTime = Time.time + firstShotDelay + state.PhaseOffset;
    }




    #endregion
}
