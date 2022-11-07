using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = System.Random;

public class DFExtendHelper
{
    public static Random r;

    static DFExtendHelper()
    {
        r = new Random();
    }
}

public static class ExtendTrans
{
    public static Transform[] GetSubs(this Transform trs)
    {
        Transform[] subs = new Transform[trs.childCount];
        for (int i = 0; i < trs.childCount; i++)
        {
            subs[i] = trs.GetChild(i);
        }

        return subs;
    }

    public static Transform GetMinDisTransform(this Transform trs1, Transform[] trs, Func<Transform, bool> func = null, bool filterHide = true)
    {
        if (trs.Length == 0)
            return null;

        float disMax = float.PositiveInfinity;
        Transform temp = null; //= trs[0];
        for (int i = 0; i < trs.Length; i++)
        {
            if (filterHide)
            {
                if (!trs[i].gameObject.activeInHierarchy)
                {
                    continue;
                }
            }

            bool? ret = func?.Invoke(trs[i]);
            if (ret != null && ret == true)
            {
                continue;
            }

            //.net 低版本
            // if (func!=null)
            // {
            //     bool ret= func.Invoke(trs[i]);
            //     if (ret)
            //     {
            //         continue;
            //     }
            // }

            var dis = Vector3.Distance(trs1.position, trs[i].position);
            if (dis < disMax)
            {
                disMax = dis;
                temp = trs[i];
            }
        }

        return temp;
    }

    public static void State(this Transform trs, bool state)
    {
        trs.gameObject.SetActive(state);
    }

    public static void State(this GameObject go, bool state)
    {
        State(go.transform, state);
    }

    public static Transform Parent(this Transform trs, Transform parent, bool zero = true)
    {
        if (parent == null)
            DF.Log("parent is null", E_ColorType.Err);
        trs.SetParent(parent);
        if (zero)
            trs.localPosition = Vector3.zero;
        return trs;
    }

    public static Transform Parent(this Transform trs, GameObject parent, bool zero = true)
    {
        return Parent(trs, parent.transform, zero);
    }

    public static Transform Parent(this GameObject go, Transform parent, bool zero = true)
    {
        return Parent(go.transform, parent, zero);
    }

    public static Transform Parent(this GameObject go, GameObject parent, bool zero = true)
    {
        return Parent(go.transform, parent, zero);
    }

    public static T AddComponentType<T>(this GameObject go, string type) where T : Component
    {
        var t = Type.GetType(type);
        if (t == null)
        {
            DF.Log("not exit the type " + type);
            return null;
        }

        var comp = go.AddComponent(t);
        var target = comp as T;
        if (target == null) DF.Log("transfer is not suport " + t + "  to " + typeof(T), E_ColorType.Err);

        return target;
    }

    public static string Path(this Transform trs)
    {
        return DFHelper.ToolTransform.PrintP(trs.gameObject);
    }
}

public static class ExtendList
{
    //print each element of list
    public static void Print<T>(this List<T> list, bool sort = false, Action<T> act = null, bool record = false, E_ColorType ecolor = E_ColorType.Init)
    {
        if (sort) list.Sort((a, b) => { return a.ToString().CompareTo(b.ToString()); });

        DF.Log("Print-listCount " + list.Count, E_ColorType.UI);
        if (record)
        {
            DFLog.Log("Print-listCount " + list.Count);
        }

        int i = 0;
        foreach (var v in list)
        {
            DF.Log(v, ecolor);
            if (record)
            {
                DFLog.Log(string.Concat("[", i, "]   ", v.ToString()));
            }

            act?.Invoke(v);
            i++;
        }

        DF.Log(DF.LogLine() + list.Count, E_ColorType.Over);
        if (record)
        {
            DFLog.Log(DF.LogLine() + list.Count);
        }
    }

    //get each of element and concat a line
    public static string GetOneLine(this List<string> list, E_ColorType ecolor = E_ColorType.Init)
    {
        var sb = new StringBuilder();
        foreach (var v in list) sb.Append(v + "#");

        return sb.ToString(0, sb.Length - 1);
    }

    public static List<T> Combo<T>(this List<T> list, List<T> list2)
    {
        var listT = new List<T>(list);
        listT.AddRange(list2);
        return listT;
    }

    public static List<T> Reverse<T>(this List<T> list)
    {
        var temp = new List<T>();
        foreach (var v in list) temp.Insert(0, v);

        return temp;
    }

