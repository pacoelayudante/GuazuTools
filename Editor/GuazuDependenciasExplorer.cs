using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;

public class GuazuDependenciasExplorer : EditorWindow
{
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
                else
                {
                    detalleDeDependientes = BuscarDependientes(objetoInteres,busquedaRecursiva);
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

    public static List<Object> BuscarDependientes(Object objetoInteres, bool busquedaRecursiva)
    {
        var resultado = new List<Object>();
        var pathDeInteres = AssetDatabase.GetAssetPath(objetoInteres);
        if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias", "Esto puede tardar un rato...", 0f))
        {
            EditorUtility.ClearProgressBar();
            return null;
        }
        var escenas = EditorBuildSettings.scenes;
        for (int i = 0; i < escenas.Length; i++)
        {
            float progreso = (i + .25f) / escenas.Length;
            if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias (" + resultado.Count + ")", "Cargando " + escenas[i].path, progreso))
            {
                EditorUtility.ClearProgressBar();
                return null;
            }
            var objEscena = AssetDatabase.LoadMainAssetAtPath(escenas[i].path);
            progreso = (i + .5f) / escenas.Length;
            if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias (" + resultado.Count + ")", "Colectando dependencias de " + escenas[i].path, progreso))
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
                foreach (string pathDePrefabEnEscena in coleccion)
                {
                    var prefabEnEscena = AssetDatabase.LoadMainAssetAtPath(pathDePrefabEnEscena);
                    miniProgreso += .5f / (coleccion.Length * escenas.Length);
                    if ((prefabEnEscena as GameObject) == null && (prefabEnEscena as RuntimeAnimatorController) == null && (prefabEnEscena as AnimationClip) == null) continue;
                    if (resultado.Contains(prefabEnEscena)) continue;
                    if (prefabEnEscena)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Recolectando dependencias (" + resultado.Count + ")", "Colectando dependencias de " + prefabEnEscena + "que esta en " + escenas[i].path, progreso + miniProgreso))
                        {
                            EditorUtility.ClearProgressBar();
                            return null;
                        }
                        //var prefabDepends = EditorUtility.CollectDependencies(new Object[] { prefabEnEscena });
                        //if (System.Array.Exists(prefabDepends, cadaUno => cadaUno.Equals(objetoInteres))) resultado.Add(prefabEnEscena);
                        var prefabDependsPath = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefabEnEscena),busquedaRecursiva);
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
    List<Object> Dependibles
    {
        get
        {
            return dependibles;
        }
        set
        {
            if (value != null)
            {
                borrables = null;
                dependibles = value;
            }
        }
    }
    List<Object> Borrables
    {
        get
        {
            return borrables;
        }
        set
        {
            if (value != null)
            {
                dependibles = null;
                borrables = value;
            }
        }
    }
    List<Object> dependibles = new List<Object>();
    List<Object> borrables = null;
    List<Object> detalleDeDependientes = null;
    Vector2 scroll;
    bool busquedaRecursiva = true;

    private void OnEnable()
    {
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
        GetWindow<GuazuDependenciasExplorer>().Dependibles = dependibles;
    }
    public static void MostrarBorrables(List<string> pathsBorrables)
    {
        var borrables = new List<Object>();
        foreach (var path in pathsBorrables) borrables.Add(AssetDatabase.LoadMainAssetAtPath(path));
        GetWindow<GuazuDependenciasExplorer>().Borrables = borrables;
    }

    private void OnGUI()
    {
        if (Dependibles != null)
        {
            OnGUIDependibles();
        }
        if (Borrables != null)
        {
            OnGUIBorrables();
        }
    }

    void OnGUIBorrables()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (GUILayout.Button("Seleccionar Todos"))
        {
            Selection.objects = Borrables.ToArray();
        }
        if (GUILayout.Button("Borrar Todos"))
        {
            if (EditorUtility.DisplayDialog("Borrar", "Borrar " + Borrables.Count + " elementos", "Kaboom!", "Nope!"))
            {
                var ftot = 0f;
                var fsum = 1f / Borrables.Count;
                foreach (var dep in Borrables)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Borrando", AssetDatabase.GetAssetPath(dep), ftot))
                    {
                        break;
                    }
                    // FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(dep));
                    ftot += fsum;
                }
                EditorUtility.ClearProgressBar();
            }
        }
        foreach (var dep in Borrables)
        {
            EditorGUILayout.ObjectField(dep, typeof(Object), false);
        }
        EditorGUILayout.EndScrollView();
    }
    void OnGUIDependibles()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        busquedaRecursiva = EditorGUILayout.Toggle("busquedaRecursiva",busquedaRecursiva);
        var nuevoInteres = EditorGUILayout.ObjectField(ObjetoDeMiInteres, typeof(Object), false);
        EditorGUI.BeginDisabledGroup(detalleDeDependientes == null);
        dependientesVisibles.target = EditorGUILayout.Foldout(dependientesVisibles.target, "Objetos dependientes");
        if (detalleDeDependientes != null)
        {
            if (EditorGUILayout.BeginFadeGroup(dependientesVisibles.faded))
            {
                if (detalleDeDependientes.Count == 0) GUILayout.Label("Nadie depende de este objeto");
                else
                {
                    if(nuevoInteres && GUILayout.Button("Exportar Lista")){
                        var url = EditorUtility.SaveFilePanelInProject("exportar","dependientes de "+nuevoInteres.name,"asset","guardar como");
                        if(!string.IsNullOrEmpty(url)){
                            var lista = ScriptableObject.CreateInstance<SimpleListaDeReferencias>();
                            lista.referencias = detalleDeDependientes;
                            AssetDatabase.CreateAsset(lista,url);
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                    foreach (var dep in detalleDeDependientes)
                    {
                        EditorGUILayout.ObjectField(dep, typeof(Object), false);
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUI.EndDisabledGroup();
        foreach (var dep in Dependibles)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(dep, typeof(Object), false);
            EditorGUI.BeginChangeCheck();
            var val = GUILayout.Toggle(dep == ObjetoDeMiInteres, "Buscar dependientes", "Button");
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

    public static List<string> ExpandirSeleccionAsPaths(string[] inputGUIDS)
    {
        var outPaths = new List<string>();
        return ExpandirSeleccionAsPaths(inputGUIDS, outPaths);
    }
    public static List<string> ExpandirSeleccionAsPaths(string[] inputGUIDS, List<string> outPaths)
    {
        foreach (var guid in inputGUIDS)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                var encontrados = AssetDatabase.FindAssets("", new string[] { path });
                ExpandirSeleccionAsPaths(encontrados, outPaths);
            }
            else if (!outPaths.Contains(path)) outPaths.Add(path);
        }
        return outPaths;
    }

    [MenuItem("Assets/Guazu/Seleccionar Borrables")]
    public static void DisplayBorrables()
    {
        var dependenciasDeBuild = CollectDependenciesSegunBuildSettings();
        var seleccionExpandida = ExpandirSeleccionAsPaths(Selection.assetGUIDs);
        var pathsNoDependibles = new List<string>();
        foreach (var path in seleccionExpandida)
        {
            if (string.IsNullOrEmpty(path)) continue;
            if (AssetDatabase.IsValidFolder(path)) continue;
            if (!dependenciasDeBuild.Contains(path)) pathsNoDependibles.Add(path);
        }
        if (pathsNoDependibles.Count > 0) MostrarBorrables(pathsNoDependibles);
    }

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
    [MenuItem("Assets/Guazu/Seleccionar Borrables", true)]
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
