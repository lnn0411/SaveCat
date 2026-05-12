using UnityEngine;

/// <summary>
/// 战斗匹配中心。
/// 负责把底槽弹药和龙节段进行颜色匹配，并按固定频率自动攻击。
/// 当前版本先不做弹道表现，只做延迟后的逻辑命中。
/// </summary>
public class MatchManager : Singleton<MatchManager>
{
    #region Inspector Config

    [Header("Attack Timing")]
    [SerializeField] private float firstShotDelay = 0.25f;
    [SerializeField] private float shotInterval = 0.45f;

    #endregion

    #region Runtime State

    private bool _isRunning;
    private float _nextShotTime;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        EventManager.AddListener(EventID.OnBlockSlotted, OnBlockSlotted);
        EventManager.AddListener(EventID.OnDragonSegmentHit, OnDragonSegmentHit);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener(EventID.OnBlockSlotted, OnBlockSlotted);
        EventManager.RemoveListener(EventID.OnDragonSegmentHit, OnDragonSegmentHit);
    }

    private void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        if (Time.time < _nextShotTime)
        {
            return;
        }

        bool didShoot = TryProcessOneMatch();

        if (didShoot)
        {
            _nextShotTime = Time.time + shotInterval;
        }
        else
        {
            _isRunning = false;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 有新弹药入槽后，不立刻攻击，先等待 firstShotDelay，
    /// 让 UI 有时间把颜色和数字展示出来。
    /// </summary>
    private void OnBlockSlotted()
    {
        StartMatchingWithDelay(firstShotDelay);
    }

    /// <summary>
    /// 当前版本 TryProcessOneMatch 已经由 Update 控制节奏。
    /// 这里收到命中事件时，只确保匹配循环仍然处于运行状态。
    /// </summary>
    private void OnDragonSegmentHit()
    {
        if (!_isRunning)
        {
            StartMatchingWithDelay(shotInterval);
        }
    }

    #endregion

    #region Match Flow

    private void StartMatchingWithDelay(float delay)
    {
        _isRunning = true;

        float targetTime = Time.time + Mathf.Max(0f, delay);

        if (_nextShotTime <= Time.time)
        {
            _nextShotTime = targetTime;
        }
        else
        {
            _nextShotTime = Mathf.Min(_nextShotTime, targetTime);
        }
    }

    /// <summary>
    /// 处理一发攻击。
    /// 成功攻击返回 true；没有弹药或没有同色目标返回 false。
    /// </summary>
    private bool TryProcessOneMatch()
    {
        if (SlotManager.Instance == null || DragonManager.Instance == null)
        {
            return false;
        }
        // 是否有弹药
        bool hasAmmo = SlotManager.Instance.TryGetFirstAmmo(out int slotIndex, out SlotData ammoData);

        if (!hasAmmo || ammoData == null)
        {
            return false;
        }
        // 是否有目标
        bool hasTarget = DragonManager.Instance.TryFindFrontMostSegment(
            ammoData.ColorType,
            out DragonSegmentView targetSegment
        );

        if (!hasTarget || targetSegment == null)
        {
            return false;
        }
        // 尝试攻击
        bool hitSuccess = DragonManager.Instance.TryHitSegment(targetSegment);

        if (!hitSuccess)
        {
            return false;
        }

        SlotManager.Instance.TryConsumeAmmoAt(slotIndex);

        return true;
    }

    #endregion
}
