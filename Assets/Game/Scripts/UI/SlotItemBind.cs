using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//绑定组件用
public partial class SlotItemView : BaseView
{

    protected Image image;
    protected TextMeshProUGUI text;
    protected GameObject contentRoot;


    protected override void BindComponents()
    {
        image = transform.Find("Content/BgImage")?.GetComponent<Image>();
        text = transform.Find("Content/CountText")?.GetComponent<TextMeshProUGUI>();
        Transform content = transform.Find("Content");
        contentRoot = content != null ? content.gameObject : null;

        if (image == null || text == null || contentRoot == null)
        {
            Debug.LogError($"[{gameObject.name}] UI 组件绑定失败，请检查路径！");
        }
    }
}
