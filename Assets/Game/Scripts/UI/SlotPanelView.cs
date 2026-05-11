using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 底部槽位面板。
/// 只负责收集并绑定 SlotItemView，不处理弹药规则。
/// </summary>
public class SlotPanelView : BaseView
{
    #region Inspector Config

    [Header("Slot Items")]
    [SerializeField] private List<SlotItemView> slotItemViews = new List<SlotItemView>();

    [Header("Auto Bind")]
    [SerializeField] private Transform slotItemRoot;

    #endregion

    #region BaseView Lifecycle

    protected override void BindComponents()
    {
        AutoCollectSlotItemsIfNeeded();
    }

    protected override void InitLogic()
    {
        BindSlotViewsToManager();
    }

    protected override void OnShow()
    {
        BindSlotViewsToManager();
    }

    #endregion

    #region Bind Logic

    /// <summary>
    /// 如果 Inspector 没有手动配置槽位，则从 slotItemRoot 下自动收集。
    /// 推荐正式项目中手动拖 7 个，顺序更可控。
    /// </summary>
    private void AutoCollectSlotItemsIfNeeded()
    {
        if (slotItemViews.Count > 0)
        {
            return;
        }

        if (slotItemRoot == null)
        {
            slotItemRoot = transform;
        }

        SlotItemView[] views = slotItemRoot.GetComponentsInChildren<SlotItemView>(true);

        for (int i = 0; i < views.Length; i++)
        {
            if (views[i] != null)
            {
                slotItemViews.Add(views[i]);
            }
        }
    }

    /// <summary>
    /// 将当前面板上的 7 个槽位 View 交给 SlotManager。
    /// SlotManager 负责数据，SlotItemView 负责显示。
    /// </summary>
    private void BindSlotViewsToManager()
    {
        if (SlotManager.Instance == null)
        {
            Debug.LogWarning("[SlotPanelView] SlotManager is not ready.");
            return;
        }

        SlotManager.Instance.BindViews(slotItemViews);
    }

    #endregion
}
