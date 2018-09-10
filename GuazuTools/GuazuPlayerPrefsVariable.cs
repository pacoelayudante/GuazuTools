using System.Collections;
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
                bCache = PlayerPrefs.GetInt(key,b?1:0)==1;
                cargar = false;
            }
            return bCache;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) bCache = value;
            else PlayerPrefs.SetInt(key, (bCache = value)?1:0);
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
                iCache = PlayerPrefs.GetInt(key, i);
                cargar = false;
            }
            return iCache;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) iCache = value;
            else PlayerPrefs.SetInt(key, iCache = value);
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
                fCache = PlayerPrefs.GetFloat(key, f);
                cargar = false;
            }
            return fCache;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) fCache = value;
            else PlayerPrefs.SetFloat(key, fCache = value);
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
                sCache = PlayerPrefs.GetString(key, s);
                cargar = false;
            }
            return sCache;
        }
        set
        {
            if (string.IsNullOrEmpty(key)) sCache = value;
            else PlayerPrefs.SetString(key, sCache = value);
        }
    }

    public void Reset()
    {
        //PlayerPrefs.DeleteKey(key);
        Bool = b;
        Int = i;
        Float = f;
        String = s;
        //cargar = true;
    }

    public TipoVariable Tipo
    {
        get { return tipo; }
    }

    [SerializeField]
    string key="";
    [SerializeField]
    float f;
    [SerializeField]
    int i;
    [SerializeField]
    bool b;
    [SerializeField]
    string s="";
    [SerializeField]
    TipoVariable tipo;
    bool cargar=true;
    bool bCache;
    int iCache;
    float fCache;
    string sCache;
    
    public string ToString(string formato)
    {
        if (tipo == TipoVariable.Boolean) return Bool.ToString();
        else if (tipo == TipoVariable.Integer) return Int.ToString(formato);
        else if (tipo == TipoVariable.Float) return Float.ToString(formato);
        else if (tipo == TipoVariable.String) return String;
        return "ERROR GIGANTE!";
    }
    public override string ToString()
    {
        return ToString("");
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

       EditorGUI.PropertyField(position,sfValor,label);
       /* esto es para escribr en las player prefs en realite
        if (sfValor.propertyType == SerializedPropertyType.Boolean) GUIBool(position, property, label);
        else if (sfValor.propertyType == SerializedPropertyType.Integer) GUIInt(position, property, label);
        else if (sfValor.propertyType == SerializedPropertyType.Float) GUIFloat(position, property, label);
        else if (sfValor.propertyType == SerializedPropertyType.String) GUIString(position, property, label);*/
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