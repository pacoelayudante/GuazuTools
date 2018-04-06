using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GuazuAyuditasEditor : EditorWindow {

    [MenuItem("Assets/Copiar Path De Asset %w")]
    public static void CopiarPathDeAsset()
    {
        if (Selection.assetGUIDs.Length > 0)
        {
            EditorGUIUtility.systemCopyBuffer = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
        }
    }

}
