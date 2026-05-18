/**
 * ==================== Block冻结效果系统使用指南 ====================
 * 
 * 本文档说明如何在SaveCat游戏中使用冻结效果系统
 * 
 * ==================== 系统概述 ====================
 * 
 * 冻结效果系统允许Block具有冻结状态：
 * 1. 冻结的Block无法被玩家点击
 * 2. 冻结的Block呈现冷蓝色视觉效果
 * 3. 只有其他Block直接撞击它才能解冻
 * 4. 解冻后Block恢复原色并可以被点击
 * 
 * ==================== 实现细节 ====================
 * 
 * 已创建的文件：
 * - Assets/Game/Scripts/Entity/Effects/IBlockEffect.cs
 *   └─ 定义Block效果的接口
 * 
 * - Assets/Game/Scripts/Entity/Effects/FrozenBlockEffect.cs
 *   └─ 冻结效果的具体实现
 * 
 * - Assets/Game/Scripts/Entity/Effects/BlockEffectManager.cs
 *   └─ 添加Effect的辅助管理器
 * 
 * 修改的源文件：
 * - BlockData.cs
 *   └─ 添加Effect管理系统（AddEffect、RemoveEffect、CanBeClicked等）
 * 
 * - BlockView.cs
 *   └─ InitView中初始化所有Effect
 * 
 * - BlockManager.cs
 *   └─ 添加BlockData->BlockView映射
 *   └─ 添加GetBlockViewByData()方法
 * 
 * - GridMapManager.cs
 *   └─ 记录碰撞的Block ID
 *   └─ 添加TryUnfreezeBlockingBlock()方法
 * 
 * ==================== 使用方法 ====================
 * 
 * 在LevelManager.StartLevel()中添加以下代码示例：
 * 
 * --- 方式1：为所有Block添加30%概率冻结 ---
 * 
 *     List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
 *     
 *     // 为每个Block添加30%概率冻结
 *     var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 *     foreach (var block in actualBoardBlocks)
 *     {
 *         BlockEffectManager.AddFrozenEffectWithProbability(
 *             block, 
 *             0.3f,  // 30%概率
 *             blockViewMap[block]  // 传入BlockView
 *         );
 *     }
 * 
 * --- 方式2：随机冻结指定数量的Block ---
 * 
 *     List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
 *     var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 *     
 *     // 随机冻结3个Block
 *     BlockEffectManager.AddFrozenEffectToRandomBlocks(
 *         actualBoardBlocks,
 *         blockViewMap,
 *         3  // 要冻结的Block数量
 *     );
 * 
 * --- 方式3：只冻结特定颜色的Block ---
 * 
 *     List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
 *     var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 *     
 *     // 只冻结蓝色Block
 *     BlockEffectManager.AddFrozenEffectToBlocksByType(
 *         actualBoardBlocks,
 *         BlockType.Blue,
 *         blockViewMap
 *     );
 * 
 * --- 方式4：手动为单个Block添加冻结 ---
 * 
 *     List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
 *     var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 *     
 *     if (actualBoardBlocks.Count > 0)
 *     {
 *         BlockEffectManager.AddFrozenEffect(
 *             actualBoardBlocks[0],
 *             blockViewMap[actualBoardBlocks[0]]
 *         );
 *     }
 * 
 * ==================== 工作流程 ====================
 * 
 * 1. 玩家点击Block
 *    │
 *    └─> BlockManager.ProcessBlockEscapeRequest()
 *        ├─ 检查Block.CanBeClicked()（冻结检查）✨ NEW
 *        │  └─ 如果冻结，显示阻挡反馈并返回
 *        ├─ 检查前方是否有其他Block阻挡
 *        │  (GridMapManager.CanBlockEscape)
 *        │
 *        └─ 如果被阻挡
 *           ├─ GridMapManager.TryUnfreezeBlockingBlock()✨ NEW
 *           │  └─ 调用阻挡Block的所有Effect.OnHitByOtherBlock()
 *           │     └─ FrozenBlockEffect.OnHitByOtherBlock()
 *           │        ├─ 改变颜色回原始颜色
 *           │        ├─ 从BlockData中移除此Effect
 *           │        └─ 下次点击时就可以逃离
 *           └─ 显示碰撞反馈动画
 * 
 * 2. 被冻结的Block无法逃离，但会显示"被阻挡"的反馈
 * 
 * 3. 下次点击该Block时，如果已解冻，则可以正常逃离
 * 
 * ==================== 扩展方式 ====================
 * 
 * 要添加新的效果（如燃烧、毒液等），只需：
 * 
 * 1. 创建一个实现IBlockEffect的新类
 *    例如：BurningBlockEffect.cs
 * 
 * 2. 实现接口的方法：
 *    - OnBlockInitialized() - 初始化视觉
 *    - CanBeClicked() - 返回是否可点击
 *    - OnHitByOtherBlock() - 被撞击时的行为
 *    - OnEffectRemoved() - 清理
 * 
 * 3. 在BlockEffectManager中添加相应的快捷方法
 * 
 * 4. 在LevelManager中调用即可
 * 
 * ==================== 注意事项 ====================
 * 
 * 1. 效果初始化必须在BlockView.InitView()之前或之后立即调用
 *    使用BlockEffectManager提供的方法可以确保正确初始化
 * 
 * 2. 如果Effect修改了Block的Material，需要使用material实例
 *    否则会影响所有同类Block的外观
 * 
 * 3. 冻结解冻是完全在BlockData中处理的，
 *    GridMapManager只负责触发解冻逻辑
 * 
 * 4. 一个Block可以有多个Effect同时存在
 *    CanBeClicked()会检查所有Effect
 * 
 * ==================== ✨ 安全检查机制 ====================
 * 
 * 重要！为了避免死局，所有冻结方法都会自动检查：
 * "该Block前面是否有其他Block能撞击它"
 * 
 * 如果没有其他Block能撞击某个Block，即使满足冻结条件也不会冻结它。
 * 
 * 检查方式：
 * - GridMapManager.HasBlockCanHitThis(BlockData block)
 * - 检查Block沿着其Direction方向，前面是否有其他Block
 * 
 * 行为说明：
 * 
 * AddFrozenEffectWithProbability() 
 *   └─ 先检查 HasBlockCanHitThis()，只有返回true才会冻结
 * 
 * AddFrozenEffectToRandomBlocks()
 *   └─ 先筛选出可冻结的Block（前面有其他Block的）
 *   └─ 再从其中随机选择冻结
 *   └─ 若可冻结Block少于需求数量，只冻结可冻结的
 * 
 * AddFrozenEffectToBlocksByType()
 *   └─ 只冻结符合类型且可冻结的Block
 * 
 * 日志输出：
 * - [BlockEffectManager] Block X cannot be frozen because no other block can hit it!
 * - [BlockEffectManager] Froze N blocks (from M freezable blocks)
 * 
 * 这些信息可以帮助调试和理解冻结的实际分布。
 * 
 */
