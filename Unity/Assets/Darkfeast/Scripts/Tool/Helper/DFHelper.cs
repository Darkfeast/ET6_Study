using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

//因为是多个模块的合集，为了方便代码感应提示，方法开头改为模块名字
//如GetFormatDateTime  改为 DateTimeFormat
//GetFileHash  ->  FileGetHash

//这种格式命名太麻烦了 而且不符合语言顺序  改回原样
public enum E_DateType
{
    Default, //默认时间格式
    Log //记录日志格式
}

public partial class DFHelper
{
    //Transform
    public class ToolTransform
    {
        public static Transform GetParent(Transform trs, string namePar)
        {
            var temp = trs;
            while (temp.parent != null)
            {
                temp = temp.parent;
                if (temp.name.Contains(namePar))
                {
                    break;
                }
            }

            if (temp == trs || !temp.name.Contains(namePar))
                return null;
            return temp;
        }

        public static T GetParent<T>(Transform trs) where T : Component
        {
            var temp = trs;
            T t = null;
            while (temp.parent != null)
            {
                temp = temp.parent;
                t = temp.GetComponent<T>();
                if (t)
                {
                    break;
                }
            }

            if (temp == trs || t == null)
                return null;
            return t;
        }

        public static string PrintP(GameObject go)
        {
            var listPath = new List<string> {go.transform.name};
            var parent = go.transform.parent;
            while (parent)
            {
                listPath.Add(parent.name);
                parent = parent.parent;
            }

            var path = "";
            for (var i = listPath.Count - 1; i >= 0; i--) path += listPath[i] + "/";

            path = path.Substring(0, path.Length - 1);
            return path;
        }

        public static Vector3 World2Local(Transform trs, Vector3 v3)
        {
            var pos = trs.InverseTransformPoint(v3);
            return pos;
        }

        public static Vector3 Local2World(Transform trs, Vector3 v3)
        {
            var pos = trs.TransformPoint(v3);
            return pos;
        }

