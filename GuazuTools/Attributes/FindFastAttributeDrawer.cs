using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FindFastAttribute : PropertyAttribute{
    public System.Type tipoABuscar;
    public FindFastAttribute(System.Type tipo)
    {
        tipoABuscar = tipo;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FindFastAttribute))]
public class FindFastAttributeDrawer : PropertyDrawer
{
    bool primera = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (primera)
        {
            primera = false;
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                FindFastAttribute findFast = attribute as FindFastAttribute;
                System.Type clase = findFast.tipoABuscar;

                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    foreach (Component comp in property.serializedObject.targetObjects)
                    {
                        PrefabType prefabType = PrefabUtility.GetPrefabType(comp.gameObject);
                        if (prefabType != PrefabType.ModelPrefab && prefabType != PrefabType.Prefab)
                        {
                            if (property.objectReferenceValue == null)
                            {
                                property.objectReferenceValue = GameObject.FindObjectOfType(clase);
                                EditorGUIUtility.PingObject(property.objectReferenceValue);
                            }
                        }
                    }
                }
            }
        }
        EditorGUI.PropertyField(position, property);
    }

}
#endif