    public static void ToLower<T>(this List<T> list, E_ColorType ecolor = E_ColorType.Init) where T : class
    {
        DF.Log("ToLower-listCount " + list.Count, E_ColorType.UI);
        for (var i = 0; i < list.Count; i++)
            if (typeof(T) == typeof(string))
            {
                var vv = list[i] as string;

                var t = vv.ToLower() as T;
                list[i] = t;
            }

        DF.Log(DF.LogLine() + list.Count, E_ColorType.Over);
    }

    public static T RandomOne<T>(this List<T> list)
    {
        int index = DFExtendHelper.r.Next(0, list.Count);
        return list[index];
    }

    public static void Randdom<T>(this List<T> list)
    {
        if (list == null || list.Count < 2)
        {
            return;
        }

        var randCount = list.Count / 2;
        for (var c = 0; c < randCount; c++)
        {
            int i = DFExtendHelper.r.Next(0, list.Count);
            int j = DFExtendHelper.r.Next(0, list.Count);

            if (list.Count > 2)
            {
                while (i == j)
                {
                    j = DFExtendHelper.r.Next(0, list.Count);
                }
            }

            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static T Last<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            return list[list.Count - 1];
        }

        throw new IndexOutOfRangeException("---this list is empty---");
    }
}

public static class ExtendDict
{
    public static void Print<T, K>(this Dictionary<T, K> dict, UnityAction<KeyValuePair<T, K>> act = null, bool record = false, E_ColorType ecolor = E_ColorType.Init)
    {
        DF.Log("Print-dictCount " + dict.Count, E_ColorType.UI);
        if (record)
        {
            DFLog.Log("Print-dictCount " + dict.Count);
        }

        int i = 0;
        foreach (var v in dict)
        {
            DF.Log("k " + v.Key + "  v " + v.Value, ecolor);
            if (record)
            {
                DFLog.Log("[" + i + "]   k " + v.Key + "  v " + v.Value);
            }

            act?.Invoke(v);
            i++;
        }

        DF.Log("---------------------" + dict.Count, E_ColorType.Over);
        if (record)
        {
            DFLog.Log("---------------------" + dict.Count);
        }
    }

    public static void Action<T, K>(this Dictionary<T, K> dict, UnityAction<KeyValuePair<T, K>> act)
    {
        foreach (var v in dict)
        {
            if (act != null)
            {
                act(v);
            }
        }
    }
}

public static class ExtendFloat
{
    /// <param name="f"></param>
    /// <param name="precision"></param>精度
    /// <returns></returns>
    public static string Float2String(this float f, int precision = 0)
    {
        if (precision < 0)
            precision = 0;
        return f.ToString("f" + precision);
    }

    public static string Float2Percent(this float f, int precision = 0)
    {
        if (precision < 0)
            precision = 0;
        return (f * 100).ToString("f" + precision) + "%";
    }

    public static float Float2Float(this float f, int precision = 0)
    {
        if (precision < 0)
            precision = 0;

        return float.Parse(Float2String(f, precision));
    }

    public static string Float2NetSpeed(this float bps, int precision = 0)
    {
        if (precision < 0)
            precision = 0;

        int kb = 1 << 10;
        string[] prefix = new[] {"b", "k", "m", "g", "t", "p"};

        int pos = 0;
        while (bps > kb)
        {
            bps /= kb;
            pos += 1;
        }

        return $"{bps.Float2String(precision)} {prefix[pos]}/s";
    }
}

public static class ExtendByte
{
    public static string ToHex(this byte b)
    {
        return b.ToString("x2");
    }

    public static string ToHex(this IEnumerable<byte> bytes)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in bytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    public static string ToHex(this IEnumerable<byte> bytes, string format)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in bytes)
        {
            stringBuilder.Append(b.ToString(format));
        }

        return stringBuilder.ToString();
    }
}

public static class ExtendString
{
    public static string Split(this string str, string splitStr, int index = 0)
    {
        string[] strs = str.Split(new string[] {splitStr}, StringSplitOptions.RemoveEmptyEntries);
        if (strs.Length > 0)
            return strs[index];
        DF.Log("split err ", E_ColorType.Err);
        return str;
    }

    public static string ToUpperFirst(this string str)
    {
        if (str.Length <= 0)
            return "";

        string f = str[0].ToString().ToUpper();
        if (str.Length > 1)
        {
            f += str.Substring(1);
        }

        return f;
    }

    public static string Link(this string str, string split = "/", params string[] pars)
    {
        if (pars.Length <= 0)
            return str;
        str = pars.Aggregate(str, (current, t) => current + (split + t));
        return str;
    }
}