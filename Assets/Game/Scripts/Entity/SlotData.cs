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
    }
#endregion

}
