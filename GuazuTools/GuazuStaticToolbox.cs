using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Guazu {

    public static Vector3 MirrorVector
    {
        get { return new Vector3(-1, 1, 1); }
    }

    public static Quaternion QuaternionRandom2D{
        get{
            return Quaternion.Euler(0, 0, Random.value * 360f);
        }
        }

    public static Quaternion QuaternionEuler(Vector2 vec2)
    {
        return Quaternion.Euler( 0, 0, Mathf.Atan2( vec2.y, vec2.x )*Mathf.Rad2Deg );
    }

    public static Quaternion MirrorQuaternion(Quaternion q)
    {
        q.x *= -1;
        q.w *= -1;
        return q;
    }

    public static Vector2 Vector2DesdeAngulo(float grados)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * grados), Mathf.Sin(Mathf.Deg2Rad * grados));
    }
    public static float AnguloDesdeVector2(Vector2 vector)
    {
        return Mathf.Atan2(vector.y,vector.x);
    }

    public static Color BlancoClear
    {
        get { return new Color(1f, 1f, 1f, 0f); }
    }

    public static Color ColorClear(Color c)
    {
        return ColorAlfa(c,0f);
    }
    public static Color ColorAlfa(Color c, float alfa)
    {
        c.a = alfa;
        return c;
    }

    public static Mesh MeshDeSprite(Sprite sprite)
    {
        Mesh m = new Mesh();
        return MeshDeSprite(sprite, m);
    }
    public static Mesh MeshDeSprite(Sprite sprite, Mesh destino)
    {
        Vector2[] spV = sprite.vertices;
        Vector3[] v = new Vector3[spV.Length];
        ushort[] spT = sprite.triangles;
        int[] t = new int[spT.Length];
        for (int i = 0; i < v.Length; i++) v[i] = spV[i];
        for (int i = 0; i < t.Length; i++) t[i] = spT[i];
        destino.vertices = v;
        destino.uv = sprite.uv;
        destino.triangles = t;
        return destino;
    }
}
