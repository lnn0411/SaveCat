using System;
using UnityEngine;


// 龙的每个Segment的View，负责显示和动画等视觉效果
public class DragonSegmentView:DragonBaseView
{
    // 核心数据标志位
    public BlockType CurrentType { get; private set; }

    [SerializeField] private MeshRenderer _renderer;
    private static MaterialPropertyBlock _mpb;

    [Header("Break Effect")]
    [SerializeField] private GameObject breakPrefab; // 拖 DragonSegmentSplice
    [SerializeField] private float breakLifeTime = 1.2f;
    [SerializeField] private float explosionForce = 0.05f;
    [SerializeField] private float upwardForce = 0.01f;
    [SerializeField] private float torqueForce = 4f;

    // 生成时的初始化
    public void InitializeData(BlockType type)
    {
        CurrentType = type;
        
        // 表现层的工作：根据接收到的数据枚举，去查表或用 switch 来换色
        Color displayColor = GetColorByType(type);
        SetColor(displayColor);
    }

    //颜色对应转换
    private Color GetColorByType(BlockType type)
    {
        // 简易查表
        switch (type)
        {
            case BlockType.Red: return Color.red;
            case BlockType.Blue: return Color.blue;
            case BlockType.Green: return Color.green;
            case BlockType.Yellow: return Color.yellow;
            case BlockType.Purple: return new Color(0.5f, 0, 0.5f);
            default: return Color.white;
        }
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
        if (breakPrefab != null)
        {
            // 当前位置实例化出来，并设置颜色 位置和旋转用当前龙身体的位置和旋转
            GameObject breakObj = Instantiate(
                breakPrefab,
                transform.position,
                transform.rotation
            );
            // 世界缩放
            breakObj.transform.localScale = transform.lossyScale;
            // 碎片颜色设置成当前身体颜色
            ApplyCurrentColorToFragments(breakObj);
            // 施加力
            ExplodeFragments(breakObj);

            Destroy(breakObj, breakLifeTime);
        }

        recycleRoot?.Invoke();
    }

    private void ExplodeFragments(GameObject breakObj)
    {   
        // 记录爆炸中心和每个碎片的刚体
        Vector3 center = transform.position;
        Rigidbody[] bodies = breakObj.GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in bodies)
        {
            rb.gameObject.SetActive(true);
            // 刚体属性 解除冻结
            rb.constraints = RigidbodyConstraints.None;
            //控制下质量
            rb.mass = 1f;
            //恢复物理系统对刚体的控制，开启重力，重置速度
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // 计算碎片从中心往外的方向
            Vector3 dir = (rb.worldCenterOfMass - center).normalized;
            if (dir.sqrMagnitude < 0.01f)
            {
                // 方向太小 不可靠 随机一个方向
                dir = UnityEngine.Random.onUnitSphere;
            }

            //瞬间的力
            rb.AddForce((dir + Vector3.up * upwardForce) * explosionForce, ForceMode.Impulse);
            // 随即旋转
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
        }
    }
    // 碎片变色
    private void ApplyCurrentColorToFragments(GameObject breakObj)
    {
        Color color = GetColorByType(CurrentType);
        MeshRenderer[] renderers = breakObj.GetComponentsInChildren<MeshRenderer>(true);

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        foreach (MeshRenderer renderer in renderers)
        {
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
        }
    }

    #endregion
}
