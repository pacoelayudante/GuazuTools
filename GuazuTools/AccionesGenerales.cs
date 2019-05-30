using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccionesGenerales : MonoBehaviour {
    public void Destruir(GameObject go)
    {
        if (go) Destroy(go);
    }
    public void Spawnear(GameObject go)
    {
        if (go) Instantiate(go,transform.position,transform.rotation).SetActive(true);
    }
    public void SpawnearSinActivar(GameObject go)
    {
        if (go) Instantiate(go,transform.position,transform.rotation);
    }
}
