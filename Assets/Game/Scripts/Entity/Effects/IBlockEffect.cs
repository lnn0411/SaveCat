/// <summary>
/// Block效果的接口
/// 定义了Block可以拥有的各种效果的基础行为
/// </summary>
public interface IBlockEffect
{
    /// <summary>
    /// 效果名称
    /// </summary>
    string EffectName { get; }

    /// <summary>
    /// Block初始化时调用，应用视觉效果等
    /// </summary>
    void OnBlockInitialized(BlockData data, BlockView view);

    /// <summary>
    /// 检查Block是否可以被点击
    /// 冻结效果会返回false以禁用点击
    /// </summary>
    bool CanBeClicked();

    /// <summary>
    /// 当有其他Block撞击此Block时调用
    /// </summary>
    void OnHitByOtherBlock(BlockData hitByBlock);

    /// <summary>
    /// 效果被移除时调用
    /// </summary>
    void OnEffectRemoved();
}
