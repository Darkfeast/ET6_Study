using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DFEventCenter
{
    static Dictionary<DFEventType, Delegate> m_EventTable = new Dictionary<DFEventType, Delegate>();
    
    static void IAddListener(DFEventType eventType, Delegate callBack)
    {
        if (!m_EventTable.ContainsKey(eventType))
        {
            m_EventTable.Add(eventType, null);
        }
        Delegate d = m_EventTable[eventType];
        if (d != null && d.GetType() != callBack.GetType())
        {
            Debug.LogError("AddListener Error: type err");
        }
    }
    public static void AddListener(DFEventType eventType, Action callBack)
    {
        IAddListener(eventType, callBack);
        m_EventTable[eventType] = (Action)m_EventTable[eventType] + callBack;
    }
    public static void AddListener<T>(DFEventType eventType, Action<T> callBack)
    {
        IAddListener(eventType, callBack);
        m_EventTable[eventType] = (Action<T>)m_EventTable[eventType] + callBack;
    }
    public static void AddListener<T,X>(DFEventType eventType, Action<T, X> callBack)
    {
        IAddListener(eventType, callBack);
        m_EventTable[eventType] = (Action<T, X>)m_EventTable[eventType] + callBack;
    }
    public static void AddListener<T, X,Y>(DFEventType eventType, Action<T, X, Y> callBack)
    {
        IAddListener(eventType, callBack);
        m_EventTable[eventType] = (Action<T, X, Y>)m_EventTable[eventType] + callBack;
    }
    public static void AddListener<T, X, Y, Z>(DFEventType eventType, Action<T, X, Y, Z> callBack)
    {
        IAddListener(eventType, callBack);
        m_EventTable[eventType] = (Action<T, X, Y, Z>)m_EventTable[eventType] + callBack;
    }
    static int IRemoveListener(DFEventType eventType, Delegate callBack)
    {
        if (m_EventTable.ContainsKey(eventType))
        {
            Delegate d = m_EventTable[eventType];
            if (d == null)
            {
                //throw new Exception("RemoveListener Error: doesnot exist the delegate of the removed event !["+eventType+"]");
                return 1;
            }
            else if (d.GetType() != callBack.GetType())
            {
                //throw new Exception("RemoveListener Error: the removed event has a diff type!");
                return 2;
            }
            return 0;
        }
        else
        {
            //throw new Exception("RemoveListener Error: m_EventTable is not contaioned eventType!");
            return 3;
        }
    }
    public static void RemoveListener(DFEventType eventType, Action callBack)
    {
       int res=IRemoveListener(eventType, callBack);
       if(res==0)
            m_EventTable[eventType] = (Action)m_EventTable[eventType] - callBack;
    }
    public static void RemoveListener<T>(DFEventType eventType, Action<T> callBack)
    {
        int res= IRemoveListener(eventType, callBack);
        if(res==0)
            m_EventTable[eventType] = (Action<T>)m_EventTable[eventType] - callBack;
    }
    public static void RemoveListener<T, X>(DFEventType eventType, Action<T, X> callBack)
    {
        int res=IRemoveListener(eventType, callBack);
        if(res==0)
            m_EventTable[eventType] = (Action<T, X>)m_EventTable[eventType] - callBack;
    }
    public static void RemoveListener<T, X,Y>(DFEventType eventType, Action<T, X,Y> callBack)
    {
        int res=IRemoveListener(eventType, callBack);
        if(res==0)
            m_EventTable[eventType] = (Action<T, X,Y>)m_EventTable[eventType] - callBack;
    }
    public static void RemoveListener<T, X, Y, Z>(DFEventType eventType, Action<T, X, Y, Z> callBack)
    {
        int res=IRemoveListener(eventType, callBack);
        if(res==0)
            m_EventTable[eventType] = (Action<T, X, Y, Z>)m_EventTable[eventType] - callBack;
    }
    public static void Broadcast(DFEventType eventType)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            Action callBack = d as Action;
            if (callBack != null)
                callBack();
            else
                Debug.LogError("Broadcast Error: the callBack is null!");
        }
    }
    public static void Broadcast<T>(DFEventType eventType, T arg1)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            Action<T> callBack = d as Action<T>;
            if (callBack != null)
                callBack(arg1);
            else
                Debug.LogError("Broadcast Error: the callBack is null![" + eventType + "]");
        }
    }
    public static void Broadcast<T,X>(DFEventType eventType, T arg1, X arg2)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            Action<T,X> callBack = d as Action<T,X>;
            if (callBack != null)
                callBack(arg1, arg2);
            else
                Debug.LogError("Broadcast Error: the callBack is null![" + eventType + "]");
        }
    }
    public static void Broadcast<T, X,Y>(DFEventType eventType, T arg1, X arg2,Y arg3)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            Action<T, X,Y> callBack = d as Action<T, X,Y>;
            if (callBack != null)
                callBack(arg1, arg2,arg3);
            else
                Debug.LogError("Broadcast Error: the callBack is null![" + eventType + "]");
        }
    }
    public static void Broadcast<T, X, Y, Z>(DFEventType eventType, T arg1, X arg2, Y arg3, Z arg4)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            Action<T, X, Y, Z> callBack = d as Action<T, X, Y, Z>;
            if (callBack != null)
                callBack(arg1, arg2, arg3, arg4);
            else
                Debug.LogError("Broadcast Error: the callBack is null![" + eventType + "]");
        }
    }
    public static bool IsExist(DFEventType e)
    {
        if(m_EventTable.ContainsKey(e))
        {
            return true;
        }
        return false;
    }
    public static void IsExistAndRemove(DFEventType e)
    {
        if (m_EventTable.ContainsKey(e))
        {
            m_EventTable.Remove(e);
        }
    }
}
