using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件分发中心
/// </summary>
public class EventManager
{
    private static readonly Dictionary<int, Delegate> eventTable = new Dictionary<int, Delegate>();

    #region Add Listener - EventID

    public static void AddListener(EventID eventId, Action listener)
    {
        AddListener((int)eventId, listener);
    }

    public static void AddListener<T>(EventID eventId, Action<T> listener)
    {
        AddListener((int)eventId, listener);
    }

    public static void AddListener<T1, T2>(EventID eventId, Action<T1, T2> listener)
    {
        AddListener((int)eventId, listener);
    }

    public static void AddListener<T1, T2, T3>(EventID eventId, Action<T1, T2, T3> listener)
    {
        AddListener((int)eventId, listener);
    }

    #endregion

    #region Add Listener - int

    public static void AddListener(int eventId, Action listener)
    {
        if (!CanAddListener(eventId, listener)) return;
        eventTable[eventId] = (Action)eventTable[eventId] + listener;
    }

    public static void AddListener<T>(int eventId, Action<T> listener)
    {
        if (!CanAddListener(eventId, listener)) return;
        eventTable[eventId] = (Action<T>)eventTable[eventId] + listener;
    }

    public static void AddListener<T1, T2>(int eventId, Action<T1, T2> listener)
    {
        if (!CanAddListener(eventId, listener)) return;
        eventTable[eventId] = (Action<T1, T2>)eventTable[eventId] + listener;
    }

    public static void AddListener<T1, T2, T3>(int eventId, Action<T1, T2, T3> listener)
    {
        if (!CanAddListener(eventId, listener)) return;
        eventTable[eventId] = (Action<T1, T2, T3>)eventTable[eventId] + listener;
    }

    #endregion

    #region Remove Listener - EventID

    public static void RemoveListener(EventID eventId, Action listener)
    {
        RemoveListener((int)eventId, listener);
    }

    public static void RemoveListener<T>(EventID eventId, Action<T> listener)
    {
        RemoveListener((int)eventId, listener);
    }

    public static void RemoveListener<T1, T2>(EventID eventId, Action<T1, T2> listener)
    {
        RemoveListener((int)eventId, listener);
    }

    public static void RemoveListener<T1, T2, T3>(EventID eventId, Action<T1, T2, T3> listener)
    {
        RemoveListener((int)eventId, listener);
    }

    #endregion

    #region Remove Listener - int

    public static void RemoveListener(int eventId, Action listener)
    {
        if (!CanRemoveListener(eventId, listener)) return;
        eventTable[eventId] = (Action)eventTable[eventId] - listener;
        RemoveEventIfEmpty(eventId);
    }

    public static void RemoveListener<T>(int eventId, Action<T> listener)
    {
        if (!CanRemoveListener(eventId, listener)) return;
        eventTable[eventId] = (Action<T>)eventTable[eventId] - listener;
        RemoveEventIfEmpty(eventId);
    }

    public static void RemoveListener<T1, T2>(int eventId, Action<T1, T2> listener)
    {
        if (!CanRemoveListener(eventId, listener)) return;
        eventTable[eventId] = (Action<T1, T2>)eventTable[eventId] - listener;
        RemoveEventIfEmpty(eventId);
    }

    public static void RemoveListener<T1, T2, T3>(int eventId, Action<T1, T2, T3> listener)
    {
        if (!CanRemoveListener(eventId, listener)) return;
        eventTable[eventId] = (Action<T1, T2, T3>)eventTable[eventId] - listener;
        RemoveEventIfEmpty(eventId);
    }

    #endregion

    #region Broadcast - EventID

    public static void Broadcast(EventID eventId)
    {
        Broadcast((int)eventId);
    }

    public static void Broadcast<T>(EventID eventId, T arg)
    {
        Broadcast((int)eventId, arg);
    }

    public static void Broadcast<T1, T2>(EventID eventId, T1 arg1, T2 arg2)
    {
        Broadcast((int)eventId, arg1, arg2);
    }

    public static void Broadcast<T1, T2, T3>(EventID eventId, T1 arg1, T2 arg2, T3 arg3)
    {
        Broadcast((int)eventId, arg1, arg2, arg3);
    }

    #endregion

    #region Broadcast - int

    public static void Broadcast(int eventId)
    {
        if (!eventTable.TryGetValue(eventId, out Delegate handler)) return;

        if (handler is Action action)
        {
            action.Invoke();
        }
        else
        {
            LogTypeMismatch(eventId, typeof(Action), handler.GetType());
        }
    }

    public static void Broadcast<T>(int eventId, T arg)
    {
        if (!eventTable.TryGetValue(eventId, out Delegate handler)) return;

        if (handler is Action<T> action)
        {
            action.Invoke(arg);
        }
        else
        {
            LogTypeMismatch(eventId, typeof(Action<T>), handler.GetType());
        }
    }

    public static void Broadcast<T1, T2>(int eventId, T1 arg1, T2 arg2)
    {
        if (!eventTable.TryGetValue(eventId, out Delegate handler)) return;

        if (handler is Action<T1, T2> action)
        {
            action.Invoke(arg1, arg2);
        }
        else
        {
            LogTypeMismatch(eventId, typeof(Action<T1, T2>), handler.GetType());
        }
    }

    public static void Broadcast<T1, T2, T3>(int eventId, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!eventTable.TryGetValue(eventId, out Delegate handler)) return;

        if (handler is Action<T1, T2, T3> action)
        {
            action.Invoke(arg1, arg2, arg3);
        }
        else
        {
            LogTypeMismatch(eventId, typeof(Action<T1, T2, T3>), handler.GetType());
        }
    }

    #endregion

    #region 辅助方法
    public static void ClearAll()
    {
        eventTable.Clear();
    }

    private static bool CanAddListener(int eventId, Delegate listener)
    {
        if (listener == null)
        {
            Debug.LogError($"[EventManager] Cannot add null listener to event {eventId}.");
            return false;
        }

        if (!eventTable.TryGetValue(eventId, out Delegate existingHandler))
        {
            eventTable.Add(eventId, null);
            return true;
        }

        if (existingHandler != null && existingHandler.GetType() != listener.GetType())
        {
            LogTypeMismatch(eventId, listener.GetType(), existingHandler.GetType());
            return false;
        }

        return true;
    }

    private static bool CanRemoveListener(int eventId, Delegate listener)
    {
        if (listener == null)
        {
            Debug.LogError($"[EventManager] Cannot remove null listener from event {eventId}.");
            return false;
        }

        if (!eventTable.TryGetValue(eventId, out Delegate existingHandler))
        {
            return false;
        }

        if (existingHandler == null)
        {
            return false;
        }

        if (existingHandler.GetType() != listener.GetType())
        {
            LogTypeMismatch(eventId, listener.GetType(), existingHandler.GetType());
            return false;
        }

        return true;
    }

    // 主要是检查 移除某个委托后是否空了
    private static void RemoveEventIfEmpty(int eventId)
    {
        if (eventTable.TryGetValue(eventId, out Delegate handler) && handler == null)
        {
            eventTable.Remove(eventId);
        }
    }

    private static void LogTypeMismatch(int eventId, Type expectedType, Type actualType)
    {
        Debug.LogError($"[EventManager] Event {eventId} delegate type mismatch. Expected: {expectedType}, Actual: {actualType}");
    }

    #endregion
}
