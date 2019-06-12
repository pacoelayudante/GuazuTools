using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
#pragma warning disable 414

//MODO DE USO v.03
// >> (opcional) Se elige Animator
// >> Se elige AnimationClip (de manera comun o usando popup generado por Animator)
// >> (opcional) Se prueba exclusividad de sprites (esto se fija si dentro del
//    Animator elegido los sprites de este AnimationClip son exclusivos)
// >> Cargar animacion levanta los sprites
//    La visualizacion se genera en el (0,0,0) de la SceneView
// >> El slider es para elegir el keyframe visualizado
// >> Se mueve el offset del origen de los sprites ya sea con Handles o con numeros
// >> "Aplicar" hace lo obvio, pero guarda que no hay UNDO!
//    Si queremos revertir, aplicamos el offset invertido (por eso te recuerda el recien aplicado)
//
// --fix solo probado con import mode single
// by Paco (pacoelayudante@gmail.com)
// https://gist.github.com/pacoelayudante
//
public class AcomodaSprites : EditorWindow
{
    static Animator animatorPrev;
    static UnityEditor.Animations.AnimatorController animator;
    static AnimationClip animClip;
    static AnimationClip[] listaClips;
    static bool modoMoverTodos = true;
    static string noClip = "- - -";
    static string[] listaClipsNombres = new string[] { noClip};
    static string[] textoExclusiva = new string[] { "Exclusividad desconocida", "Exclusiva", "No exclusiva" };
    static int exclusividad = 0;
    static Material previewSpriteMaterial;
    Vector2 freeMoveHandlePos,memoriaMoveHandlePos;
    
    List<Sprite> sprites = new List<Sprite>();
    int keyFrameSeleccionado = 0;
    ObjectReferenceKeyframe[] keyframes;

    Sprite SpriteVisualizado
    {
        get
        {
            if (keyframes != null || sprites.Count > 0)
            {
                if (keyframes == null ? sprites.Count > 0 : keyframes.Length > keyFrameSeleccionado)
                {
                    if (keyframes == null ? sprites[keyFrameSeleccionado] : keyframes[keyFrameSeleccionado].value != null)
                    {
                        return (keyframes == null ? sprites[keyFrameSeleccionado] : keyframes[keyFrameSeleccionado].value) as Sprite;
                    }
                }
            }
            return null;
        }
    }

