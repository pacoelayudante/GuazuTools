﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class GuazuPlayerPrefsVariable
{
    public enum TipoVariable
    {
        Boolean=0, Integer=1, Float=2, String=3
    }

    public static implicit operator bool(GuazuPlayerPrefsVariable entra)
    {
        return entra.Bool;
    }
    public static implicit operator int(GuazuPlayerPrefsVariable entra)
    {
        return entra.Int;
    }
    public static implicit operator float(GuazuPlayerPrefsVariable entra)
    {
        return entra.Float;
    }
    public static implicit operator string(GuazuPlayerPrefsVariable entra)
    {
        return entra.String;
    }

    public string Key
    { get { return key; } }
    public bool Bool
    {
        get
        {
#if UNITY_EDITOR
            if (tipo != TipoVariable.Boolean) Debug.LogError("Boolean no boolean");
#endif
            if (cargar)
            {
                b = PlayerPrefs.GetInt(key,b?1:0)==1;
                cargar = false;
            }
            return b;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) b = value;
            else PlayerPrefs.SetInt(key, (b = value)?1:0);
        }
    }
    public int Int
    {
        get
        {
#if UNITY_EDITOR
            if (tipo != TipoVariable.Integer) Debug.LogError("Integer no integer");
#endif
            if (cargar)
            {
                i = PlayerPrefs.GetInt(key, i);
                cargar = false;
            }
            return i;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) i = value;
            else PlayerPrefs.SetInt(key, i = value);
        }
    }
    public float Float
    {
        get
        {
#if UNITY_EDITOR
            if (tipo != TipoVariable.Float) Debug.LogError("Float no float");
#endif
            if (cargar)
            {
                f = PlayerPrefs.GetFloat(key, f);
                cargar = false;
            }
            return f;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) f = value;
            else PlayerPrefs.SetFloat(key, f = value);
        }
    }
    public string String
    {
        get
        {
#if UNITY_EDITOR
            if (tipo != TipoVariable.String) Debug.LogError("String no string");
#endif
            if (cargar)
            {
                s = PlayerPrefs.GetString(key, s);
                cargar = false;
            }
            return s;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) s = value;
            else PlayerPrefs.SetString(key, s = value);
        }
    }

    public void Reset()
    {
        PlayerPrefs.DeleteKey(key);
    }

    public TipoVariable Tipo
    {
        get { return tipo; }
    }

    [SerializeField]
    string key;
    [SerializeField]
    float f;
    [SerializeField]
    int i;
    [SerializeField]
    bool b;
    [SerializeField]
    string s;
    [SerializeField]
    TipoVariable tipo;
    bool cargar;

    public override string ToString()
    {
        if (tipo == TipoVariable.Boolean) return b.ToString();
        else if (tipo == TipoVariable.Integer) return i.ToString();
        else if (tipo == TipoVariable.Float) return f.ToString();
        else if (tipo == TipoVariable.String) return s.ToString();
        return "ERROR GIGANTE!";
    }

    public GuazuPlayerPrefsVariable(bool b)
    {
        this.b = b;
        cargar = true;
        tipo = TipoVariable.Boolean;
    }
    public GuazuPlayerPrefsVariable(int i)
    {
        this.i = i;
        cargar = true;
        tipo = TipoVariable.Integer;
    }
    public GuazuPlayerPrefsVariable(float f)
    {
        this.f = f;
        cargar = true;
        tipo = TipoVariable.Float;
    }
    public GuazuPlayerPrefsVariable(string s)
    {
        this.s = s;
        cargar = true;
        tipo = TipoVariable.String;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GuazuPlayerPrefsVariable))]
public class GuazuPlayerPrefsVariableDrawer : PropertyDrawer
{
    SerializedProperty sfKey,sfValor;
    string key;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (sfKey == null)
        {
            sfKey = property.FindPropertyRelative("key");
            if (string.IsNullOrEmpty(sfKey.stringValue))
            {
                sfKey.stringValue = property.serializedObject.FindProperty("m_Script").objectReferenceValue.name + "." + property.name;
            }
            key = sfKey.stringValue;

            sfValor = property.FindPropertyRelative("tipo");
            if (sfValor.enumValueIndex == 0) sfValor = property.FindPropertyRelative("b");
            else if (sfValor.enumValueIndex == 1) sfValor = property.FindPropertyRelative("i");
            else if (sfValor.enumValueIndex == 2) sfValor = property.FindPropertyRelative("f");
            else if (sfValor.enumValueIndex == 3) sfValor = property.FindPropertyRelative("s");
        }
        
        if(sfValor.propertyType == SerializedPropertyType.Boolean) GUIBool(position, property, label);
        else if (sfValor.propertyType == SerializedPropertyType.Integer) GUIInt(position, property, label);
        else if (sfValor.propertyType == SerializedPropertyType.Float) GUIFloat(position, property, label);
        else if (sfValor.propertyType == SerializedPropertyType.String) GUIString(position, property, label);
    }

    void GUIBool(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        sfValor.boolValue = EditorGUI.Toggle(position, label, PlayerPrefs.GetInt(key, sfValor.boolValue?1:0)==1);
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt(key, sfValor.boolValue? 1:0);
        }
    }
    void GUIInt(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        sfValor.intValue = EditorGUI.IntField(position, label, PlayerPrefs.GetInt(key, sfValor.intValue));
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt(key, sfValor.intValue);
        }
    }
    void GUIFloat(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        sfValor.floatValue = EditorGUI.FloatField(position, label, PlayerPrefs.GetFloat(key, sfValor.floatValue));
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetFloat(key, sfValor.floatValue);
        }
    }
    void GUIString(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        sfValor.stringValue = EditorGUI.TextField(position, label, PlayerPrefs.GetString(key, sfValor.stringValue));
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetString(key, sfValor.stringValue);
        }
    }
}
#endif