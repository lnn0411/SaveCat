/**
 * ==================== 冻结效果系统 - 测试和验证指南 ====================
 * 
 * 此文档说明如何验证安全检查机制是否正常工作
 * 
 * ==================== 快速集成测试 ====================
 * 
 * 在 LevelManager.StartLevel() 中添加以下代码：
 * 
 * --- 测试代码 ---
 * 
 * // 获取Block映射
 * List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
 * var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 * 
 * // 测试：30%概率冻结
 * foreach (var block in actualBoardBlocks)
 * {
 *     BlockEffectManager.AddFrozenEffectWithProbability(block, 0.3f, blockViewMap[block]);
 * }
 * 
 * Debug.Log($"[FrozenEffect Test] Total blocks: {actualBoardBlocks.Count}");
 * 
 * --- 预期结果 ---
 * 
 * ✓ 控制台输出多条日志：
 *   [BlockEffectManager] Block X cannot be frozen because no other block can hit it!
 *   （表示某些Block前面没有其他Block，跳过了冻结）
 * 
 * ✓ 游戏中约30%的Block显示冷蓝色
 *   （实际比例可能低于30%，因为有些Block无法冻结）
 * 
 * ✓ 冷蓝色Block无法被点击（点击无响应，无逃离动画）
 * 
 * ✓ 其他Block撞击冷蓝色Block时：
 *   - 被撞击的冷蓝色Block恢复原色
 *   - 下次点击时可以正常逃离
 * 
 * ==================== 验证清单 ====================
 * 
 * □ 冻结检查
 *   - [ ] 冻结的Block显示冷蓝色颜色
 *   - [ ] 冻结的Block无法点击
 *   - [ ] 点击冻结Block时显示"被阻挡"的反馈动画（碰撞反馈）
 * 
 * □ 安全性检查
 *   - [ ] 检查控制台是否输出"cannot be frozen because no other block can hit it"
 *   - [ ] 统计：冻结Block数 < 所有Block数 * 冻结概率（因为有些无法冻结）
 *   - [ ] 验证每个冻结Block前面确实有其他Block
 * 
 * □ 解冻逻辑
 *   - [ ] 点击前方Block，让它撞击冻结Block
 *   - [ ] 冻结Block被撞击后恢复原色
 *   - [ ] 恢复原色后，可以点击该Block并成功逃离
 *   - [ ] 解冻的Block不会再冻结
 * 
 * □ 游戏流程
 *   - [ ] 冻结Block不影响龙的攻击
 *   - [ ] 冻结Block不影响弹药系统
 *   - [ ] 游戏胜利/失败逻辑正常
 * 
 * ==================== 调试技巧 ====================
 * 
 * 如果冻结效果不符合预期：
 * 
 * 1. 检查冻结Block数量
 *    - 打开控制台，搜索"Froze"关键字
 *    - 查看实际冻结了多少Block
 *    - 与预期概率对比
 * 
 * 2. 检查安全检查是否生效
 *    - 搜索"cannot be frozen because"
 *    - 查看有多少Block被跳过
 *    - 若数量为0，说明所有Block都能被冻结（可能棋盘设计特殊）
 * 
 * 3. 调试Block的位置和方向
 *    在 GridMapManager.HasBlockCanHitThis() 中添加：
 * 
 *    Debug.Log($"[HasBlockCanHitThis] Block {block.Id} at ({block.GridX},{block.GridY}) " +
 *              $"dir={block.Dir}, headPos={block.GetHeadPosition()}, " +
 *              $"canBeHit={result}");
 * 
 * 4. 验证冻结的视觉效果
 *    - 确认Renderer组件存在
 *    - 检查FrozenBlockEffect中的颜色值是否正确
 *    - 尝试更改冻结颜色，看是否生效
 * 
 * 5. 验证解冻的触发
 *    在 FrozenBlockEffect.OnHitByOtherBlock() 中添加：
 * 
 *    Debug.Log($"[FrozenEffect] Block {_blockData.Id} hit by {hitByBlock.Id}, unfreezing...");
 * 
 * ==================== 常见问题 ====================
 * 
 * Q: 为什么有些Block没有被冻结，即使概率是100%？
 * A: 因为那些Block前面没有其他Block能撞击它们。这是安全检查的预期行为。
 *    可以在控制台查看"cannot be frozen"日志确认。
 * 
 * Q: 冷蓝色Block被撞击后还是蓝色，没变回原色？
 * A: 检查：
 *    1. 是否正确调用了解冻逻辑（TryUnfreezeBlockingBlock）
 *    2. 是否保存了原始颜色（_originalColor）
 *    3. Renderer是否被正确修改了材质
 * 
 * Q: 冻结Block被解冻后无法逃离？
 * A: 检查：
 *    1. Block是否真的从Effect列表中移除了
 *    2. CanBeClicked()是否返回true
 *    3. 前方是否仍有Block阻挡
 * 
 * Q: 某些Block永远无法被冻结？
 * A: 这是正常的。那些Block的前面没有其他Block，所以不能冻结。
 *    如果希望某个特定Block被冻结，需要确保它前面有其他Block。
 * 
 * ==================== 性能考虑 ====================
 * 
 * 安全检查机制的性能影响：
 * 
 * • HasBlockCanHitThis() 的复杂度：O(width + height)
 * • 在所有Block生成后调用一次，不是每帧调用
 * • 冻结100个Block的检查总耗时通常 < 1ms
 * 
 * 不会产生性能问题。
 * 
 */
