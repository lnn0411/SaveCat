using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//泛型单例基类
public class Singleton<T> : MonoBehaviour where T: Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if(Instance == null)
        {
            Instance = this as T;
            //可选 跨场景存在
            DontDestroyOnLoad(this.gameObject);
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }
}

