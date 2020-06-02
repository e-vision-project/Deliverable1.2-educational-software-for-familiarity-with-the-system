using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

public abstract class MenuComponent
{
    //-----------------------------------------------------------------

    public GameObject PanelObj;

    protected Menu p_ParentMenu;

    //-----------------------------------------------------------------

    public virtual void ShowComponent(bool show)
    {
        if(PanelObj == null) return;
        PanelObj.SetActive(show);
    }

    //-----------------------------------------------------------------

    public void SetMethod(Button button, UnityAction action)
    {
        if (button == null)
        {
            //Debug.LogError( action.Method.Name );
            return;
        }
        //Remove the existing events
        button.onClick.RemoveAllListeners();
        //Add your new event
        button.onClick.AddListener(action);
    }
    public void SetMethod(Toggle button, UnityAction<bool> action)
    {
        //Remove the existing events
        button.onValueChanged.RemoveAllListeners();
        //Add your new event
        button.onValueChanged.AddListener(action);
    }

    //-----------------------------------------------------------------
}
