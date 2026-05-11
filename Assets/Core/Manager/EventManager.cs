using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局事件中心
/// </summary>
public class EventManager
{
    //核心存储容器
    private static readonly Dictionary<int, Delegate> m_EventTable = new Dictionary<int, Delegate>();

    #region 注册监听
    /// <summary>
    /// 无参数事件监听
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="listener"></param>
    public static void AddListener(int eventId, Action listener)
    {
        OnListenerAdding(eventId, listener);
        m_EventTable[eventId] = (Action)m_EventTable[eventId] + listener;
    }

    /// <summary>
    /// 一个参数事件监听
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="listener"></param>
    public static void AddListener<T>(int eventId, Action<T> listener)
    {
        OnListenerAdding(eventId, listener);
        m_EventTable[eventId] = (Action<T>)m_EventTable[eventId] + listener;
    }

    /// <summary>
    /// 两个参数事件监听
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="listener"></param>
    public static void AddListener<T1, T2>(int eventId, Action<T1, T2> listener)
    {
        OnListenerAdding(eventId, listener);
        m_EventTable[eventId] = (Action<T1, T2>)m_EventTable[eventId] + listener;
    }
    /// <summary>
    /// 三个参数事件监听
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="listener"></param>
    /// 
    public static void AddListener<T1, T2, T3>(int eventId, Action<T1, T2, T3> listener)
    {
        OnListenerAdding(eventId, listener);
        m_EventTable[eventId] = (Action<T1, T2, T3>)m_EventTable[eventId] + listener;
    }
    #endregion

    #region 移除监听
    /// <summary>
    /// 无参数事件移除监听
    /// </summary>
    public static void RemoveListener(int eventId, Action listener)
    {
        if(OnListenerRemoving(eventId, listener))
        {
            m_EventTable[eventId] = (Action)m_EventTable[eventId] - listener;
        }
    }

    /// <summary>
    /// 1个参数事件移除监听
    /// </summary>
    public static void RemoveListener<T>(int eventId, Action<T> listener)
    {
        if(OnListenerRemoving(eventId, listener))
        {
            m_EventTable[eventId] = (Action<T>)m_EventTable[eventId] - listener;
        }
    }

    /// <summary>
    /// 2个参数事件移除监听
    /// </summary>
    public static void RemoveListener<T1, T2>(int eventId, Action<T1, T2> listener)
    {
        if(OnListenerRemoving(eventId, listener))
        {
            m_EventTable[eventId] = (Action<T1, T2>)m_EventTable[eventId] - listener;
        }
    }

    /// <summary>
    /// 3个参数事件移除监听
    /// </summary>
    public static void RemoveListener<T1, T2, T3>(int eventId, Action<T1, T2, T3> listener)
    {
        if(OnListenerRemoving(eventId, listener))
        {
            m_EventTable[eventId] = (Action<T1, T2, T3>)m_EventTable[eventId] - listener;
        }
    }



    #endregion

    #region 触发事件

    /// <summary>
    /// 无参
    /// </summary>
    /// <param name="eventId"></param>
    public static void Broadcast(int eventId)
    {
        if(m_EventTable.TryGetValue(eventId, out var d))
        {
            if(d is Action action) action.Invoke();
            else Debug.LogError($"[EventManager] 事件 {eventId} 的委托类型不匹配! 现有: {d.GetType()}");
        }
    }

    /// <summary>
    /// 1参
    /// </summary>
    /// <param name="eventId"></param>
    public static void Broadcast<T>(int eventId, T arg)
    {
        if(m_EventTable.TryGetValue(eventId, out var d))
        {
            if(d is Action<T> action) action.Invoke(arg);
            else Debug.LogError($"[EventManager] 事件 {eventId} 的委托类型不匹配! 现有: {d.GetType()}");
        }
    }

    /// <summary>
    /// 2参
    /// </summary>
    /// <param name="eventId"></param>
    public static void Broadcast<T1, T2>(int eventId, T1 arg1, T2 arg2)
    {
        if(m_EventTable.TryGetValue(eventId, out var d))
        {
            if(d is Action<T1, T2> action) action.Invoke(arg1, arg2);
            else Debug.LogError($"[EventManager] 事件 {eventId} 的委托类型不匹配! 现有: {d.GetType()}");
        }
    }

    /// <summary>
    /// 3参
    /// </summary>
    /// <param name="eventId"></param>
    public static void Broadcast<T1, T2, T3>(int eventId, T1 arg1, T2 arg2, T3 arg3)
    {
        if(m_EventTable.TryGetValue(eventId, out var d))
        {
            if(d is Action<T1, T2, T3> action) action.Invoke(arg1, arg2, arg3);
            else Debug.LogError($"[EventManager] 事件 {eventId} 的委托类型不匹配! 现有: {d.GetType()}");
        }
    }

    #endregion

    #region 内部方法

    //安全校验 防止将不同类型委托加在同一个Key上
    private static void OnListenerAdding(int eventId, Delegate listenerBegingAdded)
    {
        //新事件 添加
        if(!m_EventTable.ContainsKey(eventId))
        {
            m_EventTable.Add(eventId, null);
        }

        Delegate d = m_EventTable[eventId];

        if(d != null && d.GetType() != listenerBegingAdded.GetType())
        {
            Debug.LogError($"[EventManager] 尝试为事件 {eventId} 添加不同类型的委托! 现有: {d.GetType()}");
        }

    }

    //移除
    private static bool OnListenerRemoving(int eventId, Delegate listenerBeingRemoved)
    {
        if(m_EventTable.TryGetValue(eventId, out var d))
        {
            if(d == null) return false;
            if(d.GetType() != listenerBeingRemoved.GetType())
            {
                Debug.LogError($"[EventManager] 尝试为事件 {eventId} 移除不同类型的委托! 现有: {d.GetType()}");
                return false;
            }
            return true;
        }

        return false;
    }
    #endregion
}
