using System;

namespace ActivityCenter.GrandTreasury
{
    public class GrandTreasuryData
    {
        public event Action OnDataUpdated;

        // 金币收集进度
        public int CurrentCoins { get; private set; }
        public int MaxCoins { get; private set; }

        // 奖励是否已领取
        public bool IsRewardClaimed { get; private set; }

        // 活动结束时间
        public DateTime EndTime { get; private set; }

        public GrandTreasuryData()
        {
            // 模拟从服务器拉取活动数据
            CurrentCoins = 850;   // 假设当前收集了 850 个
            MaxCoins = 1000;      // 目标是 1000 个
            IsRewardClaimed = false;
            
            // 假设活动在 3天又 5小时 后结束
            EndTime = DateTime.Now.AddDays(3).AddHours(5); 
        }

        // 供外部查询是否满足领取条件
        public bool CanClaimReward()
        {
            return CurrentCoins >= MaxCoins && !IsRewardClaimed;
        }

        // 领取奖励的逻辑
        public void ClaimReward()
        {
            if (CanClaimReward())
            {
                IsRewardClaimed = true;
                // 数据发生改变，通知刷新
                OnDataUpdated?.Invoke(); 
            }
        }

        // 模拟玩家在其他地方获得了金币，进度增加
        public void AddCoins(int amount)
        {
            if (CurrentCoins < MaxCoins)
            {
                CurrentCoins = Math.Min(CurrentCoins + amount, MaxCoins);
                OnDataUpdated?.Invoke();
            }
        }
    }
}