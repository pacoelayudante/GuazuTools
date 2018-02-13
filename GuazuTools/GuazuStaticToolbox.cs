using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Guazu {

    public static Quaternion QuaternionRandom2D{
        get{
            return Quaternion.Euler(0, 0, Random.value * 360f);
        }
        }

    public static Quaternion QuaternionEuler(Vector2 vec2)
    {
        return Quaternion.Euler( 0, 0, Mathf.Atan2( vec2.y, vec2.x )*Mathf.Rad2Deg );
    }

    public static Color BlancoClear
    {
        get { return new Color(1f, 1f, 1f, 0f); }
    }

    public static Color ColorClear(Color c)
    {
        c.a = 0f;
        return c;
    }
}
