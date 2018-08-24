using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using an = UnityEditor.Animations;
#endif

public class AnimatorVariableAttribute : PropertyAttribute
{
    public bool buscarEnChildren = false;
    public string buscarCampoEspecifico = "";
    public AnimatorVariableAttribute() { }
    public AnimatorVariableAttribute(bool buscarEnChildren) { this.buscarEnChildren = buscarEnChildren; }
    public AnimatorVariableAttribute(string buscarCampoEspecifico) { this.buscarCampoEspecifico = buscarCampoEspecifico; }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AnimatorVariableAttribute))]
public class AnimatorVariableAttributeDrawer : PropertyDrawer
{
    readonly static GUIContent[] noMulti = new GUIContent[] { new GUIContent("Multiobject editing no permitido") };
    readonly static GUIContent[] noAnimator = new GUIContent[] { new GUIContent("Animator no hallado o hallado pero sin Animator Controller") };
    //readonly static GUIContent[] noAnimCont = new GUIContent[] { new GUIContent("Animator sin Animator Controller") };
    readonly static GUIContent[] noField = new GUIContent[] { new GUIContent("El campo que se quiere referenciar no se encuentra en este componente") };
    //readonly static GUIContent[] noFieldGameObj = new GUIContent[] { new GUIContent("El campo que se quiere referenciar debe ser un vinculo a un objeto") };
    readonly static GUIContent[] noBehav = new GUIContent[] { new GUIContent("No reconocido como behaviour") };
    readonly static GUIContent[] noGameObj = new GUIContent[] { new GUIContent("No asociado a Objeto") };

    static GUIContent[] errorNoAnimator;
    static an.AnimatorController RecuperarAnimator(SerializedProperty property, AnimatorVariableAttribute att, Behaviour beh)
    {
        errorNoAnimator = null;
        if (string.IsNullOrEmpty(att.buscarCampoEspecifico))
        {
            Animator animator = beh.GetComponent<Animator>();
            if (!animator)
            {
                if (att.buscarEnChildren) animator = beh.GetComponentInChildren<Animator>();
            }
            if (animator) return animator.runtimeAnimatorController as an.AnimatorController;
            return null;
        }
        else
        {
            SerializedProperty prop = property.serializedObject.FindProperty(att.buscarCampoEspecifico);
            if (prop == null)
            {
                errorNoAnimator = noField;
                return null;
            }
            else if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object elOtro = prop.objectReferenceValue;
                if (!elOtro) return null;
                an.AnimatorController esController = elOtro as an.AnimatorController;
                if (esController) return esController;
                Animator esAnimator = elOtro as Animator;
                if (!esAnimator)
                {
                    GameObject esGameObject = elOtro as GameObject;
                    if (esGameObject) esAnimator = esGameObject.GetComponent<Animator>();
                    else
                    {
                        Component esComponent = elOtro as Component;
                        if (esComponent) esAnimator = esComponent.GetComponent<Animator>();
                    }
                }
                if (esAnimator) return esAnimator.runtimeAnimatorController as an.AnimatorController;
                else return null;
            }
            else
            {
                errorNoAnimator = noGameObj;
                return null;
            }
        }
    }

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
            else if (property.serializedObject.targetObject)
            {
                Behaviour beh = property.serializedObject.targetObject as Behaviour;
                if (!beh)
                {
                    EditorGUI.Popup(position, -1, noBehav);
                }
                else if (beh.gameObject)
                {
                    an.AnimatorController animatorController = RecuperarAnimator(property, attribute as AnimatorVariableAttribute, beh);

                    if (animatorController)
                    {
                        string[] anims = AnimatorListaDeVariables.ListaDeVariables(animatorController);
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
                    else EditorGUI.Popup(position, -1, errorNoAnimator == null ? noAnimator : errorNoAnimator);
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

public static class AnimatorListaDeVariables
{
    public static string[] ListaDeVariables(Animator anim)
    {
        if (anim)
        {
            if (anim.runtimeAnimatorController)
            {
                return ListaDeVariables(anim.runtimeAnimatorController as an.AnimatorController);
            }
            else return null;
        }
        else return null;
    }

    public static string[] ListaDeVariables(an.AnimatorController cont)
    {
        if (cont)
        {
            string[] salida = new string[cont.parameters.Length];
            for (int i = 0; i < salida.Length; i++)
            {
                salida[i] = cont.parameters[i].name;
            }
            return salida;
        }
        else return null;
    }
}
#endif