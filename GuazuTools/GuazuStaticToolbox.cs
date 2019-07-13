using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Guazu
{

    public static readonly Vector3 MirrorVector = new Vector3(-1f, 1f, 1f);
    public static readonly Vector3 FlipVector = new Vector3(1f, -1f, 1f);

    public static Quaternion QuaternionRandom2D
    {
        get
        {
            return Quaternion.Euler(0, 0, UnityEngine.Random.value * 360f);
        }
    }

    public static Quaternion QuaternionEuler(Vector2 vec2)
    {
        if (vec2.sqrMagnitude == 0) return Quaternion.identity;
        else return Quaternion.Euler(0, 0, Mathf.Atan2(vec2.y, vec2.x) * Mathf.Rad2Deg);
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
        return Mathf.Atan2(vector.y, vector.x);
    }

    public static Color BlancoClear
    {
        get { return new Color(1f, 1f, 1f, 0f); }
    }

    public static Color ColorClear(Color c)
    {
        return ColorAlfa(c, 0f);
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


    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
    {
        float bx = p2.x - p1.x;
        float by = p2.y - p1.y;
        float dx = p4.x - p3.x;
        float dy = p4.y - p3.y;
        float bDotDPerp = bx * dy - by * dx;
        if (bDotDPerp == 0)
        {
            return false;
        }
        float cx = p3.x - p1.x;
        float cy = p3.y - p1.y;
        float t = (cx * dy - cy * dx) / bDotDPerp;

        result.Set(p1.x + t * bx, p1.y + t * by);
        return true;
    }

    public static bool PuntoEnTriangulo(Vector2 triA, Vector2 triB, Vector2 triC, Vector2 pt)
    {
        // Compute vectors        
        var v0 = triC - triA;
        var v1 = triB - triA;
        var v2 = pt - triA;

        // Compute dot products
        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);

        // Compute barycentric coordinates
        var invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    public static float QueLadoDeLaLinea(Vector2 lineaA, Vector2 lineaB, Vector2 punto)
    {
        return (lineaB.x - lineaA.x) * (punto.y - lineaA.y) - (lineaB.y - lineaA.y) * (punto.x - lineaA.x);
    }
    public static bool PuntoEnConoInfinito(Vector2 centroCono, Vector2 conoContraReloj, Vector2 conoConReloj, Vector2 punto)
    {
        return QueLadoDeLaLinea(centroCono, conoConReloj, punto) < 0f && QueLadoDeLaLinea(centroCono, conoContraReloj, punto) > 0f;
    }

    public static IEnumerator EsperarFalseYEntonces(System.Func<bool> esperarFalse, System.Action yEntonces)
    {
        while (esperarFalse()) yield return null;
        yEntonces();
    }
    public static IEnumerator DelayYEntonces(float t, System.Action yEntonces, bool unscaled = false)
    {
        if (unscaled) yield return new WaitForSecondsRealtime(t);
        else yield return new WaitForSeconds(t);
        yEntonces();
    }

    static Mesh quad;
    public static Mesh Quad
    {
        get
        {
            if (quad == null) quad = GenerarQuad();
            return quad;
        }
    }

    public static Mesh GenerarQuad(bool cocinar = true)
    {
        Vector3[] vertices = new Vector3[]{
            new Vector3(-.5f,-.5f,0f),new Vector3(.5f,-.5f,0f),new Vector3(-.5f,.5f,0f),new Vector3(.5f,.5f,0f)};
        int[] triangulos = new int[] { 0, 1, 2, 1, 3, 2 };

        Mesh m = new Mesh();
        m.name = "Guazu Quad";
        m.vertices = vertices;
        m.triangles = triangulos;
        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();
        if (cocinar) m.UploadMeshData(false);

        return m;
    }

    public static Rect RectSuperPosicion(Rect a, Rect b, bool amplitudMinimaCero = true)
    {
        var minX = Mathf.Max(a.xMin, b.xMin);
        var maxX = Mathf.Min(a.xMax, b.xMax);
        if (amplitudMinimaCero && minX > maxX) minX = maxX = (minX + maxX) / 2f;

        var minY = Mathf.Max(a.yMin, b.yMin);
        var maxY = Mathf.Min(a.yMax, b.yMax);
        if (amplitudMinimaCero && minY > maxY) minY = maxY = (minX + maxY) / 2f;

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
    public static List<Rect> RectSustraccion(Rect inicial, Rect sustraido)
    {
        var resultado = new List<Rect>();
        if (inicial.Overlaps(sustraido))
        {
            if (sustraido.Contains(inicial.min)&&sustraido.Contains(inicial.max)){
                //en este caso, la sutraccion abarca y contiene a todo el rect inicial
                return resultado;
            }
            if (inicial.xMin < sustraido.xMin)
            {
                resultado.Add( Rect.MinMaxRect(inicial.xMin,inicial.yMin,sustraido.xMin,inicial.yMax) );
            }
            if(inicial.xMax > sustraido.xMax)
            {
                resultado.Add( Rect.MinMaxRect(sustraido.xMax,inicial.yMin,inicial.xMax,inicial.yMax) );
            }
            inicial .xMin = Mathf.Max(inicial.xMin,sustraido.xMin);
            inicial .xMax = Mathf.Min(inicial.xMax,sustraido.xMax);
            if (inicial.yMin < sustraido.yMin)
            {
                resultado.Add( Rect.MinMaxRect(inicial.xMin,inicial.yMin,inicial.xMax,sustraido.yMin) );
            }
            if(inicial.yMax > sustraido.yMax)
            {
                resultado.Add( Rect.MinMaxRect(inicial.xMin,sustraido.yMax,inicial.xMax,inicial.yMax) );
            }
        }
        else
        {// NO HAY OVERLAP
            resultado.Add(inicial);
        }
        return resultado;
    }
}
