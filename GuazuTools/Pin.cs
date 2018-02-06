/*
 * Para esas veces que poner un objeto como hijo no alcanza o no sirve
 * 
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {

    [Header("Quien")]
    public Transform quien;
    [Header("Que")]
    public bool posicion;
    public bool rotacion, escala;
    [Header("Como")]
    public Space como = Space.World;
    [Header("Cuando")]
    public bool enFixedUpdate;
    public bool enUpdate, enLateUpdate;

    private void FixedUpdate()
    {
        if (enFixedUpdate) Hacer();
    }
    private void Update()
    {
        if (enUpdate) Hacer();
    }
    private void LateUpdate()
    {
        if (enLateUpdate) Hacer();
    }

    // Update is called once per frame
    void Hacer () {
		if (quien)
        {
            if (como == Space.Self)
            {
                if (posicion) transform.localPosition = quien.localPosition;
                if (rotacion) transform.localRotation = quien.localRotation;
                if (escala) transform.localScale = quien.localScale;
            }
            else
            {
                if (posicion) transform.position = quien.position;
                if (rotacion) transform.rotation = quien.rotation;
            }
        }
	}
}
