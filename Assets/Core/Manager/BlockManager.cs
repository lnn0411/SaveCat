using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景方块的控制器。负责处理关卡初始化生成的实体调度，以及接收玩家的射线点击输入。
/// </summary>
/// 
public class BlockManager : Singleton<BlockManager>
{
    [Header("Dependencies")]
    public Camera bottomCamera; //下方摄像机
    public LayerMask blockLayerMask; //方块所在的Layer

    private LevelConfigSO _config; //关卡配置数据

    private List<BlockType> _colorPool; //颜色池（1:1的目标方块数量和颜色）
    //模拟生成环境配置

    private int startBlockIdCounter = 1; //用于生成唯一Id的计数器

#region 对外接口

    /// <summary>
    /// 初始化关卡，并向上级回传这批方块确定的全部实体数据（方便上级根据实际随机出的长度去生成龙）
    /// </summary>
    public List<BlockData> InitLevel(LevelConfigSO config, List<BlockType> colorPool)
    {
        _config = config;
        _colorPool = colorPool;
        // 初始化底层逻辑地图
        GridMapManager.Instance.InitMap(config.maxWidth, config.maxHeight);

        //管卡生成 获取真实摆放
        List<BlockData> actualSpawnedBlocks = GenerateGuaranteedSolvableMap(_config.targetBlockCount, _config.maxWidth, _config.maxHeight);
        return actualSpawnedBlocks;
    }

#endregion

#region 玩家输入控制层
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            HandlePlayerClick();
        }
    }

    //处理点击事件
    private void HandlePlayerClick()
    {
        if(bottomCamera == null)
        {
            Debug.LogError("Bottom camera reference is missing!");
            return;
        }

        //屏幕点击转换为三维射线
        Ray ray = bottomCamera.ScreenPointToRay(Input.mousePosition);

        //物理射线
        if(Physics.Raycast(ray, out RaycastHit hit, 100f, blockLayerMask))
        {
            BlockView clickedBlock = hit.collider.GetComponent<BlockView>();
            if(clickedBlock != null)
            {
                Debug.Log($"Clicked on Block {clickedBlock.Data.Id}");
                ProcessBlockEscapeRequest(clickedBlock);
            }
        }
    }

    //处理方块被点击时的逻辑
    private void ProcessBlockEscapeRequest(BlockView blockView)
    {
        int id = blockView.Data.Id;

        //询问数据层 是否可以逃脱
        if(GridMapManager.Instance.CanBlockEscape(id, out int availableSteps))
        {
            Debug.Log($"方块{id}可以逃脱，执行逃脱动画");
            // 抹除展位
            GridMapManager.Instance.RemoveBlockFromMap(id);

            //调用出界动画 并且委托在飞出屏幕后销毁与UI里面能量增加
            blockView.PlayEscapeAnimation(blockView.transform.position + blockView.transform.forward * 20f,() =>
            {
                // 调用事件 当前方块逃脱成功了，回收这个方块对象，增加能量
                EventManager.Broadcast(EventID.OnBlockEscapeSuccess, blockView.Data);
                PoolManager.Instance.Recycle(blockView.gameObject);
            });
        }
        else
        {
            Debug.Log($"方块{id}被阻挡，前方还有 {availableSteps} 格，播放撞墙反馈");

            blockView.PlayBlockedFeedback(availableSteps);
        }
    }

#endregion

#region 方法工具

    // 逆向推演法：最先放入的方块 一定是最后逃逸的，所以我们从后往前放，直到放满指定数量的方块或者没有合适的位置了
    // 后面放的方块，它的逃逸路线不会被之前存在的方块阻挡即可
    // 棋盘必定有解
    private List<BlockData> GenerateGuaranteedSolvableMap(int targetCount, int mapWidth, int mapHeight)
    {
        List<BlockData> spawnedDataList = new List<BlockData>();
        int currentCount = 0;
        int maxAttempts = 1000; // 避免死循环的安全措施
        int retries = 0;

        while(currentCount < targetCount && retries < maxAttempts)
        {
            retries++;
            // 从颜色池中获取颜色 先生成targetCount个方块的颜色，保证数量和颜色的1:1关系 后续长度数值可以由算法随机生成
            BlockType assignedType = _colorPool[currentCount];
            Direction randomDir = (Direction)Random.Range(0, 4);
            int randomLength = Random.Range(1,4);
            int randomX = Random.Range(0, mapWidth);
            int randomY = Random.Range(0, mapHeight);
            int testId = startBlockIdCounter;
            BlockData testBlock = new BlockData(testId, assignedType, randomLength, randomDir, randomX, randomY);
            
            //第一重校验 判断是否可以放到目前的地图中 （不越界 不与现有方块重叠）
            if(!IsPositionValid(testBlock, mapWidth, mapHeight))
            {
                continue; // 无效位置，重新生成
            }

            //第二重校验：逃跑路径是否畅通，新的必须可以飞
            if(GridMapManager.Instance.CanBlockEscape(testBlock))
            {
                // 通过两重校验，正式注册这个方块
                bool registered = GridMapManager.Instance.RegisterBlock(testBlock);
                if(registered)
                {
                    InstantiateBlockView(testBlock);

                    startBlockIdCounter++;
                    currentCount++;
                    retries = 0; // 成功摆放一个，重置超时重试计数
                    spawnedDataList.Add(testBlock);
                }
            }
        }
        return spawnedDataList;
    }
#endregion

#region 辅助小工具
    /// <summary>
    /// 模拟测试用，不写入实际地图数据
    /// </summary>
    private bool IsPositionValid(BlockData block, int width, int height)
    {
        // 我们利用现有的 GridMapManager 里的判定机制即可
        // 但需要你在 GridMapManager 里暴露一个只查阅不写入的方法，这里做简化演示
        foreach(Vector2Int cell in block.GetOccupiedCells())
        {
            if(cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return false;
        }
        return GridMapManager.Instance.IsCellsEmpty(block.GetOccupiedCells());
    }

    // 根据数据生成 3D 实体
    private void InstantiateBlockView(BlockData data)
    {
        // 从对象池索取 (假设你刚才根据 Config 注册了它)
        GameObject blockObj = PoolManager.Instance.Get(_config.blockPrefab, Vector3.zero, Quaternion.identity, this.transform);
        if (blockObj != null)
        {
            BlockView view = blockObj.GetComponent<BlockView>();
            view.InitView(data, 1.0f);
        }
    }
#endregion
}
