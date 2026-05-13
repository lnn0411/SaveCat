using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;
using DG.Tweening;
public class DragonManager : Singleton<DragonManager>
{
    //读取配置的地方
    private LevelConfigSO _config;
    // 地图曲线
    private SplineContainer _targetSpline;
    //龙身体的颜色池
    private List<BlockType> _dragonColorPool;

    private DragonState _currentState = DragonState.Chasing;
    // 龙头
    private GameObject _headInstance;
    //龙身体
    private List<DragonSegmentView> _activeSegments = new List<DragonSegmentView>();
    // 龙尾
    private GameObject _tailInstance;

    private catView _catInstance;
    private float _catTimeT = 0f;

    //生成的物体相对于路面的高度
    public float heightOffset = 0.5f;

#region Local Path Time

    // 头、身体、尾分别维护自己的路径时间。 方便精确判断
    private float _headTimeT;
    private float _tailTimeT;
    private readonly List<float> _segmentTimeTs = new List<float>();

#endregion

#region Combat Feedback

    [Header("Combat Feedback")]
    [SerializeField] private float recoilDuration = 0.5f;
    [SerializeField] private float recoilOvershoot = 0.025f;
    [SerializeField] private Ease recoilBackEase = Ease.OutCubic;
    [SerializeField] private Ease recoilReturnEase = Ease.OutSine;

    private Tween _recoilTween;
    private bool _isRecoiling;  // 当前是否处于回退状态
    private int _recoilAffectedBodyCount; // 当前受击的身体数量
    private float _recoilOffsetT; // 当前视觉回退偏移量

    private bool _isVictory;


#endregion

#region 初始化
    /// <summary>
    /// 唯一初始化入口，由外部的控制器 (如 GameRoot 或 LevelManager) 以代码调用
    /// </summary>
    public void Init(LevelConfigSO config, SplineContainer spline, List<BlockType> dragonBodyTypes)
    {
        _config = config;
        _targetSpline = spline;
        // 龙的颜色池
        _dragonColorPool = dragonBodyTypes;
        _currentState = DragonState.Chasing;
        _isVictory = false;

        if (_config == null || _targetSpline == null)
        {
            Debug.LogError("[DragonManager] 初始化失败：配置或路径为空");
            return;
        }

        //重置关卡
        RecycleCurrentDragon();

        _headTimeT = 0f;
        _tailTimeT = 0f;
        _catTimeT = 0f;
        ResetRecoilState();

        SpawnDragonSegments();
        SpawnCat();
    }

    // 生成龙的身体
    private void SpawnDragonSegments()
    {
        //生成龙头
        if(_config.headPrefab != null)
        {
            _headInstance = PoolManager.Instance.Get(_config.headPrefab, Vector3.zero, Quaternion.identity, this.transform);
        }
        else Debug.LogWarning("头部预制体未配置！");

        if (_dragonColorPool == null || _dragonColorPool.Count == 0)
        {
            Debug.LogError("[DragonManager] Dragon color pool is empty.");
            return;
        }
        // 获取配置长度
        int segmentCountToSpawn = _dragonColorPool.Count;

        for (int i = 0; i < segmentCountToSpawn; i++)
        {
            GameObject go = PoolManager.Instance.Get(_config.segmentPrefab, Vector3.zero, Quaternion.identity, this.transform);
            DragonSegmentView view = go.GetComponent<DragonSegmentView>();
            if (view != null)
            {
                BlockType segmentType = _dragonColorPool[i]; // 从配置的颜色池中获取对应位置的颜色
                
                // 叫 View 把这个类型存起来，并根据这个枚举刷新自己身上的材质颜色
                view.InitializeData(segmentType);

                _activeSegments.Add(view);

                // 初始状态：身体依次排在龙头后方。
                float segmentT = _headTimeT - ((i + 1) * _config.spacing);
                //储存身体节点的时间点
                _segmentTimeTs.Add(segmentT);
            }
        }

        //生成龙尾
        if(_config.tailPrefab != null)
        {
            _tailInstance = PoolManager.Instance.Get(_config.tailPrefab, Vector3.zero, Quaternion.identity, this.transform);
            _tailTimeT = _headTimeT - ((_activeSegments.Count + 1) * _config.spacing);

        }
        else Debug.LogWarning("尾部预制体未配置！");
    }

    // 生成猫咪
    private void SpawnCat()
    {
        _catTimeT = _config.chaseEndThresholdT + 0.1f;
        GameObject catObj = PoolManager.Instance.Get(_config.catPrefab, Vector3.zero, Quaternion.identity, this.transform);
        _catInstance = catObj.GetComponent<catView>();
        UpdateCatTransform(_catTimeT);
    }

