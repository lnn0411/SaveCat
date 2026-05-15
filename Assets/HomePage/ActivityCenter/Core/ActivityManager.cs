using System.Collections.Generic;
using UnityEngine;

public class ActivityManager : Singleton<ActivityManager>
{
    private const string ActivityResourcePath = "Prefab/Activity";
    public const string SignInActivityPrefab = "SignIn/SignIn Canvas";

    public event System.Action<string> OnActivityOpened;
    public event System.Action<string> OnActivityClosed;

    private List<BaseView> registeredActivities = new List<BaseView>();
    private int _activityCount = 0;
    private Dictionary<string, BaseView> activeActivities = new Dictionary<string, BaseView>();

    private void Update()
    {
        if (_activityCount == 0) return;

        float dt = Time.deltaTime;
        for (int i = 0; i < _activityCount; i++)
        {
            if (registeredActivities[i] != null && registeredActivities[i].NeedsUpdate)
            {
                registeredActivities[i].OnUpdateView(dt);
            }
        }
    }

    public BaseView OpenActivity(string prefabName, Transform parent = null)
    {
        if (activeActivities.TryGetValue(prefabName, out BaseView existingView))
        {
            if (existingView != null)
            {
                existingView.Show();
                return existingView;
            }
            activeActivities.Remove(prefabName);
        }

        GameObject prefab = Resources.Load<GameObject>($"{ActivityResourcePath}/{prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"Activity prefab not found: {ActivityResourcePath}/{prefabName}");
            return null;
        }

        GameObject activityObject = Instantiate(prefab, parent ?? transform);
        BaseView view = FindOrCreateActivityView(activityObject, prefabName);
        if (view == null)
        {
            Debug.LogError($"Activity prefab {prefabName} does not contain a BaseView component and could not be auto-created");
            Destroy(activityObject);
            return null;
        }

        RegisterActivity(view);
        view.Show();
        activeActivities[prefabName] = view;
        OnActivityOpened?.Invoke(prefabName);
        return view;
    }

    public void CloseActivity(string prefabName)
    {
        if (!activeActivities.TryGetValue(prefabName, out BaseView view) || view == null)
        {
            return;
        }

        view.Hide();
        activeActivities.Remove(prefabName);
        UnregisterActivity(view);
        OnActivityClosed?.Invoke(prefabName);
    }

    public void CloseAllActivities()
    {
        List<string> keys = new List<string>(activeActivities.Keys);
        foreach (string key in keys)
        {
            CloseActivity(key);
        }
    }

    public BaseView OpenSignIn(Transform parent = null)
    {
        return OpenActivity(SignInActivityPrefab, parent);
    }

    public void CloseSignIn()
    {
        CloseActivity(SignInActivityPrefab);
    }

    private BaseView FindOrCreateActivityView(GameObject activityObject, string prefabName)
    {
        BaseView view = activityObject.GetComponent<BaseView>() ?? activityObject.GetComponentInChildren<BaseView>(true);
        if (view != null)
        {
            return view;
        }

        if (prefabName == SignInActivityPrefab)
        {
            SignInView signInView = activityObject.AddComponent<SignInView>();
            SignInController controller = activityObject.GetComponent<SignInController>() ?? activityObject.AddComponent<SignInController>();
            SignInData data = activityObject.GetComponent<SignInData>() ?? activityObject.AddComponent<SignInData>();
            return signInView;
        }

        return null;
    }

    private void RegisterActivity(BaseView view)
    {
        if (!registeredActivities.Contains(view))
        {
            registeredActivities.Add(view);
            _activityCount = registeredActivities.Count;
        }
    }

    private void UnregisterActivity(BaseView view)
    {
        if (registeredActivities.Contains(view))
        {
            registeredActivities.Remove(view);
            _activityCount = registeredActivities.Count;
        }
    }
}
