using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RectMinMaxAttribute : PropertyAttribute
{ }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RectMinMaxAttribute))]
public class RectMinMaxAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Rect)
        {
            EditorGUI.HelpBox(position, "Este atributo solo puede ser usado con Rect", MessageType.Error);
            return;
        }

        Rect r = property.rectValue;
        float[] lbrt = new float[] { r.xMin, r.yMin, r.xMax, r.yMax };

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        EditorGUI.MultiFloatField(position, label, new GUIContent[] { new GUIContent("L"), new GUIContent("B"), new GUIContent("R"), new GUIContent("T") }, lbrt);
        if (EditorGUI.EndChangeCheck())
        {
            property.rectValue = Rect.MinMaxRect(lbrt[0],lbrt[1],lbrt[2],lbrt[3]);
        }
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * (EditorGUIUtility.wideMode?1f:2f);
    }
}
#endif