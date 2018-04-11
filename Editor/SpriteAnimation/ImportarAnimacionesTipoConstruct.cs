using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class ImportarAnimacionesTipoConstruct : EditorWindow {
    
    const float frameRate = 24f;
    const string urlAnimacionesImportadas = "Assets/Animaciones Importadas Tipo Construct";

    [MenuItem("Assets/Guazu/Imporar Animacion Tipo Construct")]
    static void AbrirDelContext()
    {
        ProcesarCarpeta(Selection.activeObject);
    }
    [MenuItem("Assets/Guazu/Imporar Animacion Tipo Construct", true)]
    static bool AbrirDelContextValidator()
    {
        if (Selection.activeObject == null) return false;
        return AssetDatabase.GetSubFolders(AssetDatabase.GetAssetPath(Selection.activeObject)).Length > 0;
    }
    public static void ProcesarCarpeta(Object carpetaElegida)
    {
        string url = AssetDatabase.GetAssetPath(carpetaElegida);
        string[] carpetas = AssetDatabase.GetSubFolders(url);
        if (EditorUtility.DisplayDialog("Importar Animacion Tipo Construct", "Deseas importar la carpeta " + url+" que contiene "+carpetas.Length+" subcarpetas", "Total", "Nop"))
        {
            List<AnimationClip> animaciones = new List<AnimationClip>();
            float carga = 0f;
            foreach (string carpeta in carpetas)
            {
                EditorUtility.DisplayProgressBar("Generando Animaciones",carpeta,carga += 1f/carpetas.Length);
                AnimationClip generado = GenerarAnimationClip(carpeta);
                if(generado)animaciones.Add(generado);
            }
            if (animaciones.Count > 0)
            {
                carga = 0f;
                EditorUtility.DisplayProgressBar("Generando Animaciones", "Creando Animator", carga);
                if (!AssetDatabase.IsValidFolder(urlAnimacionesImportadas)) AssetDatabase.CreateFolder("Assets", "Animaciones Importadas Tipo Construct");
                if (!AssetDatabase.IsValidFolder(urlAnimacionesImportadas+"/" + carpetaElegida.name)) AssetDatabase.CreateFolder(urlAnimacionesImportadas, carpetaElegida.name);
                AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPath(urlAnimacionesImportadas+"/" + carpetaElegida.name + "/" + carpetaElegida.name + ".controller");
                foreach (AnimationClip clip in animaciones)
                {
                    EditorUtility.DisplayProgressBar("Generando Animaciones", "Compilando ("+clip.name+")", carga += 1f/animaciones.Count);
                    animator.AddMotion(clip);
                    AssetDatabase.CreateAsset(clip, urlAnimacionesImportadas + "/" + carpetaElegida.name + "/" +carpetaElegida.name+"_"+ clip.name + ".anim");
                }
                EditorUtility.DisplayProgressBar("Generando Animaciones", "Actualizando", carga);
                AssetDatabase.SaveAssets();
            }
        }
        EditorUtility.ClearProgressBar();
    }

    static AnimationClip GenerarAnimationClip(string carpeta)
    {
        string nombre = "sinnombre_raro";
        System.Text.RegularExpressions.Regex filtro = new System.Text.RegularExpressions.Regex("[^\\/]*$");
        System.Text.RegularExpressions.Match match = filtro.Match(carpeta);
        if (match.Success) nombre = match.Value;
        string[] guidSprites = AssetDatabase.FindAssets("t:Sprite", new string[] { carpeta });
        if (guidSprites.Length > 0)
        {
            AnimationClip nuevo = new AnimationClip();
            nuevo.name = nombre;
            nuevo.frameRate = frameRate;
            EditorCurveBinding bind = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] kf = new ObjectReferenceKeyframe[guidSprites.Length];
            for (int i = 0; i < guidSprites.Length; i++)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath( guidSprites[i] ) );
                kf[i] = new ObjectReferenceKeyframe();
                kf[i].time = i / frameRate;
                kf[i].value = s;
            }
            AnimationUtility.SetObjectReferenceCurve(nuevo, bind, kf);
            return nuevo;
        }
        return null;
    }
}