    [MenuItem("Guazu/Acomoda Sprites")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(AcomodaSprites));
    }

    void OnEnable()
    {
#if UNITY_2018
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#else
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
#endif
    }
    void OnDisable()
    {
#if UNITY_2018
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#else
        SceneView.duringSceneGui -= this.OnSceneGUI;
#endif
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (keyframes != null || sprites.Count > 0)
        {
            if ( keyframes==null? sprites.Count > 0: keyframes.Length > keyFrameSeleccionado)
            {
                if (keyframes==null?sprites[keyFrameSeleccionado]: keyframes[keyFrameSeleccionado].value != null)
                {
                    if (previewSpriteMaterial == null)
                        previewSpriteMaterial = new Material(Shader.Find("Sprites/Default"));
                  
                    Sprite s = (keyframes==null?sprites[keyFrameSeleccionado]: keyframes[keyFrameSeleccionado].value) as Sprite;
                    previewSpriteMaterial.mainTexture = s.texture;
                    GL.PushMatrix();
                    GL.MultMatrix( Matrix4x4.TRS(freeMoveHandlePos,Quaternion.identity,Vector3.one) );
                    GL.Begin(GL.TRIANGLES);
                    previewSpriteMaterial.SetPass(0);
                    for (int i = 0; i < s.triangles.Length; i++)
                    {
                        GL.TexCoord(s.uv[s.triangles[i]]);
                        GL.Vertex(s.vertices[s.triangles[i]]);
                    }
                    GL.End();
                    GL.PopMatrix();

                    float esc = 10f / s.pixelsPerUnit;
                    Handles.color = Color.red;
                    Handles.DrawLine(-Vector3.right * esc, Vector3.right * esc);
                    Handles.color = Color.blue;
                    Handles.DrawLine(-Vector3.forward * esc, Vector3.forward * esc);
                    Handles.color = Color.green;
                    Handles.DrawLine(-Vector3.up * esc, Vector3.up * esc);
                    EditorGUI.BeginChangeCheck();
                    freeMoveHandlePos = Handles.Slider2D(freeMoveHandlePos, Vector3.forward, Vector3.right, Vector3.up,
                        esc*3, Handles.RectangleHandleCap, 0);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Repaint();
                    }
                }
            }
        }
    }

    void CargarSprites(AnimationClip[] clips)
    {
        sprites.Clear();
        foreach (AnimationClip ac in clips) CargarSprites(ac, false);
        keyframes = null;
    }
    void CargarSprites(AnimationClip clip, bool limpiar = true)
    {
        if(limpiar)sprites.Clear();
        EditorCurveBinding curvBind = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        keyframes = AnimationUtility.GetObjectReferenceCurve(clip, curvBind);
            foreach (ObjectReferenceKeyframe frame in keyframes)
            {
                Sprite s = (Sprite)frame.value;
                if (!sprites.Contains(s) && s != null)
                {
                    sprites.Add(s);
                }
            }
    }
    
    void OnGUI()
    {
        int sel = 0;
        if (animClip && listaClips != null)
        {
            for (int i=0; i<listaClips.Length; i++)
            {
                if (listaClips[i] == animClip) { sel = i+1; break; }
            }
        }

        EditorGUI.BeginChangeCheck();
        animator = EditorGUILayout.ObjectField("Animator", animator, typeof(UnityEditor.Animations.AnimatorController), false) as UnityEditor.Animations.AnimatorController;
        if (EditorGUI.EndChangeCheck())
        {
            exclusividad = 0;
            keyframes = null;
            sprites.Clear();
            if (animator)
            {
                //RuntimeAnimatorController animatorCont = animator.runtimeAnimatorController;
                //listaClips = animatorCont.animationClips;
                listaClips = animator.animationClips;
                listaClipsNombres = new string[listaClips.Length+1];
                listaClipsNombres[0] = noClip;
                for (int i = 0; i < listaClips.Length; i++) listaClipsNombres[i+1] = listaClips[i].name;
            }
            else
            {
                listaClips = null;
            }
        }

        EditorGUI.BeginChangeCheck();
        animClip = EditorGUILayout.ObjectField("Clip", animClip, typeof(AnimationClip), false) as AnimationClip;
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Sprite", SpriteVisualizado, typeof(Sprite), false,GUILayout.Height(EditorGUIUtility.singleLineHeight));
        GUI.enabled = true;
        if (EditorGUI.EndChangeCheck()) exclusividad = 0;

        if (listaClips != null)
        {
            EditorGUI.BeginChangeCheck();
            sel = EditorGUILayout.Popup(sel, listaClipsNombres);
            if (EditorGUI.EndChangeCheck())
            {
                exclusividad = 0;
                if (sel == 0) animClip = null;
                else animClip = listaClips[sel-1];
            }
        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.Popup(0, listaClipsNombres );
            GUI.enabled = true;
        }
        
            GUI.enabled = (animClip && listaClips!=null && sel > 0);
        if (GUILayout.Button("Probar exclusividad de sprites."))
        {
            exclusividad = ProbarExclusividadSprites(animClip, listaClips) ? 1 : 2;
        }
        GUILayout.Label(textoExclusiva[exclusividad]);
        GUI.enabled = listaClips != null;
        if (GUILayout.Button("Cargar todas las animaciones"))
        {
            freeMoveHandlePos = Vector2.zero;
            CargarSprites(listaClips);
        }
        GUI.enabled = animClip != null;
        if (GUILayout.Button("Cargar animacion"))
        {
            freeMoveHandlePos = Vector2.zero;
            CargarSprites(animClip);
        }
        if (keyframes == null)
        {
            GUI.enabled = sprites.Count > 0;
            EditorGUI.BeginChangeCheck();
            keyFrameSeleccionado = EditorGUILayout.IntSlider(keyFrameSeleccionado, 0, (sprites.Count > 0 ? sprites.Count-1 : 1));
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }
        else
        {
            GUI.enabled = keyframes.Length > 0;
            EditorGUI.BeginChangeCheck();
            keyFrameSeleccionado = EditorGUILayout.IntSlider(keyFrameSeleccionado, 0, keyframes.Length-1);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        GUI.enabled = true;
        GUILayout.Label("Sprites cargados : " + sprites.Count);
        GUILayout.Label("Keyframes cargados : " + (keyframes == null ? "null" : keyframes.Length.ToString()));

        if (keyframes != null) GUI.enabled = keyframes.Length > keyFrameSeleccionado;
        else GUI.enabled = sprites.Count > keyFrameSeleccionado;
        EditorGUI.BeginChangeCheck();
        freeMoveHandlePos = EditorGUILayout.Vector2Field("Offset de origen", freeMoveHandlePos);
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
        GUI.enabled = false;
        EditorGUILayout.Vector2Field("Offset aplicado", memoriaMoveHandlePos);
        EditorGUILayout.Vector2Field("Tamaño Sprite", SpriteVisualizado?SpriteVisualizado.rect.size:Vector2.zero);
        EditorGUILayout.Vector2Field("Pivot Actual", SpriteVisualizado ? SpriteVisualizado.pivot : Vector2.zero);
        GUI.enabled = (keyframes==null? (sprites.Count > keyFrameSeleccionado) : keyframes.Length > keyFrameSeleccionado) && freeMoveHandlePos != Vector2.zero;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancelar")) {
            freeMoveHandlePos = Vector2.zero;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Aplicar Todos Los Frames"))
        {
            Aplicar(sprites);
        }
        GUI.enabled = SpriteVisualizado&&GUI.enabled;
        if (GUILayout.Button("Aplicar Solo Este Frame"))
        {
            Aplicar( new List<Sprite>( new Sprite[] { SpriteVisualizado} ) );
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }
    
    void Aplicar(List<Sprite> sprites)
    {
        float f = 0;
        List<TextureImporter> tImps = new List<TextureImporter>();
        Vector2 offsetSpriteVisible = -freeMoveHandlePos * SpriteVisualizado.pixelsPerUnit;
         offsetSpriteVisible += SpriteVisualizado ? SpriteVisualizado.pivot : Vector2.zero;
        foreach (Sprite s in sprites)
        {
            if (s == null) continue;

            TextureImporter ti = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(s.texture)) as TextureImporter;

            if (!tImps.Contains(ti)) tImps.Add(ti);
           
            if (ti.spriteImportMode == SpriteImportMode.Single)
            {
                TextureImporterSettings tis = new TextureImporterSettings();
                ti.ReadTextureSettings(tis);
                //if (tis.spriteAlignment == (int)SpriteAlignment.Custom)
                {
                    tis.spritePivot=new Vector2(offsetSpriteVisible.x / s.rect.size.x, offsetSpriteVisible.y / s.rect.size.y);
                    /*tis.spritePivot = offsetSpriteVisible 
                        - (new Vector2(
                        freeMoveHandlePos.x / s.rect.size.x
                        , freeMoveHandlePos.y  / s.rect.size.y)
                        ) * s.pixelsPerUnit;*/
                }
                /*else
                {
                    tis.spritePivot =
                        new Vector2(freeMoveHandlePos.x / s.rect.size.x
                        , freeMoveHandlePos.y / s.rect.size.y);
                }*/
                tis.spriteAlignment = (int)SpriteAlignment.Custom;
                ti.SetTextureSettings(tis);
            }
            else if (ti.spriteImportMode == SpriteImportMode.Multiple)
            {
                SpriteMetaData[] metas = ti.spritesheet;
                for (int i=0; i<metas.Length; i++)
                {
                    if (metas[i].name.Equals(s.name))
                    {
                        metas[i].pivot = new Vector2(offsetSpriteVisible.x / s.rect.size.x, offsetSpriteVisible.y / s.rect.size.y);/*
                        if (metas[i].alignment == (int)SpriteAlignment.Custom)
                        {
                            metas[i].pivot -=
                                new Vector2(freeMoveHandlePos.x / s.rect.size.x
                                , freeMoveHandlePos.y / s.rect.size.y);
                        }
                        else
                        {
                            metas[i].pivot =
                                new Vector2(freeMoveHandlePos.x / s.rect.size.x
                                , freeMoveHandlePos.y / s.rect.size.y);
                        }
                        metas[i].alignment = (int)SpriteAlignment.Custom;*/
                        break;
                    }
                }
                ti.spritesheet = metas;
            }

            f += 1f / sprites.Count;
            EditorUtility.DisplayProgressBar("Aplicando cambio", ti.assetPath + "->" + s.name, f);
        }
        memoriaMoveHandlePos = freeMoveHandlePos;
        freeMoveHandlePos = Vector2.zero;
        f = 0;
        foreach (TextureImporter ti in tImps)
        {            
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            f += 1f / tImps.Count;
            EditorUtility.DisplayProgressBar("Aplicando cambio", "Reimportando -> "+ti.assetPath, f);
        }
        EditorUtility.ClearProgressBar();
    }

    bool ProbarExclusividadSprites(AnimationClip clip, AnimationClip[] demasClips)
    {
        EditorCurveBinding curvBind = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keyfs = AnimationUtility.GetObjectReferenceCurve(clip, curvBind);
        for (int i=0; i<demasClips.Length; i++)
        {
            if (clip != demasClips[i])
            {
                if (EditorUtility.DisplayCancelableProgressBar("Comprobando","Sprites solo en esta animacion",i/(float)demasClips.Length))
                {
                    return false;
                }
                ObjectReferenceKeyframe[] otraKF = AnimationUtility.GetObjectReferenceCurve(demasClips[i], curvBind);
                foreach(ObjectReferenceKeyframe mia in keyfs)
                {
                    if (mia.value == null) { continue; }
                    foreach(ObjectReferenceKeyframe suya in otraKF)
                    {
                        if (mia.value == suya.value)
                        {
                            EditorUtility.ClearProgressBar();
                            EditorUtility.DisplayDialog("No exclusiva", "Los sprites de esta animacion no son exclusivos.\nLa animacion que comparte sprites es "+demasClips[i].name+" y el sprite es "+mia.value.name+".", "Bajon");
                            return false;
                        }
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Exclusiva", "Los sprites de esta animacion son exclusivos.", "Cope");
        return true;
    }

}
