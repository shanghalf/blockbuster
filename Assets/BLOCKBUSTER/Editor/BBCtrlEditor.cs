﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEditor;
//using BlockbusterControll;






/// <summary>
/// to place the relevant data for serialization 
/// Like Childrens buffer for this configuration 
/// </summary>
[System.Serializable]
public class BBCtrlEditorLayer
{



}


public class BBCtrlEditor : EditorWindow
{
    [MenuItem("BlockBuster/BBControllEditor")]
    public static void init()
    {
        EditorWindow E = EditorWindow.GetWindow<BBCtrlEditor>();
        MethodInfo[] MethodInfoList = typeof(BBCtrlNode).GetMethods();
        foreach (MethodInfo i in MethodInfoList)
            if (i.Name == "onhwcalback")
            {
                MethodBody B=  i.GetMethodBody();
            }
        // 2 TIMERS FOR EDITOR PURPOSES 
        BBCtrlEditortimerList.Clear();
        BBCtrlEditortimerList.Add("T1", new EditorTimer());
        BBCtrlEditortimerList.Add("T2", new EditorTimer());
        BBCtrlEditortimerList["T2"].StartCountdown(1.0f);
        // ROOT IS THE FIRST AND MANDATORY NODE FOR THE VIEW 
        BBCtrlNode.THEGRAPH.ROOTNODE = new BBCtrlNode(BBCtrlNode.ROOTPOS, "ROOT");
        BBCtrlNode.THEGRAPH.ROOTNODE.isroot = true;
        BBCtrlNode.THEGRAPH.ROOTNODE.name = "ROOT";
        BBCtrlNode.THEGRAPH.ROOTNODE.isroot = true;
        // init BBCTRL 
        // since this view is linked to Movepad ( BBCTRL is the movepad ) 
        BBMovepad.Init();
        if (NodeGraph.autoload)
        {
            if (System.IO.File.Exists(NodeGraph.autosavefilename))
                BBCtrlNode.THEGRAPH.Load(NodeGraph.autosavefilename);
        }
        
    }
    // the ofset from window title bar to drawing area 
    int WINDOWHEADOFFSET = 20;
    // list of timers used in the editor view 
    public static Dictionary<string,EditorTimer> BBCtrlEditortimerList = new Dictionary<string,EditorTimer>();
    public static bool BBCTRLEditorFocused = false ;
    public static bool showdebuginfo;
    public static BBCtrlNode RootNode;


    public static BBMovepadControll MovepadButtonEdited;

    /// <summary>
    ///  check if a type can be converted 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    [BBCtrlVisible] // define a function visible for BBControl 
    public static bool CanChangeType(object value, Type conversionType)
    {
        if (conversionType == null)
            return false;
        if (value == null)
            return false;
        IConvertible convertible = value as IConvertible;
        if (convertible == null)
            return false;
        return true;
    }
    

    /// <summary>
    /// display a linear indexed grid on top of a rectangle 
    /// based on layer definition 
    /// this is used for debug purposes 
    /// </summary>
    /// <param name="layername"></param>
    /// <param name="pos"></param>
    /// <param name="linear"></param>
    public void ShowMovePadGrid(BBMovepadLayerDescriptor layer, Vector2 pos, bool linear)
    {
        // call inside a gui event draw 
        for (int ic = 0; ic < Math.Pow(BBMovepad.MVPGSZ, 2); ic++)
        {
            int index = 0;
            switch (linear)
            {
                case true:
                    index = ic;
                    break;
                case false:
                    Vector2 mpos = Event.current.mousePosition - BBMovepad.mvpd_rect.position;
                    Color32 C = layer.TEXTURES[(int)TXTINDEX.NORMAL].GetPixel((int)mpos.x, BBMovepad.MVPGSZ - (int)mpos.y);
                    index = C.r;
                    break;
            }
            int[] I = BBMovepad.CalcRectFromIndex(ic);  //Rect(px, py, bsz, bsz);
            Rect NR = new Rect(I[0], I[1], I[2], I[3]);
            NR.position += pos;
            GUI.TextField(NR, (index).ToString(), BBMovepad.BBGuiStyle );
        }
    }


    /// <summary>
    /// Display the Texture Target of the BBCTRL layer 
    /// this is a window function to place in ONGUI Begin / end Window loop
    /// </summary>
    /// <param name="id"></param>
    void DoBBCtrlWindow(int id)
    {
        Rect R = new Rect(0, WINDOWHEADOFFSET, BBMovepad.mvpd_rect.width, BBMovepad.mvpd_rect.height );
        GUI.DrawTexture( R , BBMovepad.Mainlayer.TEXTURES[(int)TXTINDEX.TARGET]); // draw the target 
        //ShowMovePadGrid("bbmain", new Vector2(0, WINDOWHEADOFFSET), true);
        GUI.DragWindow();
    }

