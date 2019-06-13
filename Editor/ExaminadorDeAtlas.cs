using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Sprites;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEditor;

namespace GuazuTools
{
    public class ExaminadorDeAtlas : EditorWindow
    {
        bool fitTextura;
        SpriteRenderer viendoSpriteRend;
        SpriteAtlas viendoAtlas;
        Vector2 scrollAtlas;
        Texture2D tex2dAtlas = null;
        Texture2D tex2dSprite = null;
        Sprite[] spritesDeAtlas = new Sprite[0];
        Vector2[] uvsSprite = new Vector2[0];
        ushort[] triangulos = new ushort[0];
        Color colMostraZona = new Color(1f, 0f, 0f, .6f);
        Color colMostraZonaHover = new Color(0f, 1f, 0f, .6f);
        Material mat;
        Material Mat
        {
            get
            {
                if (mat == null)
                {
                    var shader = Shader.Find("Hidden/Internal-Colored");
                    mat = new Material(shader);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                }
                return mat;
            }
        }
        SpriteRenderer ViendoSpriteRend
        {
            get { return viendoSpriteRend; }
            set
            {
                viendoSpriteRend = value;
                tex2dAtlas = null;
                tex2dSprite = null;
                if (viendoSpriteRend && viendoSpriteRend.sprite)
                {
                    tex2dSprite = SpriteUtility.GetSpriteTexture(ViendoSpriteRend.sprite, false);
                    try
                    {
                        tex2dAtlas = SpriteUtility.GetSpriteTexture(ViendoSpriteRend.sprite, true);
                    }
                    catch { }
                    if (tex2dAtlas)
                    {
                        uvsSprite = SpriteUtility.GetSpriteUVs(ViendoSpriteRend.sprite, true);
                        triangulos = viendoSpriteRend.sprite.triangles;
                        for (int i = 0; i < uvsSprite.Length; i++)
                        {
                            if (i == 0) scrollAtlas.Set(uvsSprite[i].x * tex2dAtlas.width, (1f - uvsSprite[i].y) * tex2dAtlas.height);
                            else
                            {
                                if (uvsSprite[i].x * tex2dAtlas.width < scrollAtlas.x) scrollAtlas.x = uvsSprite[i].x * tex2dAtlas.width;
                                if ((1f - uvsSprite[i].y) * tex2dAtlas.height < scrollAtlas.y) scrollAtlas.y = (1f - uvsSprite[i].y) * tex2dAtlas.height;
                            }
                        }
                    }
                }

            }
        }
        SpriteAtlas ViendoAtlas
        {
            get { return viendoAtlas; }
            set
            {
                viendoAtlas = value;
                if (viendoAtlas == null)
                {
                    ViendoSpriteRend = viendoSpriteRend;
                    return;
                }
                spritesDeAtlas = new Sprite[viendoAtlas.spriteCount];
                //Aca habria que hacer un recorrido por todos los sprites y ver cuantas texturas son, sino, solo nos quedamos con una, y no sirve viste
                if (viendoAtlas.GetSprites(spritesDeAtlas) > 0)
                {
                    try
                    {
                        tex2dAtlas = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(spritesDeAtlas[0], true);
                    }
                    catch
                    {
                        tex2dAtlas = null;
                    }
                }
            }
        }

        [MenuItem("Guazu/Ver Atlas")]
        static void Abrir()
        {
            GetWindow<ExaminadorDeAtlas>();
        }

        private void OnEnable()
        {
            OnSelectionChanged();
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }
        private void OnDisable()
        {
            if (mat != null) DestroyImmediate(mat);
            Selection.selectionChanged -= OnSelectionChanged;
        }

