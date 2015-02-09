using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using UnityEditor;
using System.Xml;


public class splineeditor : EditorWindow
{



    [MenuItem("Window/splines")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(splineeditor));
    }


	// Use this for initialization
	void Start () 
    
    {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
