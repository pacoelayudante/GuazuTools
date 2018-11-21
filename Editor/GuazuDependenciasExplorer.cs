using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;

public class GuazuDependenciasExplorer : EditorWindow{
    Object ObjetoDeMiInteres
    {
        get { return objetoInteres; }
        set
        {
            if (objetoInteres != value)
            {
                objetoInteres = value;
                if (objetoInteres == null)
                {
                    dependientesVisibles.target = false;
                    detalleDeDependientes = null;
                }
                else {
                    detalleDeDependientes = BuscarDependientes(objetoInteres);
                    if (detalleDeDependientes == null)
                    {
                        dependientesVisibles.target = false;
                        objetoInteres = null;
                    }
                    else
                    {
                        detalleDeDependientes.Sort((Object obj, Object obj2) => { return obj.GetType().Name.CompareTo(obj2.GetType().Name); });
                    }
                }
            }
        }
    }

    public static List<Object> BuscarDependientes(Object objetoInteres)
    {
        var resultado = new List<Object>();
        var pathDeInteres = AssetDatabase.GetAssetPath(objetoInteres);
        if(EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias", "Esto puede tardar un rato...", 0f))
        {
            EditorUtility.ClearProgressBar();
            return null;
        }
        var escenas = EditorBuildSettings.scenes;
        for (int i = 0; i < escenas.Length; i++)
        {
            float progreso = (i+.25f)/escenas.Length;
            if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias (" + resultado.Count + ")", "Cargando " + escenas[i].path, progreso))
            {
                EditorUtility.ClearProgressBar();
                return null;
            }
            var objEscena = AssetDatabase.LoadMainAssetAtPath(escenas[i].path);
            progreso = (i + .5f) / escenas.Length;
            if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias ("+resultado.Count+")", "Colectando dependencias de " + escenas[i].path, progreso))
            {
                EditorUtility.ClearProgressBar();
                return null;
            }
            //var coleccion = EditorUtility.CollectDependencies(new Object[] { objEscena });
            var coleccion = AssetDatabase.GetDependencies(escenas[i].path);
            //if (System.Array.Exists(coleccion, cadaUno => cadaUno.Equals(objetoInteres)))
            if (System.Array.Exists(coleccion, cadaUno => cadaUno.Equals(pathDeInteres)))
            {
                resultado.Add(objEscena);
                //y ahora buscar en los objetos que son onda, prefabsss o animator o animationclip
                float miniProgreso = 0f;
                //foreach(Object prefabEnEscena in coleccion)
                foreach(string pathDePrefabEnEscena in coleccion)
                {
                    var prefabEnEscena = AssetDatabase.LoadMainAssetAtPath(pathDePrefabEnEscena);
                    miniProgreso += .5f / (coleccion.Length* escenas.Length);
                    if ((prefabEnEscena as GameObject) == null && (prefabEnEscena as RuntimeAnimatorController) == null && (prefabEnEscena as AnimationClip) == null) continue;
                    if (resultado.Contains(prefabEnEscena)) continue;
                    if (prefabEnEscena)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias (" + resultado.Count + ")", "Colectando dependencias de "+prefabEnEscena+ "que esta en " + escenas[i].path, progreso+ miniProgreso))
                        {
                            EditorUtility.ClearProgressBar();
                            return null;
                        }
                        //var prefabDepends = EditorUtility.CollectDependencies(new Object[] { prefabEnEscena });
                        //if (System.Array.Exists(prefabDepends, cadaUno => cadaUno.Equals(objetoInteres))) resultado.Add(prefabEnEscena);
                        var prefabDependsPath = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefabEnEscena));
                        if (System.Array.Exists(prefabDependsPath, cadaUno => cadaUno.Equals(pathDeInteres))) resultado.Add(prefabEnEscena);
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
        return resultado;
    }

    Object objetoInteres;
    AnimBool dependientesVisibles;
    List<Object> dependibles = new List<Object>();
    List<Object> detalleDeDependientes = null;
    Vector2 scroll;

    private void OnEnable()
    {
        EditorUtility.ClearProgressBar();
        dependientesVisibles = new AnimBool(Repaint);
    }
    private void OnDisable()
    {
        dependientesVisibles.valueChanged.RemoveListener(Repaint);
    }

    public static void Abrir(List<string> pathsDependibles)
    {
        var dependibles = new List<Object>();
        foreach (var path in pathsDependibles) dependibles.Add(AssetDatabase.LoadMainAssetAtPath(path));
        GetWindow<GuazuDependenciasExplorer>().dependibles = dependibles;
    }

    private void OnGUI()
    {
        scroll=EditorGUILayout.BeginScrollView(scroll);
        var nuevoInteres = EditorGUILayout.ObjectField(ObjetoDeMiInteres, typeof(Object), false);
        EditorGUI.BeginDisabledGroup(detalleDeDependientes==null);
        dependientesVisibles.target = EditorGUILayout.Foldout(dependientesVisibles.target, "Objetos dependientes");
        if (detalleDeDependientes != null)
        {
            if (EditorGUILayout.BeginFadeGroup(dependientesVisibles.faded))
            {
                if (detalleDeDependientes.Count == 0) GUILayout.Label("Nadie depende de este objeto");
                else
                {
                    foreach (var dep in detalleDeDependientes)
                    {
                        EditorGUILayout.ObjectField(dep, typeof(Object), false);
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUI.EndDisabledGroup();
        foreach (var dep in dependibles)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(dep,typeof(Object),false);
            EditorGUI.BeginChangeCheck();
            var val = GUILayout.Toggle(dep == ObjetoDeMiInteres,"Buscar dependientes","Button");
            if (EditorGUI.EndChangeCheck() && val)
            {
                nuevoInteres = dep;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        ObjetoDeMiInteres = nuevoInteres;
    }

    public static List<string> CollectDependenciesSegunBuildSettings()
    {
        EditorUtility.DisplayProgressBar("Recolectando dependencias", "Esto puede tardar un rato...", 1f);
        var escenas = EditorBuildSettings.scenes;
        var escenasPaths = new string[escenas.Length];
        for (int i = 0; i < escenas.Length; i++)
        {
            escenasPaths[i] = escenas[i].path;
        }
        var resultado = AssetDatabase.GetDependencies(escenasPaths);
        EditorUtility.ClearProgressBar();
        return new List<string>(resultado);
    }
    /*public static List<Object> CollectDependenciesSegunBuildSettingsObjetos()
    {
        EditorUtility.DisplayProgressBar("Recolectando dependencias", "Esto puede tardar un rato...", 1f);
        var escenas = EditorBuildSettings.scenes;
        var escenasPaths = new Object[escenas.Length];
        for (int i = 0; i < escenas.Length; i++)
        {
            escenasPaths[i] = AssetDatabase.LoadMainAssetAtPath( escenas[i].path );
        }
        var resultado = EditorUtility.CollectDependencies(escenasPaths);
        EditorUtility.ClearProgressBar();
        return new List<Object>(resultado);
    }*/

    [MenuItem("Assets/Guazu/Chequear si es seguro borrar")]
    public static void DisplayProyectoDependeDe()
    {
        string seDepende = "";
        var dependenciasDeBuild = CollectDependenciesSegunBuildSettings();
        foreach (var guid in Selection.assetGUIDs)
        {
            if (string.IsNullOrEmpty(guid)) continue;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                if (dependenciasDeBuild.Exists(cadaUno => cadaUno.StartsWith(path)))
                {
                    seDepende += path + "\n";
                }
            }
            else
            {
                if (dependenciasDeBuild.Contains(path)) seDepende += path + "\n";
            }
        }
        if (!string.IsNullOrEmpty(seDepende))
        {
            List<string> pathsDependibles = new List<string>();
            if (EditorUtility.DisplayDialog("Se depende de:", seDepende, "Detallame", "Ya fué"))
            {
                foreach (var guid in Selection.assetGUIDs)
                {
                    if (string.IsNullOrEmpty(guid)) continue;
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        if (dependenciasDeBuild.Exists(cadaUno => cadaUno.StartsWith(path)))
                        {
                            dependenciasDeBuild.ForEach(
                                cadaUno =>
                                { if (cadaUno.StartsWith(path)) pathsDependibles.Add(cadaUno); }
                            );
                        }
                    }
                    else
                    {
                        if (dependenciasDeBuild.Contains(path)) pathsDependibles.Add(path);
                    }
                }
                if (pathsDependibles.Count > 0) Abrir(pathsDependibles);
                else Debug.LogError("! (objsDependibles.Count > 0) ");
            }
        }
        else EditorUtility.DisplayDialog("Es borrable", "De esta seleccion no se depende pa la build, se puede borrar seguramente", "Piola");
    }
    [MenuItem("Assets/Guazu/Chequear si es seguro borrar", true)]
    public static bool DisplayProyectoDependeDeValidator()
    {
        if (!Selection.activeObject) return false;
        foreach (var s in Selection.assetGUIDs)
        {
            if (!string.IsNullOrEmpty(s)) return true;
        }
        return false;
    }
}
