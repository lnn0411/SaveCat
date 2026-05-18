/**
 * ==================== Block冻结安全检查机制 ====================
 * 
 * 问题说明：
 * =========
 * 原来的冻结机制可能导致死局：如果一个Block被随机赋予冻结效果，
 * 但在它前面的方向上没有任何其他Block能够撞击它，那么这个冻结
 * 就永远无法被解除。
 * 
 * 解决方案：
 * =========
 * 新增了安全检查机制：GridMapManager.HasBlockCanHitThis(BlockData block)
 * 
 * 所有添加冻结效果的方法现在都会先检查：
 * 1. 该Block前面是否有其他Block存在
 * 2. 只有当确实有其他Block能撞击它时，才会赋予冻结效果
 * 
 * 检查原理：
 * =========
 * 
 * Block A有一个方向（Direction），前面就是这个方向指向的格子。
 * 
 * 例如：Block A在位置(2,2)，方向为向上（Up）
 *      它的"头部"在(2,4)（假设长度为3）
 *      检查逻辑：从(2,4)开始，沿着Up方向继续向前
 *                (2,5) - (2,6) - (2,7) ...
 *      如果任何一个格子被其他Block占用，说明有Block能撞击A
 * 
 *      一直检查到超出地图边界，都没有找到其他Block，则不能冻结A
 * 
 * 实现细节：
 * =========
 * 
 * GridMapManager.HasBlockCanHitThis(BlockData block)
 * ├─ 获取Block的头部位置：block.GetHeadPosition()
 * ├─ 获取Block的方向向量：DirectionUtility.ToGridVector(block.Dir)
 * ├─ 从头部开始沿方向向前遍历
 * │  ├─ 超出地图边界 → 返回false（没有Block能撞击）
 * │  └─ 发现被其他Block占用 → 返回true（有Block能撞击）
 * └─ 检查完整个范围都没找到 → 返回false
 * 
 * BlockEffectManager的改动：
 * =========================
 * 
 * 1. AddFrozenEffectWithProbability()
 *    └─ 先调用 HasBlockCanHitThis()
 *    └─ 若返回false，直接返回，不添加效果
 *    └─ 若返回true，才根据概率决定是否冻结
 * 
 * 2. AddFrozenEffectToRandomBlocks()
 *    └─ 先筛选出所有"可冻结的Block"（前面有其他Block的）
 *    └─ 从可冻结的Block中随机选择需要的数量
 *    └─ 若可冻结Block少于需求数量，只冻结可冻结的
 * 
 * 3. AddFrozenEffectToBlocksByType()
 *    └─ 先检查每个符合类型的Block是否可冻结
 *    └─ 只冻结可冻结的Block
 * 
 * 使用示例：
 * =========
 * 
 * // 仍然使用相同的方式，但现在完全安全
 * var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 * 
 * // 方式1：概率冻结（会自动跳过无法冻结的）
 * foreach (var block in actualBoardBlocks)
 * {
 *     BlockEffectManager.AddFrozenEffectWithProbability(block, 0.3f, blockViewMap[block]);
 * }
 * 
 * // 方式2：随机冻结N个（如果可冻结的少于N个，只冻结可冻结的）
 * BlockEffectManager.AddFrozenEffectToRandomBlocks(actualBoardBlocks, blockViewMap, 5);
 * 
 * // 方式3：冻结特定颜色（只冻结可冻结的该颜色Block）
 * BlockEffectManager.AddFrozenEffectToBlocksByType(actualBoardBlocks, BlockType.Blue, blockViewMap);
 * 
 * 日志输出：
 * =========
 * 
 * 在运行时，控制台会输出：
 * 
 * [BlockEffectManager] Block 5 cannot be frozen because no other block can hit it!
 * [BlockEffectManager] Froze 3 blocks (from 8 freezable blocks)
 * [BlockEffectManager] Froze 2 blocks of type Blue
 * 
 * 这些日志帮助调试和理解冻结的实际情况。
 * 
 * 注意事项：
 * =========
 * 
 * 1. 检查只在Block生成后进行，此时所有Block位置已确定
 * 2. HasBlockCanHitThis() 调用时需要GridMapManager已初始化
 * 3. 如果关卡设计时希望所有Block都可能被冻结，
 *    应该确保棋盘中每个Block前面都有其他Block
 * 4. 某些Block无法冻结是正常的，不会导致错误
 * 
 */
