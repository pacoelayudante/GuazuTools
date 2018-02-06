using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformRandomInit : MonoBehaviour {

    public bool escalaRegular = true;
    public Vector3 randomEulerMin = Vector3.zero,randomEulerMax= Vector3.one*360f,randomScaleMin = Vector3.one,randomScaleMax=Vector3.one;

    private void OnEnable()
    {
        transform.Rotate( Random.Range(randomEulerMin.x,randomEulerMax.x),
            Random.Range(randomEulerMin.y, randomEulerMax.y),
            Random.Range(randomEulerMin.z, randomEulerMax.z));
        if (escalaRegular)
        {
            transform.localScale = Vector3.Scale(transform.localScale,
                Vector3.Lerp(randomScaleMin, randomScaleMax, Random.value));
        }
        else
        {
            transform.localScale = Vector3.Scale(new Vector3(Random.Range(randomScaleMin.x, randomScaleMax.x),
                Random.Range(randomScaleMin.y, randomScaleMax.y),
                Random.Range(randomScaleMin.z, randomScaleMax.z)),
                transform.localScale);
        }
    }

}
