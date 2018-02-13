using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccionesGenerales : MonoBehaviour {
    public void Destruir(GameObject go)
    {
        if (go) Destroy(go);
    }
}
