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
        activo = EditorPrefs.GetBool("GuazuGizmoSpawner.Activo",activo);
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
    static bool activo=false;
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
        SceneView.duringSceneGui -= Actualizar;
        SceneView.duringSceneGui += Actualizar;
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
            Handles.DrawWireDisc(posicion, Vector3.forward,radio);
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
            this.direccion = direccion==Vector3.zero?Vector3.right: direccion;
            this.color = color;
        }
        public override void Dibujar(SceneView sceneView)
        {
            Handles.color = color;
            DrawArrow( posicion, Quaternion.LookRotation(direccion), HandleUtility.GetHandleSize(posicion) * .3f, Vector3.zero);            
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

    static Mesh Cono
    {
        get
        {
            if (!cono)
            {
                CargarMallasBasicas();
            }
            return cono;
        }
    }
    static void CargarMallasBasicas()
    {
        GameObject handleGo = (GameObject)EditorGUIUtility.Load("SceneView/HandlesGO.fbx");
        if (!handleGo)
        {
            Debug.Log("Couldn't find SceneView/HandlesGO.fbx");
        }
        handleGo.SetActive(false);
        const string k_AssertMessage = "mesh is null. A problem has occurred with `SceneView/HandlesGO.fbx`";
        foreach (Transform t in handleGo.transform)
        {
            var meshFilter = t.GetComponent<MeshFilter>();
            switch (t.name)
            {
                /*case "Cube":
                    s_CubeMesh = meshFilter.sharedMesh;
                    Debug.AssertFormat(s_CubeMesh != null, k_AssertMessage);
                    break;
                case "Sphere":
                    s_SphereMesh = meshFilter.sharedMesh;
                    Debug.AssertFormat(s_SphereMesh != null, k_AssertMessage);
                    break;*/
                case "Cone":
                    cono = meshFilter.sharedMesh;
                    Debug.AssertFormat(cono != null, k_AssertMessage);
                    break;
                    /* case "Cylinder":
                         s_CylinderMesh = meshFilter.sharedMesh;
                         Debug.AssertFormat(s_CylinderMesh != null, k_AssertMessage);
                         break;
                     case "Quad":
                         s_QuadMesh = meshFilter.sharedMesh;
                         Debug.AssertFormat(s_QuadMesh != null, k_AssertMessage);
                         break;*/
            }
        }
    }
    static Mesh cono;

    static void DrawArrow(Vector3 position, Quaternion rotation, float size, Vector3 coneOffset)
    {
        Vector3 direction = rotation * Vector3.forward;
        Handles.DrawLine(position, position + (direction + coneOffset) * size * .9f);
        Graphics.DrawMeshNow(Cono, StartCapDraw(position + (direction + coneOffset) * size, Quaternion.LookRotation(direction), size * .2f));
    }
    static Matrix4x4 StartCapDraw(Vector3 position, Quaternion rotation, float size)
    {
        Shader.SetGlobalColor("_HandleColor", Handles.color);
        Shader.SetGlobalFloat("_HandleSize", size);
        Matrix4x4 mat = Handles.matrix * Matrix4x4.TRS(position, rotation, Vector3.one);
        Shader.SetGlobalMatrix("_ObjectToWorld", mat);
        HandleUtility.handleMaterial.SetInt("_HandleZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        HandleUtility.handleMaterial.SetPass(0);
        return mat;
    }
#endif
}
