using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class DFPackerTagToolEditor : EditorWindow
{

    public string filterDir = "";
    public string packerTag = "";
    public string defaultSelectPath = "";
    string retf = "";
    string searchRes = "";
    int currentIndex = 0;
    int testIndex = 0;
    int branch;

    static int width = 800;
    static int height = 600;
    int buttonWidth = 80;
    GUIStyle guiStylePath = new GUIStyle();
    GUIStyle guiStyleSearchPath = new GUIStyle();
    bool validPath;
    bool clearPackerTag;
    int fileCount;
    [MenuItem("DF/Tool/PackTag")]
    public static void Openwindows()
    {
        EditorWindow window = GetWindow(typeof(DFPackerTagToolEditor));
        window.minSize = new Vector2(width, height);
        window.maxSize = new Vector2(width, height);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("输入过滤的目录,存在多个目录用;隔开 (单个目录需以;结尾)");
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        filterDir = EditorGUILayout.TextField(filterDir);
        GUILayout.EndHorizontal();


        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("输入packTag");
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        packerTag = EditorGUILayout.TextField(packerTag);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Space(width - buttonWidth);
        if (GUILayout.Button("select...", GUILayout.Width(buttonWidth), GUILayout.Height(30)))
        {
            validPath = true;
            searchRes = "";
            retf = SelectFolder();
            guiStylePath.normal.textColor = Color.cyan * 0.85f;
            VerifyValid();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("current path:");
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUILayout.TextField(retf, guiStylePath);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(width - buttonWidth);
        if (GUILayout.Button("PackerTag", GUILayout.Width(buttonWidth), GUILayout.Height(30)))
        {
            if (!validPath) return;
            currentIndex = 0;
            branch = 1;
            PackTag();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUIStyle guiStyleIndex = new GUIStyle();

        guiStyleIndex.normal.textColor = Color.green * 0.85f;
        GUILayout.Label("complete: " + currentIndex.ToString(), guiStyleIndex);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Space(width - buttonWidth + 20);
        GUILayout.Label("clear", GUILayout.Width(35), GUILayout.Height(20));
        clearPackerTag = GUILayout.Toggle(clearPackerTag, "");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(width - buttonWidth);
        if (GUILayout.Button("SearchTag", GUILayout.Width(buttonWidth), GUILayout.Height(30)))
        {
            if (!validPath)
            {
                searchRes = "validPath";
                guiStyleSearchPath.normal.textColor = Color.red * 0.8f;
                return;
            }
            if (packerTag == "")
            {
                searchRes = "search packerTag is null";
                guiStyleSearchPath.normal.textColor = Color.red * 0.8f;
                //return;
            }
            currentIndex = 0;
            branch = 2;
            PackTag();
            searchRes = "search complete...";
            guiStyleSearchPath.normal.textColor = Color.cyan * 0.85f;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("search res:");
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUILayout.TextField(searchRes, guiStyleSearchPath);
        GUILayout.EndHorizontal();

        GUILayout.Space(40);
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("testComplete: " + testIndex.ToString(), guiStyleIndex);
        GUILayout.EndHorizontal();
        GUILayout.Space(40);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("testAdd", GUILayout.Width(80), GUILayout.Height(30)))
        {
            testIndex = 0;
            //EditorApplication.update += UpdateProgress;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(40);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("testRemove", GUILayout.Width(80), GUILayout.Height(30)))
        {
            //EditorApplication.update -= UpdateProgress;
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void UpdateProgress()
    {
        EditorUtility.DisplayProgressBar("packer tag progress", currentIndex+"/"+42, currentIndex / (float)42);
        testIndex++;
    }

    void OnInspectorUpdate() //更新
    {
        Repaint();  //重新绘制
    }
    string SelectFolder()
    {
        //defaultSelectPath = Application.dataPath + @"\App\Textrues";
        defaultSelectPath = Application.dataPath;
        //EditorUtility.RevealInFinder()
        string selectFolder = EditorUtility.OpenFolderPanel("select dir", defaultSelectPath, "");
        //DF.Log("select Folder " + selectFolder);
        return selectFolder;
    }
    void VerifyValid()
    {
        string currentP = Application.dataPath;
        //DF.Log("current " + currentP);
        //DF.Log("current2 " + retf);
        if (retf.IndexOf("Assets") < 0)
        {
            retf = "path is error";
            guiStylePath.normal.textColor = Color.red * 0.85f;
            validPath = false;
        }
        else if (currentP.Substring(0, currentP.IndexOf("Assets")) != retf.Substring(0, retf.IndexOf("Assets")))
        {
            retf = "path is error";
            guiStylePath.normal.textColor = Color.red * 0.85f;
            validPath = false;
        }

    }
    void PackTag()
    {
        List<string> listFilter = new List<string>(filterDir.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
        for (int i = 0; i < listFilter.Count; i++)
        {
            listFilter[i] = listFilter[i].ToLower();
        }
        //DF.Log("filterCount " + listFilter.Count);
        //listFilter.Print();
        //DF.Log("---------------------", E_ColorType.Over);
        fileCount = 0;
        if (Directory.Exists(retf))
            FindFiles(retf, listFilter);

        EditorUtility.ClearProgressBar();
    }

    void FindFiles(string parent, List<string> filter)
    {
        //DF.Log("parent " + parent);
        //DFS
        string[] curDir = Directory.GetDirectories(parent);
        for (int i = 0; i < curDir.Length; i++)
        {
            if (filter.Contains(curDir[i].Split('\\')[curDir[i].Split('\\').Length - 1].ToLower()))
            {
                //DF.Log("return " + curDir[i], E_ColorType.Temp);
                continue;
            }
            fileCount += Directory.GetFiles(curDir[i], "*.png").Length;
            fileCount += Directory.GetFiles(curDir[i], "*.jpg").Length;
            //DF.Log("curDir " + curDir[i]);
            FindFiles(curDir[i], filter);
       
        }

        DF.Log("fileCount " + fileCount, E_ColorType.Temp);
        string[] curFile = Directory.GetFiles(parent, "*.png");
        for (int i = 0; i < curFile.Length; i++)
        {
            //DF.Log("curFile  " + curFile[i], E_ColorType.Over);
            string asspath = curFile[i].Substring(curFile[i].IndexOf("Assets"));
            //DF.Log("curFile2  " + asspath, E_ColorType.Over);
            asspath = asspath.Replace('\\', '/');
            //DF.Log("curFile3.png  " + asspath, E_ColorType.Over);
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(asspath);
            if (sp == null)
            {
                //DF.Log("[not sprite] " + asspath, E_ColorType.Err);
                continue;
            }
            //DF.Log("sp  " + sp.name, E_ColorType.UI);

            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sp)) as TextureImporter;
            if (branch == 1)
            {
                importer.spritePackingTag = packerTag;
                //importer.SetTextureSettings(imp);
                importer.SaveAndReimport();
                currentIndex++;
            }
            else if (branch == 2)
            {
                if (importer.spritePackingTag.ToLower() == packerTag)
                {
                    //DF.Log(asspath, E_ColorType.UI);
                    if (clearPackerTag)
                    {
                        importer.spritePackingTag = "";
                        //importer.SetTextureSettings(imp);
                        importer.SaveAndReimport();
                    }
                }
            }

        }
        curFile = Directory.GetFiles(parent, "*.jpg");
        for (int i = 0; i < curFile.Length; i++)
        {
            string asspath = curFile[i].Substring(curFile[i].IndexOf("Assets"));
            asspath = asspath.Replace('\\', '/');
            //DF.Log("curFile3.jpg  " + asspath, E_ColorType.Over);
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(asspath);
            if (sp == null)
            {
                //DF.Log("[not sprite] " + asspath, E_ColorType.Err);
                continue;
            }
            //DF.Log("sp  " + sp.name, E_ColorType.UI);
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sp)) as TextureImporter;
            if (branch == 1)
            {
                importer.spritePackingTag = packerTag;
                importer.SaveAndReimport();
                currentIndex++;
            }
            else if (branch == 2)
            {
                if (importer.spritePackingTag.ToLower() == packerTag)
                {
                    //DF.Log(asspath, E_ColorType.UI);
                    if (clearPackerTag)
                    {
                        importer.spritePackingTag = "";
                        importer.SaveAndReimport();
                    }
                }
            }
            UpdateProgress();
        }

    }

}
