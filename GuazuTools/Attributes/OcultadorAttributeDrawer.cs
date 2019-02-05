using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class OcultadorAttribute : PropertyAttribute
{
    string nombreData;
    int intMin, intMax;
    float floatMin, floatMax;
    bool xorInverter;
#if UNITY_EDITOR
    public SerializedProperty GetProperty(SerializedProperty propHermana)
    {
        return propHermana.FindPropertyRelative(nombreData);
    }
    public SerializedProperty GetProperty(SerializedObject objContenedor)
    {
        return objContenedor.FindProperty(nombreData);
    }
    public bool Comp(SerializedProperty prop)
    {
        if(prop==null)return true;//El default es true porqu si  todo  falla queremos que la  cosa  sea visible
        if (prop.propertyType == SerializedPropertyType.Boolean) return prop.boolValue ^ xorInverter;
        else if (prop.propertyType == SerializedPropertyType.Integer) return Comp(prop.intValue) ^ xorInverter;
        else if (prop.propertyType == SerializedPropertyType.Float) return Comp(prop.floatValue) ^ xorInverter;
        else if (prop.propertyType == SerializedPropertyType.ObjectReference) return (prop.objectReferenceValue!=null) ^ xorInverter;
        return true;//El default es true porqu si  todo  falla queremos que la  cosa  sea visible
    }
    bool Comp(int valor)
    {
        if (valor >= intMin) return valor <= intMax;
        else return false;
    }
    bool Comp(float valor)
    {
        if (valor >= floatMin) return valor <= floatMax;
        else return false;
    }
#endif
    public OcultadorAttribute(string nombreData, bool negarValorBool = false)
    {
        this.nombreData = nombreData;
        this.xorInverter = negarValorBool;
    }
    public OcultadorAttribute(string nombreData, int valInt, bool negarValorBool = false)
    {
        this.nombreData = nombreData;
        this.intMin = this.intMax = valInt;
        this.xorInverter = negarValorBool;
    }
    public OcultadorAttribute(string nombreData, float valFloat, bool negarValorBool = false)
    {
        this.nombreData = nombreData;
        this.floatMin = this.floatMax = valFloat;
        this.xorInverter = negarValorBool;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OcultadorAttribute))]
public class OcultadorAttributeDrawer : PropertyDrawer
{
    float Visibilidad
    {
        get
        {
            if (visibilidad == null) return 1f;
            else return visibilidad.target?1f:0f;
        }

    }
    OcultadorAttribute ocultador;
    SerializedProperty propControl;
    UnityEditor.AnimatedValues.AnimBool visibilidad;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ocultador == null)
        {
            ocultador = attribute as OcultadorAttribute;
            propControl=ocultador.GetProperty(property.serializedObject);
            visibilidad=new UnityEditor.AnimatedValues.AnimBool(ocultador.Comp(propControl));
            
            // visibilidad.valueChanged += 
        }
        else visibilidad.target=ocultador.Comp(propControl);
        
        if(visibilidad.target)EditorGUI.PropertyField(position, property, label);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property) * Visibilidad;
    }
}
#endif