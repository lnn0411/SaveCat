using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig")]
public class LevelConfigSO : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject headPrefab;

    public GameObject tailPrefab;
    public GameObject segmentPrefab;
    public GameObject catPrefab;

    [Header("Dragon Settings")]
    public float chaseSpeed = 0.15f;
    public float normalSpeed = 0.05f;
    public float chaseEndThresholdT = 0.35f; // 当龙头接近终点的这个T值时，结束追逐状态
    public float spacing = 0.03f; // 每个Segment之间的间距
    public int initialSegmentCount = 10; // 初始Segment数量

    [Header("Dragon Logic Settings")]
    public BlockType[] dragonBodyTypes; 

    public int maxWidth = 10;
    public int maxHeight = 10;
    public int targetBlockCount = 20;

    [Header("Block specs")]
    public BlockSpec[] blockSpecs; //不同类型的方块规格
}

//方块
[System.Serializable]
public class BlockSpec
{
    public string id;
    public int gridLength; //占据的格子长度
    public int strengthCount; //弹药
    public GameObject prefab;
}
