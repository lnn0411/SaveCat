
using UnityEngine;


// 龙的每个Segment的View，负责显示和动画等视觉效果
public class DragonSegmentView:DragonBaseView
{
    // 核心数据标志位
    public BlockType CurrentType { get; private set; }

    [SerializeField] private MeshRenderer _renderer;
    private static MaterialPropertyBlock _mpb;

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

}
