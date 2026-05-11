using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 纯数据结构 记录一个槽位中的当前情况
public class SlotData 
{
    public bool IsEmpty = true;

    //当前拥有的颜色
    public BlockType ColorType;

    public int StrengthCount;

    //清空该数据槽位
    public void Clear()
    {
        IsEmpty = true;
        StrengthCount = 0;
        ColorType = BlockType.None;
    }

    //装填弹药
    public void Load(BlockType color, int amount)
    {
        IsEmpty = false;
        ColorType = color;
        StrengthCount = amount;
        //可以有特效
    }
}
