using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class DFJsonBase
{
    string txtJson;
    protected TextAsset ta;
    protected bool isInit; //子类初始化

    protected DFJsonBase(string str)
    {
        txtJson = str;
        Load();
    }

    void Load()
    {
        ta = Resources.Load<TextAsset>("Json/" + txtJson);
        if (ta == null)
        {
            DF.Log("parseJsonErr " + txtJson, E_ColorType.Err);
        }
    }

    public virtual void Clear()
    {
        isInit = false;
    }

    public abstract void Init();
    protected abstract void Parse();
}