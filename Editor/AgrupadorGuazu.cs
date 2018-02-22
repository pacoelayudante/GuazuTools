using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AgrupadorGuazu : EditorWindow {

    [MenuItem("Guazu/Agrupador &g")]
    public static void AgruparSeleccion()
    {
        if (Selection.transforms.Length > 0)
        {
            Init(Selection.transforms);
        }
    }

    static void Init(Transform[] objetos)
    {
        Transform padre = objetos[0].parent;
        foreach(Transform t in objetos)
        {
            if (padre != t.parent)
            {
                EditorUtility.DisplayDialog("Opa!", "Onda que todos tienen que tener el mismo padre (o ninguno)", "Ah, claro");
                return;
            }
        }

        AgrupadorGuazu popupNombre = ScriptableObject.CreateInstance<AgrupadorGuazu>();
        popupNombre.objetosAgrupados = Selection.transforms;
        popupNombre.padre = Selection.transforms[0].parent;
        popupNombre.position = new Rect(Screen.width / 2, Screen.height / 2, 250, EditorGUIUtility.singleLineHeight*5);
        popupNombre.ShowPopup();
        popupNombre.Focus();
    }

    string nombreNuevo = "Grupo";
    public Transform[] objetosAgrupados = new Transform[0];
    public Transform padre;
    bool focusear = true;
    void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            Realizar();
        }
        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            this.Close();
        }
        EditorGUILayout.LabelField("Agrupando "+ objetosAgrupados .Length+ " objetos", EditorStyles.wordWrappedLabel);
        GUI.SetNextControlName("nombre");
        nombreNuevo = EditorGUILayout.TextField("Nombre", nombreNuevo);
        if (GUILayout.Button("Agrupalo")) Realizar();
        if (focusear)
        {
            focusear = false;
            EditorGUI.FocusTextInControl("nombre");
        }
    }

    private void OnLostFocus()
    {
        PreguntarQueOnda();
    }

    void PreguntarQueOnda()
    {
        if (EditorUtility.DisplayDialog("¿Agrupamos?","Que onda amigue, armo el grupo o cancelo?","Agrupalo","Nah, ya fue"))
        {
            Realizar();
        }
        else
        {
            this.Close();
        }
    }

    void Realizar()
    {
        GameObject nuevo = new GameObject(nombreNuevo);
        if (padre) nuevo.transform.SetParent(padre,false);
        Undo.RecordObjects(objetosAgrupados, "Agrupar en " + nombreNuevo);
        foreach (Transform t in objetosAgrupados)
        {
            t.SetParent(nuevo.transform);
        }
        Undo.RegisterCreatedObjectUndo(nuevo, "Agrupar en " + nombreNuevo);
        EditorGUIUtility.PingObject(nuevo);
        this.Close();
    }
}
