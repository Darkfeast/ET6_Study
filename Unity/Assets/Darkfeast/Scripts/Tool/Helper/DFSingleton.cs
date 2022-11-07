using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DFSingleton<T> where T : new()
{
    static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // DF.Log("<----->first  " + typeof(T), E_ColorType.UI);
                Create();
                return instance;
            }
            else
            {
                // DF.Log("<----->exist  " + typeof(T), E_ColorType.Over);
                return instance;
            }
        }
        set { instance = value; }
    }

    public static Type Type()
    {
        return typeof(T);
    }

    static void Create()
    {
        if (instance == null)
        {
            instance = new T();
        }
    }

    public static T CreateEx()
    {
        Create();
        return Instance;
    }

    public static void Dispose()
    {
        if (instance != null)
            instance = default(T);
    }
}