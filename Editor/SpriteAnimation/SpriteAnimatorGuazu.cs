//actualizacion 1/6/2017

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SpriteAnimatorGuazu : EditorWindow
{
    public static bool editando { get; private set; }
    public static bool sinSprites { get; private set; }
    static readonly EditorCurveBinding spriteCurveBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
    static readonly string propiedadSprite = "m_Sprite";
    static readonly Color grisOscuro = new Color(.2f, .2f, .2f, 1);
    static readonly Color grisClaro = new Color(.8f, .8f, .8f, 1);
    static readonly Vector2 tamUnidad = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight * 2);
    static readonly float altoBarraFrames = tamUnidad.y + EditorGUIUtility.singleLineHeight*2;
    static readonly GUIContent textoFondoClaro = new GUIContent("Fondo claro"),
        textoFondoOscuro = new GUIContent("Fondo oscuro");
    static readonly string textoSinSprites = "Esta animacion no tiene sprites",
        textoCargandAnimacion = "Cargando animacion",
    textoNoDeberiaTomarMucho = "no deberia tomar mucho...",
    textoInterrumpiendoEdicionTitulo = "Woah! ¿Cancelar edicion?",
        textoInterrumpiendoEdicion = "¿Queres cancelar la edicion?\nNo te la estaria guardando en ese caso.",
        textoInterrumpir = "Hacela mierda nomas",
        textoNoInterrumpir = "[WIP] ¡Uy, espera! [WIP]";

    static readonly string urlIconos = "Assets/GuazuTools/Editor/Editor Default Resources";
    static float escalaVistaPrevia = .25f;
    static bool generarGUISkins = true, estirarPreview = false;
    static GUIContent texPlay, /*texPausa,*/ texEliOff, texEliOn, texDupOff, texDupOn;
    static GUIStyle stiloFrameApagado,stiloFrameNormal,stiloFrameElegido,stiloFrameElegidoFusion,
        stiloFrameElegidoPrevio,stiloFrameNormalFusionable,
        stiloBorrarDisabled, stiloBorrarOff, stiloBorrarOn,
        stiloDuplicarDisabled, stiloDuplicarOff, stiloDuplicarOn;

    public class LiveKeyframe
    {
        public Sprite sprite { get { return (Sprite)keyframeReal.value; } set { keyframeReal.value = value; } }
        public Rect posicion { get { return posicionLocal; } }
        public Rect posicionReal { get { return posicionRealLocal; } }
        public int duracion { get { return duracionLocal; } }
        ObjectReferenceKeyframe keyframeReal;
        Rect posicionLocal, posicionRealLocal;
        Vector2 posicionEstoy, posicionVoy;
        int duracionLocal;

        public LiveKeyframe(ObjectReferenceKeyframe keyframe, int duracion, int offset, Vector2 tamUnidad)
        {
            keyframeReal = keyframe;
            duracionLocal = duracion;
            posicionVoy = new Vector2(tamUnidad.x * offset, tamUnidad.x * (duracion + offset));
            posicionLocal = new Rect(0, 0, 0, tamUnidad.y);
        }
        public LiveKeyframe(LiveKeyframe padre)
        {
            keyframeReal = new ObjectReferenceKeyframe();
            keyframeReal.time = padre.keyframeReal.time;
            keyframeReal.value = padre.keyframeReal.value;
            duracionLocal = padre.duracionLocal;
            posicionVoy = padre.posicionVoy;
            posicionEstoy = padre.posicionEstoy;
            posicionLocal = padre.posicionLocal;
        }
        public bool ModificarDuracion(int cuanto)
        {
            duracionLocal += cuanto;
            if (duracionLocal <= 0)
            {
                duracionLocal = 1;
                return false;
            }
            return true;
        }
        public void ActualizarPosicion(int offset, Vector2 tamUnidad)
        {
            posicionVoy.Set(tamUnidad.x * offset, tamUnidad.x * duracionLocal);
            if (posicionEstoy != posicionVoy)
            {
                posicionEstoy = Vector2.Lerp(posicionEstoy, posicionVoy, .1f);
                posicionLocal.Set(posicionEstoy.x, 0, posicionEstoy.y, tamUnidad.y);
                posicionRealLocal.Set(posicionVoy.x, 0, posicionVoy.y, tamUnidad.y);
            }
        }
        public ObjectReferenceKeyframe GenerarObjectReferenceKeyframe(int offset, float frameRate)
        {
            ObjectReferenceKeyframe orkf = new ObjectReferenceKeyframe();
            orkf.time = offset/frameRate;
            orkf.value = sprite;
            return orkf;
        }
    }


    AnimationEvent[] eventos = new AnimationEvent[0];
    AnimationClip clipSeleccionado;
    LiveKeyframe[] keyframes;
    List<LiveKeyframe> fusionados = new List<LiveKeyframe>();
    LiveKeyframe frameElegido, frameElegidoPrevio;
    Sprite spriteParaAgregar;
    Rect tamAnimacion, rectBarra;
    Vector2 posicionScroll, grabMousePos;
    bool usaGrisClaro, pausa, drageando, eliminar, fusion, duplicar;
    int duracionTotal, posicionElegida, mouseOver, cantidadKeyframesOriginal;
    float frameRate;
    double ultimoClick, cabezal, lastTime;

    public static void Editar(AnimationClip clip)
    {
        SpriteAnimatorGuazu ventana = GetWindow<SpriteAnimatorGuazu>();
        ventana.CargarClip(clip, true);
    }
    
    void GenerarGUISKins()
    {
        generarGUISkins = false;
        if (texPlay == null) texPlay = new GUIContent(EditorGUIUtility.FindTexture("PlayButton"));
        //if (texPlay == null) texPausa = new GUIContent(EditorGUIUtility.FindTexture("PauseButton"));
        if (texEliOff == null) texEliOff = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(urlIconos+"/iconotachooff.png"));
        if (texEliOn == null) texEliOn = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(urlIconos + "/iconotachoon.png"));
        if (texDupOff == null) texDupOff = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(urlIconos + "/iconoduplicaroff.png"));
        if (texDupOn == null) texDupOn = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(urlIconos + "/iconoduplicaron.png"));
        stiloFrameApagado = "flow node 0";
          stiloFrameNormal =  "flow node 1" ;
          stiloFrameElegido = "flow node 3 on";
          stiloFrameElegidoFusion =  "flow node 2 on";
          stiloFrameElegidoPrevio =  "flow node 1 on";
          stiloFrameNormalFusionable =  "flow node 2";
        stiloBorrarDisabled = new GUIStyle("flow node hex 0");
        stiloBorrarOff = new GUIStyle("flow node hex 6");
        stiloBorrarOn = new GUIStyle("flow node hex 6 on");
        stiloDuplicarDisabled = new GUIStyle("flow node hex 0");
        stiloDuplicarOff = new GUIStyle("flow node hex 3");
        stiloDuplicarOn = new GUIStyle("flow node hex 3 on");
        stiloBorrarDisabled.contentOffset = stiloBorrarOff.contentOffset = stiloBorrarOn.contentOffset = -Vector2.up * texEliOff.image.height*.85f;
        stiloDuplicarDisabled.contentOffset = stiloDuplicarOff.contentOffset = stiloDuplicarOn.contentOffset = -Vector2.up* texDupOff.image.height*.85f;
    }
    void Update()
    {
        if (keyframes != null) Repaint();
    }

    void OnEnable()
    {
        minSize = new Vector2(310, 310);
    }
    void OnDestroy()
    {
        if (editando)
        {
            if (EditorUtility.DisplayDialog(textoInterrumpiendoEdicionTitulo, textoInterrumpiendoEdicion, textoInterrumpir, textoNoInterrumpir))
            {
                editando = false;
                clipSeleccionado = null;
                keyframes = null;
            }
            else
            {
                //Rescatate
            }
        }
    }

    [MenuItem("Assets/Guazu/Sprite Animator")]
    static void AbrirDelContext()
    {
        Editar((AnimationClip)Selection.activeObject);
    }
    [MenuItem("Assets/Guazu/Sprite Animator", true)]
    static bool AbrirDelContextValidator()
    {
        if (Selection.activeObject == null) return false;
        return Selection.activeObject.GetType() == typeof(AnimationClip);
    }

    public void AplicarCambios(AnimationClip clip, LiveKeyframe[] kfs, float fps)
    {
        if (clip == null) return;
        else if (kfs == null) return;
        else if (kfs.Length == 0) AnimationUtility.SetObjectReferenceCurve(clip, spriteCurveBinding, new ObjectReferenceKeyframe[0]);
        else
        {
            ObjectReferenceKeyframe[] generados = new ObjectReferenceKeyframe[kfs.Length];
            int offsetContado = 0;
            for(int i=0; i<kfs.Length; i++)
            {
                generados[i] = kfs[i].GenerarObjectReferenceKeyframe(offsetContado,fps);
                offsetContado += kfs[i].duracion;
            }
            clip.frameRate = fps;
            AnimationUtility.SetObjectReferenceCurve(clip, spriteCurveBinding, generados);
        }
    }

    public void CargarClip(AnimationClip nuevoClip, bool editar)
    {
        if (editando)
        {
            if (EditorUtility.DisplayDialog(textoInterrumpiendoEdicionTitulo, textoInterrumpiendoEdicion, textoInterrumpir, textoNoInterrumpir))
            {
                editando = false;
            }
            else
                return;
        }
        EditorUtility.DisplayProgressBar(textoCargandAnimacion, textoNoDeberiaTomarMucho, 0);

        clipSeleccionado = nuevoClip;
        eventos = AnimationUtility.GetAnimationEvents(clipSeleccionado);
        ObjectReferenceKeyframe[] kfs = AnimationUtility.GetObjectReferenceCurve(clipSeleccionado, EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), propiedadSprite));
        cantidadKeyframesOriginal = kfs.Length;

        if (kfs == null) sinSprites = true;
        else if (kfs.Length == 0) sinSprites = true;
        else
        {
            frameRate = clipSeleccionado.frameRate;
            sinSprites = false;
            System.Array.Sort(kfs, new ObjectReferenceKeyframeTimeComparer());
            keyframes = new LiveKeyframe[kfs.Length];
            posicionScroll = Vector2.zero;
            duracionTotal = Mathf.RoundToInt(kfs[kfs.Length - 1].time * frameRate) + 1;
            int offsetAcumulado = 0;
            for (int i = 0; i < keyframes.Length; i++)
            {
                int duracion = 1;
                if (i < keyframes.Length - 1) duracion = Mathf.RoundToInt((kfs[i + 1].time - kfs[i].time) * frameRate);
                keyframes[i] = new LiveKeyframe(kfs[i], duracion, offsetAcumulado, tamUnidad);
                offsetAcumulado += duracion;
            }
        }

        editando = editar && !sinSprites;
        EditorUtility.ClearProgressBar();
    }

    void OnGUI()
    {
        if (generarGUISkins) GenerarGUISKins();
        GUI.enabled = !editando;
        EditorGUILayout.BeginHorizontal();
        AnimationClip nuevaSeleccion = (AnimationClip)EditorGUILayout.ObjectField(clipSeleccionado, typeof(AnimationClip), false);
        if (GUILayout.Button("Editar",(GUIStyle)"minibuttonleft",GUILayout.ExpandWidth(false))) CargarClip(clipSeleccionado,true);
        GUI.enabled = editando;
        if (GUILayout.Button("Revertir", (GUIStyle)"minibuttonmid", GUILayout.ExpandWidth(false))) CargarClip(clipSeleccionado,editando = false);
        if (GUILayout.Button("Aplicar", (GUIStyle)"minibuttonright", GUILayout.ExpandWidth(false)))
        {
            AplicarCambios(clipSeleccionado, keyframes, frameRate);
            CargarClip(clipSeleccionado, editando = false);//editando = false;
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
        if (nuevaSeleccion != clipSeleccionado && !editando)
        {
            CargarClip(nuevaSeleccion, false);
        }

        if (clipSeleccionado == null || keyframes == null) return;
        else if (sinSprites)
        {
            EditorGUILayout.HelpBox(textoSinSprites, MessageType.Info);
            return;
        }
        
        Animador();
    }

    void Animador()
    {
        EditorGUILayout.BeginHorizontal();
        usaGrisClaro = GUILayout.Toggle(usaGrisClaro, usaGrisClaro ? textoFondoClaro : textoFondoOscuro, GUILayout.ExpandWidth(false));
        escalaVistaPrevia = GUILayout.HorizontalSlider(escalaVistaPrevia,.03f,3);
        frameRate = EditorGUILayout.FloatField("FrameRate",frameRate,GUILayout.ExpandWidth(false));
        if (frameRate <= 0) frameRate = 1;
        EditorGUILayout.EndHorizontal();
        MostrarBarra();
        SpritePlayer();
        if (mouseOver >= 0)
        {
            if (frameElegido != null)
            {
                DibujarVistaPrevia(frameElegido.sprite, grabMousePos, escalaVistaPrevia, new Color(1, 1, 1, .5f));
            }
            else
            {
                grabMousePos = Event.current.mousePosition + tamUnidad;
            }
            DibujarVistaPrevia(keyframes[mouseOver].sprite, grabMousePos, escalaVistaPrevia, frameElegido == null);
        }
    }

    void SpritePlayer()
    {
        cabezal += (EditorApplication.timeSinceStartup - lastTime) * frameRate;
        if (cabezal >= duracionTotal) cabezal %= duracionTotal;
        lastTime = EditorApplication.timeSinceStartup;

        Sprite spriteActual = null;
        double copiaCabezal = cabezal;
        for (int i = 0; i < keyframes.Length; i++)
        {
            if (copiaCabezal < keyframes[i].duracion)
            {
                spriteActual = keyframes[i].sprite;
                break;
            }
            else copiaCabezal -= keyframes[i].duracion;
        }

        MostrarSprite(spriteActual);
    }

    void MostrarBarra()
    {

        posicionScroll = EditorGUILayout.BeginScrollView(posicionScroll, false, false, GUILayout.Height(altoBarraFrames));
        rectBarra = GUILayoutUtility.GetRect(duracionTotal * tamUnidad.x, tamUnidad.y);
        EditorGUI.DrawRect(rectBarra, grisOscuro);
        rectBarra = GUILayoutUtility.GetRect(duracionTotal * tamUnidad.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.DrawRect(rectBarra, grisClaro);

        if (clipSeleccionado && eventos.Length > 0)
        {
            List<string> grupoDeEventos = new List<string>();
            rectBarra.width = tamUnidad.x;
            for (int i = 0; i < eventos.Length; i++)
            {
                grupoDeEventos.Add(string.Format("{0} -> {1} ({2})", eventos[i].time, eventos[i].functionName,eventos[i].intParameter));
                if (i + 1 < eventos.Length && eventos[i].time == eventos[i + 1].time) continue;
                rectBarra.x = eventos[i].time * tamUnidad.x * frameRate;
                EditorGUI.Popup(rectBarra, -1, grupoDeEventos.ToArray());
                grupoDeEventos.Clear();
            }
        }

        if (Event.current.type == EventType.MouseDown && editando)
        {
            for (int i = 0; i < keyframes.Length; i++)
            {
                if (keyframes[i].posicionReal.Contains(Event.current.mousePosition))
                {                    
                    frameElegido = keyframes[posicionElegida = i];
                    ultimoClick = EditorApplication.timeSinceStartup;
                    fusion = Event.current.button == 1;
                    if (fusion) fusionados.Clear();
                    duplicar = eliminar = drageando = false;
                    Event.current.Use();

                    spriteParaAgregar = frameElegido.sprite;
                    break;
                }
            }
        }
        else if (frameElegido != null)
        {
            if (Event.current.rawType == EventType.MouseUp)
            {
                if (!drageando && EditorApplication.timeSinceStartup - ultimoClick < .2f && frameElegido.posicionReal.Contains(Event.current.mousePosition))
                {
                    int mod = Event.current.button == 0 ? 1 : -1;
                    if (frameElegido.ModificarDuracion(mod))
                    {
                        duracionTotal += mod;
                    }
                    Event.current.Use();
                }
                else if (drageando && frameElegido != null)
                {
                    if ( fusion && fusionados.Count > 0) {
                        foreach (LiveKeyframe fundir in fusionados)
                        {
                            frameElegido.ModificarDuracion(+fundir.duracion);
                            ArrayUtility.Remove(ref keyframes, fundir);
                        } }
                    else if (duplicar)
                    {
                        ArrayUtility.Remove(ref keyframes, frameElegido);
                        ArrayUtility.Insert(ref keyframes, posicionElegida, new LiveKeyframe(frameElegido));
                    }
                }
                frameElegidoPrevio = frameElegido;
                frameElegido = null;
                duplicar = eliminar = drageando = false;
            }
            else if (Event.current.type == EventType.MouseDrag && !eliminar && !duplicar)
            {
                if (fusion && fusionados.Count > 0) fusionados.Clear();
                if (drageando)
                {
                    if (Event.current.mousePosition.y < frameElegido.posicion.yMin || Event.current.mousePosition.y > frameElegido.posicion.yMax)
                    {
                        if (ArrayUtility.Contains(keyframes, frameElegido) && keyframes[posicionElegida] != frameElegido)
                        {
                            ArrayUtility.Remove(ref keyframes, frameElegido);
                            ArrayUtility.Insert(ref keyframes, posicionElegida, frameElegido);
                        }
                    }
                    else
                    {
                        int elegido = -1;
                        for (int i = 0; i < keyframes.Length; i++)
                        {
                            if (keyframes[i] == frameElegido) break;
                            else
                            {
                                if (Event.current.mousePosition.x <= keyframes[i].posicionReal.xMin + (fusion ? 1 : .5f) * keyframes[i].posicionReal.width)
                                {
                                    elegido = i;
                                    if (fusion) fusionados.Add(keyframes[i]);
                                    else break;
                                }
                            }
                        }
                        if (elegido == -1)
                        {
                            for (int i = keyframes.Length - 1; i >= 0; i--)
                            {
                                if (keyframes[i] == frameElegido) break;
                                else
                                {
                                    if (Event.current.mousePosition.x > keyframes[i].posicionReal.xMax - (fusion?1:.5f)* keyframes[i].posicionReal.width)
                                    {
                                        elegido = i;
                                        if (fusion) fusionados.Add(keyframes[i]);
                                        else break;
                                    }
                                }
                            }
                        }
                        if (elegido != -1 && !fusion)
                        {
                            ArrayUtility.Remove(ref keyframes, frameElegido);
                            ArrayUtility.Insert(ref keyframes, elegido, frameElegido);
                            Event.current.Use();
                        }
                    }
                }
                else
                {
                    drageando = true;
                    Event.current.Use();
                }
            }
        }

        int offsetAcumulado = 0;
        mouseOver = -1;
        for (int i = 0; i < keyframes.Length; i++)
        {
            if (keyframes[i].posicion.Contains(Event.current.mousePosition) || keyframes[i].posicion.Contains(Event.current.mousePosition-Vector2.up*EditorGUIUtility.singleLineHeight)) mouseOver = i;
            keyframes[i].ActualizarPosicion(offsetAcumulado, tamUnidad);
            if (!editando) GUI.Box(keyframes[i].posicion, GUIContent.none, stiloFrameApagado);
            else if (fusion) GUI.Box(keyframes[i].posicion, GUIContent.none, frameElegido == keyframes[i] ? stiloFrameElegidoFusion : fusionados.Contains(keyframes[i]) ? stiloFrameNormalFusionable : frameElegidoPrevio == keyframes[i] ? stiloFrameElegidoPrevio : stiloFrameNormal);
                else GUI.Box(keyframes[i].posicion, GUIContent.none, frameElegido == keyframes[i] ? stiloFrameElegido : frameElegidoPrevio == keyframes[i] ? stiloFrameElegidoPrevio : stiloFrameNormal);
            offsetAcumulado += keyframes[i].duracion;
        }
        if (Event.current.type == EventType.ScrollWheel && mouseOver != -1)
        {
            escalaVistaPrevia -= Event.current.delta.y * .01f;
            if (escalaVistaPrevia < .05f) escalaVistaPrevia = .01f;
        }
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box(eliminar?texEliOn:texEliOff, drageando ? (eliminar ? stiloBorrarOn : stiloBorrarOff) : stiloBorrarDisabled);
        if (!duplicar && !fusion && drageando && frameElegido != null && Event.current.type == EventType.MouseDrag && keyframes.Length > 1)
        {
            Rect r = GUILayoutUtility.GetLastRect();
            if (r.Contains(Event.current.mousePosition))
            {
                if (!eliminar)
                {
                    eliminar = true;
                    duracionTotal -= frameElegido.duracion;
                    ArrayUtility.Remove(ref keyframes, frameElegido);
                    Event.current.Use();
                }
            }
            else if (eliminar)
            {
                eliminar = false;
                duracionTotal += frameElegido.duracion;
                ArrayUtility.Insert(ref keyframes, posicionElegida, frameElegido);
                Event.current.Use();
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.Box(duplicar ? texDupOn : texDupOff, drageando ? (duplicar ? stiloDuplicarOn : stiloDuplicarOff):stiloDuplicarDisabled);
        if (!eliminar && !fusion && drageando && frameElegido != null && Event.current.type == EventType.MouseDrag)
        {
            Rect r = GUILayoutUtility.GetLastRect();
            if (r.Contains(Event.current.mousePosition))
            {
                if (!duplicar)
                {
                    duplicar = true;
                    if (ArrayUtility.Contains(keyframes, frameElegido) && keyframes[posicionElegida] != frameElegido)
                    {
                        ArrayUtility.Remove(ref keyframes, frameElegido);
                        ArrayUtility.Insert(ref keyframes, posicionElegida, frameElegido);
                    }
                    duracionTotal += frameElegido.duracion;
                    ArrayUtility.Insert(ref keyframes, posicionElegida, frameElegido);
                    Event.current.Use();
                }
            }
            else if (duplicar)
            {
                duplicar = false;
                duracionTotal -= frameElegido.duracion;
                ArrayUtility.Remove(ref keyframes, frameElegido);
                Event.current.Use();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        spriteParaAgregar = EditorGUILayout.ObjectField(spriteParaAgregar, typeof(Sprite), false) as Sprite;
        if (GUILayout.Button("Agregar Sprite"))
        {
            ObjectReferenceKeyframe ork = new ObjectReferenceKeyframe();
            ork.value = spriteParaAgregar;
            duracionTotal++;
            ArrayUtility.Add(ref keyframes, new LiveKeyframe(ork,1,0,tamUnidad));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Duracion Actual : " + duracionTotal / frameRate);
        GUILayout.Label("Duracion Original : " + clipSeleccionado.length);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Frames Actual : " + keyframes.Length);
        GUILayout.Label("Frames Original : " + cantidadKeyframesOriginal);
        EditorGUILayout.EndHorizontal();

    }

    void MostrarSprite(Sprite sprite)
    {
        Rect espacio = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        if (sprite != null)
        {
            float aspecto = sprite.rect.width / sprite.rect.height;
            float maxWidth = espacio.height * aspecto;
            if (!estirarPreview && maxWidth > sprite.rect.width) maxWidth = sprite.rect.width;
            if (espacio.width > maxWidth) espacio.width = maxWidth;
            espacio.height = espacio.width / aspecto;
            tamAnimacion = espacio;     
        }
        EditorGUI.DrawRect(tamAnimacion, usaGrisClaro ? grisClaro : grisOscuro);
        if (sprite != null)
        {
            Rect texR = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height, sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(tamAnimacion, sprite.texture, texR);
        }

        if (tamAnimacion.Contains(Event.current.mousePosition))
        {
            GUIContent estirar = new GUIContent("Estirar");
            Vector2 tam = GUI.skin.toggle.CalcSize(estirar);
            estirarPreview = EditorGUI.ToggleLeft(new Rect(tamAnimacion.position, tam), estirar, estirarPreview);
        }
        EditorGUILayout.EndVertical();
    }
    
    void DibujarVistaPrevia(Sprite s, Vector2 pos, float escala, bool conFondo = true)
    {
        DibujarVistaPrevia(s,pos,escala, Color.white, conFondo);
    }
    void DibujarVistaPrevia(Sprite s, Vector2 pos, float escala, Color tinte, bool conFondo = true)
    {
        if (s != null)
        {
            Rect r = new Rect(pos, s.rect.size * escala);
            if (r.xMax > position.width) r.x -= r.width;
            if (r.xMin < 0) r.x += r.width;
            Rect texto = new Rect(r.x, r.y - EditorGUIUtility.singleLineHeight, r.width, EditorGUIUtility.singleLineHeight);
            Rect texR = new Rect(s.rect.x / s.texture.width, s.rect.y / s.texture.height, s.rect.width / s.texture.width, s.rect.height / s.texture.height);
            if (conFondo)
            {
                EditorGUI.DrawRect(texto, grisClaro);
                GUI.Label(texto, s.name);
                EditorGUI.DrawRect(r, usaGrisClaro ? grisClaro : grisOscuro);
            }
            else
            {
                texto.y += EditorGUIUtility.singleLineHeight;
                GUI.Label(texto, s.name);
            }
            Color previo = GUI.color;
            GUI.color = tinte;
            GUI.DrawTextureWithTexCoords(r, s.texture, texR);
            GUI.color = previo;
        }
    }
}

public class ObjectReferenceKeyframeTimeComparer : IComparer<ObjectReferenceKeyframe>
{
    public int Compare(ObjectReferenceKeyframe x, ObjectReferenceKeyframe y)
    {
        if (x.time == y.time) return 0;
        else return x.time > y.time ? 1 : -1;
    }
}