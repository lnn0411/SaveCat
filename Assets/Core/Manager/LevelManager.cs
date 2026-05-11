using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// 管卡总控制器
/// 负责乘胜发牌池
/// 负责监听游戏的胜利与失败
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    [Header("Current Level")]
    public LevelConfigSO currentLevelConfig; //当前关卡配置

    //总方块数
    private int totalBlocksToSpawn;
    // 剩下的龙节段数
    private int remainingDragonSegments;

    //获取场景内现成的依赖引用
    public Camera mainUIBottomCamera;


    #region 开始游戏
    public void StartLevel(LevelConfigSO config)
    {
        currentLevelConfig = config;
        totalBlocksToSpawn = config.targetBlockCount;
        remainingDragonSegments = totalBlocksToSpawn;

        //生成 1：1的颜色池
        List<BlockType> colorPool = GenerateColorPool(totalBlocksToSpawn, config.dragonBodyTypes);

        //颜色打乱
        List<BlockType> boardColorPool = ShuffleList(colorPool);

        //确保底下引用存在
        if(BlockManager.Instance.bottomCamera == null && mainUIBottomCamera != null)
        {
            BlockManager.Instance.bottomCamera = mainUIBottomCamera;
        }

        //生成方块 拿到实际摆放的方块数据（包含长度等信息，方便上级生成龙）
        List<BlockData> actualBoardBlocks = BlockManager.Instance.InitLevel(config, boardColorPool);

        //根据实际摆放方块的长度倍数，将其展开为龙的颜色列表
        List<BlockType> dragonColorPool = new List<BlockType>();

        foreach(BlockData block in actualBoardBlocks)
        {
            //长度是几 就生成几节颜色的身体
            for(int i = 0; i < block.Length; i++)
            {
                dragonColorPool.Add(block.Type);
            }
        }

        //---------------------------------可选是否重新洗牌增加难度
        List<BlockType> shuffledDragonColorPool = ShuffleList(dragonColorPool);

        // 获取路径预制体 后期可以放在管卡配置里面
        SplineContainer targetSpline = FindObjectOfType<SplineContainer>();
        // 龙和猫咪出现
        DragonManager.Instance.Init(config, targetSpline, shuffledDragonColorPool);

        //防守方血量
        remainingDragonSegments = shuffledDragonColorPool.Count;
        Debug.Log($"[LevelManager] 关卡开始！总方块数: {totalBlocksToSpawn}, 龙的节数: {remainingDragonSegments}");
        
    }

    #endregion

    #region 发牌算法
    /// <summary>
    /// 根据管卡配置生成指定数量的发牌池
    /// </summary>
    /// <param name="count"></param>
    /// <param name="availableTypes"></param>
    /// <returns></returns>
    private List<BlockType> GenerateColorPool(int count, BlockType[] availableTypes)
    {
        List<BlockType> pool = new List<BlockType>();
        int typesCount = availableTypes.Length;

        for (int i = 0; i < count; i++)
        {
            BlockType selectedType = availableTypes[i % typesCount];
            pool.Add(selectedType);
        }
        return pool;
        
    }

    //洗牌算法 Inside-Out Algorithm 空间复杂度为On，时间复杂度为On 适合用于不知道长度的洗牌
    private List<T> ShuffleList<T>(IReadOnlyList<T> source)
    {   
        // 开辟一个新的数组、列表来存储生成的序列
        List<T> shuffled = new List<T>(new T[source.Count]);

        //从前往后扫描数据  随即交换
        for(int i = 0; i < source.Count; i++)
        {
            //随机生成一个0到i之间的整数
            int j = Random.Range(0, i + 1);

            //如果j不等于i，说明要把之前生成的第j个元素放到当前i的位置
            if(j != i)
            {
                shuffled[i] = shuffled[j];
            }
            //把当前元素放到第j个位置
            shuffled[j] = source[i];
        }
        
        return shuffled;
    }
    #endregion

}
