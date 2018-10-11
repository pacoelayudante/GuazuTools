using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GuazuPostprocesarCosas {

    public static void TexturaSoloAlfa(string assetPath, Texture2D texture)
    {
        TexturaCambiarColor(assetPath, texture, Color.white);
    }
    public static void TexturaCambiarColor(string assetPath, Texture2D texture, Color col)
    {
        for (int m = 0; m < texture.mipmapCount; m++)
        {
            Color[] c = texture.GetPixels(m);

            for (int i = 0; i < c.Length; i++)
            {
                c[i].r = col.r;
                c[i].g = col.g;
                c[i].b = col.b;
            }
            texture.SetPixels(c, m);
        }
    }
    public static void TexturaMultiplicarColor(string assetPath, Texture2D texture, Color col)
    {
        for (int m = 0; m < texture.mipmapCount; m++)
        {
            Color[] c = texture.GetPixels(m);

            for (int i = 0; i < c.Length; i++)
            {
                c[i].r *= col.r;
                c[i].g *= col.g;
                c[i].b *= col.b;
            }
            texture.SetPixels(c, m);
        }
    }

    [MenuItem("Assets/Guazu/Peligro/Retroprocesar Textura")]
    static void RetroprocesarTextura()
    {
        var esTextura = Selection.activeObject as Texture2D;
        if (esTextura)
        {
            var pathYop = AssetDatabase.GetAssetPath(esTextura).Substring(("Assets").Length);
            pathYop = Application.dataPath+pathYop;
            var encoded = esTextura.EncodeToPNG();
            if (encoded != null)
            {
                Debug.Log("SOBRESCRIBIENDO -> " + pathYop);
                System.IO.File.WriteAllBytes(pathYop, encoded);
            }
        }
    }
    [MenuItem("Assets/Guazu/Peligro/Retroprocesar Textura",true)]
    static bool RetroprocesarTexturaValidator()
    {
        var esTextura = Selection.activeObject as Texture2D;
        if (esTextura)
        {
            return AssetDatabase.IsMainAsset(esTextura);
        }
        else return false;
    }
}