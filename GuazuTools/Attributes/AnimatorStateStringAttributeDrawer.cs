using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using an = UnityEditor.Animations;
#endif

public class AnimatorStateStringAttribute : PropertyAttribute
{
    public bool buscarEnChildren = false;
    public AnimatorStateStringAttribute() { }
    public AnimatorStateStringAttribute(bool buscarEnChildren) { this.buscarEnChildren = buscarEnChildren; }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AnimatorStateStringAttribute))]
public class AnimatorStateStringAttributeDrawer : PropertyDrawer
{
    readonly static GUIContent[] noMulti = new GUIContent[] { new GUIContent("Multiobject editing no permitido") };
    readonly static GUIContent[] noAnimator = new GUIContent[] { new GUIContent("Animator no hallado") };
    readonly static GUIContent[] noAnimCont = new GUIContent[] { new GUIContent("Animator sin Animator Controller") };
    readonly static GUIContent[] noBehav = new GUIContent[] { new GUIContent("No reconocido como behaviour") };
    readonly static GUIContent[] noGameObj = new GUIContent[] { new GUIContent("No asociado a GameObject") };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            position.width -= EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property);
            position.x += position.width;
            position.width = EditorGUIUtility.singleLineHeight;
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.Popup(position, -1, noMulti);
            }
            else if(property.serializedObject.targetObject)
            {
                Behaviour beh = property.serializedObject.targetObject as Behaviour;
                if (!beh)
                {
                    EditorGUI.Popup(position, -1, noBehav);
                }
                else if (beh.gameObject)
                {
                    Animator animator = beh.GetComponent<Animator>();
                    if (!animator)
                    {
                        AnimatorStateStringAttribute aVarAtt = attribute as AnimatorStateStringAttribute;
                        if (aVarAtt.buscarEnChildren) animator = beh.GetComponentInChildren<Animator>();
                    }
                    if (animator)
                    {
                        string[] anims = AnimatorListaDeEstados.ListaDeEstados(animator);
                        if (anims == null) EditorGUI.Popup(position, -1, noAnimCont);
                        else
                        {
                            int sel = 0;
                            for (sel = 0; sel < anims.Length; sel++) { if (anims[sel].Equals(property.stringValue)) break; }
                            EditorGUI.BeginChangeCheck();
                            sel = EditorGUI.Popup(position, sel, anims);
                            if (EditorGUI.EndChangeCheck())
                            {
                                property.stringValue = anims[sel];
                                property.serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                    else EditorGUI.Popup(position, -1, noAnimator);
                }
                else EditorGUI.Popup(position, -1, noGameObj);
            }
        }
        else
        {
            EditorGUI.LabelField(position, label, "AnimatorStateString solo con strings");
        }
    }
}

public static class AnimatorListaDeEstados
{
    public static string[] ListaDeEstados(Animator anim)
    {
        if (anim)
        {
            if (anim.runtimeAnimatorController)
            {
                List<string> resultados = new List<string>();
                an.AnimatorController cont = anim.runtimeAnimatorController as an.AnimatorController;
                foreach (an.AnimatorControllerLayer lay in cont.layers)
                {
                    AgregarEstadosRecursivo(resultados, lay.stateMachine);
                }
                return resultados.ToArray();
            }
            else return null;
        }
        else return null;
    }

    public static void AgregarEstadosRecursivo(List<string> lista, an.AnimatorStateMachine stMach)
    {
        foreach (an.ChildAnimatorState stat in stMach.states)
        {
            lista.Add(stMach.name + "." + stat.state.name);
        }
        foreach (an.ChildAnimatorStateMachine stat in stMach.stateMachines)
        {
            AgregarEstadosRecursivo(lista, stat.stateMachine);
        }
    }
}
#endif