using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DirectionUtility
{
    public const int DirectionCount = 8;

    public static Vector2Int ToGridVector(Direction dir)
    {
        return dir switch
        {
            Direction.Up => new Vector2Int(0,1),
            Direction.UpRight => new Vector2Int(1,1),
            Direction.Right => new Vector2Int(1,0),
            Direction.DownRight => new Vector2Int(1,-1),
            Direction.Down => new Vector2Int(0,-1),
            Direction.DownLeft => new Vector2Int(-1,-1),
            Direction.Left => new Vector2Int(-1,0),
            Direction.UpLeft => new Vector2Int(-1,1),
            _ => Vector2Int.zero
        };
    }

    // 判断是否是斜向
    public static bool IsDiagonal(Direction dir)
    {
        return dir == Direction.UpRight
        || dir == Direction.DownRight
        || dir == Direction.DownLeft
        || dir == Direction.UpLeft;
    }

    // 获取世界坐标方向
    public static Vector3 ToWorldVector(Direction dir)
    {
        Vector2Int grid = ToGridVector(dir);
        Vector3 world = new Vector3(grid.x, 0, grid.y);
        return world.sqrMagnitude > 0f? world.normalized : Vector3.zero;
    }

    // 获取旋转
    public static Quaternion ToRotation(Direction dir)
    {
        return Quaternion.LookRotation(ToWorldVector(dir), Vector3.up);
    }
}
