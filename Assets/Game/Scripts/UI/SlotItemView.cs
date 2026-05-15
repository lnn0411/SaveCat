using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SlotItemView : BaseView
{   
    // 格子对应的slot数据
    private SlotData _data;


    protected override void InitLogic()
    {
        Clear();
    }

    //根据格子对应的数据刷新
    public void Refresh(SlotData data)
    {
        _data = data;

        if(_data == null || _data.IsEmpty)
        {
            Clear();
            return;
        }

        if(contentRoot != null) contentRoot.gameObject.SetActive(true);
        if(image != null) image.color = BlockColorUtility.GetColor(_data.ColorType);
        if(text != null) text.text = _data.StrengthCount.ToString();    
        
    }

    //quick clear
    public void Clear()
    {
        _data = null;

        if(contentRoot != null) contentRoot.gameObject.SetActive(false);

        if(text != null) text.text = string.Empty;
    }

    // 获取射击锚点 用于特效等的定位
    public RectTransform GetShootAnchor()
    {
        return contentRoot != null?contentRoot.transform as RectTransform:transform as RectTransform;
    }

   
}
