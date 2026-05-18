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

    [Header("Escape Path Settings")]
    [SerializeField] private float cellSize = 1.0f; //方块的尺寸
    [SerializeField] private float escapeOutsidePadding = 1.5f; //边缘的填充


    private LevelConfigSO _config; //关卡配置数据

    private List<BlockType> _colorPool; //颜色池（1:1的目标方块数量和颜色）
    //模拟生成环境配置

    private int startBlockIdCounter = 1; //用于生成唯一Id的计数器

    // ✨ BlockData与BlockView的映射（用于添加Effect后初始化视觉）
    private Dictionary<BlockData, BlockView> _blockDataToViewMap = new Dictionary<BlockData, BlockView>();

#region 对外接口

    /// <summary>
    /// 初始化关卡，并向上级回传这批方块确定的全部实体数据（方便上级根据实际随机出的长度去生成龙）
    /// </summary>
    public List<BlockData> InitLevel(LevelConfigSO config, List<BlockType> colorPool)
    {
        _config = config;
        _colorPool = colorPool;
        // ✨ 清除旧的映射
        _blockDataToViewMap.Clear();
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
                //先让技能系统判断是否要接管此次点击
                if(SkillManager.Instance != null && SkillManager.Instance.TryHandleBlockClick(clickedBlock))
                {
                    return; //技能系统接管了点击，不继续处理
                }

                //正常逻辑
                ProcessBlockEscapeRequest(clickedBlock);
            }
        }
    }

    //处理方块被点击时的逻辑
    private void ProcessBlockEscapeRequest(BlockView blockView)
    {
        if (blockView == null || blockView.Data == null)
        {
            return;
        }

        int id = blockView.Data.Id;

        // ✨ 检查Block是否可以被点击（考虑所有效果）
        if (!blockView.Data.CanBeClicked())
        {
            Debug.Log($"[BlockManager] Block {id} is frozen and cannot be clicked!");
            blockView.PlayBlockedFeedback(0);
            return;
        }

        // 是否可以逃逸
        if (!GridMapManager.Instance.CanBlockEscape(id, out int availableSteps))
        {
            GridMapManager.Instance.TryNotifyForwardBlockingBlockHit(id);
            blockView.PlayBlockedFeedback(availableSteps);
            return;
        }

        // 方块确认可以逃逸后，再尝试预定槽位。
        int targetSlotIndex = -1;

        if (SlotManager.Instance != null && !SlotManager.Instance.TryReserveFirstEmptySlot(out targetSlotIndex))
        {
            Debug.Log($"[BlockManager] 槽位已满，方块 {id} 暂时不能逃逸");

            EventManager.Broadcast(EventID.OnSlotFull);
            blockView.PlaySlotFullFeedback();

            return;
        }

        // 3. 槽位也预定成功后，再生成逃逸路径。
        if (!TryBuildSlotTargetWorldPoint(targetSlotIndex, out Vector3 targetWorldPoint))
        {
            Debug.LogWarning($"[BlockManager] 无法获取槽位 {targetSlotIndex} 的精准世界坐标，释放预定槽位。");

            if (SlotManager.Instance != null)
            {
                SlotManager.Instance.ReleaseReservedSlot(targetSlotIndex);
            }

            blockView.PlayBlockedFeedback(availableSteps);
            return;
        }

        bool hasPath = GridMapManager.Instance.TryBuildEscapePath(
            id,
            targetWorldPoint,
            escapeOutsidePadding,
            cellSize,
            out Vector3[] pathPoints
        );

        // 4. 如果路径生成失败，必须释放刚刚预定的槽位。
        if (!hasPath)
        {
            Debug.LogWarning($"[BlockManager] 方块 {id} 路径生成失败，释放预定槽位 {targetSlotIndex}");

            if (SlotManager.Instance != null)
            {
                SlotManager.Instance.ReleaseReservedSlot(targetSlotIndex);
            }

            // 阻挡反馈
            blockView.PlayBlockedFeedback(availableSteps);

            return;
        }

        // 5. 到这里才可以从棋盘数据中移除。
        GridMapManager.Instance.RemoveBlockFromMap(id);

        blockView.PlayEscapePathAnimation(
            pathPoints,
            () =>
            {
                if (SlotManager.Instance != null)
                {
                    SlotManager.Instance.TryLoadReservedSlot(
                        targetSlotIndex,
                        blockView.Data.Type,
                        blockView.Data.StrengthCount
                    );
                }

                PoolManager.Instance.Recycle(blockView.gameObject);
            }
        );
    }

#endregion

#region 方法工具
    // 坐标转换   UI转屏幕转世界坐标
    private bool TryBuildSlotTargetWorldPoint(int slotIndex, out Vector3 targetWorldPoint)
    {
        targetWorldPoint = Vector3.zero;
        if(SlotManager.Instance == null) return false;

        if(!SlotManager.Instance.TryGetSlotShootAnchor(slotIndex, out RectTransform anchor)) return false;
        if(bottomCamera == null) return false;
        //获取对应的canvas
        Canvas canvas = anchor.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // 说明依赖相机渲染
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : bottomCamera;
        }
        //转为屏幕坐标
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, anchor.position);
        // 从屏幕坐标发射一条射线
        Ray ray = bottomCamera.ScreenPointToRay(screenPoint);

        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);

        if (!boardPlane.Raycast(ray, out float enter))
        {
            return false;
        }

        targetWorldPoint = ray.GetPoint(enter);
        targetWorldPoint.y = 0f;

        return true;
    }


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
            Direction randomDir = (Direction)Random.Range(0, DirectionUtility.DirectionCount);
            // 随机配置预制体
            BlockSpec spec = GetRandomBlockSpec();
            if(spec == null)
            {
                Debug.LogError("[BlockManager] 未配置方块预制体");
                break;
            }
            // 随机的坐标（我们后续会验证这个坐标是否合法，不合法就丢弃重试）
            int randomX = Random.Range(0, mapWidth);
            int randomY = Random.Range(0, mapHeight);
            // 方块编号
            int testId = startBlockIdCounter;
            BlockData testBlock = new BlockData(testId, assignedType, spec, randomDir, randomX, randomY);
            
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

    //从配置里面随机获取一个方块规格
    private BlockSpec GetRandomBlockSpec()
    {
        if(_config == null || _config.blockSpecs == null || _config.blockSpecs.Length == 0) return null;

        return _config.blockSpecs[Random.Range(0, _config.blockSpecs.Length)];
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
        GameObject prefab = data.Spec.prefab;
        // 从对象池索取 (假设你刚才根据 Config 注册了它)
        GameObject blockObj = PoolManager.Instance.Get(prefab, Vector3.zero, Quaternion.identity, this.transform);
        if (blockObj != null)
        {
            BlockView view = blockObj.GetComponent<BlockView>();
            view.InitView(data, 1.0f);

            // ✨ 记录BlockData->BlockView的映射
            _blockDataToViewMap[data] = view;
        }
    }

    /// <summary>
    /// 获取BlockData对应的BlockView（用于Effect初始化）
    /// </summary>
    public BlockView GetBlockViewByData(BlockData blockData)
    {
        if (_blockDataToViewMap.TryGetValue(blockData, out BlockView view))
        {
            return view;
        }
        return null;
    }

    /// <summary>
    /// 获取所有Block的BlockData->BlockView映射（用于批量操作）
    /// </summary>
    public Dictionary<BlockData, BlockView> GetAllBlockMappings()
    {
        return new Dictionary<BlockData, BlockView>(_blockDataToViewMap);
    }
#endregion

}
