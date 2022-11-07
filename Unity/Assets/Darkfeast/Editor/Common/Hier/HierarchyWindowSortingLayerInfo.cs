using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class HierarchyWindowSortingLayerInfo
{
    static readonly GUIStyleState _state = new GUIStyleState()
    {
        textColor = DFHelper.ToolColor.GetColor("#84BF34FF")  //green   //ver black
        // textColor = DFColor.GetColor("#008B8B")    //        //ver gray
        // textColor = DFColor.GetColor("#00c0ff")       //blue    
    };

    static readonly GUIStyle _style = new GUIStyle()
    {
        fontSize = 10, alignment = TextAnchor.MiddleRight, normal = _state
    };

    static readonly GUIStyleState _state2 = new GUIStyleState()
    {
        textColor = DFHelper.ToolColor.GetColor("#58C8FFFF")
    };

    static readonly GUIStyle _style2 = new GUIStyle()
    {
        fontSize = 10, alignment = TextAnchor.MiddleRight, normal = _state2
    };

    static HierarchyWindowSortingLayerInfo()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierachyWindowItemOnGUI;

        //EditorApplication.update += Update;
        //App.MgrTool.YieldMethod(2, () => {
        //    EditorApplication.delayCall += Delay;
        //});
    }

    static void Delay()
    {
        EditorApplication.update += Update;
    }


    [RuntimeInitializeOnLoadMethod]
    static void HHH()
    {
        // DF.Log("hhhhhhh",E_ColorType.Temp);
    }

    [RuntimeInitializeOnLoadMethod]
    static void Update()
    {
       
        foreach (var kv in dict)
        {
            DF.Log("GGGG");
            if (kv.Value == null)
                continue;
            HandleHierachyWindowItemOnGUI(kv.Key, kv.Value);
        }
    }


    static Dictionary<int, Rect> dict = new Dictionary<int, Rect>();


    static void HandleHierachyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        //DF.Log(instanceID + "     " + selectionRect);

        var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go != null)
        {
            if (!dict.ContainsKey(instanceID))
            {
                dict.Add(instanceID, selectionRect);
            }


            var renderer = go.GetComponent<Renderer>();

            StringBuilder sb = new StringBuilder();

            if (renderer)
            {
                sb.Append(renderer.sortingLayerName);
                sb.Append(": ");
                sb.Append(renderer.sortingOrder.ToString() + " ");

                EditorGUI.LabelField(selectionRect, sb.ToString(), _style);
            }
            else
            {
                SortingGroup sg = go.GetComponent<SortingGroup>();
                if (sg)
                {
                    sb.Append("sg_" + sg.sortingLayerName);
                    sb.Append(": ");
                    sb.Append(sg.sortingOrder.ToString() + " ");
                    EditorGUI.LabelField(selectionRect, sb.ToString(), _style2);
                }
            }
        }
    }
}