    //回收Dragon相关的所有实例  包括头身体尾和猫咪 用于关卡重置
    private void RecycleCurrentDragon()
    {
        if(PoolManager.Instance != null) return;
        if(_headInstance != null)
        {
            PoolManager.Instance.Recycle(_headInstance);
            _headInstance = null;
        }
        for(int i = 0; i < _activeSegments.Count; i++)
        {
            if(_activeSegments[i] != null)
            {
                PoolManager.Instance.Recycle(_activeSegments[i].gameObject);
            }
        }
        if(_tailInstance != null)
        {
            PoolManager.Instance.Recycle(_tailInstance);
            _tailInstance = null;
        }
        if(_catInstance != null)
        {
            PoolManager.Instance.Recycle(_catInstance.gameObject);
            _catInstance = null;
        }
        _activeSegments.Clear();
        _segmentTimeTs.Clear();
    }
#endregion

#region 游戏循环

    void Update()
    {
        if ( _targetSpline == null || _currentState == DragonState.GameOver) return;
        //获胜了 可能动画没做完 龙头没有归为 因此继续保持 直到状态变了
        if (_isVictory)
        {
            UpdateSegmentsTransform();
            return;
        }

        float currentSpeed = _config.normalSpeed;

        if (_currentState == DragonState.Chasing)
        {
            currentSpeed = _config.chaseSpeed;
            // 直接用 _globalTimeT 判断是否到达追击阈值
            if (_headTimeT >= _config.chaseEndThresholdT)
            {
                _headTimeT = _config.chaseEndThresholdT; // 锁定截断避免超量
                _currentState = DragonState.Pacing;
                Debug.Log("进入匀速尾随！");
            }
        }
        else if (_currentState == DragonState.Pacing)
        {
            currentSpeed = _config.normalSpeed;
            _catTimeT += _config.chaseSpeed * Time.deltaTime; // 猫逃跑到终点
            UpdateCatTransform(_catTimeT);

            // 直接用 _globalTimeT 判断龙头是否到达终点
            if (_headTimeT >= 0.98f) // 你之前改为了 0.95f
            {
                _headTimeT = 0.98f;
                _currentState = DragonState.GameOver;
                Debug.Log("防守失败！");
            }
        }

        AdvanceDragon(currentSpeed * Time.deltaTime);

        UpdateSegmentsTransform();
    }

    /// <summary>
    /// 正常推进整条龙  无受击
    /// </summary>
    /// <param name="deltaT"></param>
    private void AdvanceDragon(float deltaT)
    {
        _headTimeT += deltaT;
        for(int i = 0; i < _activeSegments.Count; i++) _segmentTimeTs[i] += deltaT;
        _tailTimeT += deltaT;
        
    }

    private void UpdateSegmentsTransform()
    {
        
        // 1. 更新龙头：龙头现在是绝对的基准 0 偏移
        if(_headInstance != null)
        {
            float displayT = _headTimeT;
            if(_isRecoiling) displayT -= _recoilOffsetT; // 如果正在回退，龙头也要被影响
            UpdateSingleTransform(_headInstance.transform, displayT);
        }

        // 2. 更新龙的身体：因为龙头占了 0 的位置，所以身体从 1 个 spacing 开始往后排
        for(int i = 0; i < _activeSegments.Count; i++)
        {
            if(i >= _segmentTimeTs.Count) continue; // 安全检查
            float displayT = _segmentTimeTs[i]; // 身体的显示时间是它自己的路径时间
            if(_isRecoiling && i < _recoilAffectedBodyCount) displayT -= _recoilOffsetT; // 如果正在回退，受影响的身体要被偏移
            
            UpdateSingleTransform(_activeSegments[i].transform, displayT);
        }

        // 3. 更新龙尾：排在所有身体之后
        if(_tailInstance != null)
        {
            UpdateSingleTransform(_tailInstance.transform, _tailTimeT);
        }

    }
    /// <summary>
    /// 通用的单体坐标位置更新      给一个Transform和曲线进度
    /// </summary>
    /// <param name="target"></param>
    /// <param name="localT"></param>
    private void UpdateSingleTransform(Transform target, float localT)
    {
        if(target == null) return;

        localT = Mathf.Clamp01(localT);
        //反算贝塞尔曲线来获得位置和旋转
        SplineUtility.Evaluate(_targetSpline.Spline, localT, out float3 pos, out float3 tang, out float3 up);
        // 由于SplineUtility.Evaluate得到的是局部坐标，所以需要转换到世界坐标
        Vector3 worldPos = _targetSpline.transform.TransformPoint(pos);
        Vector3 worldForward = _targetSpline.transform.TransformDirection(tang);
        Vector3 worldUp = _targetSpline.transform.TransformDirection(up);

        Quaternion rot = Quaternion.identity;
        if(worldForward.sqrMagnitude > Mathf.Epsilon)
            rot = Quaternion.LookRotation(worldForward, worldUp);

        // 现在只需要获取基类组件，统一调用更新！
        DragonBaseView view = target.GetComponent<DragonBaseView>();
        if(view != null)
        {
             view.UpdatePositionAndRotation(worldPos + Vector3.up * heightOffset, rot);
        }
    }

