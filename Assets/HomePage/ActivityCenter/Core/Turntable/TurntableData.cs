using System;
using UnityEngine;

namespace ActivityCenter.Turntable
{
    public class TurntableData
    {
        public event Action OnDataUpdated;

        // 常量配置
        public const int LevelsPerFreeDraw = 10;
        public const int DrawCost = 600;
        public const int MaxAdDrawsPerDay = 3;

        // 核心数据
        public int UsedLevelsForDraw { get; private set; } // 已用于免费抽奖的关卡数
        public int AdDrawsUsedToday { get; private set; }  // 今日已看广告抽奖的次数
        
        // 模拟当前关卡数，实际项目中请从你的关卡系统中获取
        private int _currentLevel = 35; 

        public TurntableData()
        {
            // 这里通常从本地存档或服务器读取数据
            UsedLevelsForDraw = 0; 
            LoadFromPrefs();
            // _currentLevel = LevelManager.Instance.CurrentLevel;
        }

        private void LoadFromPrefs()
        {
            string lastDate = PlayerPrefs.GetString("Turntable_AdDrawsLastDate", "");
            string today = DateTime.Today.ToString("yyyy-MM-dd");

            if (lastDate == today)
            {
                AdDrawsUsedToday = PlayerPrefs.GetInt("Turntable_AdDrawsUsedToday", 0);
            }
            else
            {
                AdDrawsUsedToday = 0;
            }
        }

        // 计算属性：是否可以免费抽奖
        public bool CanFreeDraw => (_currentLevel - UsedLevelsForDraw) >= LevelsPerFreeDraw;

        // 计算属性：距离下次免费抽奖还差几关
        public int LevelsUntilNextFreeDraw
        {
            get
            {
                if (CanFreeDraw) return 0;
                return LevelsPerFreeDraw - ((_currentLevel - UsedLevelsForDraw) % LevelsPerFreeDraw);
            }
        }

        public bool CanAdDraw => AdDrawsUsedToday < MaxAdDrawsPerDay;

        public void RecordFreeDraw()
        {
            UsedLevelsForDraw += LevelsPerFreeDraw;
            OnDataUpdated?.Invoke();
        }

        public void RecordPaidDraw()
        {
            // 扣除金币的逻辑在Controller中判断，这里仅触发UI刷新
            OnDataUpdated?.Invoke();
        }

        public void RecordAdDraw()
        {
            AdDrawsUsedToday++;
            SaveToPrefs();
            OnDataUpdated?.Invoke();
        }

        private void SaveToPrefs()
        {
            PlayerPrefs.SetInt("Turntable_AdDrawsUsedToday", AdDrawsUsedToday);
            PlayerPrefs.SetString("Turntable_AdDrawsLastDate", DateTime.Today.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }
    }
}
