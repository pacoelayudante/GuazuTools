using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuazuWeird2DCamWatch : MonoBehaviour {

    Camera camara;
    Camera Camara
    {get{if (!camara) camara = GetComponent<Camera>();return camara; }}

    public Transform objetivo;
    public float velocidadMaxima = 1f, aceleracion = 10f, desaceleracion = 30f, radioCentral = 0.25f, bordeActivador = .3f;
    float velocidad;

    bool yendo;

    private void Update()
    {
        if (!objetivo || !Camara) return;
        Vector2 posEnCamara = Camara.WorldToScreenPoint(objetivo.transform.position) - new Vector3(Camara.pixelWidth / 2, Camara.pixelHeight / 2, 0);
        if (velocidad > 0f)
        {
            transform.Translate(posEnCamara.normalized * velocidad * Time.deltaTime, Space.Self);
        }
        if (yendo)
        {
            if (velocidad < velocidadMaxima)
            {
                velocidad = Mathf.MoveTowards(velocidad, velocidadMaxima, aceleracion * Time.deltaTime);
            }
            if (posEnCamara.magnitude < radioCentral * Camara.pixelHeight * .5f / Camara.orthographicSize) yendo = false;
        }
        else
        {
            if (velocidad > 0f)
            {
                velocidad = Mathf.MoveTowards(velocidad, 0f, desaceleracion * Time.deltaTime);
            }
            if (Mathf.Abs(posEnCamara.x) * 2 > Camara.pixelWidth - Camara.pixelHeight * bordeActivador / Camara.orthographicSize
                || Mathf.Abs(posEnCamara.y) * 2 > Camara.pixelHeight - Camara.pixelHeight * bordeActivador / Camara.orthographicSize)
            {
                yendo = true;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Camara) return;
        Vector2 posEnCamara = objetivo?Camara.WorldToScreenPoint(objetivo.transform.position) - new Vector3(Camara.pixelWidth / 2, Camara.pixelHeight / 2, 0)
            :Vector3.zero;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = Camara.transform.localToWorldMatrix;
        if (objetivo) {
            if (Mathf.Abs(posEnCamara.x)*2 > Camara.pixelWidth-Camara.pixelHeight*bordeActivador/Camara.orthographicSize
                || Mathf.Abs(posEnCamara.y)*2 > Camara.pixelHeight - Camara.pixelHeight * bordeActivador / Camara.orthographicSize)
                Gizmos.color = Color.red;
        }
        Gizmos.DrawWireCube(Vector3.forward, new Vector3(Camara.orthographicSize*Camara.aspect*2f - bordeActivador*2f,Camara.orthographicSize*2f-bordeActivador * 2f, 0f));

        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.matrix = Camara.transform.localToWorldMatrix;
        if (objetivo)
        {
            if (posEnCamara.magnitude < radioCentral * Camara.pixelHeight * .5f / Camara.orthographicSize) UnityEditor.Handles.color = Color.green;
        }
        UnityEditor.Handles.CircleHandleCap(-1, Vector3.forward, Quaternion.identity, radioCentral, EventType.Repaint);        
    }
#endif
}
