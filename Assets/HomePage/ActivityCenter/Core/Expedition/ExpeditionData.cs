using System;

namespace ActivityCenter.Expedition
{
    public class ExpeditionData
    {
        // 数据更新事件，通知 Controller 刷新 View
        public event Action OnDataUpdated;

        // 挑战进度数据
        public int CompletedLevels { get; private set; }
        public int TotalLevels { get; private set; }

        // 参与玩家数据
        public int ActivePlayers { get; private set; }
        public int TotalPlayers { get; private set; }

        public ExpeditionData()
        {
            // 模拟从服务器或本地读取初始数据
            CompletedLevels = 3;
            TotalLevels = 10;
            ActivePlayers = 15;
            TotalPlayers = 20;
        }

        // 模拟数据刷新或关卡变更的逻辑
        public void UpdateProgress(int newCompletedLevels)
        {
            CompletedLevels = newCompletedLevels;
            OnDataUpdated?.Invoke();
        }
    }
}