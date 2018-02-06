using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneAssetPathAsStringAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneAssetPathAsStringAttribute))]
public class SceneAssetPathAsStringAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.HelpBox(position, "Solo usar con strings", MessageType.Error);
            return;
        }

        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

        EditorGUI.BeginChangeCheck();
        scene = EditorGUI.ObjectField(position,label, scene, typeof(SceneAsset), false) as SceneAsset;
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = AssetDatabase.GetAssetPath(scene);
        }
    }
}
#endif
