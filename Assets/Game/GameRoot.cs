using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class GameRoot : MonoBehaviour
{
    public LevelConfigSO initialLevelConfig; 

    private void Start()
    {
        Debug.Log("[GameRoot] == 游戏启动开始 ==");

        // 1. 初始化核心基础设施 (Managers) 
        // 架构原则：GameRoot 只负责组件的“装配挂载”，绝不染指具体的业务调用。
        GameObject coreGroup = new GameObject("CoreManagers");
        
        // 对象池与数据底层
        coreGroup.AddComponent<PoolManager>();
        coreGroup.AddComponent<GridMapManager>();
        
        // 交互与发牌
        BlockManager blockMgr = coreGroup.AddComponent<BlockManager>();
        blockMgr.bottomCamera = GameObject.Find("BottomCamera")?.GetComponent<Camera>(); 
        blockMgr.blockLayerMask = LayerMask.GetMask("Blocks");

        // 祖玛龙系统
        GameObject dragonGroup = new GameObject("[DragonManager]");
        dragonGroup.AddComponent<DragonManager>(); // 不需要存变量了，等 LevelManager 内部去查单例即可

        // ======【新增】关卡总指挥官 ======
        LevelManager levelMgr = coreGroup.AddComponent<LevelManager>();
        levelMgr.mainUIBottomCamera = blockMgr.bottomCamera; 
        
        // 2. 加载关卡配置
        if (initialLevelConfig == null)
        {
            initialLevelConfig = Resources.Load<LevelConfigSO>("Configs/Level1");
            if (initialLevelConfig == null)
            {
               Debug.LogError("致命错误：未找到初始关卡配置！");
               return; 
            }
        }

        // 3. ============ 权力移交 ============
        // 所有组件装挂并准备就绪，交给 LevelManager 统一组织一次完整的初始化！
        levelMgr.StartLevel(initialLevelConfig);

        Debug.Log("[GameRoot] == 结构搭建完毕！当前权力已移交 LevelManager ==");
    }
}