    // 猫咪的位置更新
    private void UpdateCatTransform(float t)
    {
        if (_catInstance == null) return;
        t = Mathf.Clamp01(t);

        SplineUtility.Evaluate(_targetSpline.Spline, t, out float3 pos, out float3 tang, out float3 up);
        Vector3 worldPos = _targetSpline.transform.TransformPoint(pos);
        Vector3 worldForward = _targetSpline.transform.TransformDirection(tang);
        Vector3 worldUp = _targetSpline.transform.TransformDirection(up);

        Quaternion rot = Quaternion.identity;
        if (worldForward.sqrMagnitude > Mathf.Epsilon)
            rot = Quaternion.LookRotation(worldForward, worldUp);

        //偏移量主要是为了让物体在道路上方 而不是中间
        _catInstance.UpdatePositionAndRoatation(worldPos + Vector3.up * heightOffset, rot); // 确保 CatView 里的方法名拼写正确
    }
#endregion


#region 龙的局部反馈

    private void BeginLocalRecoil(int hitIndex)
    {
        if(_config == null) return;

        _isRecoiling = true;
        _recoilAffectedBodyCount = hitIndex;
        _recoilOffsetT = 0f; //回退进度

        // 计算回退距离
        float finalOffset = _config.spacing;
        // 回退超出
        float overshootOffset = finalOffset * (1f + recoilOvershoot);

        Sequence seq = DOTween.Sequence();

        //第一段，快速被打退，超过目标点，产生冲击感
        seq.Append(DOTween.To(
            () => _recoilOffsetT,
            value =>
            {
                _recoilOffsetT = value;
            },
            overshootOffset,
            recoilDuration
        ).SetEase(recoilBackEase));

        //第二段，回退到目标点
        seq.Append(DOTween.To(
            () => _recoilOffsetT,
            value =>
            {
                _recoilOffsetT = value;
            },
            finalOffset,
            recoilDuration * 0.4f
        ).SetEase(recoilReturnEase));

        seq.OnComplete(() =>
        {
            CommitRecoilToLocalTimes(finalOffset);
        });

        _recoilTween = seq;
        

    
    }
    /// <summary>
    /// 更新时间线
    /// </summary>
    /// <param name="finalOffset"></param>
    private void CommitRecoilToLocalTimes(float finalOffset)
    {
        _headTimeT = Mathf.Max(0f, _headTimeT - finalOffset);
        for(int i = 0; i < _recoilAffectedBodyCount &&  i <_segmentTimeTs.Count ; i++)
        {
            _segmentTimeTs[i] = Mathf.Max(0f, _segmentTimeTs[i] - finalOffset);
        }
        ResetRecoilState();

        //保证获胜后继续更新
        if(_isVictory)
        {
            _isVictory = false;
            _currentState = DragonState.GameOver;
            EventManager.Broadcast(EventID.OnLevelVictory);
            
        }
        
    }

    /// <summary>
    /// 如果连续命中发生在回退未结束时，先立即完成上一段回退。
    /// 防止多个回退状态互相覆盖。
    /// </summary>
    private void CompleteRecoilImmediately()
    {
        if(!_isRecoiling) return;
        if (_recoilTween != null && _recoilTween.IsActive())
        {
            _recoilTween.Kill();
        }

        CommitRecoilToLocalTimes(_config.spacing);
    }

    private void ResetRecoilState()
    {
        _isRecoiling = false;
        _recoilAffectedBodyCount = 0;
        _recoilOffsetT = 0f;
        _recoilTween = null;
    }

#endregion


#region 对外接口 
    /// <summary>
    /// 根据颜色 找到最前面的存活龙节段
    /// 后续可能要有遮盖层的概念  比如说有些节段被冰冻了  就不能被找到  只能找到前面一个正常的
    /// </summary>
    /// <param name="type"></param>
    /// <param name="segment"></param>
    /// <returns></returns>
    public bool TryFindFrontMostSegment(BlockType type, out DragonSegmentView segment)
    {
        for(int i = 0; i < _activeSegments.Count; i++)
        {
            DragonSegmentView current = _activeSegments[i];

            if(current == null) continue;

            if(current.CurrentType == type)
            {
                segment = current;
                return true;
            }
        }
        segment = null;
        return false;
    }

    // 尝试命中龙节段 返回是否命中成功
    // 目前是直接移除 后续可以加特效
    public bool TryHitSegment(DragonSegmentView segment)
    {
        if (segment == null)
        {
            return false;
        }

        int hitIndex = _activeSegments.IndexOf(segment);

        if (hitIndex < 0)
        {
            return false;
        }

        // 如果上一段回退还没结束，先把它提交到真实时间，避免叠加时错位。
        CompleteRecoilImmediately();

        _activeSegments.RemoveAt(hitIndex);

        if (hitIndex >= 0 && hitIndex < _segmentTimeTs.Count)
        {
            _segmentTimeTs.RemoveAt(hitIndex);
        }

        segment.PlayBreakAndRecycle(() =>
        {
            PoolManager.Instance.Recycle(segment.gameObject);
        });

        BeginLocalRecoil(hitIndex);

        EventManager.Broadcast(EventID.OnDragonSegmentHit);

        if (_activeSegments.Count <= 0)
        {
            _isVictory = true;
        }

        return true;
    }

    public bool HasAliveSegment()
    {
        return _activeSegments.Count > 0;
    }
#endregion
}