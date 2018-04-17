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
}