        void OnSelectionChanged()
        {
            if (!ViendoSpriteRend || !Selection.Contains(ViendoSpriteRend.gameObject))
            {
                var srSeleccionados = Selection.GetFiltered<SpriteRenderer>(SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable);
                if (srSeleccionados.Length > 0)
                {
                    ViendoSpriteRend = srSeleccionados[0];
                    Repaint();
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var niuViendoAtlas = EditorGUILayout.ObjectField(ViendoAtlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            if (EditorGUI.EndChangeCheck())
            {
                ViendoAtlas = niuViendoAtlas;
            }
            fitTextura = EditorGUILayout.Toggle("fitTextura", fitTextura);
            EditorGUILayout.EndHorizontal();
            if (ViendoAtlas) ConAtlas();
            else SinAtlas();
        }

        void ConAtlas()
        {
            var e = Event.current;
            var eventType = Event.current.type;
            scrollAtlas = EditorGUILayout.BeginScrollView(scrollAtlas);
            if (tex2dAtlas)
            {
                var rectAtlasPreview = fitTextura ? GUILayoutUtility.GetAspectRect(tex2dAtlas.width / (float)tex2dAtlas.height) : GUILayoutUtility.GetRect(tex2dAtlas.width, tex2dAtlas.height);
                EditorGUI.DrawTextureAlpha(rectAtlasPreview, tex2dAtlas, ScaleMode.ScaleToFit);
                for (int i=0; i<spritesDeAtlas.Length; i++){
                    if(!spritesDeAtlas[i])continue;
                    var uvs = SpriteUtility.GetSpriteUVs(spritesDeAtlas[i], true);
                    var hover = PuntoEnUVs(rectAtlasPreview,uvs,spritesDeAtlas[i].triangles,e.mousePosition);
                    DrawUVs(rectAtlasPreview, uvs, spritesDeAtlas[i].triangles,hover?colMostraZonaHover: colMostraZona);
                    if(hover && eventType==EventType.MouseDown && e.button==0){
                        Selection.activeObject = spritesDeAtlas[i].texture;
                        Repaint();
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            var scrollRect = GUILayoutUtility.GetLastRect();
            if(eventType == EventType.MouseDrag && scrollRect.Contains(e.mousePosition) && e.button == 2){
                scrollAtlas -= e.delta;
                Repaint();
            }
        }

        private void SinAtlas()
        {
            var srSeleccionados = Selection.GetFiltered<SpriteRenderer>(SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable);
            var opciones = new GUIContent[srSeleccionados.Length];
            var viendoIndex = -1;
            for (int i = 0; i < opciones.Length; i++)
            {
                if (ViendoSpriteRend == srSeleccionados[i]) viendoIndex = i;
                opciones[i] = new GUIContent(srSeleccionados[i].name + " - " + srSeleccionados[i].sprite);
            }
            EditorGUI.BeginChangeCheck();
            viendoIndex = EditorGUILayout.Popup(viendoIndex, opciones);
            if (EditorGUI.EndChangeCheck())
            {
                ViendoSpriteRend = srSeleccionados[viendoIndex];
            }
            if (ViendoSpriteRend && ViendoSpriteRend.sprite)
            {
                if (GUILayout.Button(ViendoSpriteRend.name)) EditorGUIUtility.PingObject(ViendoSpriteRend);
                scrollAtlas = EditorGUILayout.BeginScrollView(scrollAtlas);
                if (tex2dAtlas)
                {
                    var rectAtlasPreview = fitTextura ? GUILayoutUtility.GetAspectRect(tex2dAtlas.width / (float)tex2dAtlas.height) : GUILayoutUtility.GetRect(tex2dAtlas.width, tex2dAtlas.height);
                    EditorGUI.DrawTextureAlpha(rectAtlasPreview, tex2dAtlas, ScaleMode.ScaleToFit);
                    DrawUVs(rectAtlasPreview, uvsSprite, triangulos,colMostraZona);
                }
                else
                {
                    var rectSpritePreview = GUILayoutUtility.GetAspectRect(tex2dSprite.width / (float)tex2dSprite.height, GUILayout.MaxWidth(tex2dSprite.width), GUILayout.MaxHeight(tex2dSprite.height));
                    EditorGUI.DrawTextureAlpha(rectSpritePreview, tex2dSprite, ScaleMode.ScaleToFit);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        bool PuntoEnUVs(Rect rect,Vector2[] puntos, ushort[] tris, Vector2 puntoEn){
            if(puntos != null){
                for (int i = 0; i < tris.Length; i+=3)
                {
                    var pA = puntos[tris[i]];
                    var pB = puntos[tris[i+1]];
                    var pC = puntos[tris[i+2]];
                    pA.Set(pA.x*rect.width, (1f-pA.y)*rect.height);
                    pB.Set(pB.x*rect.width, (1f-pB.y)*rect.height);
                    pC.Set(pC.x*rect.width, (1f-pC.y)*rect.height);
                    var dentro = Guazu.PuntoEnTriangulo(pA,pB,pC,puntoEn);
                    if(dentro) return true;
                }
            }
            return false;
        }
        void DrawUVs(Rect rect, Vector2[] puntos, ushort[] tris, Color color)
        {
            if (Event.current.type == EventType.Repaint && Mat && puntos != null)
            {
                GUI.BeginClip(rect);
                GL.PushMatrix();
                Mat.SetPass(0);

                GL.Begin(GL.TRIANGLES);
                GL.Color(color);
                for (int i = 0; i < tris.Length; i++)
                {
                    GL.Vertex3(puntos[tris[i]].x * rect.width, (1f - puntos[tris[i]].y) * rect.height, 0f);
                }
                GL.End();
                GL.PopMatrix();
                GUI.EndClip();
            }
        }
    }
}