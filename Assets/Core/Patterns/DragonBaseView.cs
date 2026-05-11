using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 龙节点试图的抽象基类 用于统一头、身体、尾的表现层更新逻辑
public abstract class DragonBaseView : MonoBehaviour
{
    /// <summary>
    /// 更新模型的位置与旋转（所有派生类必须自行实现）
    /// </summary>
    public abstract void UpdatePositionAndRotation(Vector3 position, Quaternion rotation);

    /// <summary>
    /// 设置颜色接口。使用 virtual 虚函数而不是抽象函数，
    /// 因为有些部位（如龙头/龙尾）可能不需要变色，派生类按需重写即可。
    /// </summary>
    public virtual void SetColor(Color color)
    {
        // 基类留空，默认不执行变色逻辑
    }
}