        public static Vector3 GetWorldPosFromMouse()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, -Camera.main.transform.position.z));
            return pos;
        }

        public static Vector3 GetScreenPosFromWorld(Transform trs)
        {
            var pos = Camera.main.WorldToScreenPoint(trs.position);
            return pos;
        }

        public static int GetQuadrant()
        {
            Vector2 mousePos = Input.mousePosition;
            var w = Screen.width;
            var h = Screen.height;
            if (mousePos.x >= w / 2) return mousePos.y >= h / 2 ? 1 : 4;
            return mousePos.y >= h / 2 ? 2 : 3;
        }

        public static Vector3 GetDegByV2(Vector2 v2)
        {
            var v3 = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(v2.y, v2.x));
            return v3;
        }

        public static void ModifyLayer(Transform trs, int layerId)
        {
            if (trs.childCount > 0)
                for (var i = 0; i < trs.childCount; i++)
                    ModifyLayer(trs.GetChild(i), layerId);

            trs.gameObject.layer = layerId;
        }

        public static void ModifySortingLayerOnce(Transform trs, Renderer spr, int offset = 5)
        {
            var render = trs.GetComponent<Renderer>();
            if (render)
            {
                render.sortingLayerName = spr.sortingLayerName;
                render.sortingOrder = spr.sortingOrder + offset;
            }
            else
            {
                var sg = trs.GetComponent<SortingGroup>();
                if (sg)
                {
                    sg.sortingLayerName = spr.sortingLayerName;
                    sg.sortingOrder = spr.sortingOrder + offset;
                }
                else
                {
                    DF.Log("render not found " + trs);
                }
            }
        }

        public static void ModifySortingLayerOnce(Transform trs, string sortingLayerName, int orderLayer = -1)
        {
            var render = trs.GetComponent<Renderer>();
            if (render)
            {
                render.sortingLayerName = sortingLayerName;
                if (orderLayer != -1)
                    render.sortingOrder = orderLayer;

                var sg = trs.GetComponent<SortingGroup>();
                if (sg != null)
                {
                    sg.sortingLayerName = sortingLayerName;
                    sg.sortingOrder = orderLayer;
                }
            }
            else
            {
                var sg = trs.GetComponent<SortingGroup>();
                if (sg)
                {
                    sg.sortingLayerName = sortingLayerName;
                    if (orderLayer != -1)
                        sg.sortingOrder = orderLayer;
                }
                else
                {
                    DF.Log("render not found " + trs);
                }
            }
        }

        public static void ModifySortingLayer(Transform trs, string sortingLayerName, int orderLayer = -1,
            bool addMode = true, bool filter = false, string filterStr = "filter")
        {
            if (filter && trs.name == filterStr)
                return;

            if (trs.childCount > 0)
                for (var i = 0; i < trs.childCount; i++)
                    if (addMode)
                        ModifySortingLayer(trs.GetChild(i), sortingLayerName, orderLayer + 1, addMode, filter,
                            filterStr);
                    else
                        ModifySortingLayer(trs.GetChild(i), sortingLayerName, orderLayer, addMode, filter, filterStr);

            var render = trs.GetComponent<Renderer>();
            var ps = trs.GetComponent<ParticleSystem>();

            if (render && ps == null)
            {
                render.sortingLayerName = sortingLayerName;
                if (orderLayer != -1)
                    render.sortingOrder = orderLayer;

                var sg = trs.GetComponent<SortingGroup>();
                if (sg != null)
                {
                    sg.sortingLayerName = sortingLayerName;
                    sg.sortingOrder = orderLayer;
                }
            }
            else
            {
                var sg = trs.GetComponent<SortingGroup>();
                if (sg != null)
                {
                    sg.sortingLayerName = sortingLayerName;
                    sg.sortingOrder = orderLayer;
                }
            }
        }

        public static void ModifyOrderOnce(Transform trs, int orderLayer)
        {
            var render = trs.GetComponent<Renderer>();
            if (render) render.sortingOrder = orderLayer;
        }

        //last root
        public static void FindType<T>(Transform root, Action<T> action = null, bool filter = false,
            string filterStr = "filter") where T : Component
        {
            if (filter && root.name == filterStr)
                return;

            if (root.childCount > 0)
                for (var i = 0; i < root.childCount; i++)
                    FindType(root.GetChild(i), action, filter, filterStr);

            var targetT = root.GetComponent<T>();
            if (targetT == null)
                //DF.Log("not exist: [" + typeof(T) + "] in [" + root + "]    __path: " + AssetDatabase.GetAssetPath(root), E_ColorType.Err);
                return;

            action?.Invoke(targetT);
        }

        //last root
        public static void FindType<T, K>(Transform root, Action<T, K> action = null, bool filter = false,
            string filterStr = "filter") where T : Component where K : Component
        {
            if (filter && root.name == filterStr)
                return;

            if (root.childCount > 0)
                for (var i = 0; i < root.childCount; i++)
                    FindType(root.GetChild(i), action, filter, filterStr);

            var targetT = root.GetComponent<T>();
            if (targetT == null)
                //DF.Log("not exist: [" + typeof(T) + "] in [" + root + "]    __path: " + AssetDatabase.GetAssetPath(root), E_ColorType.Err);
                return;

            var targetK = root.GetComponent<K>();
            if (targetK == null) return;

            action?.Invoke(targetT, targetK);
        }

        //first root
        public static void Recurrence<T>(Transform root, Action<T, int> act, int deep = 1) where T : Component
        {
            // var targetT = root.GetComponent<T>();
            // if (targetT != null)
            //     act?.Invoke(targetT,deep);

            for (var i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                var type = t.GetComponent<T>();
                if (type != null)
                {
                    act?.Invoke(type, deep);
                    Recurrence<T>(t, act, deep + 1);
                }
                else
                    Recurrence<T>(t, act, deep);
            }
        }


        public static T AddComponentType<T>(GameObject go, string type) where T : Component
        {
            var t = Type.GetType(type);
            if (t == null)
            {
                DF.Log($"not exist the type {type}", E_ColorType.Err);
                return null;
            }

            var com = go.AddComponent(t);
            var destT = com as T;
            if (destT == null)
            {
                DF.Log($"transfer is not suport {t}  to  {typeof(T)}", E_ColorType.Err);
                return null;

                //Type df = Type.GetType("DF");
                //DF.Log($"{df}   {df.GetType()}");
                //打印结果: DF   System.RuntimeType
            }

            return destT;
        }

        public static T GetComponentTypeByTag<T>(string tag)
        {
            var go = GameObject.FindWithTag(tag);
            if (go == null)
                DF.Log("find tag [" + tag + "]type " + typeof(T) + "  gameObject is null", E_ColorType.Err);
            var t = go.GetComponent<T>();
            if (t == null)
                DF.Log("find tag [" + tag + "]" + "  find  type " + typeof(T) + "  is null", E_ColorType.Err);
            return t;
        }

        /// <summary>
        ///     UICamera Raycast  sprite分支 获取最上层spr
        /// </summary>
        /// <param name="listTrs"></param>
        /// <returns></returns>
        public static Transform GetTopSpriteRenderer(List<Transform> listTrs)
        {
            if (listTrs == null || listTrs.Count == 0)
            {
                DF.Log("list is null ", E_ColorType.Err);
                return null;
            }

            var t = listTrs[0].transform;
            var layerId = -999;
            var sortId = -999;

            for (var i = 0; i < listTrs.Count; i++)
            {
                //DF.Log("hit " + hit[i].transform);
                Renderer sp;
                sp = listTrs[i].GetComponent<Renderer>();
                if (sp == null)
                    continue;

                //DF.Log("name " + sp.sortingLayerName,E_ColorType.Temp); 
                if (layerId <= DFConfig.Map.dictSortId[sp.sortingLayerName])
                {
                    if (sortId < sp.sortingOrder)
                    {
                        //DF.Log("layid " + listTrs[i].transform,E_ColorType.UI);
                        layerId = DFConfig.Map.dictSortId[sp.sortingLayerName];
                        t = listTrs[i].transform;
                        sortId = sp.sortingOrder;
                    }
                    else if (layerId < DFConfig.Map.dictSortId[sp.sortingLayerName])
                    {
                        layerId = DFConfig.Map.dictSortId[sp.sortingLayerName];
                        t = listTrs[i].transform;
                        sortId = sp.sortingOrder;
                    }
                }
            }

            return t;
        }

        /// <summary>
        ///     调用多层SpriteRenderer最上层的那个SpriteRenderer
        /// </summary>
        /// <param name="calcRoot"></param>
        /// calc sortinggroup
        /// <returns></returns>
        public static Transform GetTopSpriteRenderer(UnityAction<GameObject> act = null, bool calcRoot = false)
        {
            var mousePos =
                Camera.main.ScreenToWorldPoint(Input.mousePosition +
                                               new Vector3(0, 0, -Camera.main.transform.position.z));
            var mousePos2D = new Vector2(mousePos.x, mousePos.y);

            var hit = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

            if (hit.Length > 0)
            {
                var dictLevel = new Dictionary<Transform, List<int>>();
                var t = hit[0].transform;

                var layerId = -999;
                var sortId = -999;

                for (var i = 0; i < hit.Length; i++)
                {
                    DF.Log("hit " + hit[i].transform);
                    if (calcRoot)
                    {
                        DF.Log(hit[i].transform.name);
                        dictLevel.Add(hit[i].transform, CalcLevel(hit[i].transform));
                    }
                    else
                    {
                        SpriteRenderer sp;
                        sp = hit[i].transform.GetComponent<SpriteRenderer>();
                        if (sp == null)
                            continue;

                        //DF.Log("name " + sp.sortingLayerName,E_ColorType.Temp); 
                        if (layerId < DFConfig.Map.dictSortId[sp.sortingLayerName])
                        {
                            if (sortId < sp.sortingOrder)
                            {
                                DF.Log("layid " + hit[i].transform, E_ColorType.UI);
                                layerId = DFConfig.Map.dictSortId[sp.sortingLayerName];
                                t = hit[i].transform;
                                sortId = sp.sortingOrder;
                            }
                            else if (layerId < DFConfig.Map.dictSortId[sp.sortingLayerName])
                            {
                                layerId = DFConfig.Map.dictSortId[sp.sortingLayerName];
                                t = hit[i].transform;
                                sortId = sp.sortingOrder;
                            }
                        }

                        //int order = sp.sortingOrder;
                        //DF.Log(hit[i].transform.name +"  order " + order);
                        //sp = t.GetComponent<SpriteRenderer>();
                        //if (sp == null)
                        // continue;
                        //if (order > sp.sortingOrder)
                        // t = hit[i].transform;
                        //if (sortId < sp.sortingOrder)
                        //{
                        //    DF.Log("sortid " + hit[i].transform);
                        //    t = hit[i].transform;
                        //}
                    }
                }

                //if(t!=null)
                //act?.Invoke(t.gameObject);
                if (t != null && act != null)
                    act(t.gameObject);
                return t;
            }

            return null;
        }

        public static List<int> CalcLevel(Transform target)
        {
            DF.Log(target.name);
            var level = 0;
            var sort = 0;
            var hasCalc = false;
            while (target.parent != null)
            {
                if (!hasCalc)
                {
                    var spR = target.GetComponent<SpriteRenderer>();
                    if (spR != null) sort += spR.sortingOrder;
                }

                target = target.parent;
                var sg = target.GetComponent<SortingGroup>();
                if (sg != null)
                {
                    hasCalc = true;
                    sort += sg.sortingOrder;
                }
                else
                {
                    hasCalc = false;
                }

                level++;
            }

            var list = new List<int>();
            list.Add(level);
            list.Add(sort);
            DF.Log(level + "  " + sort);
            return list;
        }
    }

    public class ToolFormat
    {
    }

    public class ToolMath
    {
    }

    public class ToolColor
    {
        public static Color GetColor(string str)
        {
            // ColorUtility.TryParseHtmlString(str, out var c);
            Color c;
            ColorUtility.TryParseHtmlString(str, out c);
            return c;
        }
    }

    public class ToolPlayerPrefs
    {
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static void SetStringPlus(string key, string value, char split = '|')
        {
            if (PlayerPrefs.HasKey(key))
            {
                var oldValue = PlayerPrefs.GetString(key);
                oldValue += split + value;
                PlayerPrefs.SetString(key, oldValue);
            }
            else
            {
                DF.Log("SetStringPlus not allow    doesn't exist old value");
            }
        }

        public static int GetInt(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                var v = PlayerPrefs.GetInt(key);
                return v;
            }

            DF.Log("GetInt not exist " + key);
            return -1;
        }

        public static void GetString(string key, out List<string> value, char split = '|')
        {
            if (PlayerPrefs.HasKey(key))
            {
                var v = PlayerPrefs.GetString(key);
                value = new List<string>(v.Split(split));
            }
            else
            {
                DF.Log("GetString not exist " + key);
                value = new List<string>();
            }
        }

        public static void SetPos(string key, Vector3 pos)
        {
            var x = pos.x.Float2String(2);
            var y = pos.y.Float2String(2);
            var z = pos.z.Float2String(2);
            var p = x + "|" + y + "|" + z;
            PlayerPrefs.SetString(key, p);
        }

        public static Vector3 GetPos(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                var pos = PlayerPrefs.GetString(key);
                var strs = pos.Split('|');
                var x = float.Parse(strs[0]);
                var y = float.Parse(strs[1]);
                var z = float.Parse(strs[2]);
                return new Vector3(x, y, z);
            }

            return Vector3.zero;
        }
    }

    public class ToolFile
    {
        //File  
        //c# GetFileMD5 GetFileMD52 返回的md5值一样
        //下面py计算的md5 也跟c# 计算的一样
        //说明unity ab自带的计算出的md5是特殊的
        // fd = open(path, "rb")
        // fcont = fd.read()
        // fmd5 = hashlib.md5(fcont)
        // fd.close()
        // print(fmd5.hexdigest())
        /// <summary>
        ///     string md5 = getFileHash("E:\\MyPro\\cubetest.unity3d");
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileMD5(string filePath)
        {
            try
            {
                var fs = new FileStream(filePath, FileMode.Open);
                var len = (int) fs.Length;
                var data = new byte[len];
                fs.Read(data, 0, len);
                fs.Close();
                MD5 md5 = new MD5CryptoServiceProvider();
                var result = md5.ComputeHash(data);
                var fileMD5 = "";
                foreach (var b in result) fileMD5 += Convert.ToString(b, 16);

                //或者
                //for (int i = 0; i < result.Length; i++)
                //{

                //	//“x2”表示输出按照16进制，且为2位对齐输出。
                //	fileMD5+=result[i].ToString("x2");
                //}

                return fileMD5;
            }
            catch (FileNotFoundException e)
            {
                DF.Log(" FileNotFoundException   " + e.Message, E_ColorType.Err);
                return "";
            }
        }

        public static string GetFileMD52(string filePath)
        {
            byte[] retVal;
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                retVal = md5.ComputeHash(file);
            }

            return retVal.ToHex("x2");
        }

        /// <summary>
        ///     string[] files = GetFiles("*.gif", "*.jpg", "*.png");
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="recurrence"></param>默认只搜索第一层
        /// <returns></returns>
        public static string[] GetFiles(string dirPath, bool recurrence = false, params string[] searchPatterns)
        {
            if (searchPatterns.Length <= 0)
                searchPatterns = new[] {"*.*"};
            
            SearchOption searchOpt = recurrence ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var di = new DirectoryInfo(dirPath);
            var fis = new FileInfo[searchPatterns.Length][];
            var count = 0;
            for (var i = 0; i < searchPatterns.Length; i++)
            {
                var fileInfos = di.GetFiles(searchPatterns[i], searchOpt);
                fis[i] = fileInfos;
                count += fileInfos.Length;
            }

            var files = new string[count];
            var n = 0;
            for (var i = 0; i <= fis.GetUpperBound(0); i++)
            for (var j = 0; j < fis[i].Length; j++)
            {
                var temp = fis[i][j].FullName;
                files[n] = temp;
                n++;
            }

            return files;
        }

        /// <summary>
        ///     传入的如果是目录路径必须以 /或\结尾
        /// </summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        public static bool PathCheck(string path)
        {
            FileInfo info = new FileInfo(path);
            info.Refresh();
            DF.Log(info.Exists + "   " + path);
            if (info.Exists)
            {
                return true;
            }


            var index = path.LastIndexOfAny(new[] {'/', '\\'}); //支持两种路径写法
            var dir = path.Substring(0, index);
            DF.Log("checkPath " + dir);

            if (!Path.IsPathRooted(path))
            {
                DF.Log("invalid path", E_ColorType.Err);
                return false;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                DF.Log("create Dir: " + dir, E_ColorType.UI);
                return true;
            }

            if (index < path.Length - 1 && path.IndexOf('.') > 0)
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                    DF.Log("create file: " + path, E_ColorType.UI);

                    // FileSecurity security = File.GetAccessControl(path);
                    // FileSystemAccessRule rule= new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,PropagationFlags.NoPropagateInherit,AccessControlType.Allow);
                    // FileSystemAccessRule rule= new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, AccessControlType.Allow);
                    // security.AddAccessRule(rule);
                    GrantAccess(path);
                    return true;
                }

            return false;
        }


        private static void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit |
                InheritanceFlags.ContainerInherit,
                PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow));

            dInfo.SetAccessControl(dSecurity);
        }

        public static byte[] BinaryReader(string pathFile)
        {
            FileStream fs = new FileStream(pathFile, FileMode.Open);
            byte[] data = new byte[fs.Length];
            BinaryReader br = new BinaryReader(fs);
            int lenRead = 1024;
            long lenReaded = 0;
            for (int j = 0; j < fs.Length; j+=lenRead)
            {
                if (fs.Length - lenReaded < lenRead)
                {
                    lenRead = (int)(fs.Length - lenReaded);
                }
                byte[] temp= br.ReadBytes(lenRead);
                for (int k = 0; k < lenRead; k++)
                {
                    data[lenReaded + k] = temp[k];
                }
                lenReaded += lenRead;
            }
            
            br.Dispose();
            fs.Dispose();
            return data;
        }

        public static void BinaryWriter(string pathFile, byte[] data)
        {
            FileStream fs = new FileStream(pathFile, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(data);
            bw.Flush();
            bw.Dispose();
            fs.Dispose();
        }

        public static byte[] EncryBtyes(byte[] data, string encryStr)
        {
            if (data == null || data.Length==0)
                return null;
            for (int j = 0; j < encryStr.Length; j++)
            {
                byte b = (byte)encryStr[j];
                data[j]= (byte) (data[j] ^ b);
            }

            return data;
        }

        public static byte[] DecryptBytes(byte[] data, string decryStr)
        {
            return EncryBtyes(data, decryStr);
        }
    }

    public class ToolTime
    {
        //Time
        private static List<DTItem> listDT;

        private static Stopwatch watch;
        //public static void TestGetDateTime()
        //{
        //    DF.Log(DateTime.Today);
        //    DF.Log(DateTime.Now);
        //    DF.Log(DateTime.UtcNow);
        //    DF.Log(GetFormatDateTime(DateTime.Now));

        //    DF.Log(GetMonthDay());
        //}
        public static string GetFormatDateTime(DateTime? dt = null, E_DateType t = E_DateType.Default)
        {
            switch (t)
            {
                case E_DateType.Default:
                    return (dt ?? DateTime.Now).ToString("yyyy/MM/dd-HH:mm:ss");
                case E_DateType.Log:
                    return (dt ?? DateTime.Now).ToString("yyyyMMdd_HHmmss");
                default:
                    return "xxx";
            }
        }

        public static List<int> GetSeparateDateTime()
        {
            var dateTime = GetFormatDateTime(DateTime.Now);
            var date_time = dateTime.Split('-');
            var date = date_time[0].Split('/');
            var time = date_time[1].Split(':');
            var year = int.Parse(date[0]);
            var month = int.Parse(date[1]);
            var day = int.Parse(date[2]);
            var hour = int.Parse(time[0]);
            var minute = int.Parse(time[1]);
            var second = int.Parse(time[2]);

            var dt = new List<int>();
            dt.Add(year);
            dt.Add(month);
            dt.Add(day);
            dt.Add(hour);
            dt.Add(minute);
            dt.Add(second);
            return dt;
        }

        public static string GetMonthDay()
        {
            var dt = GetSeparateDateTime();
            var month = dt[1];
            var day = dt[2];
            return month + "-" + day;
        }

        public static string GetFormatDateFromSecond(int second)
        {
            var min = second / 60;
            if (min == 0) return "<1m";

            var hour = min / 60;
            min = min % 60;

            if (hour == 0)
                return min + "m";
            return hour + "h" + min + "m";
        }

        public static void ClearDTStack()
        {
            var key = "dtStack";
            if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
        }

        public static string DTStack(string dt)
        {
            //string key = "dayTimeStack";
            var newDt = "";
            //if (PlayerPrefs.HasKey(key))
            //{

            //string dtStack = PlayerPrefs.GetString(key);
            //string[] strsTemp = dtStack.Split('|');

            if (listDT.Count > 0)
                if (listDT[listDT.Count - 1].md == dt)
                {
                    var keys = "";
                    foreach (var item in listDT) keys += item.md + "|";

                    keys = keys.Substring(0, keys.Length - 1);
                    return keys;
                }

            //dtStack += "|" + dt;
            //string[] strs = dtStack.Split('|');
            if (listDT.Count < 7)
            {
                var item = new DTItem(dt);
                listDT.Add(item);
            }
            else
            {
                listDT.RemoveAt(0);
                var item = new DTItem(dt);
                listDT.Add(item);
            }

            foreach (var item in listDT) newDt += item.md + "|";

            newDt = newDt.Substring(0, newDt.Length - 1);

            return newDt;
        }

        public static void StartTimer()
        {
            DF.Log(SceneManager.GetActiveScene().name + "  StartTimer t ", E_ColorType.UI);
            watch = new Stopwatch();
            watch.Start();
        }

        public static double StopTimer()
        {
            if (watch == null)
                return 0;
            DF.Log(SceneManager.GetActiveScene().name + "  StopTimer t ", E_ColorType.UI);
            watch.Stop();
            var time = watch.Elapsed.TotalSeconds;
            watch = null;
            return time;
        }

        public static void RecordTimeUpdate(double t, string day)
        {
            DF.Log(SceneManager.GetActiveScene().name + "  RecordTimeUpdate t " + t, E_ColorType.UI);
            if (listDT.Count == 0)
                listDT.Add(new DTItem(GetMonthDay()));
            //更新当天累积时间
            var item = listDT[listDT.Count - 1];
            var keyD = item.md;
            item.time += Mathf.RoundToInt((float) t);
        }

        public static void RecordPkUpdate(double t, bool win, string day)
        {
            DF.Log(SceneManager.GetActiveScene().name + "  RecordPkUpdate t " + t + "  win  " + win, E_ColorType.Temp);
            if (listDT.Count == 0)
                listDT.Add(new DTItem(GetMonthDay()));

            var item = listDT[listDT.Count - 1];
            var keyD = item.md;
            item.timePk += Mathf.RoundToInt((float) t);
            item.times += 1;
            if (win)
                item.timesWin += 1;
        }

        public static int RecordTimeGet()
        {
            //string key = "recordTimeReport";
            //int t = 0;
            //if (PlayerPrefs.HasKey(key))
            //{
            //    string record= PlayerPrefs.GetString(key);
            //    string[] strs = record.Split('|');
            //    t=int.Parse(strs[1]);
            //    Darkfeast.Log("t " + strs[0],E_ColorType.Over);
            //}
            //return t;
            var item = listDT[listDT.Count - 1];
            DF.Log("t " + item.md, E_ColorType.Over);
            return item.time;
        }

        public static string DayTimeStack(string dt)
        {
            var key = "dayTimeStack";
            var newDt = "";

            if (PlayerPrefs.HasKey(key))
            {
                var dtStack = PlayerPrefs.GetString(key);
                var strsTemp = dtStack.Split('|');
                if (strsTemp[strsTemp.Length - 1] == dt) return dtStack;

                dtStack += "|" + dt;
                var strs = dtStack.Split('|');
                if (strs.Length <= 7)
                {
                    PlayerPrefs.SetString(key, dtStack);
                    newDt = dtStack;
                }
                else
                {
                    for (var i = strs.Length - 7; i < strs.Length; i++) newDt += "|" + strs[i];

                    newDt = newDt.Substring(1, newDt.Length - 1);
                    PlayerPrefs.SetString(key, newDt);
                }
            }
            else
            {
                PlayerPrefs.SetString(key, dt);
                newDt = dt;
            }

            return newDt;
        }

        public static void LoadDT()
        {
            //JsonUtility.fro
            //1-3_1111_999|
            var key = "dt";
            if (PlayerPrefs.HasKey(key))
            {
                var dtStr = PlayerPrefs.GetString(key);
                var dtArr = dtStr.Split('|');
                var listDTItem = new List<DTItem>();
                for (var i = 0; i < dtArr.Length; i++)
                {
                    var dtItemArr = dtArr[i].Split('_');
                    var item = new DTItem();
                    item.md = dtItemArr[0];
                    item.time = int.Parse(dtItemArr[1]);
                    item.times = int.Parse(dtItemArr[2]);
                    item.timePk = int.Parse(dtItemArr[3]);
                    item.timesWin = int.Parse(dtItemArr[4]);
                    listDTItem.Add(item);
                }

                listDT = listDTItem;
                if (listDT[listDT.Count - 1].md != GetMonthDay()) listDT.Add(new DTItem(GetMonthDay()));
            }
            else
            {
                listDT = new List<DTItem>();
                listDT.Add(new DTItem(GetMonthDay()));
            }
        }

        public static void SaveDT()
        {
            var key = "dt";
            var dtStr = "";
            foreach (var item in listDT) dtStr += "|" + item.md + "_" + item.time + "_" + item.times + "_" + item.timePk + "_" + item.timesWin;

            dtStr = dtStr.Substring(1, dtStr.Length - 1);
            PlayerPrefs.SetString(key, dtStr);
        }

        public static void PrintDT()
        {
            foreach (var item in listDT) DF.Log(item.md + "  " + item.time + "  " + item.times, E_ColorType.UI);
        }

        public static void ClearDT()
        {
            var key = "dt";
            if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
        }

        public static List<DTItem> GetDT()
        {
            return listDT;
        }
    }
}

[Serializable]
public class DT
{
    public List<DTItem> dt;
}

[Serializable]
public class DTItem
{
    public string md;
    public int time;
    public int times; //pk
    public int timePk; //pk
    public int timesWin; //pk

    public DTItem()
    {
    }

    public DTItem(string m)
    {
        md = m;
    }
}