using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DFConsole : MonoBehaviour
{

    [HideInInspector]
    public string output = "";
    [HideInInspector]
    public string stack = "";

    public bool isError = true;
    public bool isLog = true;
    public bool isWarning = true;
    public bool isException = true;
	public static int messagesNum=200;

    private void Awake()
    {
        Application.logMessageReceived += HandleLog;
        gameObject.AddComponent<MyDebug>();
        string str = "isLog : " + isLog + "; isWarning : " + isWarning + "; isError : " + isError + "; isException : " + isException;
        MyDebug.Add("调试测试程序", "已启动！"+str);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        if (type == LogType.Error && isError)
        {
            output = "<color=#A90404FF>" + output + "</color>";
            stack = "<color=#A90404FF>" + stack + "</color>";
            MyDebug.Add(output, stack);
        }
        else if (type == LogType.Warning && isWarning)
        {
			output = "<color=#0002FFFF>" + output + "</color>";
			stack = "<color=#0002FFFF>" + stack + "</color>";
            MyDebug.Add(output, stack);
        }
        else if (type == LogType.Log && isLog)
        {
			output = "<color=#000000FF>" + output + "</color>";
			stack = "<color=#000000FF>" + stack + "</color>";
            MyDebug.Add(output, stack);
        }
        else if (type == LogType.Exception && isException)
        {
            output = "<color=#A90404FF>" + output + "</color>";
            stack = "<color=#A90404FF>" + stack + "</color>";
            MyDebug.Add(output, stack);
        }

    }
    public class MyDebug : MonoBehaviour
    {
		private Vector2 ScrollPos;
		public static List<string> messages = new List<string>();
		public static List<string> names = new List<string>();
		public static bool isShow = false;

		bool Show = false;
		string btnstr = "Show";
		string language = "English";

        public float scale;//比例   屏幕宽度/720   获得了比例 
		int fontSizeG;
		private void Start()
		{
			scale = Screen.width / (float)720;
			fontSizeG = (int)(20 * scale);
		}
		void OnGUI()
		{
			GUIStyle a = new GUIStyle
			{
				fontSize = (int)(20 * scale)
			};

			if (GUI.Button(new Rect(Screen.width - 140 * scale, Screen.height - 70 * scale, 50 * scale, 50 * scale), "Clear", a))
			{
				Clear();
			}

			if (GUI.Button(new Rect(Screen.width - 70 * scale, Screen.height - 70 * scale, 50 * scale, 50 * scale), btnstr, a))
			{
				Show = !Show;
				btnstr = Show == true ? "Hide" : "Show";
			}
			if (Show)
			{
				int count;
				count = names.Count;
				if (count >50)
					count = 50;

				ScrollPos = GUI.BeginScrollView(new Rect(25 * scale, 10 * scale, Screen.width * 0.8f, Screen.height * 0.5f), ScrollPos, new Rect(0, 0, Screen.width*0.8f, 300 * scale + 300  * 250));
				float posY = 50 * scale;
				int num = 1;
				for (int i = 0; i < names.Count; i++)
				{
					GUIContent tempContent = new GUIContent();
					GUILayout.Space(10 * scale);
					tempContent.text = " <" + num + "> " + names[i] + " : " + messages[i];
					GUIStyle bb = new GUIStyle
					{
						fixedWidth = Screen.width,
						wordWrap = true,
						//fontSize = (int)(10 * scale)
						fontSize = fontSizeG
					};
					float H = bb.CalcHeight(tempContent, Screen.height);
					GUI.Label(new Rect(0, posY, Screen.width, H), tempContent, bb);
					posY += H;
					num += 1;
					GUILayout.Space(30 * scale);
				}
				GUI.EndScrollView();

                //language ="Language：\n"+ DF.MgrLanguage._Language.ToString();
                if (GUI.Button(new Rect(70, Screen.height - 70 * scale, 50 * scale, 50 * scale), language, a))
                {
                    //int index = (int)DF.MgrLanguage._Language;
                    //index++;
                    //index %= 10;
                    //Debug.Log(index);
                    //DF.MgrLanguage.ChangeLanguage(index);
                }
            }
            
		}

        public static void Add(string name, string message)
        {
            names.Add(name);
            messages.Add(message);
        }

			public void Clear()
			{
				names.Clear();
				messages.Clear();
			}
    }
}
