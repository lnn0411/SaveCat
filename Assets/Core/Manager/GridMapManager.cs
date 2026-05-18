using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        return CanBlockEscapeBySweptFootprint(block, out availableSteps);
    }

    /// <summary>
    /// 生成外环路线
    /// </summary>
    /// <param name="blockId">方块ID</param>
    /// <param name="targetWorldPoint">目标投影点</param>
    /// <param name="outsidePadding">距离棋盘有多远</param>
    /// <param name="cellSize">方块大小</param>
    /// <param name="pathPoints">路径点</param>
    /// <returns></returns>
    public bool TryBuildEscapePath(
        int blockId,
        Vector3 targetWorldPoint,
        float outsidePadding,
        float cellSize,
        out Vector3[] pathPoints)
    {
        pathPoints = null;
        //通过ID取到对应的方块数据
        if (!allBlocksData.TryGetValue(blockId, out BlockData block))
        {
            Debug.LogError($"[GridMapManager] Block with ID {blockId} not found when building escape path.");
            return false;
        }
        // 是否可以逃脱
        if (!CanBlockEscape(blockId, out _))
        {
            return false;
        }
        // 四条边生成
        GetOuterRingBounds(
            cellSize,
            outsidePadding,
            out float leftLaneX,
            out float rightLaneX,
            out float bottomLaneZ,
            out float topLaneZ
        );
        //将方块本身方向转换为逃跑路线方向
        if (!TryGetEscapeExit(
            block,
            cellSize,
            leftLaneX,
            rightLaneX,
            bottomLaneZ,
            topLaneZ,
            out EscapeLane startLane,
            out Vector3 startPoint))
        {
            return false;
        }
        // 外围的起始点坐标 棋盘逃逸的终点
        //获得外围的终极坐标targetOnRing 方块的目标点 以及它落在哪条车道上
        EscapeLane targetLane = ProjectToClosestLane(
            targetWorldPoint,
            leftLaneX,
            rightLaneX,
            bottomLaneZ,
            topLaneZ,
            out Vector3 targetOnRing
        );

        // 顺时针路径和逆时针路径
        List<Vector3> clockwisePath = BuildRingPath(startPoint, startLane, targetOnRing, targetLane, 1, leftLaneX, rightLaneX, bottomLaneZ, topLaneZ);
        List<Vector3> counterPath = BuildRingPath(startPoint, startLane, targetOnRing, targetLane, -1, leftLaneX, rightLaneX, bottomLaneZ, topLaneZ);

        // 找到哪个方向最近
        float clockwiseDistance = GetPathDistance(clockwisePath);
        float counterDistance = GetPathDistance(counterPath);
        // 选择了最近的路径
        List<Vector3> selectedPath = SelectShorterRingPath(clockwisePath, clockwiseDistance, counterPath, counterDistance, rightLaneX);
        // 去除相邻的点
        RemoveAdjacentDuplicates(selectedPath);
        // 非法路径报错
        for (int i = 0; i < selectedPath.Count; i++)
        {
            if (!IsOutsideBoard(selectedPath[i], cellSize))
            {
                Debug.LogWarning($"[GridMapManager] Escape path point {selectedPath[i]} is inside the board.");
                return false;
            }
        }

        pathPoints = selectedPath.ToArray();
        return pathPoints.Length > 0;
    }

    /// <summary>
    /// 当Block向前移动撞到其他Block时，通知被撞Block身上的Effect
    /// </summary>
    public void TryNotifyForwardBlockingBlockHit(int movingBlockId)
    {
        if (!allBlocksData.TryGetValue(movingBlockId, out BlockData movingBlock))
        {
            return;
        }

        if (!TryGetFirstBlockingBlockOnForwardMove(movingBlock, out int blockingBlockId, out _))
        {
            return;
        }

        if (!allBlocksData.TryGetValue(blockingBlockId, out BlockData blockingBlock))
        {
            return;
        }

        foreach (var effect in blockingBlock.GetEffects())
        {
            effect.OnHitByOtherBlock(movingBlock);
        }
    }

    /// <summary>
    /// 兼容旧调用名：这里的Block不会逃逸，只会尝试撞击前方阻挡Block并触发解冻。
    /// </summary>
    public void TryUnfreezeBlockingBlock(int movingBlockId)
    {
        TryNotifyForwardBlockingBlockHit(movingBlockId);
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

    /// <summary>
    /// 兼容旧接口名。实际逻辑是检查是否存在其他Block向前移动时会撞到此Block。
    /// </summary>
    public bool HasBlockCanHitThis(BlockData block)
    {
        return CanBeHitByOtherBlocks(block);
    }

    /// <summary>
    /// 检查是否存在未被忽略的其他Block，向自身方向移动时第一个撞到的Block就是目标Block。
    /// </summary>
    /// <param name="block">要检查的方块</param>
    /// <returns>如果可以被撞击，返回true；否则返回false</returns>
    public bool CanBeHitByOtherBlocks(BlockData block, HashSet<int> ignoredMovingBlockIds = null)
    {
        if (block == null)
        {
            return false;
        }

        foreach (var otherBlock in allBlocksData.Values)
        {
            if (otherBlock.Id == block.Id)
            {
                continue;
            }

            if (ignoredMovingBlockIds != null && ignoredMovingBlockIds.Contains(otherBlock.Id))
            {
                continue;
            }

            if (TryGetFirstBlockingBlockOnForwardMove(otherBlock, out int blockingBlockId, out _) &&
                blockingBlockId == block.Id)
            {
                return true;
            }
        }

        return false;
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
        return CanBlockEscapeBySweptFootprint(block, out _);
    }

    private bool CanBlockEscapeBySweptFootprint(BlockData block, out int availableSteps)
    {
        availableSteps = 0;

        Vector2Int step = DirectionUtility.ToGridVector(block.Dir);
        if (step == Vector2Int.zero)
        {
            return false;
        }

        List<Vector2Int> baseCells = block.GetOccupiedCells();
        int maxMoveSteps = width + height + block.GridLength + 4;

        for (int moveStep = 1; moveStep <= maxMoveSteps; moveStep++)
        {
            bool hasAnyCellInsideBoard = false;
            Vector2Int offset = step * moveStep;

            for (int i = 0; i < baseCells.Count; i++)
            {
                Vector2Int movedCell = baseCells[i] + offset;

                if (!isCellValid(movedCell.x, movedCell.y))
                {
                    continue;
                }

                hasAnyCellInsideBoard = true;

                int occupiedBy = mapGrid[movedCell.x, movedCell.y];
                if (occupiedBy != 0 && occupiedBy != block.Id)
                {
                    return false;
                }
            }

            availableSteps = moveStep;

            if (!hasAnyCellInsideBoard)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetFirstBlockingBlockOnForwardMove(BlockData block, out int blockingBlockId, out int availableSteps)
    {
        blockingBlockId = -1;
        availableSteps = 0;

        if (block == null)
        {
            return false;
        }

        Vector2Int step = DirectionUtility.ToGridVector(block.Dir);
        if (step == Vector2Int.zero)
        {
            return false;
        }

        List<Vector2Int> baseCells = block.GetOccupiedCells();
        int maxMoveSteps = width + height + block.GridLength + 4;

        for (int moveStep = 1; moveStep <= maxMoveSteps; moveStep++)
        {
            Vector2Int offset = step * moveStep;

            for (int i = 0; i < baseCells.Count; i++)
            {
                Vector2Int movedCell = baseCells[i] + offset;

                if (!isCellValid(movedCell.x, movedCell.y))
                {
                    continue;
                }

                int occupiedBy = mapGrid[movedCell.x, movedCell.y];
                if (occupiedBy != 0 && occupiedBy != block.Id)
                {
                    blockingBlockId = occupiedBy;
                    return true;
                }
            }

            availableSteps = moveStep;
        }

        return false;
    }
    #endregion
#endregion

#region 辅助工具
    /// 判断一个格子是否在地图范围内
    private bool isCellValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //生成对应路线
    private void GetOuterRingBounds(float cellSize,float outsidePadding,out float leftLaneX,out float rightLaneX,out float bottomLaneZ,out float topLaneZ)
    {
        // 地图x最小值
        float boardMinX = 0f;
        // 地图x最大值
        float boardMaxX = (width - 1) * cellSize;
        // 地图Z最小值
        float boardMinZ = 0f;
        // 地图Z最大值
        float boardMaxZ = (height - 1) * cellSize;
        // 在四个点的基础上 往外扩展outsidePadding的距离 就得到了外环的四条边界线坐标
        leftLaneX = boardMinX - outsidePadding;
        rightLaneX = boardMaxX + outsidePadding;
        bottomLaneZ = boardMinZ - outsidePadding;
        topLaneZ = boardMaxZ + outsidePadding;
    }

    // 方向转换成逃逸跑道
    private EscapeLane GetEscapeLane(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return EscapeLane.Top;
            case Direction.Down:
                return EscapeLane.Bottom;
            case Direction.Left:
                return EscapeLane.Left;
            case Direction.Right:
                return EscapeLane.Right;
            default:
                return EscapeLane.Bottom;
        }
    }

    private bool TryGetEscapeExit(
        BlockData block,
        float cellSize,
        float leftLaneX,
        float rightLaneX,
        float bottomLaneZ,
        float topLaneZ,
        out EscapeLane lane,
        out Vector3 point)
    {
        lane = EscapeLane.Bottom;
        point = Vector3.zero;

        Vector2Int step = DirectionUtility.ToGridVector(block.Dir);
        if (step == Vector2Int.zero)
        {
            return false;
        }

        Vector2Int current = block.GetHeadPosition();
        Vector2Int next = current + step;

        while (isCellValid(next.x, next.y))
        {
            current = next;
            next += step;
        }

        Vector3 currentWorld = new Vector3(current.x * cellSize, 0f, current.y * cellSize);
        Vector3 nextWorld = new Vector3(next.x * cellSize, 0f, next.y * cellSize);
        Vector3 direction = nextWorld - currentWorld;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        direction.Normalize();

        float bestT = float.PositiveInfinity;
        EscapeLane bestLane = EscapeLane.Bottom;
        Vector3 bestPoint = Vector3.zero;

        TryCandidateLane(currentWorld, direction, EscapeLane.Right, rightLaneX, true, bottomLaneZ, topLaneZ, ref bestT, ref bestLane, ref bestPoint);
        TryCandidateLane(currentWorld, direction, EscapeLane.Left, leftLaneX, true, bottomLaneZ, topLaneZ, ref bestT, ref bestLane, ref bestPoint);
        TryCandidateLane(currentWorld, direction, EscapeLane.Top, topLaneZ, false, leftLaneX, rightLaneX, ref bestT, ref bestLane, ref bestPoint);
        TryCandidateLane(currentWorld, direction, EscapeLane.Bottom, bottomLaneZ, false, leftLaneX, rightLaneX, ref bestT, ref bestLane, ref bestPoint);

        if (float.IsPositiveInfinity(bestT))
        {
            return false;
        }

        lane = bestLane;
        point = bestPoint;
        return true;
    }

    private void TryCandidateLane(
        Vector3 origin,
        Vector3 direction,
        EscapeLane lane,
        float laneCoord,
        bool isVerticalLane,
        float minOtherCoord,
        float maxOtherCoord,
        ref float bestT,
        ref EscapeLane bestLane,
        ref Vector3 bestPoint)
    {
        const float epsilon = 0.0001f;

        if (isVerticalLane)
        {
            if (Mathf.Abs(direction.x) < epsilon)
            {
                return;
            }

            float t = (laneCoord - origin.x) / direction.x;
            if (t <= epsilon || t >= bestT)
            {
                return;
            }

            float z = origin.z + direction.z * t;
            if (z < minOtherCoord - epsilon || z > maxOtherCoord + epsilon)
            {
                return;
            }

            bestT = t;
            bestLane = lane;
            bestPoint = new Vector3(laneCoord, 0f, Mathf.Clamp(z, minOtherCoord, maxOtherCoord));
            return;
        }

        if (Mathf.Abs(direction.z) < epsilon)
        {
            return;
        }

        float horizontalT = (laneCoord - origin.z) / direction.z;
        if (horizontalT <= epsilon || horizontalT >= bestT)
        {
            return;
        }

        float x = origin.x + direction.x * horizontalT;
        if (x < minOtherCoord - epsilon || x > maxOtherCoord + epsilon)
        {
            return;
        }

        bestT = horizontalT;
        bestLane = lane;
        bestPoint = new Vector3(Mathf.Clamp(x, minOtherCoord, maxOtherCoord), 0f, laneCoord);
    }
    /// <summary>
    /// 获取起始点坐标
    /// </summary>
    /// <param name="block">方块数据</param>
    /// <param name="lane">逃跑路线</param>
    /// <param name="cellSize">格子大小</param>
    /// <param name="leftLaneX">左车道X坐标</param>
    /// <param name="rightLaneX">右车道X坐标</param>
    /// <param name="bottomLaneZ">底车道Z坐标</param>
    /// <param name="topLaneZ">顶车道Z坐标</param>
    /// <returns></returns>
    private Vector3 GetLaneStartPoint(BlockData block,EscapeLane lane,float cellSize,float leftLaneX,float rightLaneX,float bottomLaneZ,float topLaneZ)
    {   
        //获得逃逸起始点
        Vector2Int headCell = block.GetHeadPosition();
        float x = headCell.x * cellSize;
        float z = headCell.y * cellSize;
        // 根据逃跑路线选择起始点
        switch (lane)
        {
            case EscapeLane.Top:
                return new Vector3(x, 0f, topLaneZ);
            case EscapeLane.Bottom:
                return new Vector3(x, 0f, bottomLaneZ);
            case EscapeLane.Left:
                return new Vector3(leftLaneX, 0f, z);
            case EscapeLane.Right:
                return new Vector3(rightLaneX, 0f, z);
            default:
                return new Vector3(x, 0f, bottomLaneZ);
        }
    }

    // 近似计算目标点在外环上的投影点，并判断落在哪条车道上
    private EscapeLane ProjectToClosestLane(Vector3 targetWorldPoint,float leftLaneX,float rightLaneX,float bottomLaneZ,float topLaneZ,out Vector3 pointOnRing)
    {
        // 计算出当前目标点距离外环的各边的距离
        float leftDistance = Mathf.Abs(targetWorldPoint.x - leftLaneX);
        float rightDistance = Mathf.Abs(targetWorldPoint.x - rightLaneX);
        float bottomDistance = Mathf.Abs(targetWorldPoint.z - bottomLaneZ);
        float topDistance = Mathf.Abs(targetWorldPoint.z - topLaneZ);

        EscapeLane lane = EscapeLane.Bottom;
        float bestDistance = bottomDistance;
        // 比较得到最近的path
        if (topDistance < bestDistance)
        {
            lane = EscapeLane.Top;
            bestDistance = topDistance;
        }
        if (leftDistance < bestDistance)
        {
            lane = EscapeLane.Left;
            bestDistance = leftDistance;
        }
        if (rightDistance < bestDistance)
        {
            lane = EscapeLane.Right;
        }
        // 以Top为例 如果x在范围之内，则保留对应的x       y为Top跑道的值
        switch (lane)
        {
            case EscapeLane.Top:
                pointOnRing = new Vector3(Mathf.Clamp(targetWorldPoint.x, leftLaneX, rightLaneX), 0f, topLaneZ);
                break;
            case EscapeLane.Bottom:
                pointOnRing = new Vector3(Mathf.Clamp(targetWorldPoint.x, leftLaneX, rightLaneX), 0f, bottomLaneZ);
                break;
            case EscapeLane.Left:
                pointOnRing = new Vector3(leftLaneX, 0f, Mathf.Clamp(targetWorldPoint.z, bottomLaneZ, topLaneZ));
                break;
            case EscapeLane.Right:
                pointOnRing = new Vector3(rightLaneX, 0f, Mathf.Clamp(targetWorldPoint.z, bottomLaneZ, topLaneZ));
                break;
            default:
                pointOnRing = targetWorldPoint;
                break;
        }

        return lane;
    }

    // 计算路径 不停的找到拐弯处
    private List<Vector3> BuildRingPath(Vector3 startPoint,EscapeLane startLane,Vector3 targetPoint,EscapeLane targetLane,
        int step,float leftLaneX,float rightLaneX,float bottomLaneZ,float topLaneZ)
    {
        // 棋盘的出口算第一个路径点
        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint);

        EscapeLane currentLane = startLane;
        //跑到目标车道
        while (currentLane != targetLane)
        {
            EscapeLane nextLane = GetNextLane(currentLane, step);
            points.Add(GetCornerPoint(currentLane, nextLane, leftLaneX, rightLaneX, bottomLaneZ, topLaneZ));
            currentLane = nextLane;
        }

        points.Add(targetPoint);
        return points;
    }
    // 找到下一条跑道
    private EscapeLane GetNextLane(EscapeLane lane, int step)
    {
        int next = ((int)lane + step + 4) % 4;
        return (EscapeLane)next;
    }

    // 获取两个跑道中的拐角处位置
    private Vector3 GetCornerPoint(EscapeLane from,EscapeLane to,float leftLaneX,float rightLaneX,float bottomLaneZ,float topLaneZ)
    {
        if (IsLanePair(from, to, EscapeLane.Top, EscapeLane.Right))
        {
            return new Vector3(rightLaneX, 0f, topLaneZ);
        }
        if (IsLanePair(from, to, EscapeLane.Right, EscapeLane.Bottom))
        {
            return new Vector3(rightLaneX, 0f, bottomLaneZ);
        }
        if (IsLanePair(from, to, EscapeLane.Bottom, EscapeLane.Left))
        {
            return new Vector3(leftLaneX, 0f, bottomLaneZ);
        }
        if (IsLanePair(from, to, EscapeLane.Left, EscapeLane.Top))
        {
            return new Vector3(leftLaneX, 0f, topLaneZ);
        }

        return Vector3.zero;
    }

    private bool IsLanePair(EscapeLane a, EscapeLane b, EscapeLane first, EscapeLane second)
    {
        return (a == first && b == second) || (a == second && b == first);
    }

    // 获得路径的距离
    private float GetPathDistance(List<Vector3> points)
    {
        float distance = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            distance += Vector3.Distance(points[i - 1], points[i]);
        }
        return distance;
    }

    // 选一条最短的路径 如果两条路径长度相同 则优先选那个经过右车道的（因为视觉上更贴近玩家）
    private List<Vector3> SelectShorterRingPath(List<Vector3> clockwisePath,float clockwiseDistance,List<Vector3> counterPath,
        float counterDistance,float rightLaneX)
    {
        if (Mathf.Abs(clockwiseDistance - counterDistance) <= 0.001f)
        {
            return PathTouchesRightLane(counterPath, rightLaneX) ? counterPath : clockwisePath;
        }

        return clockwiseDistance < counterDistance ? clockwisePath : counterPath;
    }

    private bool PathTouchesRightLane(List<Vector3> path, float rightLaneX)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (Mathf.Abs(path[i].x - rightLaneX) <= 0.001f)
            {
                return true;
            }
        }
        return false;
    }

    // 去除相邻的点
    private void RemoveAdjacentDuplicates(List<Vector3> points)
    {
        for (int i = points.Count - 1; i > 0; i--)
        {
            if ((points[i] - points[i - 1]).sqrMagnitude <= 0.0001f)
            {
                points.RemoveAt(i);
            }
        }
    }

    // 是否在棋盘内
    private bool IsOutsideBoard(Vector3 point, float cellSize)
    {
        float boardMinX = 0f;
        float boardMaxX = (width - 1) * cellSize;
        float boardMinZ = 0f;
        float boardMaxZ = (height - 1) * cellSize;

        return point.x < boardMinX || point.x > boardMaxX || point.z < boardMinZ || point.z > boardMaxZ;
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
