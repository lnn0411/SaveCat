using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//绑定组件用
public partial class SlotItemView : BaseView
{

    protected Image image;
    protected Text text;
    protected GameObject contentRoot;


    protected override void BindComponents()
    {
        image = transform.Find("Content/BgImage")?.GetComponent<Image>();
        text = transform.Find("Content/CountText")?.GetComponent<Text>();
        contentRoot = transform.Find("Content").gameObject;

        if (image == null || text == null || contentRoot == null)
        {
            Debug.LogError($"[{gameObject.name}] UI 组件绑定失败，请检查路径！");
        }
    }
}
