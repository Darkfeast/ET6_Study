using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace ET
{

    enum CodeOptimization
    {
        Debug,
        Release,
    }
    
    public static class BuildAssemblieEditor
    {
        private const string CodeDir = "Assets/Bundles/Code/";

        [MenuItem("Tools/Build/EnableAutoBuildCodeDebug _F1")]
        public static void SetAutoBuildCode()
        {
            PlayerPrefs.SetInt("AutoBuild", 1);
            ShowNotification("AutoBuildCode Enabled");
        }
        
        [MenuItem("Tools/Build/DisableAutoBuildCodeDebug _F2")]
        public static void CancelAutoBuildCode()
        {
            PlayerPrefs.DeleteKey("AutoBuild");
            ShowNotification("AutoBuildCode Disabled");
        }

        [MenuItem("Tools/Build/BuildCodeDebug _F5")]
        public static void BuildCodeDebug()
        {
            BuildAssemblieEditor.BuildMuteAssembly("Code", new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
                "Codes/Hotfix/",
                "Codes/HotfixView/"
            }, Array.Empty<string>(), CodeOptimization.Debug);

            AfterCompiling();
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Tools/Build/BuildCodeRelease _F6")]
        public static void BuildCodeRelease()
        {
            BuildAssemblieEditor.BuildMuteAssembly("Code", new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
                "Codes/Hotfix/",
                "Codes/HotfixView/"
            }, Array.Empty<string>(), CodeOptimization.Release);

            AfterCompiling();
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Tools/Build/BuildData _F7")]
        public static void BuildData()
        {
            BuildAssemblieEditor.BuildMuteAssembly("Data", new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
            }, Array.Empty<string>(), CodeOptimization.Debug);
        }
        
        
        [MenuItem("Tools/Build/BuildLogic _F8")]
        public static void BuildLogic()
        {
            string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Logic_*");
            foreach (string file in logicFiles)
            {
                File.Delete(file);
            }
            
            int random = RandomHelper.RandomNumber(100000000, 999999999);
            string logicFile = $"Logic_{random}";
            
            BuildAssemblieEditor.BuildMuteAssembly(logicFile, new []
            {
                "Codes/Hotfix/",
                "Codes/HotfixView/",
            }, new[]{Path.Combine(Define.BuildOutputDir, "Data.dll")}, CodeOptimization.Debug);
        }

        private static void BuildMuteAssembly(string assemblyName, string[] CodeDirectorys, string[] additionalReferences, CodeOptimization codeOptimization)
        {
            DirectoryInfo dt = new DirectoryInfo("."); //dt    D:\Darkfeast\GitDF\ET6_Study\Unity
            Debug.Log($"dt    {dt.FullName}");
            
            List<string> scripts = new List<string>();
            for (int i = 0; i < CodeDirectorys.Length; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(CodeDirectorys[i]);
                
                // Debug.Log($"dir {dti.FullName}");
                
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    scripts.Add(fileInfos[j].FullName);
                }
            }

            if (!Directory.Exists(Define.BuildOutputDir))
                Directory.CreateDirectory(Define.BuildOutputDir);

            string dllPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb");
            File.Delete(dllPath);
            File.Delete(pdbPath);

            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(dllPath, scripts.ToArray());
            
            //??????UnSafe
            //assemblyBuilder.compilerOptions.AllowUnsafeCode = true;

            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            // assemblyBuilder.compilerOptions.CodeOptimization = codeOptimization;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            // assemblyBuilder.compilerOptions.ApiCompatibilityLevel = ApiCompatibilityLevel.NET_4_6;

            assemblyBuilder.additionalReferences = additionalReferences;
            
            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            //AssemblyBuilderFlags.None                 ????????????
            //AssemblyBuilderFlags.DevelopmentBuild     ??????????????????
            //AssemblyBuilderFlags.EditorAssembly       ???????????????
            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;

            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;

            assemblyBuilder.buildTargetGroup = buildTargetGroup;

            assemblyBuilder.buildStarted += delegate(string assemblyPath) { Debug.LogFormat("build start???" + assemblyPath); };

            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                int errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Warnings: {0} - Errors: {1}", warningCount, errorCount);

                if (warningCount > 0)
                {
                    Debug.LogFormat("???{0}???Warning!!!", warningCount);
                }

                if (errorCount > 0)
                {
					if (PlayerPrefs.GetInt("AutoBuild") == 1)//??????????????????????????????Cancel????????????????????????
						CancelAutoBuildCode();
                    for (int i = 0; i < compilerMessages.Length; i++)
                    {
                        if (compilerMessages[i].type == CompilerMessageType.Error)
                        {
                            Debug.LogError(compilerMessages[i].message);
                        }
                    }
                }
            };
            
            //????????????
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("build fail???" + assemblyBuilder.assemblyPath);
                return;
            }
        }

        private static void AfterCompiling()
        {
            while (EditorApplication.isCompiling)
            {
                Debug.Log("Compiling wait1");
                // ?????????sleep????????????????????????
                Thread.Sleep(1000);
                Debug.Log("Compiling wait2");
            }
            
            Debug.Log("Compiling finish");

            Directory.CreateDirectory(CodeDir);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Code.dll"), Path.Combine(CodeDir, "Code.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Code.pdb"), Path.Combine(CodeDir, "Code.pdb.bytes"), true);
            AssetDatabase.Refresh();
            Debug.Log("copy Code.dll to Bundles/Code success!");
            
            // ??????ab???
            AssetImporter assetImporter1 = AssetImporter.GetAtPath("Assets/Bundles/Code/Code.dll.bytes");
            assetImporter1.assetBundleName = "Code.unity3d";
            AssetImporter assetImporter2 = AssetImporter.GetAtPath("Assets/Bundles/Code/Code.pdb.bytes");
            assetImporter2.assetBundleName = "Code.unity3d";
            AssetDatabase.Refresh();
            Debug.Log("set assetbundle success!");
            
            Debug.Log("build success!");
            //??????????????????Game???????????????????????????
            ShowNotification("Build Code Success");
        }

        public static void ShowNotification(string tips)
        {
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }
    }
    
}