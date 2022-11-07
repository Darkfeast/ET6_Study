using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public partial class DFHelper
{
    public class ToolDelay
    {
        private static GameObject df;
        private static Transform root;
        private static long id = 0;

        static ToolDelay()
        {
            df = GameObject.Find("DF");
            if (df == null)
            {
                df = new GameObject("DF");
            }

            root = df.transform.Find("Delay");
            if (root == null)
            {
                root = new GameObject("Delay").Parent(df);
            }
        }

        public static DFDelay Delay(float time, Action act, bool isCanCancel = true)
        {
            DFDelay delay = new GameObject("delay_" + id + "_" + time.Float2String(1)).AddComponent<DFDelay>();
            delay.gameObject.Parent(root);
            delay.DelayOpration(id, time, act, isCanCancel);
            id++;
            return delay;
        }
    }
}
public class DFDelay : MonoBehaviour
{
    public long id;
    bool isCancel; //是否取消回调
    bool isStartTimer; //是否开始计时 目前没啥用 以后扩展用
    Action act;
    bool isCanCancel; //是否支持提前取消 

    void Awake()
    {
        DFEventCenter.AddListener<long, bool>(DFEventType.DFDelayCancel, DFDelayCancel);
    }

    /// <summary>
    /// 提前取消延时
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancelAct"></param>如果true 则不执行回调   
    void DFDelayCancel(long id, bool cancelAct)
    {
        if (this.id != id)
            return;
        if (isStartTimer && isCanCancel && !isCancel)
        {
            isCancel = true;
            isStartTimer = false;
            if (act != null)
            {
                if (!cancelAct)
                    act();
            }

            Destroy(gameObject);
        }
    }

    public void DelayOpration(long id, float time, Action act, bool isCanCancel = true)
    {
        this.id = id;
        this.act = act;
        isCancel = false;
        isStartTimer = true;
        this.isCanCancel = isCanCancel;
        StartCoroutine(IE_YieldMethod(time, act));
    }

    IEnumerator IE_YieldMethod(float yieldTime, Action method)
    {
        yield return new WaitForSeconds(yieldTime);
        if (method != null)
        {
            if (!isCancel)
            {
                isStartTimer = false;
                method();
                Destroy(gameObject);
            }
        }
    }

    public void Cancel()
    {
        isCancel = true;
    }

    void OnDestroy()
    {
        DFEventCenter.RemoveListener<long,bool>(DFEventType.DFDelayCancel, DFDelayCancel);
    }
}