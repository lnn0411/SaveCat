using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        List<Vector2Int> cells = new List<Vector2Int>();
        HashSet<Vector2Int> uniqueCells = new HashSet<Vector2Int>();
        // 起点
        Vector2Int origin = new Vector2Int(this.GridX, this.GridY);
        //方向
        Vector2Int step = DirectionUtility.ToGridVector(this.Dir);

        for(int i = 0; i < GridLength; i++)
        {
            //主格子对角线
            Vector2Int mainCell = origin + step * i;
            AddUnique(uniqueCells, cells, mainCell);
            if(i > 0 && DirectionUtility.IsDiagonal(this.Dir))
            {
                //上一个点
                Vector2Int previous = origin + step * (i - 1);
                //对角线从previous到mainCell 会盖住2&2中另外两格
                AddUnique(uniqueCells, cells, new Vector2Int(previous.x, mainCell.y));
                AddUnique(uniqueCells, cells, new Vector2Int(mainCell.x, previous.y));
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
        Vector2Int origin = new Vector2Int(GridX, GridY);
        Vector2Int step = DirectionUtility.ToGridVector(Dir);
        return origin + step * (GridLength - 1);
    }


    #region 私人方法
    // 增加经过点
    private void AddUnique(HashSet<Vector2Int> unique, List<Vector2Int> cells, Vector2Int cell)
    {
        //未经过
        if(unique.Add(cell))
        {
            cells.Add(cell);
        }
    }

    #endregion
}
