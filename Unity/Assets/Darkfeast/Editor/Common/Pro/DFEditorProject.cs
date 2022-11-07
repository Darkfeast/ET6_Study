using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net;

public class DFEditorProject : MonoBehaviour {

	[MenuItem("Assets/ProjectMoveAssets",false,2000)]
	static void ProjectMoveAssets()
	{
		Object[] objs= Selection.objects;

		string pathData = Application.dataPath.Substring(0,Application.dataPath.Length-6);
		// string pathSource = pathData + "/App/Prefabs/";
		// string pathDes = pathData + "Assets/App/Pre/";
		string pathDes = pathData + "Assets/Resources/Prefabs/";
		
		DF.Log("objs "+objs.Length);

		int count = 0;
		foreach (var v in objs)
		{
			string pathObj= AssetDatabase.GetAssetPath(v);
			string[] strs= pathObj.Split('/');
		
			string dirPathDes = pathDes + "room_" + strs[strs.Length - 1].Substring(0, 3)+"/";
			if (!Directory.Exists(dirPathDes))
			{
				Directory.CreateDirectory(dirPathDes);
			}
			if (!File.Exists(dirPathDes + strs[strs.Length - 1]))
			{
				File.Move(pathData+pathObj,dirPathDes+strs[strs.Length-1]);
				count++;
			}
			else
			{
				DF.Log("has exist: "+ dirPathDes+strs[strs.Length-1],E_ColorType.Err);
			}
		}
	
		AssetDatabase.Refresh();
		// DF.Log("objs "+objs.Length);
		DF.Log("ProjectMoveAssets finish..."+count,E_ColorType.Over);
	}
	
	[MenuItem("Assets/ProjectMoveAssetsForce",false,2020)]
	static void ProjectMoveAssetsForce()
	{
		Object[] objs= Selection.objects;

		string pathData = Application.dataPath.Substring(0,Application.dataPath.Length-6);
		// string pathDes = pathData + "Assets/App/Pre/";
		string pathDes = pathData + "Assets/Resources/Prefabs/";

		DF.Log("objs "+objs.Length);
		int count = 0;
		foreach (var v in objs)
		{
			string pathObj= AssetDatabase.GetAssetPath(v);
			string[] strs= pathObj.Split('/');

			string dirPathDes = pathDes + "room_" + strs[strs.Length - 1].Substring(0, 3)+"/";
			if (!Directory.Exists(dirPathDes))
			{
				Directory.CreateDirectory(dirPathDes);
			}
			
			if (!File.Exists(dirPathDes + strs[strs.Length - 1]))
			{
				File.Move(pathData+pathObj,dirPathDes+strs[strs.Length-1]);
				count++;
			}
			else
			{
				DF.Log("exist and replace: "+ dirPathDes+strs[strs.Length-1],E_ColorType.Err);
				File.Delete(dirPathDes+strs[strs.Length-1]);
				File.Move(pathData+pathObj,dirPathDes+strs[strs.Length-1]);
				count++;
			}
		}
		AssetDatabase.Refresh();
		// DF.Log("objs "+objs.Length);
		DF.Log("ProjectMoveAssetsForce finish..."+count,E_ColorType.Over);
	}

	[MenuItem("GameObject/copy2",false,40)]
	static void HierCopy()
	{
		
	}
}
