using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Linq;

public class DFEditorGenAudio : MonoBehaviour {

	const string path = @"DF/Gen/";

    static string pathGenPrefix="Assets/App/Scripts/Gen/";

    [MenuItem( path+"GenAudioCs",false,10)]
	static void GenAudioCs()
    {
		string pathAudios = "Assets/Resources/Audios";
		string pathGen = pathGenPrefix+"AudioConfig/AudioConfig.cs";
		if(!Directory.Exists(pathAudios))
        {
			Directory.CreateDirectory(pathAudios);
            DF.Log(pathAudios + " not exist");
            return;
        }
		List<string> listFiles=  Directory.GetFiles(pathAudios,"*.ogg").ToList();
		DF.Log(listFiles.Count);

        PathCheck(pathGen);
		StreamWriter sw = new StreamWriter(pathGen);
		sw.WriteLine("using UnityEngine;\n");
		sw.WriteLine("public class AudioConfig : MonoBehaviour {");
		//listFiles.Sort((a, b) => { 
		//	AudioClip ac1= AssetDatabase.LoadAssetAtPath<AudioClip>(a);
		//	AudioClip ac2= AssetDatabase.LoadAssetAtPath<AudioClip>(b);
		//	if (ac1.length < ac2.length)
		//		return -1;
		//	else if (ac1.length > ac2.length)
		//		return 1;
		//	return 0;
		//});
		for (int i = 0; i < listFiles.Count; i++)
        {
			//AudioClip ac= AssetDatabase.LoadAssetAtPath<AudioClip>(listFiles[i]);
			sw.WriteLine("\tpublic const string " + Path.GetFileNameWithoutExtension(listFiles[i]) + " = \"" + Path.GetFileNameWithoutExtension(listFiles[i]) + "\";");
        }

		sw.WriteLine("\n}");
		sw.Close();
		AssetDatabase.Refresh();
		DF.Log("gen "+pathGen+" finish...",E_ColorType.Over);
    }

	[MenuItem(path + "GenBattleAudio", false, 12)]
	static void GenBattleAudio()
	{
        string pathGen = pathGenPrefix+"Audios";
		if(!Directory.Exists(pathGen))
        {
			Directory.CreateDirectory(pathGen);
        }

		for(int i=1;i<5;i++)
        {
			string classP = "BattleAudioT" + i + "P";
			string classPP = "BattleAudioT" + i + "PP";
			string classE = "BattleAudioT" + i + "E";
			string classEP = "BattleAudioT" + i + "EP";

			GenBattleCs(classP, pathGen + "/" + classP + ".cs");
			GenBattleCs(classPP, pathGen + "/" + classPP + ".cs");
			GenBattleCs(classE, pathGen + "/" + classE + ".cs");
			GenBattleCs(classEP, pathGen + "/" + classEP + ".cs");
        }
	}

	[MenuItem(path+"ParseTagManager",false,15)]
	public static void ParseTagManager()
    {
        string pathTag = Application.dataPath.Substring(0,Application.dataPath.Length-7)+"/ProjectSettings/TagManager.asset";
        //DF.Log("path  " + pathTag);
        if(File.Exists(pathTag))
        {
            StreamReader sr = new StreamReader(pathTag);
            string content= sr.ReadToEnd();
            sr.Dispose();
            DF.Log("content " + content);

            string strStart = "m_SortingLayers";
            string nameSign = "name:";
            string uniqueSign = "uniqueID:";

            List<string> listLayer = new List<string>();
            if(content.Contains(strStart))
            {
                int ind = content.IndexOf(strStart);
                string sortStr = content.Substring(ind);
                while(ind>=0)
                {
                    int nameStart = sortStr.IndexOf(nameSign);
                    int uniqueStart = sortStr.IndexOf(uniqueSign);

                    if (nameStart >= 0 && uniqueStart > nameStart)
                    {
                        string layer = sortStr.Substring(nameStart + nameSign.Length, uniqueStart - (nameStart + nameSign.Length));
                        sortStr = sortStr.Substring(uniqueStart + uniqueSign.Length);
                        ind = content.IndexOf(strStart);
                        listLayer.Add(layer.Trim());
                    }
                    else
                        break;
                }
            }
            string fileName = pathGenPrefix + "/ParseTagManager/mapTag.txt";
            PathCheck(fileName);

            StreamWriter sw = new StreamWriter(fileName);
            sw.Write("\tpublic class SortingLayer\n\t{\n");
            for(int i=0;i<listLayer.Count;i++)
            {
                sw.Write("\t\tpublic const string " + listLayer[i] + " = \"" + listLayer[i] + "\";\n");
            }
            sw.Write("\t}\n");

            sw.Write("\tpublic class Map{\n\t\tpublic static Dictionary<string, int> dictSortId = new Dictionary<string, int>() {\n");
            for (int i = 0; i < listLayer.Count; i++)
            {
                sw.Write("\t\t\t{\"" + listLayer[i] + "\"," + i + "},\n");
            }
            sw.Write("\t\t};\n\t}");
            sw.Dispose();

            DF.Log("gen "+fileName+" finish...", E_ColorType.Over);
            AssetDatabase.Refresh();
        }
    }

	static void GenBattleCs(string className, string path)
    {
		if (File.Exists(path))
        {
			DF.Log(className + " has exit", E_ColorType.UI);
			return;
        }
		FileStream fs= File.Create(path);
		StreamWriter sw = new StreamWriter(fs);
		sw.WriteLine("using UnityEngine;\n");
		sw.WriteLine("public class "+className + " : BattleAudio {");
		sw.WriteLine("\tvoid Start(){");
		sw.WriteLine("\t\tbase.Start();");
		sw.WriteLine("\t}");
		sw.WriteLine("\n}");
		sw.Close();
		fs.Close();
		AssetDatabase.Refresh();
		DF.Log("gen "+ className+" finish...",E_ColorType.Over);
    }

    static void PathCheck(string path)
    {
        int index= path.LastIndexOfAny(new char[] { '/','\\'}); //支持两种路径写法
        string dir = path.Substring(0, index);
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            DF.Log("create Dir: " + dir, E_ColorType.UI);
        }
        
        if(!File.Exists(path))
        {
            File.Create(path).Dispose();
            DF.Log("create file: " + path,E_ColorType.UI);
        }
    }
 
}
