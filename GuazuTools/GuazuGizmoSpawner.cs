using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GuazuGizmoSpawner : MonoBehaviour
{
    public static void DibujarFlecha(Vector3 posicion, Vector3 direccion, Color color, float duracion = 0)
    {
#if UNITY_EDITOR
        Gizmito g = new Flecha(posicion, direccion, color);
        if (duracion > 0) g.duracion = duracion;
#endif
    }

    public static void DibujarCirculo(Vector3 posicion, float radio, Color color, float duracion = 0)
    {
#if UNITY_EDITOR
        Gizmito g = new Circulo(posicion, radio, color);
        if (duracion > 0) g.duracion = duracion;
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
            //Gizmos.color = color;
            //Gizmos.DrawLine(posicion, posicion + direccion);
            //Gizmos.DrawWireCube(posicion, Vector3.one* HandleUtility.GetHandleSize(posicion) * .1f);
            Handles.color = color;
            Handles.CircleHandleCap(-1, posicion, Quaternion.identity,radio, EventType.Repaint);
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
            //Gizmos.color = color;
            //Gizmos.DrawLine(posicion, posicion + direccion);
            //Gizmos.DrawWireCube(posicion, Vector3.one* HandleUtility.GetHandleSize(posicion) * .1f);
            Handles.color = color;
            Handles.ArrowHandleCap(-1, posicion, Quaternion.LookRotation(direccion), HandleUtility.GetHandleSize(posicion) * .3f,EventType.Repaint);            
        }
    }
#endif
}