    /// <summary>
    ///  Repaint on external event 
    /// </summary>
    void OnInspectorUpdate()
    {
        //***********************************************************************************
        // update inspector sheet according to the tool value 
        Repaint();
        return;
    }

    /// <summary>
    /// debug grid 
    /// </summary>
    void DrawGrid ()
    {
        Rect SCR = new Rect(0, 0, Screen.width, Screen.height);
        if (BBCtrlNode.THEGRAPH.ROOTNODE == null)
            init();
        List<BBCtrlNode> L = new List<BBCtrlNode>();
        Vector2 NodeSize = new Vector2(BBCtrlNode.THEGRAPH.ROOTNODE.Windowpos.width, BBCtrlNode.THEGRAPH.ROOTNODE.Windowpos.height);
        BBDrawing.BBDoGridLayout( SCR, NodeSize);
    }

  

    /// <summary>
    /// that the main purpose of the Editor 
    /// call and display node hierarchy 
    /// </summary>
    void OnGUI()
    {
        BBCtrlNode.hierarchy = "";
        if (BBCtrlNode.dirty)
        {
            BBCtrlNode.THEGRAPH.FlushBuffer();
            BBCtrlNode.THEGRAPH.ROOTNODE.REcusiveCollectNodes();
            BBCtrlNode.dirty = false;
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("APPLY"))
        {
           BBCtrlNode.THEGRAPH.Save();
           EditorWindow E = EditorWindow.GetWindow<BBCtrlEditor>();
           E.Close();
        }
        if (GUILayout.Button("SAVE"))
        {
            BBCtrlNode.THEGRAPH.Save(true);
        }

        if (GUILayout.Button("LOAD"))
        {
            string path ;
            if (BBMovepadLayerDescriptor.autoload)
                path = BBDir.Get(BBpath.SETING) + BBCtrlNode.THEGRAPH.Guid.ToString() + ".xml";
            else path = null;
            BBCtrlNode.THEGRAPH.ForceLoad();
    
        }


    
    


        GUILayout.EndHorizontal();


        BBDrawing.CheckInput();  // update inputs 



        // perform the layout routine 
        DrawGrid();
   


        // set node focus 
       

        BBMovepadLayerDescriptor.autoload = GUI.Toggle(new Rect(10, Screen.height -  200, 100, 20), BBMovepadLayerDescriptor.autoload, "autoload");


        if (BBCtrlNode.scrolllock = GUI.Toggle(new Rect(10, Screen.height - 150, 100, 20), BBCtrlNode.scrolllock, "scroll lock"))
        if (BBCtrlNode.editordebugmode = GUI.Toggle(new Rect(10, 200, 100, 20), BBCtrlNode.editordebugmode, "Sliders"))
        {
            BBCtrlNode.debugfloat1 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat1, 0f, 1f);
            BBCtrlNode.debugfloat2 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat2, 0f, 10f);
            BBCtrlNode.debugfloat3 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat3, 0f, 100f);
            BBCtrlNode.debugfloat4 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat4, 0f, 1000f);
        }

        BBCtrlNode.unfiltered = GUI.Toggle(new Rect(10, Screen.height - 100, 100, 20), BBCtrlNode.unfiltered, "show all method");

        String str = "";

        // get all nodes from root 


        foreach (BBCtrlNode n in BBCtrlNode.THEGRAPH.Nodes)
        {
            str += n.name + "\n";
            str += "velocity" + n.velocity.ToString() + "\n";
            //GUI.Box(n.Windowpos, "");
            

        }
        str += "\n\n\n\n";
        GUILayout.Label(str);
        BeginWindows(); // ---------------------------------------------------------------------------------------------- START WINDOWS LOOP

        GUILayout.Label(BBCtrlNode.hierarchy);


        BBCtrlNode.THEGRAPH.ROOTNODE.DoNode();

        EndWindows();

        

        Repaint();
        BBCtrlNode.NodeDebuginfos = "";



    }
}



//////////////////////////////////////////////////////////////////////////////////////// END











