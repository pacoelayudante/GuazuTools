using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class ClonadorDeAnimators : EditorWindow {

    RuntimeAnimatorController Modelo
    {
        get { return modelo; }
        set { modelo = value;
            modeloComoOverride = value as AnimatorOverrideController;
            modeloComoController = value as AnimatorController;
        }
    }
    AnimatorOverrideController modeloComoOverride;
    AnimatorController modeloComoController;
    RuntimeAnimatorController modelo;
    Object carpeta;
    string carpetaNuevosSprites = "", nombreNuevo="";
    string[] posiblesSpritesNuevos=new string[0];
    GUIContent contentBotonCarpetaSprites = new GUIContent("Carpeta de Sprites Nuevos");
    int idPickerCarpetaSprites;

    [MenuItem("Guazu/Clonador De Animators")]
    public static void GetVentana()
    {
        GetWindow<ClonadorDeAnimators>();
    }

    private void OnGUI()
    {
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            if (EditorGUIUtility.GetObjectPickerControlID() == idPickerCarpetaSprites)
            {
                carpeta = EditorGUIUtility.GetObjectPickerObject();
                if (carpeta)
                {
                    carpetaNuevosSprites = AssetDatabase.GetAssetPath(carpeta);
                    if (AssetDatabase.IsValidFolder(carpetaNuevosSprites))
                    {
                        posiblesSpritesNuevos = AssetDatabase.FindAssets("t:Sprite", new string[] { carpetaNuevosSprites });
                    }
                    else
                    {
                        carpeta = null;
                        carpetaNuevosSprites = "";
                    }
                }
                else carpetaNuevosSprites = "";
                Repaint();
            }
        }
        nombreNuevo = EditorGUILayout.TextField("Nombre de Clon", nombreNuevo);
        Modelo = EditorGUILayout.ObjectField("Modelo", Modelo, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(contentBotonCarpetaSprites))
        {
            EditorGUIUtility.ShowObjectPicker<Object>(carpeta, false, "t:folder", idPickerCarpetaSprites=EditorGUIUtility.GetControlID(contentBotonCarpetaSprites, FocusType.Passive));
        }
        GUI.enabled = false;
        EditorGUILayout.ObjectField(carpeta, typeof(Object),false);
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        string mensaje = "Elegir un modelo para clonar\n ";
        AnimationClip[] clips = new AnimationClip[0];
        if (Modelo)
        {
            clips = Modelo.animationClips;
            if (modeloComoOverride)
            {
                mensaje = modeloComoOverride.runtimeAnimatorController ? "Override de : " + modeloComoOverride.runtimeAnimatorController.name : "Override sin modelo original";
            }
            else mensaje = Modelo.name;

            mensaje += "\n" + (clips.Length > 0 ? "Contiene "+clips.Length+" Animation Clips" : "Sin Animation Clips");            
        }
        if (string.IsNullOrEmpty( carpetaNuevosSprites))
        {
            mensaje += "\nElegir carpeta de sprites nuevos";
        }
        else
        {
            if (posiblesSpritesNuevos.Length == 0) mensaje += "\nNo se encuentran sprites en " + carpetaNuevosSprites;
            else mensaje += "\nCarpeta " + carpetaNuevosSprites + " contiene " + posiblesSpritesNuevos.Length + " sprites";
        }
        EditorGUILayout.HelpBox(mensaje, MessageType.None, true);
        GUI.enabled = clips.Length > 0 && !string.IsNullOrEmpty(nombreNuevo) && !string.IsNullOrEmpty(carpetaNuevosSprites) && posiblesSpritesNuevos.Length>0;
        if (GUILayout.Button("Generar Animator Override y Clones de Clips"))
        {
            string destino = EditorUtility.SaveFolderPanel("Guardar dentro de... (se creará la carpeta " + nombreNuevo + " dentro de esta seleccion)", "Assets", "ClonesDeAnimacion");
            if (!string.IsNullOrEmpty(destino))
            {
            GenerarClon(destino);
            }
        }
        GUI.enabled=clips.Length>0 && !string.IsNullOrEmpty(carpetaNuevosSprites) && posiblesSpritesNuevos.Length > 0;
        if(GUILayout.Button("Copiar Configuracion de Importacion"))
        {
            CopiarConfiguracion();
        }
    }

    void GenerarClon(string destino)
    {
        if (destino.StartsWith(Application.dataPath)) destino = "Assets" + destino.Substring(Application.dataPath.Length);
        if (!AssetDatabase.IsValidFolder(System.IO.Path.Combine(destino, nombreNuevo))) AssetDatabase.CreateFolder(destino, nombreNuevo);

        EditorUtility.DisplayProgressBar("Cargando Modelo", "Leyendo paths, cargando el modelo ("+Modelo.name+") y generando nuevo override", .05f);
        destino = System.IO.Path.Combine(destino, nombreNuevo);
        RuntimeAnimatorController reemplaza = modeloComoOverride ? modeloComoOverride.runtimeAnimatorController : modeloComoController;
        AnimatorOverrideController nuevoOverride = new AnimatorOverrideController(reemplaza);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        var nuevosParesOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        nuevoOverride.GetOverrides(overrides);
        int conErrores = 0;
        int framesNoEncontradosTotal = 0;
        foreach (var par in overrides)
        {
            EditorUtility.DisplayProgressBar("Cargando Cada Animation Clip", "Cargando clip numero "+nuevosParesOverrides.Count+" de "+overrides.Count, .1f+ (nuevosParesOverrides.Count / (float)overrides.Count) * .9f);
            var original = par.Key;
            var nuevo = Instantiate(original);
            nuevo.name = nombreNuevo + "_" + nuevo.name;

            EditorCurveBinding bind = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var frames = AnimationUtility.GetObjectReferenceCurve(nuevo, bind);
            int framesNoEncontradosAhora = 0;
            for (int i = 0; i < frames.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Reemplazando Sprites", "buscando sprite numero "+i+" en clip numero "+nuevosParesOverrides.Count, .1f+.9f*Mathf.Lerp((nuevosParesOverrides.Count / (float)overrides.Count), (nuevosParesOverrides.Count+1f) / overrides.Count,i/(float)frames.Length));
                Sprite s = frames[i].value as Sprite;
                if (s)
                {
                    string nombreArchivoSprite = System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(s));
                    string nuevoOrigen = System.IO.Path.Combine(carpetaNuevosSprites, nombreArchivoSprite);
                    s = AssetDatabase.LoadAssetAtPath<Sprite>(nuevoOrigen);
                    if (s) frames[i].value = s;
                    else
                    {
                        framesNoEncontradosAhora++;
                        Debug.LogWarning("No se hallo " + nuevoOrigen + "("+(framesNoEncontradosAhora+framesNoEncontradosTotal)+")");
                    }
                }
            }
            if (framesNoEncontradosAhora > 0)
            {
                framesNoEncontradosTotal += framesNoEncontradosAhora;
                conErrores++;
            }
            EditorUtility.DisplayProgressBar("Guardando Clip", System.IO.Path.Combine(destino, nuevo.name + ".anim"), .1f+((nuevosParesOverrides.Count+1f)/overrides.Count)*.9f);
            AnimationUtility.SetObjectReferenceCurve(nuevo, bind, frames);
            AssetDatabase.CreateAsset(nuevo, System.IO.Path.Combine(destino, nuevo.name + ".anim"));
            nuevosParesOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(original, nuevo));
        }
        EditorUtility.DisplayProgressBar("Guardando Override", System.IO.Path.Combine(destino, nombreNuevo + ".overrideController"), 1f);
        nuevoOverride.ApplyOverrides(nuevosParesOverrides);
        AssetDatabase.CreateAsset(nuevoOverride, System.IO.Path.Combine(destino, nombreNuevo + ".overrideController"));
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        if (conErrores > 0)
        {
            Debug.LogWarning("No se hallaron " + framesNoEncontradosTotal + " sprites afectando a " + conErrores + " clips");
            EditorUtility.DisplayDialog("Surgio un tema", "No se hallaron " + framesNoEncontradosTotal + " sprites afectando a " + conErrores + " clips", "Que macana");
        }
    }

    void CopiarConfiguracion()
    {
        EditorUtility.DisplayProgressBar("Cargando Modelo", "Leyendo paths, cargando el modelo (" + Modelo.name + ")", .05f);
        RuntimeAnimatorController modeloOriginal = modeloComoOverride ? modeloComoOverride.runtimeAnimatorController : modeloComoController;
        EditorCurveBinding bind = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        int clipscontados=0;
        int clipstotal = modeloOriginal.animationClips.Length;
        int framesNoEncontradosTotal = 0;
        int conErrores = 0;
        foreach (var clip in modeloOriginal.animationClips)
        {
            EditorUtility.DisplayProgressBar("Cargando Cada Animation Clip", "Cargando clip numero " + clipscontados + " de " + clipstotal, .1f + (clipscontados / (float)clipstotal) * .9f);

            var frames = AnimationUtility.GetObjectReferenceCurve(clip, bind);
            int framesNoEncontradosAhora = 0;
            for (int i = 0; i < frames.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Modificando Sprites", "buscando sprite numero " + i + " en clip numero " + clipscontados, .1f + .9f * Mathf.Lerp((clipscontados / (float)clipstotal), (clipscontados + 1f) / clipstotal, i / (float)frames.Length));
                Sprite s = frames[i].value as Sprite;
                string pathSprite = s ? AssetDatabase.GetAssetPath(s) : null;
                    TextureImporter tImporter = pathSprite!=null? AssetImporter.GetAtPath(pathSprite)as TextureImporter:null;
                if (tImporter)
                {
                    TextureImporterSettings tiSettings=new TextureImporterSettings();
                    tImporter.ReadTextureSettings(tiSettings);
                    string nuevoPath = System.IO.Path.Combine(carpetaNuevosSprites, System.IO.Path.GetFileName(pathSprite));
                    tImporter = AssetImporter.GetAtPath(nuevoPath) as TextureImporter;
                    if (tImporter)
                    {
                        tImporter.SetTextureSettings(tiSettings);
                        tImporter.SaveAndReimport();
                    }
                    else
                    {
                        framesNoEncontradosAhora++;
                        Debug.LogWarning("No se hallo " + nuevoPath + "de los destino (" + (framesNoEncontradosAhora + framesNoEncontradosTotal) + ")");
                    }
                }
                else
                {
                    framesNoEncontradosAhora++;
                    Debug.LogWarning("No se hallo " + s + "de los de origen (" + (framesNoEncontradosAhora + framesNoEncontradosTotal) + ")");
                }
            }
            if (framesNoEncontradosAhora > 0)
            {
                framesNoEncontradosTotal += framesNoEncontradosAhora;
                conErrores++;
            }
            clipscontados++;
        }

        EditorUtility.ClearProgressBar();
        if (conErrores > 0)
        {
            Debug.LogWarning("No se hallaron " + framesNoEncontradosTotal + " sprites afectando a " + conErrores + " clips");
            EditorUtility.DisplayDialog("Surgio un tema", "No se hallaron " + framesNoEncontradosTotal + " sprites afectando a " + conErrores + " clips", "Que macana");
        }
    }
}
