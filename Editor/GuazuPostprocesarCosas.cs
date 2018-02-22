using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GuazuPostprocesarCosas {

    public static void TexturaSoloAlfa(string assetPath, Texture2D texture)
    {
        for (int m = 0; m < texture.mipmapCount; m++)
        {
            Color[] c = texture.GetPixels(m);

            for (int i = 0; i < c.Length; i++)
            {
                c[i].r = 1f;
                c[i].g = 1f;
                c[i].b = 1f;
            }
            texture.SetPixels(c, m);
        }
    }

}