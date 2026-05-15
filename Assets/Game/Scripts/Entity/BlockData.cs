using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 方块的数据模型 不负责任何渲染
/// </summary>
public class BlockData 
{
    public int Id;     //该方块的唯一标记
    public BlockType Type; //方块的类型（颜色）
    public BlockSpec Spec;
    // 物理长度
   // 地图占格长度
    public int GridLength;

    // 弹药数量 / 龙身节数
    public int StrengthCount;
    public Direction Dir; // 该方块的朝向

    //二维网格中存放的虚拟坐标 起始点
    public int GridX;
    public int GridY;

    // 构造函数
    public BlockData(int id, BlockType type, BlockSpec spec, Direction dir, int gridX, int gridY)
    {
        this.Id = id;
        this.Type = type;
        this.Spec = spec;
        this.GridLength = spec.gridLength;
        this.StrengthCount = spec.strengthCount;
        this.Dir = dir;
        this.GridX = gridX;
        this.GridY = gridY;
    }

    /// <summary>
    /// 获取当前方块所覆盖的所有格子坐标
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetOccupiedCells()
    {
        var cells = new List<Vector2Int>();
        for (int i = 0; i < this.GridLength; i++)
        {
            switch (this.Dir)
            {
                case Direction.Right:
                    cells.Add(new Vector2Int(this.GridX + i, this.GridY));
                    break;
                case Direction.Left:
                    cells.Add(new Vector2Int(this.GridX - i, this.GridY));
                    break;
                case Direction.Up:
                    cells.Add(new Vector2Int(this.GridX, this.GridY + i));
                    break;
                case Direction.Down:
                    cells.Add(new Vector2Int(this.GridX, this.GridY - i));
                    break;
            }
        }
        return cells;
    }

    /// <summary>
    /// 获取方块车头部位所在的格子坐标 (逃逸时从这里开始向前探查) 
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetHeadPosition()
    {
        Vector2Int head = new Vector2Int(this.GridX, this.GridY);
        switch(this.Dir)
        {
            case Direction.Up: head.y += this.GridLength - 1; break;
            case Direction.Down: head.y -= this.GridLength - 1; break;
            case Direction.Right: head.x += this.GridLength - 1; break;
            case Direction.Left: head.x -= this.GridLength - 1; break;
        }
        return head;
    }
}
