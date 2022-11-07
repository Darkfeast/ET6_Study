using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public static class DFLog
{
    private static List<string> listLog;
    private static string pathLog;
    private static E_State stateLog;
    private static int logMaxCapacity;
    private static int logBufferMaxNumber;


    private static string pathDefault = "ApoLog\\DFLog.txt";

    private static string logTipImportant = "@Important !!! ";
    private static string logTipWarning = "Warning ";
    public static bool stackInfo;

    static DFLog()
    {
        Application.logMessageReceived += LogCallBack;
        listLog = new List<string>();

        pathLog = "E:\\ApoLog\\DF_Log.txt";
        stateLog = E_State.Develop;
        logMaxCapacity = 200;
        logBufferMaxNumber = 1;

        if (string.IsNullOrEmpty(pathLog))
        {
            pathLog = Application.persistentDataPath + "//" + pathDefault;
        }
    }

    static void LogCallBack(string mesg, string stackMesg, LogType t)
    {
        if (t == LogType.Warning || !stackInfo)
            return;
        Log(mesg + "\n[stack]: " + stackMesg);
    }


    public static void Log(string writeFileDate, Level level = Level.Low)
    {
        if (stateLog == E_State.Stop)
        {
            return;
        }

        if (listLog.Count >= logMaxCapacity)
        {
            listLog.Clear();
        }

        if (!string.IsNullOrEmpty(writeFileDate))
        {
            writeFileDate = stateLog + "|" + DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + "|   " + writeFileDate + "\r\n";

            if (level == Level.High)
            {
                writeFileDate = logTipImportant + writeFileDate;
            }
            else if (level == Level.Special)
            {
                writeFileDate = logTipWarning + writeFileDate;
            }

            switch (stateLog)
            {
                case E_State.Develop:
                    AppendMesgToFile(writeFileDate);
                    break;
                case E_State.Speacial:
                    if (level == Level.High || level == Level.Special)
                    {
                        AppendMesgToFile(writeFileDate);
                    }

                    break;
                case E_State.Deploy:
                    if (level == Level.High)
                    {
                        AppendMesgToFile(writeFileDate);
                    }

                    break;
                case E_State.Stop:
                default:
                    break;
            }
        }
    }

    private static void AppendMesgToFile(string mesg)
    {
        if (!string.IsNullOrEmpty(mesg))
        {
            listLog.Add(mesg);
        }

        if (listLog.Count % logBufferMaxNumber == 0)
        {
            SyncLogCatchToFile();
        }
    }

    private static void CreateFile(string pathName, string info)
    {
        StreamWriter sw;
        FileInfo t = new FileInfo(pathName);
        t.Refresh();

        var index = pathName.LastIndexOfAny(new[] {'/', '\\'});
        var dir = pathName.Substring(0, index);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (!t.Exists)
        {
            sw = t.CreateText();
        }
        else
        {
            sw = t.AppendText();
        }

        sw.WriteLine(info);
        sw.Close();
    }

    public static List<string> QueryAllMesgFromLogBuffer()
    {
        return listLog;
    }

    public static int QueryLogBufferCount()
    {
        if (listLog != null)
        {
            return listLog.Count;
        }

        return -1;
    }

    public static void SyncLogCatchToFile()
    {
        if (!string.IsNullOrEmpty(pathLog))
        {
            foreach (string msg in listLog)
            {
                CreateFile(pathLog, msg);
            }

            listLog?.Clear();
        }
    }

    public enum E_State
    {
        Develop, //开发模式（输出所有日志内容）
        Speacial, //指定输出模式
        Deploy, //部署模式（只输出最核心日志信息，例如严重错误信息，用户登陆账号等）
        Stop //停止输出模式（不输出任何日志信息）
    };

    public enum Level
    {
        High,
        Special,
        Low
    }
}