using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GuazuGizmoSpawner : MonoBehaviour
{
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void LoadEstadoActivo()
    {
        activo = EditorPrefs.GetBool("GuazuGizmoSpawner.Activo",true);
    }

    [MenuItem("Guazu/Gizmos Guazu Visibles")]
    static void CambiarEstadoActivo()
    {
        Activo = !Activo;
    }
    [MenuItem("Guazu/Gizmos Guazu Visibles", true)]
    public static bool CambiarEstadoActivoValidate()
    {
        Menu.SetChecked("Guazu/Gizmos Guazu Visibles", Activo);
        return true;
    }

    public static bool Activo
    {
        get { return activo; }
        set { EditorPrefs.SetBool("GuazuGizmoSpawner.Activo", activo = value); }
    }
    static bool activo=true;
#endif

    public static void DibujarTexto(string texto, Vector3 posicion, Color color, float duracion = -1)
    {
#if UNITY_EDITOR
        if (!Activo) return;
        Gizmito g = new Texto(texto, posicion, color);
        if (duracion >= 0) g.duracion = duracion;
#endif
    }

    public static void DibujarLinea(Vector3 a, Vector3 b, Color color, float duracion = -1)
    {
#if UNITY_EDITOR
        if (!Activo) return;
        Gizmito g = new Linea(a, b, color);
        if (duracion >= 0) g.duracion = duracion;
#endif
    }

    public static void DibujarFlecha(Vector3 posicion, Vector3 direccion, Color color, float duracion = -1)
    {
#if UNITY_EDITOR
        if (!Activo) return;
        Gizmito g = new Flecha(posicion, direccion, color);
        if (duracion >= 0) g.duracion = duracion;
#endif
    }

    public static void DibujarCirculo(Vector3 posicion, float radio, Color color, float duracion = -1)
    {
#if UNITY_EDITOR
        if (!Activo) return;
        Gizmito g = new Circulo(posicion, radio, color);
        if (duracion >= 0) g.duracion = duracion;
#endif
    }

    public static void DibujarCaja(Vector3 posicion, Vector3 tam, Color color, float duracion = -1)
    {
#if UNITY_EDITOR
        if (!Activo) return;
        Gizmito g = new Caja(posicion, tam, color);
        if (duracion >= 0) g.duracion = duracion;
#endif
    }



#if UNITY_EDITOR
    static List<Gizmito> gizmitos = new List<Gizmito>();

    [InitializeOnLoadMethod]
    static void Iniciar()
    {
        SceneView.onSceneGUIDelegate -= Actualizar;
        SceneView.onSceneGUIDelegate += Actualizar;
    }

    static void Actualizar(SceneView sceneView)
    {
        for (int i=gizmitos.Count-1; i>=0; i--)
        {
            gizmitos[i].Dibujar(sceneView);
            if (gizmitos[i].Envejecer()) gizmitos.RemoveAt(i);
        }
    }

    abstract class Gizmito
    {
        public double duracion = 10d;
        double marcaTemporal;
        protected Gizmito()
        {
            marcaTemporal = EditorApplication.timeSinceStartup;
            if (gizmitos!=null) gizmitos.Add(this);
        }
        abstract public void Dibujar(SceneView sceneView);

        public bool Envejecer()
        {
            return (EditorApplication.timeSinceStartup > marcaTemporal + duracion);
        }
    }

    class Circulo : Gizmito
    {
        Color color;
        Vector3 posicion;
        float radio;
        public Circulo(Vector3 posicion, float radio, Color color)
        {
            this.posicion = posicion;
            this.radio = radio;
            this.color = color;
        }
        public override void Dibujar(SceneView sceneView)
        {
            Handles.color = color;
            Handles.CircleHandleCap(-1, posicion, Quaternion.identity,radio, EventType.Repaint);
        }
    }

    class Caja : Gizmito
    {
        Color color;
        Vector3 posicion;
        Vector3 tam;
        public Caja(Vector3 posicion, Vector3 tam, Color color)
        {
            this.posicion = posicion;
            this.tam = tam;
            this.color = color;
        }
        public override void Dibujar(SceneView sceneView)
        {
            Handles.color = color;
            Handles.DrawWireCube(posicion, tam);
        }
    }

    class Texto : Gizmito
    {
        GUIStyle style = new GUIStyle();
        Vector3 posicion;
        string texto;
        public Texto(string texto, Vector3 posicion, Color color)
        {
            this.texto = texto;
            this.posicion = posicion;
            style.normal.textColor = color;
        }
        public override void Dibujar(SceneView sceneView)
        {
            Handles.Label(posicion, texto, style);
        }
    }

    class Flecha : Gizmito
    {
        Color color;
        Vector3 posicion;
        Vector3 direccion;
        public Flecha(Vector3 posicion, Vector3 direccion, Color color)
        {
            this.posicion = posicion;
            this.direccion = direccion;
            this.color = color;
        }
        public override void Dibujar(SceneView sceneView)
        {
            Handles.color = color;
            Handles.ArrowHandleCap(-1, posicion, Quaternion.LookRotation(direccion), HandleUtility.GetHandleSize(posicion) * .3f,EventType.Repaint);            
        }
    }

    class Linea : Gizmito
    {
        Color color;
        Vector3 puntoA;
        Vector3 puntoB;
        public Linea(Vector3 puntoA, Vector3 puntoB, Color color)
        {
            this.puntoA = puntoA;
            this.puntoB = puntoB;
            this.color = color;
        }
        public override void Dibujar(SceneView sceneView)
        {
            Handles.color = color;
            Handles.DrawLine(puntoA,puntoB);
        }
    }
#endif
}
