using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    //字典映射1： 每个Prefab对应一个专用的对象池
    private Dictionary<GameObject, ObjectPool<GameObject>> _pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    // 字典映射2：记录每个生成的实例来源于哪个 Prefab，方便在回收时不用强传 Prefab
    private Dictionary<GameObject, GameObject> _instanceToPrefab = new Dictionary<GameObject, GameObject>();


    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null) return null;

        // 如果这个 Prefab 还没有对应的池子，为其创建一个
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab), // 当池子空了且需要新物体时如何生成
                actionOnGet: obj => obj.SetActive(true), // 从池子中取出时的操作
                actionOnRelease: obj => 
                {
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform); // 回收时统一将其父节点设为管理器
                },
                actionOnDestroy: obj => Destroy(obj), // 超过最大容量时如何销毁
                defaultCapacity: 20, 
                maxSize: 200 // 可根据需求调整，防止内存爆掉
            );
            _pools[prefab] = pool;
        }

        // 取出对象
        GameObject instance = pool.Get();
        _instanceToPrefab[instance] = prefab; // 登记该实例的 "身份证"

        // 统一设置基础 Transform
        instance.transform.SetPositionAndRotation(position, rotation);
        if (parent != null)
        {
            instance.transform.SetParent(parent);
        }

        return instance;
    }

    /// <summary>
    /// 回收对象（循环再利用，彻底避免 GC）
    /// </summary>
    public void Recycle(GameObject instance)
    {
        if (instance == null) return;

        // 通过身份证找到它原本归属的对象池，并归还
        if (_instanceToPrefab.TryGetValue(instance, out var prefab))
        {
            _pools[prefab].Release(instance);
            _instanceToPrefab.Remove(instance); // 从活跃追踪中移除
        }
        else
        {
            Debug.LogWarning($"[PoolManager] 尝试回收一个不属于对象池管理的物体: {instance.name}。将直接执行 Destroy。");
            Destroy(instance);
        }
    }
}
