namespace ActivityCenter.Core
{
    public interface IActivityController
    {
        // 模块生命周期
        void Open();
        void Close();

        // 集中轮询架构新增
        /// <summary>
        /// 标识当前模块是否需要执行 Update 轮询
        /// </summary>
        bool NeedUpdate { get; } 
        
        /// <summary>
        /// 集中轮询时调用的更新逻辑
        /// </summary>
        void OnUpdate();
    }
}