/*
zoom = EditorGUILayout.Slider("zoom", zoom, 0.5f, 2.0f);
Matrix4x4 zoommatrix =  new  Matrix4x4();
zoommatrix.SetTRS(Vector2.zero, Quaternion.identity, Vector3.one * zoom);
Color errorlinkcolor = new Color(0.4f, 0.4f, 0.5f);

Rect slot ;
int decal_y = 0;
//scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
BeginWindows();

Vector2 sz = new Vector2(save.width, 0);
float width =  zoommatrix.MultiplyVector(sz).magnitude;
sz = new Vector2(save.height, 0);
float height = zoommatrix.MultiplyVector(sz).magnitude;

FunctionInspectorWindowRect= GUI.Window(WFUNCID,FunctionInspectorWindowRect , doFunctionInspectorWindow, "FUNCTION");
FunctionInspectorWindowRect.width = width;
FunctionInspectorWindowRect.height = height;


        
        
BBctrlWindowRect = GUI.Window(BBCTRLID, BBctrlWindowRect, DoBBCtrlWindow, "UI");
slot = new Rect(FunctionInspectorWindowRect.width + FunctionInspectorWindowRect.position.x, FunctionInspectorWindowRect.position.y + 30, 16, 16);
GUI.Box(slot, "");

        

//GUI.matrix =  orgmatrix; 

curveFromTo(slot, BBctrlWindowRect, new Color(0.3f, 0.7f, 0.4f), errorlinkcolor , zoommatrix);

//GUI.matrix = zoommatrix; 
     
 * 
 * 
 * 
 * 
 *           /* to investigate a better curve draw ? 
                List<Keyframe>  Key = new List<Keyframe>();
                Keyframe K = new Keyframe(0, 0.5f, 1.5f, 0.5f) ;
                Key.Add ( K) ;
                Keyframe K1 = new Keyframe(1, -0.5f, -0.2f,-0.2f);
                Key.Add(K1);
                AnimationCurve Crv = new AnimationCurve( Key.ToArray());
                Rect C = new Rect(node.Windowpos.x + node.Windowpos.width, node.Windowpos.y,ROOT.Windowpos.x -  ( node.Windowpos.x + node.Windowpos.width)    , ROOT.Windowpos.yMax - node.Windowpos.y);
                
                    

                //EditorGUIUtility.DrawCurveSwatch(C, Crv, null, Color.gray, Color.clear);
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
foreach (BBCtrleditorNode pbox in paramlist)
{
    if (pbox.Paraminfo == null)
        continue;

    decal_y  += 30 ; // ofset y for slots
    slot = new Rect( FunctionInspectorWindowRect.position.x - 16 , FunctionInspectorWindowRect.position.y + decal_y , 16 ,16) ;
    GUI.Box(slot, "");
    // animation effect 


    if (BBCtrlEditortimerList["T1"].run)
    {
        float ymove = pbox.Windowpos.position.y - (BBCtrlEditortimerList["T1"].Update(false) * pbox.Windowpos.position.y);
        Rect moveposrect = new Rect(pbox.Windowpos.position.x, ymove, pbox.Windowpos.width, pbox.Windowpos.height);
        GUI.Box(moveposrect, "");
    }
    else
    {
        float fade = BBCtrlEditortimerList["T2"].Update(true);
        pbox.Windowpos = GUI.Window(pbox.windowid, pbox.Windowpos, pbox.DoNodeWindow, pbox.Paraminfo.ParameterType + " " + pbox.name);
        Color linkblinkcolor = new Color(0.5f,0.5f,0.5f);
        if (pbox.ReturnTye == typeof (int) )
            linkblinkcolor.g =  (1.0f );
        else
            linkblinkcolor.r =  (1.0f * fade);

        //GUI.matrix = orgmatrix; 
        curveFromTo(pbox.Windowpos, slot, linkblinkcolor , errorlinkcolor * fade ,zoommatrix);
        //GUI.matrix = zoommatrix; 
    }
        
}
// Debug.Log(paramlist.Count.ToString());

EndWindows();
//GUILayout.EndScrollView();
*/

