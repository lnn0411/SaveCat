using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;
using System;


/// <summary>
/// 方块的视图层表现 (View)。
/// 只负责更新材质、提供交互碰撞盒、播放自身动画。它不作任何越界/合法性判定。
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BlockView : MonoBehaviour
{
    //本实体对应的数据模型
    public BlockData Data { get; private set; }

    //射线检测用的碰撞盒子
    private BoxCollider boxCollider;
    //模型渲染器 不同的颜色
    private Renderer meshRenderer;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }
#region 对外接口

    /// <summary>
    /// 初始化试图，外部发牌器生成时调用
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cellSize"></param>
    public void InitView(BlockData data, float cellSize = 1.0f)
    {
        this.Data = data;
        this.gameObject.name = $"Block_{data.Id}_{data.Type}";

        //设置可视模型的颜色
        SetColorByType(data.Type);

        //根据数据修改自身朝向
        SetRotationByDirection(data.Dir);

        //根据数据修改碰撞盒的尺寸和位置
        transform.localScale = Vector3.one;

        //碰撞盒中心需要根据长度和朝向进行调整，使其覆盖整个方块
        boxCollider.size = new Vector3(1f, 1f, data.GridLength);
        boxCollider.center = Vector3.zero;

        //根据网格坐标 换算3D世界模型
        // 假定场景的零点在左下角，这里我们把网格索引乘以实体大小

        Vector3 tailPosition = new Vector3(data.GridX * cellSize, 0, data.GridY * cellSize);
        // 由于localscale的机制不是往前延长 而是中心向两旁发散，导致我们需要根据朝向把模型往前挪动半个长度，才能让碰撞盒正确覆盖
        // Unity 中心对齐放大，导致模型向 Local Z 轴反方向多延伸出了 ((Length - 1) / 2) 格的距离。
        // 我们必须把这个模型，顺着它的箭头方向(前进方向)硬推回去。
        float forwardOffset = (data.GridLength - 1) * 0.5f * cellSize;
        // 算出推过去的最终 3D 坐标
        Vector3 alignedWorldPos = tailPosition + transform.forward * forwardOffset;
        transform.position = alignedWorldPos;
    }


    /// <summary>
    /// 被阻挡时播放的受挫提醒
    /// </summary>
    public void PlayBlockedFeedback(int availableSteps)
    {
        Debug.Log($"Block {Data.Id} is blocked!前方还有 {availableSteps} 格，播放撞墙反馈.");
        //动画期间关闭碰撞
        boxCollider.enabled = false;

        //计算我们要冲刺的目的地 盒子宽度为1.0  最后进入0.25制造挤压感
        float travelDist = availableSteps * 1.0f + 0.1f;
        Vector3 originalPos = transform.position;
        //碰撞点
        Vector3 hitPos = originalPos + transform.forward * travelDist;

        //构建DOTween动画序列：先冲刺到碰撞点 然后弹回原位
        Sequence seq = DOTween.Sequence();

        //往前冲撞 利用距离算出时间 保证不同远近速度都是一致的快
        float speed = 5.0f; //每秒5单位
        float travelTime = Mathf.Max(0.05f, travelDist / speed); //最短0.05秒，避免过快看不清
        // 动画1 到达目的地
        seq.Append(transform.DOMove(hitPos, travelTime).SetEase(Ease.OutQuad));
        // 动画2 抖动
        seq.Append(transform.DOShakePosition(0.15f, strength: new Vector3(0.2f,0,0.2f), vibrato: 15));
        //动画3 回弹到原位
        seq.Append(transform.DOMove(originalPos, travelTime * 0.8f).SetEase(Ease.InQuad));

        // 4. 动画彻底结束后，把碰撞盒开回来，允许下次点击
        seq.OnComplete(() => {
            boxCollider.enabled = true;
        });
        Debug.Log($"Block {Data.Id} is blocked! Bumping forward {availableSteps} steps.");

    }

    /// <summary>
    /// 槽满时的播放反馈
    /// </summary>
    public void PlaySlotFullFeedback()
    {
        Debug.Log($"Block {Data.Id} cannot escape because slots are full.");
        if(boxCollider != null) boxCollider.enabled = false;
        transform.DOKill();
        Vector3 originalScale = transform.localScale;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * 1.08f, 0.08f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOShakePosition(
            duration: 0.16f,
            strength: new Vector3(0.12f,0,0.12f),
            vibrato: 12
        ));
        seq.Append(transform.DOScale(originalScale, 0.08f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            if(boxCollider != null) boxCollider.enabled = true;
            transform.localScale = originalScale;
        });
        
    }

    /// <summary>
    /// 获取正确的逃离终点坐标后 播放飞出动画
    /// </summary>
    /// <param name="targetSlotPosition"></param>
    /// <param name="onComplete"></param>
    public void PlayEscapeAnimation(Vector3 targetSlotPosition, System.Action onComplete)
    {
        //避免沿途碰撞
        if(boxCollider != null) boxCollider.enabled = false;
        Debug.Log($"Block {Data.Id} is escaping to {targetSlotPosition}! Playing escape animation.");
        //停止身上可能那种抖动动画
        transform.DOKill();

        //设定冲刺事件，飞出时长根据距离算，保证无论远近飞行速度都差不多
        float escapeTime = 0.3f;

        //动画序列
        Sequence seq = DOTween.Sequence();

        //往后缩一点点 蓄力准备
        seq.Append(transform.DOMove(transform.position - transform.forward * 0.3f, 0.1f).SetEase(Ease.InQuad));
        //然后飞出到目标点
        seq.Append(transform.DOMove(targetSlotPosition, escapeTime).SetEase(Ease.OutQuad));
        
        seq.onComplete += () => {
            Debug.Log($"Block {Data.Id} has completed escape animation!");
            //动画结束后回调上层 让它处理回收和能量增加
            onComplete?.Invoke();
        };
    }

    /// <summary>
    /// 按多端路径播放逃逸动画
    /// 先离开棋盘，再沿边缘转向槽位的路线
    /// </summary>
    /// <param name="pathPoints"></param>
    /// <param name="onComplete"></param>
    public void PlayEscapePathAnimation(Vector3[] pathPoints, Action onComplete)
    {
        if(pathPoints == null || pathPoints.Length == 0)
        {
            //如果槽位为空 用旧的
            PlayEscapeAnimation(transform.position + transform.forward * 20f, onComplete);
            return;
        }

        if(boxCollider != null) boxCollider.enabled = false;

        transform.DOKill();
        Sequence seq = DOTween.Sequence();
        Vector3 currentPoint = transform.position;
        // 移动速度
        const float moveSpeed = 8f;
        // 转向的时间
        const float turnDuration = 0.08f;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 nextPoint = pathPoints[i];
            
            Vector3 moveVector = nextPoint - currentPoint;
            moveVector.y = 0f;

            if (moveVector.sqrMagnitude <= 0.0001f)
            {
                currentPoint = nextPoint;
                continue;
            }
            // 逃逸前要看向终点
            Quaternion targetRotation = Quaternion.LookRotation(moveVector.normalized, Vector3.up);
            float moveDuration = Mathf.Max(0.08f, Vector3.Distance(currentPoint, nextPoint) / moveSpeed);

            seq.Append(transform.DORotateQuaternion(targetRotation, turnDuration).SetEase(Ease.OutSine));
            seq.Append(transform.DOMove(nextPoint, moveDuration).SetEase(Ease.Linear));

            currentPoint = nextPoint;
        }
        seq.onComplete += () =>
        {
            onComplete?.Invoke();
        };
    }

#endregion


#region 辅助工具

    //颜色转换工具 根据方块类型设置材质颜色
    private void SetColorByType(BlockType type)
    {
        if(meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component is missing on BlockView.");
            return;
        }
        Color color = BlockColorUtility.GetColor(type);

        meshRenderer.material.color = color;
    }

    //根据方块的朝向设置模型的旋转
    private void SetRotationByDirection(Direction dir)
    {
        transform.rotation = DirectionUtility.ToRotation(dir);
    }

#endregion
}
