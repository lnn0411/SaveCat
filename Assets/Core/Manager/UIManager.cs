using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    #region 集中轮询与注册UI模块
    //注册的ui
    private List<BaseView> registeredViews = new List<BaseView>();

    //缓存长度用于for循环
    private int _uiCount = 0;

    //注册UI
    public void RegisterView(BaseView view)
    {
        if(!registeredViews.Contains(view))
        {
            registeredViews.Add(view);
            _uiCount = registeredViews.Count;
        }
    }
    //注销UI
    public void UnregisterView(BaseView view)
    {
        if(registeredViews.Contains(view))
        {
            registeredViews.Remove(view);
            _uiCount = registeredViews.Count;
        }
    }

    // 集中轮询 减少每个UI的Update调用
    private void Update()
    {
        if(_uiCount == 0) return;

        float dt = Time.deltaTime;
        for(int i = 0; i < _uiCount; i++)
        {
            if(registeredViews[i] != null)
            {
                registeredViews[i].OnUpdateView(dt);
            }
        }
    }

    #endregion

    #region UI Opening/Closing Management
    // Dictionary to store active UI instances by their prefab names
    private Dictionary<string, BaseView> activeViews = new Dictionary<string, BaseView>();

    /// <summary>
    /// Opens a UI view by its prefab name
    /// </summary>
    /// <param name="prefabName">Name of the UI prefab (without extension)</param>
    /// <param name="parent">Optional parent transform for the UI</param>
    /// <returns>The opened UI view instance</returns>
    public BaseView OpenView(string prefabName, Transform parent = null)
    {
        // Check if already open
        if (activeViews.TryGetValue(prefabName, out BaseView existingView))
        {
            if (existingView != null)
            {
                existingView.Show();
                return existingView;
            }
            // If reference is lost, remove from dictionary
            activeViews.Remove(prefabName);
        }

        // Load prefab from Resources (assuming UI prefabs are in Resources/UI folder)
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"UI Prefab not found: UI/{prefabName}");
            return null;
        }

        // Instantiate UI
        GameObject uiObject = Instantiate(prefab, parent ?? transform);
        BaseView view = uiObject.GetComponent<BaseView>();
        
        if (view == null)
        {
            Debug.LogError($"UI prefab {prefabName} does not contain a BaseView component");
            Destroy(uiObject);
            return null;
        }

        // Show the view (this will trigger OnEnable -> auto-register)
        view.Show();
        activeViews[prefabName] = view;
        
        return view;
    }

    /// <summary>
    /// Closes a UI view by its prefab name
    /// </summary>
    /// <param name="prefabName">Name of the UI prefab to close</param>
    public void CloseView(string prefabName)
    {
        if (activeViews.TryGetValue(prefabName, out BaseView view))
        {
            if (view != null)
            {
                view.Hide(); // This will trigger OnDisable -> auto-unregister
            }
            activeViews.Remove(prefabName);
        }
    }

    /// <summary>
    /// Closes all active UI views
    /// </summary>
    public void CloseAllViews()
    {
        List<string> keysToRemove = new List<string>(activeViews.Keys);
        foreach (string key in keysToRemove)
        {
            CloseView(key);
        }
    }
    #endregion
}