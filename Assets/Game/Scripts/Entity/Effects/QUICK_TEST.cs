/// <summary>
/// 快速测试冻结效果系统
/// 
/// 直接复制到 LevelManager.StartLevel() 方法中，
/// 在这一行之后：
/// List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
/// 
/// 即可测试冻结效果是否正常工作
/// </summary>

/*
// ========== 快速测试代码 开始 ==========

// 获取BlockView映射
var blockViewMap = BlockManager.Instance.GetAllBlockMappings();

// 测试：为所有Block添加30%概率冻结
foreach (var block in actualBoardBlocks)
{
    BlockEffectManager.AddFrozenEffectWithProbability(block, 0.3f, blockViewMap[block]);
}

Debug.Log($"[Frozen Effect Test] Initialized frozen effects for {actualBoardBlocks.Count} blocks with 30% probability");

// ========== 快速测试代码 结束 ==========

// 预期结果：
// ✓ 游戏启动后，约30%的Block显示为冷蓝色
// ✓ 冷蓝色Block无法被点击（点击无响应）
// ✓ 其他Block撞击它们时会被解冻（恢复原色）
// ✓ 解冻后可以正常点击逃离

// 如果没有看到冷蓝色Block，检查：
// 1. 控制台是否有报错
// 2. BlockEffectManager中的Debug.Log是否打印
// 3. Renderer组件是否正确
*/
