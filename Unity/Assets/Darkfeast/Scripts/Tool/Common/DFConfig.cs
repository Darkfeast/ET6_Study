using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFConfig
{
    public const string ver = "t20210519_1"; //格式： t+Data+version(当天测试)
    public const bool Debug = true;
    public const bool Print = true;

    public static bool KillMode = false;
    public static float timeScale = 10; //快速测试

    public static float screenWidth = 1024;
    public static float screenHeight = 768;
    public static string resPath;

    public class PlayerPrefabKey
    {
        public const string pkRecords = "pkRecords"; //pk 模式
    }

    public class SceneName
    {
        public const string entry = "entry"; //首页
        public const string main = "main";
        public const string transition = "transition"; //  transition
    }

    public class ValueConfig
    {
        public const float rightRate = 0.6f; //正确率
        public const float tipVolume = 0.8f;
    }

    public class SortingLayer
    {
        public const string Default = "Default";
        public const string drag = "drag";
    }

    public class Encrypt
    {
        public const string strEnCrypt = "x23darkfeasst502010990";
    }

    public class Address
    {
        public const string remoteAddress = "http://110.42.209.76";
    }

    public class Map
    {
        public static Dictionary<string, int> dictSortId = new Dictionary<string, int>()
        { 
            {"Default", 0},
            {"item", 1},
            {"lv1", 2},
            {"lv2", 3},
            {"lv3", 4},
            {"lv4", 5},
            {"lv5", 6},
            {"lv6", 7},
            {"lv7", 8},
            {"drag", 19},
        };
    }
}