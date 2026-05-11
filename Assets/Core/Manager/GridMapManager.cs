using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 逻辑数据地图管理器，负责维护网格数组，以及运算无物理引擎的碰撞解堵判定
/// </summary>
public class GridMapManager : Singleton<GridMapManager>
{
    //地图横向格子数
    private int width;
    // 地图纵向格子数
    private int height;

    // 核心数据结构：0表示空，其余则为Block.Id
    private int[,] mapGrid;

    //用于快速通过Id查找到方块身上的数据
    private Dictionary<int, BlockData> allBlocksData = new Dictionary<int, BlockData>();

#region 对外接口
    /// <summary>
    /// 初始化地图尺寸
    /// </summary>
    /// <param name="gridWidth"></param>
    /// <param name="gridHeight"></param>
    public void InitMap(int gridWidth, int gridHeight)
    {
        width = gridWidth;
        height = gridHeight;
        mapGrid = new int[width, height];
        allBlocksData.Clear();
        Debug.Log($"GridMapManager initialized with size {width}x{height}");
    }

    /// <summary>
    /// 当生成了一个新的方快时， 记录数据
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    public bool RegisterBlock(BlockData block)
    {
        // 防止越界 或者已经被占用
        foreach(Vector2Int cell in block.GetOccupiedCells())
        {
            if(!isCellValid(cell.x, cell.y))
            {
                Debug.LogError($"Block {block.Id} has invalid cell at ({cell.x}, {cell.y})");
                return false;
            }

            if(mapGrid[cell.x, cell.y] != 0)
            {
                Debug.LogError($"Block {block.Id} overlaps with existing block at ({cell.x}, {cell.y})");
                return false;
            }
        }

        // 注册方块数据
        foreach(Vector2Int cell in block.GetOccupiedCells())
        {
            mapGrid[cell.x, cell.y] = block.Id;
        }
        allBlocksData[block.Id] = block;
        return true;
    }

    /// <summary>
    /// 玩家点击方块 判断是否可以逃离
    /// </summary>
    /// <param name="blockId"></param>
    /// <returns></returns>
    public bool CanBlockEscape(int blockId, out int availableSteps)
    {
        //记录前面有几个空地
        availableSteps = 0;
        if(!allBlocksData.TryGetValue(blockId, out BlockData block))
        {
            Debug.LogError($"Block with ID {blockId} not found!");
            return false;
        }

        //拿到方块车头所在的格子（逃逸时从这里开始向前探查）
        Vector2Int headPos = block.GetHeadPosition();

        //计算下一步方向的矢量
        Vector2Int stepDir = GetDirVector(block.Dir);

        //从车头前一格开始，一路向前推进
        Vector2Int nextPos = headPos + stepDir;

        //开始逃逸
        while(isCellValid(nextPos.x, nextPos.y))
        {
            

            if(mapGrid[nextPos.x, nextPos.y] != 0)
            {
                //前方有阻挡，无法逃离
                return false;
            }
            nextPos += stepDir;
            availableSteps++;
        }
        // 如果推进到地图边界，说明可以逃离
        return true;
    }

    /// <summary>
    /// 方块逃逸后 要从地图数据中移除该方块的占位信息
    /// </summary>
    /// <param name="blockId"></param>
    public void RemoveBlockFromMap(int blockId)
    {
        if(!allBlocksData.TryGetValue(blockId, out BlockData block))
        {
            Debug.LogError($"Block with ID {blockId} not found for removal!");
            return;
        }

        foreach(Vector2Int cell in block.GetOccupiedCells())
        {
            mapGrid[cell.x, cell.y] = 0; // 清空占位
        }
        allBlocksData.Remove(blockId); // 从字典中移除数据
    }


    #region 提供给数值生成器的查询接口
    // 查阅这几个格子是不是全都是空的(0)
    public bool IsCellsEmpty(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            if (mapGrid[cell.x, cell.y] != 0) return false;
        }
        return true;
    }

    // 专门为“发牌测试”使用的一个重载，直接传入待定的 BlockData 看能否飞出，而不依赖已注册的 allBlocksData
    public bool CanBlockEscape(BlockData block)
    {
        Vector2Int headPos = block.GetHeadPosition();
        Vector2Int stepDir = GetDirVector(block.Dir);
        Vector2Int nextPos = headPos + stepDir;

        while (isCellValid(nextPos.x, nextPos.y))
        {
            if(mapGrid[nextPos.x, nextPos.y] != 0) return false;
            nextPos += stepDir;
        }
        return true;
    }
    #endregion
#endregion

#region 辅助工具
    /// 判断一个格子是否在地图范围内
    private bool isCellValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //方向转换为数值增量
    private Vector2Int GetDirVector(Direction dir)
    {
        return dir switch
        {
            Direction.Up => new Vector2Int(0, 1),
            Direction.Down => new Vector2Int(0, -1),
            Direction.Left => new Vector2Int(-1, 0),
            Direction.Right => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };
    }
    #endregion


    #region 辅助可视化 (仅在编辑器下可见)
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 只有游戏运行且地图初始化后才绘制
        if (!Application.isPlaying || mapGrid == null) return;

        float cellSize = 1.0f; // 与你生成方块时的 cellSize 保持一致

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 计算格子的世界中心坐标
                Vector3 cellCenter = new Vector3(x * cellSize, 0, y * cellSize);

                // 如果这个格子是空的，画一个白色的线框
                if (mapGrid[x, y] == 0)
                {
                    Gizmos.color = new Color(1, 1, 1, 0.3f);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0.1f, cellSize));
                }
                else
                {
                    // 如果这个格子被占用了，画一个红色的半透明实心块
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));

                    // 还可以顺便把占用的方块ID画出来 (仅限Editor下)
                    UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.5f, mapGrid[x, y].ToString());
                }
            }
        }
    }
#endif
#endregion
}
