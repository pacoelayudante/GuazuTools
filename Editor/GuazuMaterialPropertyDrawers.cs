using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MaskFlagsDrawer : MaterialPropertyDrawer
{
    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        EditorGUIUtility.labelWidth = position.width / 3f;
        if (prop.type != MaterialProperty.PropType.Float)
        {
            editor.DefaultShaderProperty(prop, label.text);
            return;
        }
        label.text += " = " + prop.floatValue.ToString();
        int mascara = Mathf.FloorToInt(prop.floatValue) & (255);
        var rect = EditorGUI.PrefixLabel(position, label);
        rect.x += rect.width;
        rect.width = rect.height;
        for (int i = 0; i < 8; i++)
        {
            rect.x -= rect.width;
            EditorGUI.BeginChangeCheck();
            var on = EditorGUI.Toggle(rect, ((1 << i) & mascara) > 0);
            if (EditorGUI.EndChangeCheck())
            {
                if (on) mascara |= 1 << i;
                else mascara &= ~(1 << i);
                prop.floatValue = mascara;
            }
        }
    }
}

public class ToggleZeroMaterialDrawer : MaterialPropertyDrawer
{
    protected readonly string keyword;
    public ToggleZeroMaterialDrawer()
    {
    }
    public ToggleZeroMaterialDrawer(string keyword)
    {
        this.keyword = keyword;
    }
    protected void SetKeyword(MaterialProperty prop, bool on, string defaultKeywordSuffix = "_ON")
    {
        string kw = string.IsNullOrEmpty(keyword) ? prop.name.ToUpperInvariant() + defaultKeywordSuffix : keyword;
        // set or clear the keyword
        foreach (Material material in prop.targets)
        {
            if (on)
                material.EnableKeyword(kw);
            else
                material.DisableKeyword(kw);
        }
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        bool estado = false;
        if (prop.type == MaterialProperty.PropType.Color) estado = prop.colorValue.a > 0f;
        else if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range) estado = prop.floatValue != 0f;
        else if (prop.type == MaterialProperty.PropType.Texture) estado = prop.textureValue != null;
        else if (prop.type == MaterialProperty.PropType.Vector) estado = prop.vectorValue != Vector4.zero;
        EditorGUI.BeginChangeCheck();
        label.text += string.Format(" = {0}", estado);
        editor.DefaultShaderProperty(position, prop, label.text);
        if (EditorGUI.EndChangeCheck())
        {
            if (prop.type == MaterialProperty.PropType.Color) estado = prop.colorValue.a > 0f;
            else if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range) estado = prop.floatValue != 0f;
            else if (prop.type == MaterialProperty.PropType.Texture) estado = prop.textureValue != null;
            else if (prop.type == MaterialProperty.PropType.Vector) estado = prop.vectorValue != Vector4.zero;
            SetKeyword(prop, estado);
        }
    }
}
