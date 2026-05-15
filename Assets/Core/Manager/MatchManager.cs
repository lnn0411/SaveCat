using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗匹配中心。
/// 负责把底槽弹药和龙节段进行颜色匹配，并按固定频率自动攻击。
/// 当前版本先不做弹道表现，只做延迟后的逻辑命中。
/// </summary>
public class MatchManager : Singleton<MatchManager>
{
    #region Inspector Config

    [Header("Attack Timing")]
    [SerializeField] private float firstShotDelay = 0.25f;
    [SerializeField] private float slotShotInterval = 0.5f;

    [SerializeField] private float sameColorLockDuration = 0.12f;

    private readonly Dictionary<BlockType, float> _colorLockUntil = new Dictionary<BlockType, float>();


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
        if(SlotManager.Instance == null || LevelManager.Instance == null) return;

        for(int i = 0; i < SlotManager.Instance.MaxSlotCount; i++)
        {
            // 检查每个槽位的攻击时机
            TickSlot(i);
        }
        
    }

    #endregion


    #region Match Flow

    // 处理每个槽位的攻击时机
    private void TickSlot(int slotIndex)
    {
        if(!SlotManager.Instance.TryGetAmmoAt(slotIndex, out SlotData slotData)) return;

        //如果还未激活
        if(!slotData.AttackActive) return;

        if(Time.time < slotData.NextAttackTime) return;

        //尝试攻击
        TryAttackFromSlot(slotIndex, slotData);
        slotData.NextAttackTime += slotShotInterval;
    }
    //某个槽位真正攻击一次 只有命中成功才扣弹药；同色锁定期间不攻击
    private bool TryAttackFromSlot(int slotIndex, SlotData slotData)
    {
        BlockType color = slotData.ColorType;
        //检查同色锁定
        if(_colorLockUntil.TryGetValue(color, out float lockUntil) && Time.time < lockUntil)
        {
            return false; //同色锁定中，攻击无效
        }
        //是否还有攻击目标
        if(!DragonManager.Instance.HasAliveSegment()) return false;

        //找到靠前的目标颜色
        if(!DragonManager.Instance.TryFindFrontMostSegment(color, out DragonSegmentView targetSegment)) return false;
        
        //是否可以命中龙身
        if(!DragonManager.Instance.TryHitSegment(targetSegment)) return false;
        //命中成功 扣弹药
        SlotManager.Instance.TryConsumeAmmoAt(slotIndex);
        // 颜色锁
        _colorLockUntil[color] = Time.time + sameColorLockDuration;
        return true;
    }

    #endregion


    #region Event Handlers

    /// <summary>
    /// 此时动画已经结束 弹药已经入槽，传入的是槽位index
    /// </summary>
    private void OnBlockSlotted(int slotIndex)
    {
        if(SlotManager.Instance == null) return;

        // 获取槽位数据
        if(!SlotManager.Instance.TryGetAmmoAt(slotIndex, out SlotData slotData))
        {
            return;
        }
        // 计算该槽位的攻击时机     利用索引计算一个相位偏移，保证每个槽位的攻击时机错开，增加节奏感
        float phaseOffset = slotIndex * slotShotInterval / SlotManager.Instance.MaxSlotCount;
        slotData.AttackPhaseOffset = phaseOffset;
        slotData.AttackActive = true;
        slotData.NextAttackTime = Time.time + firstShotDelay + phaseOffset;
    }




    #endregion
}
