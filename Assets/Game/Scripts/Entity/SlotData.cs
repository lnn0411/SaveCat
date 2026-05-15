using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 纯数据结构 记录一个槽位中的当前情况
public class SlotData 
{
#region UI相关的数据逻辑
    public bool IsEmpty = true;
    public bool IsReserved = false; //是否被预定了 预定了就不能再放入了 直到发射了才清除预定状态
    //当前拥有的颜色
    public BlockType ColorType;
    public int StrengthCount;

    //清空该数据槽位
    public void Clear()
    {
        IsEmpty = true;
        IsReserved = false;
        StrengthCount = 0;
        ColorType = BlockType.None;

        AttackActive = false;
        NextAttackTime = 0f;
    }

    //装填弹药
    public void Load(BlockType color, int amount)
    {
        IsEmpty = false;
        IsReserved = false;
        ColorType = color;
        StrengthCount = amount;
    }
    // 预定
    public void Reserve()
    {
        IsEmpty = true;
        IsReserved = true;
        StrengthCount = 0;
        ColorType = BlockType.None;
        
        AttackActive = false;
        NextAttackTime = 0f;
    }
#endregion

#region 战斗相关的逻辑
    //战斗是否激活       预定不激活
    public bool AttackActive = false;
    // 下一次攻击时间
    public float NextAttackTime = 0f;
    // 攻击间隔
    public float AttackPhaseOffset = 0f;
#endregion
}
