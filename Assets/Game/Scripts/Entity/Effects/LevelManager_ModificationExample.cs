/// <summary>
/// LevelManager修改示例
/// 
/// 本文件展示了如何在LevelManager.StartLevel()方法中添加冻结效果
/// 
/// 在真实项目中，将以下代码的相关部分粘贴到LevelManager.StartLevel()中
/// </summary>
public class LevelManagerFrozenEffectExample
{
    /*
     * 在 LevelManager.StartLevel() 方法中，
     * 获取actualBoardBlocks列表后添加以下代码：
     * 
     * ============== 代码示例 ==============
     */

    // 示例1：30%概率冻结每个Block
    private static void Example1_FreezeByProbability()
    {
        // ... 其他代码保持不变 ...
        // List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
        
        // ✨ 冻结效果系统集成
        var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
        foreach (var block in new System.Collections.Generic.List<BlockData>() /* actualBoardBlocks */)
        {
            BlockEffectManager.AddFrozenEffectWithProbability(
                block,
                0.3f,  // 30%概率
                blockViewMap[block]
            );
        }

        // ... 继续其他初始化 ...
    }

    // 示例2：随机冻结2-5个Block
    private static void Example2_FreezeRandomBlocks()
    {
        // List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
        
        // ✨ 冻结效果系统集成
        var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
        int freezeCount = UnityEngine.Random.Range(2, 6); // 冻结2-5个
        BlockEffectManager.AddFrozenEffectToRandomBlocks(
            new System.Collections.Generic.List<BlockData>() /* actualBoardBlocks */,
            blockViewMap,
            freezeCount
        );
    }

    // 示例3：只冻结红色Block
    private static void Example3_FreezeByColor()
    {
        // List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
        
        // ✨ 冻结效果系统集成
        var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
        BlockEffectManager.AddFrozenEffectToBlocksByType(
            new System.Collections.Generic.List<BlockData>() /* actualBoardBlocks */,
            BlockType.Red,  // 只冻结红色Block
            blockViewMap
        );
    }

    // 示例4：难度进阶 - 根据关卡难度调整冻结数量
    private static void Example4_DifficultyBased(LevelConfigSO config)
    {
        // List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
        
        // ✨ 根据关卡difficulty调整冻结Block数量
        var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
        var actualBoardBlocks = new System.Collections.Generic.List<BlockData>();
        
        int freezeCount = 0;
        // 假设config有difficulty字段（0-5）
        // if (config.difficulty >= 3)
        // {
        //     freezeCount = config.difficulty - 2; // Difficulty 3+ → 1-3个冻结Block
        // }

        if (freezeCount > 0)
        {
            BlockEffectManager.AddFrozenEffectToRandomBlocks(
                actualBoardBlocks,
                blockViewMap,
                freezeCount
            );
        }
    }

    /*
     * ============== 具体修改步骤 ==============
     * 
     * 1. 找到 LevelManager.cs 中的 StartLevel() 方法
     * 
     * 2. 找到这一行：
     *    List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
     * 
     * 3. 在这一行之后立即添加：
     *    
     *    // ✨ 添加冻结效果
     *    var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
     *    foreach (var block in actualBoardBlocks)
     *    {
     *        BlockEffectManager.AddFrozenEffectWithProbability(block, 0.3f, blockViewMap[block]);
     *    }
     * 
     * 4. 保存文件
     * 
     * 5. 进入游戏测试 - 现在应该有30%的Block呈现冷蓝色且无法点击
     * 
     * ============== 预期行为 ====================
     * 
     * ✅ 冻结Block的行为：
     *    - 显示冷蓝色外观
     *    - 无法被点击（点击无响应）
     *    - 如果其他Block撞击它，会显示碰撞反馈并解冻
     * 
     * ✅ 解冻后的行为：
     *    - 颜色恢复为原始颜色
     *    - 可以正常点击逃离
     * 
     * ============== 调试技巧 ====================
     * 
     * 如果冻结不生效：
     * 1. 检查控制台是否有错误日志
     * 2. 确认BlockEffectManager调用时传入了正确的BlockView
     * 3. 在FrozenBlockEffect.OnBlockInitialized中添加Debug.Log确认调用
     * 4. 检查Renderer组件是否存在（SetColor会失败）
     * 
     * 如果解冻不生效：
     * 1. 检查GridMapManager是否正确检测到碰撞
     * 2. 确认TryUnfreezeBlockingBlock被调用
     * 3. 在FrozenBlockEffect.OnHitByOtherBlock中添加Debug.Log
     */
}
