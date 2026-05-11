using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI视图基类
/// 分离生命周期
/// 分离绑定逻辑和视图逻辑
/// </summary>
public abstract class BaseView : MonoBehaviour
{
    private bool _isInitialized = false;

    protected virtual void Awake()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }

    //统一初始化入口
    public void Initialize()
    {
        if(_isInitialized) return;

        //组件绑定
        BindComponents();
        // 初始化逻辑
        InitLogic();
        // 注册事件

        _isInitialized = true;
    }

    protected virtual void OnEnable()
    {
        //注册本UI
        if(UIManager.Instance != null)
        {
            UIManager.Instance.RegisterView(this);
        }
        RegisterEvents();
    }

    protected virtual void OnDisable()
    {
        //注销本UI
        if(UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterView(this);
        }
        UnregisterEvents();
    }

    public virtual void Show()
    {
        this.gameObject.SetActive(true);
        OnShow();

    }

    public virtual void Hide()
    {
        OnHide();
        this.gameObject.SetActive(false);
    }


    #region 子类必须实现的接口

    protected abstract void BindComponents(); //绑定组件

    protected abstract void InitLogic(); //初始化逻辑

    #endregion

    #region 可选重写的接口

    /// <summary> 面板彻底开启后的业务处理（子类可选实现，如：刷新网络数据，重置倒计时） </summary>
    protected virtual void OnShow() { }

    /// <summary> 面板关闭前的善后处理（子类可选实现，如：发送关闭统计日志，清理临时列表） </summary>
    protected virtual void OnHide() { }

    protected virtual void RegisterEvents() { } //注册事件
    protected virtual void UnregisterEvents() { } //注销事件

    public virtual void OnUpdateView(float deltaTime) { } //每帧更新UI

    #endregion


}
