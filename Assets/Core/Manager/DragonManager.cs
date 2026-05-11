using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class DragonManager : Singleton<DragonManager>
{
    //读取配置的地方
    private LevelConfigSO _config;
    // 地图曲线
    private SplineContainer _targetSpline;

    //龙身体的颜色池
    private List<BlockType> _dragonColorPool;

    private float _globalTimeT = 0f;
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

#region 初始化
    /// <summary>
    /// 唯一初始化入口，由外部的控制器 (如 GameRoot 或 LevelManager) 以代码调用
    /// </summary>
    public void Init(LevelConfigSO config, SplineContainer spline, List<BlockType> dragonBodyTypes)
    {
        _config = config;
        _targetSpline = spline;
        _dragonColorPool = dragonBodyTypes;
        _globalTimeT = 0f;
        _currentState = DragonState.Chasing;

        if (_config == null || _targetSpline == null)
        {
            Debug.LogError("[DragonManager] 初始化失败：配置或路径为空");
            return;
        }
        //每次Init清空旧数据
        _activeSegments.Clear();

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
        // 获取配置长度
        int segmentCountToSpawn = _dragonColorPool.Count;

        for (int i = 0; i < _config.initialSegmentCount; i++)
        {
            GameObject go = PoolManager.Instance.Get(_config.segmentPrefab, Vector3.zero, Quaternion.identity, this.transform);
            DragonSegmentView view = go.GetComponent<DragonSegmentView>();
            if (view != null)
            {
                BlockType segmentType = _dragonColorPool[i]; // 从配置的颜色池中获取对应位置的颜色
                
                // 叫 View 把这个类型存起来，并根据这个枚举刷新自己身上的材质颜色
                view.InitializeData(segmentType);

                _activeSegments.Add(view);
            }
        }

        //生成龙尾
        if(_config.tailPrefab != null)
        {
            _tailInstance = PoolManager.Instance.Get(_config.tailPrefab, Vector3.zero, Quaternion.identity, this.transform);
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
#endregion

#region 游戏循环

    void Update()
    {
        if (_activeSegments.Count == 0 || _targetSpline == null || _currentState == DragonState.GameOver) return;

        float currentSpeed = _config.normalSpeed;

        if (_currentState == DragonState.Chasing)
        {
            currentSpeed = _config.chaseSpeed;
            // 直接用 _globalTimeT 判断是否到达追击阈值
            if (_globalTimeT >= _config.chaseEndThresholdT)
            {
                _globalTimeT = _config.chaseEndThresholdT; // 锁定截断避免超量
                _currentState = DragonState.Pacing;
                Debug.Log("进入匀速尾随！");
            }
        }
        else if (_currentState == DragonState.Pacing)
        {
            currentSpeed = _config.normalSpeed;
            _catTimeT += _config.chaseSpeed * Time.deltaTime; // 猫依旧逃跑
            UpdateCatTransform(_catTimeT);

            // 直接用 _globalTimeT 判断龙头是否到达终点
            if (_globalTimeT >= 0.98f) // 你之前改为了 0.95f
            {
                _globalTimeT = 0.98f;
                _currentState = DragonState.GameOver;
                Debug.Log("防守失败！");
            }
        }

        _globalTimeT += currentSpeed * Time.deltaTime;
        UpdateSegmentsTransform();
    }

    private void UpdateSegmentsTransform()
    {
        // 间隔
        float spacing = _config.spacing;
        
        // 1. 更新龙头：龙头现在是绝对的基准 0 偏移
        if(_headInstance != null)
        {
            UpdateSingleTransform(_headInstance.transform, _globalTimeT);
        }

        // 2. 更新龙的身体：因为龙头占了 0 的位置，所以身体从 1 个 spacing 开始往后排
        for(int i = 0; i < _activeSegments.Count; i++)
        {
            float segmentT = _globalTimeT - ((i + 1) * spacing); 
            UpdateSingleTransform(_activeSegments[i].transform, segmentT);
        }

        // 3. 更新龙尾：排在所有身体之后
        if(_tailInstance != null)
        {
            float tailT = _globalTimeT - ((_activeSegments.Count + 1) * spacing); 
            UpdateSingleTransform(_tailInstance.transform, tailT);
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
}