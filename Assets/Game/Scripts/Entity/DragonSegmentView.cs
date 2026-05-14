using System;
using UnityEngine;
using System.Collections;

// 龙的每个Segment的View，负责显示和动画等视觉效果
public class DragonSegmentView:DragonBaseView
{
    // 核心数据标志位
    public BlockType CurrentType { get; private set; }

    [SerializeField] private MeshRenderer _renderer;
    private static MaterialPropertyBlock _mpb;

    [Header("Pooled Physics Break Effect")]
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private int shardCount = 10; //默认数量
    [SerializeField] private float shardLifeTime = 1.7f; //生存时间
    [SerializeField] private float shardForce = 1.0f; //飞散力度
    [SerializeField] private float shardUpForce = 0.8f; //向上力度
    [SerializeField] private float shardTorque = 6f; //旋转力度
    [SerializeField] private float shardScale = 0.35f; //缩放比例
    [SerializeField] private float shardSpawnRadius = 0.12f; //生成半径

    // 生成时的初始化
    public void InitializeData(BlockType type)
    {
        CurrentType = type;
        
        // 表现层的工作：根据接收到的数据枚举，去查表或用 switch 来换色
        Color displayColor = BlockColorUtility.GetColor(CurrentType);
        SetColor(displayColor);
    }


    //设置颜色
    public override void SetColor(Color color)
    {
        if (_renderer == null) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", color); // URP材质用 _BaseColor，Standard 用 _Color
        _renderer.SetPropertyBlock(_mpb);
    }
    
    // 被Manager每帧调用 性能优化：避免每帧都调用transform.position和transform.rotation
    public override void UpdatePositionAndRotation(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    #region 破碎特效

    // 外界调用的
    public void PlayBreakAndRecycle(Action recycleRoot)
    {
        SpawnPhysicsShards();

        recycleRoot?.Invoke();
    }

    private void SpawnPhysicsShards()
    {
        if (shardPrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        Color color = BlockColorUtility.GetColor(CurrentType);

        for (int i = 0; i < shardCount; i++)
        {
            Vector3 offset = UnityEngine.Random.insideUnitSphere * shardSpawnRadius;
            offset.y = Mathf.Abs(offset.y);

            //随机位置在龙节周围的一个小球范围内，保证不会生成在地面以下
            Vector3 spawnPos = transform.position + offset;

            Vector3 dir = UnityEngine.Random.onUnitSphere;
            dir.y = Mathf.Abs(dir.y) + shardUpForce;

            Vector3 impulse = dir.normalized * shardForce;
            Vector3 torque = UnityEngine.Random.insideUnitSphere * shardTorque;

            GameObject shardObj = PoolManager.Instance.Get(
                shardPrefab,
                spawnPos,
                UnityEngine.Random.rotation,
                null
            );

            DragonShardView shard = shardObj.GetComponent<DragonShardView>();

            if (shard == null)
            {
                PoolManager.Instance.Recycle(shardObj);
                continue;
            }

            shard.Play(
                color,
                spawnPos,
                transform.rotation,
                Vector3.one * shardScale,
                impulse,
                torque,
                shardLifeTime,
                PoolManager.Instance.Recycle
            );
        }
    }


    #endregion
}
