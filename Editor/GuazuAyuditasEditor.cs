using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GuazuAyuditasEditor {    
    [MenuItem("CONTEXT/PolygonCollider2D/Espejar")]
    public static void EspejarPolyColl2D(MenuCommand command)
    {
        PolygonCollider2D collider = (PolygonCollider2D)command.context;
        AcomodarPolyColl2D(collider, Guazu.MirrorVector);
    }
    [MenuItem("CONTEXT/PolygonCollider2D/Voltear")]
    public static void VoltearPolyColl2D(MenuCommand command)
    {
        PolygonCollider2D collider = (PolygonCollider2D)command.context;
        AcomodarPolyColl2D(collider, Guazu.FlipVector);
    }
    static void AcomodarPolyColl2D(PolygonCollider2D col, Vector2 modif)
    {
        Undo.RecordObject(col, "Espejar o Voltear Poly Collider");
        for (int i = 0; i < col.pathCount; i++)
        {
            Vector2[] path = col.GetPath(i);
            for (int p = 0; p < path.Length; p++)
            {
                path[p] = Vector2.Scale(path[p], modif);
            }
            col.SetPath(i, path);
        }
    }
}
