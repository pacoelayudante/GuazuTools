﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FindFastAttribute : PropertyAttribute
{
    System.Type tipoABuscar = null;
    public FindFastAttribute(System.Type tipo)
    {
        tipoABuscar = tipo;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FindFastAttribute))]
    public class FindFastAttributeDrawer : PropertyDrawer
    {
        readonly GUIContent contErrorPropTipo = new GUIContent("<!>", "Solo funciona con campos de refencia a objetos");
        readonly GUIContent contErrorTipoNull = new GUIContent("<!>", "El tipo pasado al atributo no puede ser null");
        readonly GUIContent[] listaVacia = new GUIContent[] {new GUIContent( "","No hay objetos de este tipo en escena" )};

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var botPos = position;
            botPos.width = EditorGUIUtility.singleLineHeight * 2f;
            position.width -= botPos.width;
            botPos.x += position.width;

            EditorGUI.PropertyField(position, property, label);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var attr = attribute as FindFastAttribute;
                var tipo = attr.tipoABuscar;
                if (tipo == null) GUI.Label(botPos, contErrorTipoNull);
                else
                {
                    var encontrados = Object.FindObjectsOfType(tipo);
                    if (encontrados.Length == 0)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.Popup(botPos, 0, listaVacia);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        string[] opciones = new string[encontrados.Length];
                        int sel = -1;
                        for (int i = 0; i < encontrados.Length; i++)
                        {
                            if (!property.hasMultipleDifferentValues)
                            {
                                if (property.objectReferenceValue == encontrados[i]) sel = i;
                            }
                            opciones[i] = encontrados[i].name;
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                        sel = EditorGUI.Popup(botPos, sel, opciones);
                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck() && sel != -1)
                        {
                            property.objectReferenceValue = encontrados[sel];
                            if (property.objectReferenceValue == null){
                                Debug.LogError("Es posible que el tipo indicado en el atributo no sea igual al tipo del miembro en si");
                            }
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            else
            {
                GUI.Label(botPos, contErrorPropTipo);
            }
        }
    }

}
#endif