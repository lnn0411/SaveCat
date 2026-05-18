/**
 * ==================== 冻结效果系统 - 完整修改总结 ====================
 * 
 * 本文档总结了实现Block冻结效果系统所做的所有修改
 * 
 * ==================== 核心需求 ====================
 * 
 * 给某些Block添加冻结效果，满足以下条件：
 * 1. 冻结的Block无法被玩家点击
 * 2. 只有其他Block直接撞击时才能解冻
 * 3. 避免死局 - 只冻结前面有其他Block能撞击的Block
 * 
 * ==================== 新增文件列表 ====================
 * 
 * 1. Assets/Game/Scripts/Entity/Effects/IBlockEffect.cs
 *    - Block效果接口定义
 *    - 定义CanBeClicked(), OnHitByOtherBlock()等方法
 * 
 * 2. Assets/Game/Scripts/Entity/Effects/FrozenBlockEffect.cs
 *    - 冻结效果的具体实现
 *    - 冷蓝色视觉 + 禁用点击 + 碰撞解冻
 * 
 * 3. Assets/Game/Scripts/Entity/Effects/BlockEffectManager.cs
 *    - 快捷方法：AddFrozenEffect()
 *    - 概率冻结：AddFrozenEffectWithProbability()
 *    - 随机冻结：AddFrozenEffectToRandomBlocks()
 *    - 按类型冻结：AddFrozenEffectToBlocksByType()
 *    - 所有方法都包含安全检查
 * 
 * 4. 文档文件：
 *    - USAGE_GUIDE.md - 完整使用指南
 *    - SAFETY_CHECK_MECHANISM.md - 安全检查机制说明
 *    - TESTING_AND_VERIFICATION.md - 测试验证指南
 *    - QUICK_TEST.cs - 快速测试代码
 *    - LevelManager_ModificationExample.cs - 集成示例
 * 
 * ==================== 修改的源文件 ====================
 * 
 * 1. BlockData.cs
 *    新增：
 *    - _effects 列表存储所有Effect
 *    - AddEffect(IBlockEffect effect) - 添加效果
 *    - RemoveEffect(IBlockEffect effect) - 移除效果
 *    - GetEffects() - 获取所有效果
 *    - CanBeClicked() - 检查是否可点击（综合所有Effect）
 * 
 *    修改位置：在构造函数后添加Effect管理系统
 * 
 * 2. BlockView.cs
 *    修改：InitView() 方法
 *    - 在初始化完成后调用 effect.OnBlockInitialized()
 *    - 为所有已添加的Effect初始化视觉效果
 * 
 * 3. BlockManager.cs
 *    新增：
 *    - _blockDataToViewMap 字典 - 追踪Block到View的映射
 *    - GetBlockViewByData(BlockData) - 获取单个View
 *    - GetAllBlockMappings() - 获取全部映射
 * 
 *    修改：
 *    - InitLevel() 中清空映射
 *    - InstantiateBlockView() 中记录映射
 *    - ProcessBlockEscapeRequest() 中添加冻结检查
 *      └─ 先检查 blockView.Data.CanBeClicked()
 *      └─ 如果被冻结，显示反馈并返回
 *      └─ 如果被阻挡，调用 TryUnfreezeBlockingBlock()
 * 
 * 4. GridMapManager.cs
 *    新增：
 *    - _lastBlockingBlockId 字段 - 记录碰撞的Block
 *    - HasBlockCanHitThis(BlockData block) - 检查前面是否有Block
 *    - TryUnfreezeBlockingBlock(int blockId) - 解冻被阻挡的Block
 * 
 *    修改：
 *    - CanBlockEscapeBySweptFootprint() 中记录阻挡Block的ID
 * 
 * ==================== 工作流程图 ====================
 * 
 * 【初始化阶段】
 * 
 * LevelManager.StartLevel()
 *   ↓
 * BlockManager.InitLevel()
 *   ├─ 生成所有Block
 *   └─ 记录BlockData→BlockView映射
 *   ↓
 * BlockEffectManager.AddFrozenEffect...()
 *   ├─ 检查 GridMapManager.HasBlockCanHitThis()
 *   │  └─ 遍历Block前面是否有其他Block
 *   ├─ 如果可以冻结，添加FrozenBlockEffect
 *   └─ 初始化视觉：改颜色为冷蓝色
 * 
 * 【游戏运行阶段】
 * 
 * 玩家点击Block
 *   ↓
 * BlockManager.ProcessBlockEscapeRequest()
 *   ├─ 检查 Block.CanBeClicked()
 *   │  └─ 遍历所有Effect的CanBeClicked()
 *   │  └─ 如果有Effect返回false → 显示反馈返回
 *   │
 *   └─ 检查 GridMapManager.CanBlockEscape()
 *      └─ 记录_lastBlockingBlockId
 *      └─ 如果无法逃离 → TryUnfreezeBlockingBlock()
 *         ├─ 获取阻挡的Block
 *         ├─ 调用 effect.OnHitByOtherBlock()
 *         │  └─ FrozenBlockEffect改颜色+移除自己
 *         └─ 显示碰撞反馈
 * 
 * 【解冻后】
 * 
 * 再次点击已解冻的Block
 *   └─ 该Block已无冻结Effect
 *   └─ CanBeClicked()返回true
 *   └─ 可以正常逃离
 * 
 * ==================== 安全性检查（关键！）====================
 * 
 * HasBlockCanHitThis(BlockData block) 的实现：
 * 
 * 1. 获取Block的头部位置（前端）
 * 2. 获取Block的方向向量
 * 3. 从头部开始沿方向向前逐格检查
 * 4. 遇到被其他Block占用的格子 → 返回true（有Block能撞击）
 * 5. 检查完整个范围都没找到 → 返回false（无法冻结）
 * 
 * 这确保了：
 * ✓ 不会出现永远无法解冻的死局
 * ✓ 只有有意义的冻结才会被应用
 * ✓ 游戏仍可以正常进行
 * 
 * ==================== 使用示例 ====================
 * 
 * 在 LevelManager.StartLevel() 中：
 * 
 * List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);
 * var blockViewMap = BlockManager.Instance.GetAllBlockMappings();
 * 
 * // 为每个Block添加30%概率冻结
 * foreach (var block in actualBoardBlocks)
 * {
 *     BlockEffectManager.AddFrozenEffectWithProbability(block, 0.3f, blockViewMap[block]);
 * }
 * 
 * // 继续其他初始化...
 * 
 * ==================== 代码量统计 ====================
 * 
 * 新增代码：
 * - IBlockEffect.cs: ~20 行
 * - FrozenBlockEffect.cs: ~70 行
 * - BlockEffectManager.cs: ~90 行
 * - 文档: ~500 行
 * 
 * 修改代码：
 * - BlockData.cs: +30 行
 * - BlockView.cs: +5 行
 * - BlockManager.cs: +30 行
 * - GridMapManager.cs: +50 行
 * 
 * 总计：修改量非常小，不会影响现有系统
 * 
 * ==================== 扩展路线 ====================
 * 
 * 基于此框架，可以轻松添加：
 * 
 * 1. 燃烧效果（BurningBlockEffect）
 *    - 每秒掉血
 *    - 触碰其他Block时传播燃烧
 * 
 * 2. 减速效果（SlowBlockEffect）
 *    - 降低移动速度
 *    - 可以叠加
 * 
 * 3. 脆弱效果（FragileBlockEffect）
 *    - 被撞击时直接消失
 *    - 无需逃离
 * 
 * 4. 复制效果（DuplicateBlockEffect）
 *    - 被撞击时生成副本
 *    - 增加难度
 * 
 * 所有这些只需：
 * 1. 实现IBlockEffect接口
 * 2. 在BlockEffectManager中添加快捷方法
 * 3. 在LevelManager中调用即可
 * 
 * ==================== 兼容性 ====================
 * 
 * ✓ 与现有龙系统兼容 - Block冻结不影响龙攻击
 * ✓ 与现有弹药系统兼容 - 冻结Block仍可被逃离并成为弹药
 * ✓ 与现有动画系统兼容 - 使用现有的反馈动画
 * ✓ 与事件系统兼容 - 可添加新事件通知
 * ✓ 与对象池系统兼容 - Block回收时自动清理Effect
 * 
 */
