using System;

namespace ActivityCenter.VideoPrivileges
{
    public class VideoPrivilegesData
    {
        public event Action OnDataUpdated;

        // 当前已领取的奖励阶段 (0表示未领取，4表示全部领完)
        public int CurrentRewardStage { get; private set; }
        
        // 总阶段数
        public const int MaxRewardStage = 4;

        public VideoPrivilegesData()
        {
            // 模拟从服务器或本地读取今天已看广告的次数
            CurrentRewardStage = 0; 
        }

        // 成功观看视频后，推进奖励阶段
        public void AdvanceRewardStage()
        {
            if (CurrentRewardStage < MaxRewardStage)
            {
                CurrentRewardStage++;
                OnDataUpdated?.Invoke();
            }
        }

        public bool IsAllRewardsClaimed()
        {
            return CurrentRewardStage >= MaxRewardStage;
        }
    }
}