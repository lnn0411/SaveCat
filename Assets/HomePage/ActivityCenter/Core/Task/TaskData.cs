using System;
using System.Collections.Generic;

namespace ActivityCenter.Task
{
    public class TaskData
    {
        public event Action OnDataUpdated;

        public int CurrentStars { get; private set; }
        public DateTime ActivityEndTime { get; private set; }
        public List<TaskItemData> TaskList { get; private set; }

        public int[] PrizeRequires { get; private set; }
        public int[] PrizeStates { get; private set; } 
        
        // 新增：记录玩家是否已经观看过激励广告
        public bool HasWatchedAd { get; private set; } 

        public TaskData()
        {
            CurrentStars = 15;
            ActivityEndTime = DateTime.Now.AddDays(3).AddHours(5); 
            PrizeRequires = new int[] { 20, 50, 100 };
            PrizeStates = new int[] { 0, 0, 0 };
            
            // 模拟数据初始化，假设玩家还没看过广告
            HasWatchedAd = false; 

            TaskList = new List<TaskItemData>
            {
                new TaskItemData { taskId = "001", type = TaskType.LoginDays, targetValue = 7, currentValue = 3, rewardStars = 5, state = TaskState.InProgress },
                new TaskItemData { taskId = "002", type = TaskType.CollectWool, targetValue = 10, currentValue = 10, rewardStars = 10, state = TaskState.CanClaim },
                new TaskItemData { taskId = "003", type = TaskType.ReviveCount, targetValue = 5, currentValue = 2, rewardStars = 2, state = TaskState.InProgress },
                new TaskItemData { taskId = "004", type = TaskType.ClearGetCoins, targetValue = 500, currentValue = 500, rewardStars = 20, state = TaskState.Collected }            
            }; 
            
            EvaluatePrizeStates();
        }

        public void AddStarsFromTask(int starsToAdd)
        {
            CurrentStars += starsToAdd;
            EvaluatePrizeStates();
            OnDataUpdated?.Invoke();
        }

        public void ClaimPrize(int index)
        {
            if (PrizeStates[index] == 1)
            {
                PrizeStates[index] = 2;
                OnDataUpdated?.Invoke();
            }
        }

        // 新增：处理看完广告的逻辑
        public void WatchAdComplete(int rewardStars)
        {
            HasWatchedAd = true;
            CurrentStars += rewardStars;
            EvaluatePrizeStates();
            OnDataUpdated?.Invoke();
        }

        private void EvaluatePrizeStates()
        {
            for (int i = 0; i < PrizeRequires.Length; i++)
            {
                if (PrizeStates[i] == 0 && CurrentStars >= PrizeRequires[i])
                {
                    PrizeStates[i] = 1; 
                }
            }
        }
    }
}