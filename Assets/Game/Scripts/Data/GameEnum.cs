using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//龙部位颜色
public enum BlockType
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    None //无颜色（空槽位）
}

//龙前进的状态
public enum DragonState 
{ 
    Chasing, 
    Pacing, 
    GameOver 
}

// 方块的状态
public enum Direction
{
    Up,
    UpRight,
    Right,
    DownRight,
    Down,
    DownLeft,
    Left,
    UpLeft
}


// 事件对应的标识符
public enum EventID
{
    // ------------------方块与低槽------------------
    OnBlockEscapeSuccess = 1000, //逃逸成功的方块
    OnBlockSlotted = 1001, //落入槽位的颜色

    OnSlotFull = 1002, //槽位满了



    // ------------------战斗消灭------------------
    OnDragonSegmentHit = 2000, //龙被击中的节段


    //------------------关卡生命周期------------------
    OnGameOver = 3000, //游戏结束
    OnLevelVictory = 3001, //关卡胜利

    //------------------技能系统------------------
    OnSkillUseFailed = 4000, //技能使用失败
    OnSkillInventoryChanged = 4001, //技能数量发生变化
    OnSlotUnlocked = 4002, //解锁对应的格子
    OnSkillTargetModeChanged = 4003, //是否进入等待点击目标的状态

}

//关卡状态
public enum LevelState
{
    None,
    Playing,
    win,
    Lose,
    Resetting
}

//游戏失败原因
public enum GmaeFailReason
{
    None,
    DragonReachedCat
}

//逃逸路线的四条车道
public enum EscapeLane
{
    Top = 0,
    Right = 1,
    Bottom = 2,
    Left = 3
}

// 技能枚举
public enum SkillType
{
    None,
    UnlockSlot,
    TapKill,
    DragonRecolor
}