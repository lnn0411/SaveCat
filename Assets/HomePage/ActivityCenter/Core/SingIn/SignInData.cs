using System;

namespace ActivityCenter.SignIn
{
    public class SignInData
    {
        // 数据更新事件，供Controller监听
        public event Action OnDataUpdated;

        // 核心数据
        public int CurrentDayIndex { get; private set; } // 0-6 对应第1天到第7天
        public bool[] SignedInDays { get; private set; } 
        public bool HasSignedInToday { get; private set; }

        public SignInData()
        {
            // 这里通常是从本地（如PlayerPrefs）或服务器读取数据进行初始化
            // 此处使用模拟数据
            SignedInDays = new bool[7];
            CurrentDayIndex = 0; 
            HasSignedInToday = false; 
        }

        // 签到逻辑
        public void ExecuteSignIn(int dayIndex, bool isDoubleReward)
        {
            if (dayIndex == CurrentDayIndex && !HasSignedInToday)
            {
                SignedInDays[dayIndex] = true;
                HasSignedInToday = true;
                
                // TODO: 结合实际系统，将奖励发放给玩家（单倍或双倍）
                
                // 数据更新完毕，通知外部
                OnDataUpdated?.Invoke();
            }
        }
    }
}