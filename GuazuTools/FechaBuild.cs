using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class FechaBuild : MonoBehaviour {
#if UNITY_EDITOR
    public string Text
    {
        get
        {
            if(!tui)tui= GetComponent<Text>();
            if (!tui) tm = GetComponent<TextMesh>();
            if (tui) return tui.text;
            else if (tm) return tm.text;
            else return null;
        }
        set
        {
            if (!tui) tui = GetComponent<Text>();
            else if (!tui) tm = GetComponent<TextMesh>();
            if (tui)
            {
                tui.text = value;
            }
            else if (tm)
            {
                tm.text = value;
            }
        }
    }
    TextMesh tm;
    Text tui;

    private void OnValidate()
    {
        if (BuildPipeline.isBuildingPlayer) Text = System.DateTime.Now.ToString();
    }
#endif
}