/*
void doFunctionInspectorWindow(int id)
{
    // after a build or repopen window 
    //List<BBCtrleditoParameterbox> paramlist = new List<BBCtrleditoParameterbox>();
    if (BBCtrlEditortimerList["T1"] == null)
        init();
    if (Selection.activeGameObject == null)
    {   // exit and clear param list ( to kill prev windows ) 
        GUI.TextField(new Rect(0, WINDOWHEADOFFSET, FunctionInspectorWindowRect.width , FunctionInspectorWindowRect.height - WINDOWHEADOFFSET), "Select an object To inspect ");
        paramlist.Clear();
        GUI.DragWindow();
        return;
    }
    // generic list to reuse for popup 
    List<string> genericstringlist = new List<string>();

    // get list of monobehaviour on the selected object 
    MonoBehaviour[] scripts = Selection.activeGameObject.GetComponents<MonoBehaviour>();


    List<System.Type> TLIST = new List<Type>();

    //fill up list of monobhv and a type list 
    foreach (MonoBehaviour o in scripts)
    {
        genericstringlist.Add(o.GetType().Name);
        TLIST.Add(o.GetType());
    }
    // store ispected class index 
    LookupClassindex = EditorGUILayout.Popup(LookupClassindex, genericstringlist.ToArray());

    // to check if user change selection 
    bool classchanged = false;
    bool methodchanged = false;


    if (lookupclassname != genericstringlist[LookupClassindex])
    {
        lookupclassname = genericstringlist[LookupClassindex];
        classchanged = true; // to prevent reinitialisation in loop of parameterbox
        methodchanged = true; // method could not be the same 
        userindex = 0;
        Debug.Log("CLASS CHANGED ");
    }

    // get the methods of the selected Class Monobehaviour 
    MethodInfo[] MethodInfoList = TLIST[LookupClassindex].GetMethods();
    genericstringlist.Clear();

    // we want to list only methods standing under the custom attribute BBCtrlVisible
    // and store the index in a dictionary 
    Dictionary<string,int> Methodindexdic = new Dictionary<string,int>();
    for (int mc = 0; mc < MethodInfoList.GetLength(0); mc++ )
    {
        object[] atributelist = MethodInfoList[mc].GetCustomAttributes(true);
        foreach (object o in atributelist)
            if (o.GetType() == typeof(BBCtrlVisible))
            {
                Methodindexdic.Add(MethodInfoList[mc].Name, mc); // store the index on the name 
                genericstringlist.Add(MethodInfoList[mc].Name);  // for the list that would be wrong otherwise
            }
    }
    // useless to go further if no methods are visible 
    if (Methodindexdic.Count == 0)
    {
        paramlist.Clear();
        return;
    }
    MethodInfo selectedmethod; // the method selected in combobox 

    // get the user selection index in the drop down list
    userindex = EditorGUILayout.Popup(userindex, genericstringlist.ToArray());
        
    // and the index in the full method table 
    // but init first to stored index to spot user changes 
    int methodindexfromdic = LookupMethodindex; 
    // check on the method name in dic 
    if (!Methodindexdic.TryGetValue(genericstringlist[userindex], out methodindexfromdic))
    {
        Debug.Log("no entry in methodindexfromdic");
        return;
    }

    // user made a change in ddlist 
    if (LookupMethodindex != methodindexfromdic)
    {
        LookupMethodindex = methodindexfromdic;
        methodchanged = true;
    }
    // store the name 
    LookupMethodName = MethodInfoList[LookupMethodindex].Name;
    // get the appropriated method info 
    selectedmethod = MethodInfoList[LookupMethodindex];

    // ready to parse Args for this method 

    System.Reflection.ParameterInfo[] argstypes = selectedmethod.GetParameters();


    genericstringlist.Clear();
    foreach (ParameterInfo argTypepinfo in argstypes)
        genericstringlist.Add(argTypepinfo.ParameterType.Name);

    //int argnnb = 0; 
    //EditorGUILayout.Popup(argnnb, genericstringlist.ToArray());

     
    if (methodchanged || classchanged)
    {
        paramlist.Clear();
        idcount = 10; // base index for window ID 
        //************************************************
        BBCtrlEditortimerList["T1"].StartCountdown(0.5f);
        EditorTimer T = new EditorTimer();
        T.StartCountdown(0.8f);
        //************************************************
            

        //foreach (string str in genericstringlist)
        foreach (ParameterInfo pi in argstypes)
        {
            BBCtrleditorNode newparam = new BBCtrleditorNode();
            newparam.T = pi.GetType();
            newparam.name =   pi.Name;
            newparam.Paraminfo = pi; 
            int yof = (Screen.height / argstypes.GetLength(0)) * (idcount - 10) + 20; // base to count number of params 
            idcount++;
            newparam.windowid = idcount;
            newparam.Windowpos = new Rect(20, yof, 200, 100);
            paramlist.Add(newparam); // add the window 

        }
            

        if (GUILayout.Button("invoke"))
        {
            Debug.Log("class " + lookupclassname + " method " + LookupMethodName);

        }
    }

   



    GUI.DragWindow();


}

*/