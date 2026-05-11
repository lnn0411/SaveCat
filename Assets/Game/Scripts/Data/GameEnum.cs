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
    Down,
    Left,
    
    Right
}


// 事件对应的标识符
public enum EventID
{
    // ------------------方块与低槽------------------
    OnBlockEscapeSuccess = 1000, //逃逸成功的方块
    OnBlockSlotted = 1001, //落入槽位的颜色



    // ------------------战斗消灭------------------
    OnDragonSegmentHit = 2000, //龙被击中的节段


    //------------------关卡生命周期------------------
    OnGameOver = 3000, //游戏结束
    OnLevelVictory = 3001, //关卡胜利
}

//