using UnityEngine;
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
    static void init()
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
        BBCtrlNode.ROOT = new BBCtrlNode(BBCtrlNode.ROOTPOS);
        BBCtrlNode.ROOT.isroot = true;
 
        //BBCtrleditorNode.ROOT.CutBranch();
        
        BBCtrlNode.ROOT.Childrens.Clear();

        

        BBCtrlNode.ROOT.name = "ROOT";
        BBCtrlNode.ROOT.isroot = true;
        BBCtrlNode.ROOT.windowid = -1;


        // init BBCTRL 
        // since this view is linked to Movepad ( BBCTRL is the movepad ) 
        BBCtrl.Init();
    }
    // the ofset from window title bar to drawing area 
    int WINDOWHEADOFFSET = 20;

    // list of timers used in the editor view 
    public static Dictionary<string,EditorTimer> BBCtrlEditortimerList = new Dictionary<string,EditorTimer>();
    public static bool BBCTRLEditorFocused = false ;
    public static bool showdebuginfo;

  



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
    public void ShowMovePadGrid(string layername, Vector2 pos, bool linear)
    {
        // call inside a gui event draw 
        for (int ic = 0; ic < Math.Pow(BBCtrl.MVPGSZ, 2); ic++)
        {
            int index = 0;
            switch (linear)
            {
                case true:
                    index = ic;
                    break;
                case false:
                    Vector2 mpos = Event.current.mousePosition - BBCtrl.mvpd_rect.position;
                    Texture2D T = BBCtrl.GetTextureFromLayer(layername, TXTINDEX.NORMAL);
                    Color32 C = T.GetPixel((int)mpos.x, BBCtrl.MVPGSZ - (int)mpos.y);
                    index = C.r;
                    break;
            }
            int[] I = BBCtrl.CalcRectFromIndex(ic);  //Rect(px, py, bsz, bsz);
            Rect NR = new Rect(I[0], I[1], I[2], I[3]);
            NR.position += pos;
            GUI.TextField(NR, (index).ToString(), BBCtrl.BBGuiStyle );
        }
    }


    /// <summary>
    /// Display the Texture Target of the BBCTRL layer 
    /// this is a window function to place in ONGUI Begin / end Window loop
    /// </summary>
    /// <param name="id"></param>
    void DoBBCtrlWindow(int id)
    {
        Rect R = new Rect(0, WINDOWHEADOFFSET, BBCtrl.mvpd_rect.width, BBCtrl.mvpd_rect.height );
        Texture2D T = BBCtrl.GetTextureFromLayer("bbmain", TXTINDEX.TARGET);
        GUI.DrawTexture( R , T); // draw the target 
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


    void DisplayPlacementGrid ()
    {
        Rect SCR = new Rect(0, 0, Screen.width, Screen.height);

        if (BBCtrlNode.ROOT == null)
            init();
        List<BBCtrlNode> L = new List<BBCtrlNode>();

        Vector2 NodeSize = new Vector2(BBCtrlNode.ROOT.Windowpos.width, BBCtrlNode.ROOT.Windowpos.height);
        BBDrawing.BBDoGridLayout( SCR, NodeSize);


    }

  

    /// <summary>
    /// that the main purpose of the Editor 
    /// call and display node hierarchy 
    /// </summary>
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SAVE"))
        {
           BBCtrlNode.THEGRAPH.Save();
        }
        if (GUILayout.Button("LOAD"))
        {
            string path = BBDir.Get(BBpath.SETING) + BBCtrlNode.THEGRAPH.Guid.ToString() + ".xml";
            BBCtrlNode.THEGRAPH = NodeGraph.Load(path);
        }
        GUILayout.EndHorizontal();

        float fade = 0.8f;
        // for link blink 
        EditorTimer T;
        if ( BBCtrlEditortimerList.TryGetValue("T2",out T)) 
            fade = BBCtrlEditortimerList["T2"].Update(true);

        BBDrawing.CheckInput();  // update inputs 
        BBCtrlNode root = BBCtrlNode.ROOT;


        DisplayPlacementGrid();
        Repaint();


        // set node focus 
        root.gotfocus = BBDrawing.GetRectFocus(root.Windowpos,true);


        if (BBCtrlNode.scrolllock = GUI.Toggle(new Rect(10, Screen.height - 150, 100, 20), BBCtrlNode.scrolllock, "scroll lock"))
        if (BBCtrlNode.editordebugmode = GUI.Toggle(new Rect(10, 200, 100, 20), BBCtrlNode.editordebugmode, "Sliders"))
        {
            BBCtrlNode.debugfloat1 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat1, 0f, 1f);
            BBCtrlNode.debugfloat2 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat2, 0f, 10f);
            BBCtrlNode.debugfloat3 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat3, 0f, 100f);
            BBCtrlNode.debugfloat4 = GUILayout.HorizontalSlider(BBCtrlNode.debugfloat4, 0f, 1000f);
        }

        BBCtrlNode.angrynodes = GUI.Toggle(new Rect(10, Screen.height - 100, 100, 20), BBCtrlNode.angrynodes, "Angry Nodes");

        String str = "";

        foreach (BBCtrlNode n in root.Childrens.Values)
        {
            str += n.name + "\n";
            str += "velocity" + n.velocity.ToString() + "\n";

        }
        str += "\n\n\n\n";

       




        GUILayout.Label(str);

        
        BeginWindows(); // ---------------------------------------------------------------------------------------------- START WINDOWS LOOP
        
        // a problem in the root initialization 
        // useless to keep going further 
        if (root == null)
        {
            Debug.Log("ROOT HAS NOT BEEN INITIALIZED SOMETHING WENT CRAZY HERE !! ");
            return;
        }

        

        // DO THE ROOTINE .... ( at least .. )
        root.Windowpos = GUI.Window(root.windowid, root.Windowpos, root.DoNodeWindow, root.windowid.ToString());
        root.DoNode();
     
        // process all Nodes 
        foreach (BBCtrlNode node  in root.Childrens.Values )
        {
            // check conditions to cancel this node 
            if (node.windowid == 0 || node.ParentFeedSlotInfo == null || node.Parent.slotspos.Count <= node.ParentFeedSlotInfo.index)
            {
                
                if (! node.isroot )
                //node.CutBranch();
                continue;
            }
            // this is for the animation ( useless but lovely ) 
            if (node.Timer.run)
            {
                // node timer is running the node is not ready to display 
                node.deploychildren(node.Rectorg, node.RectDest);
                GUI.Box(node.RectCurrent, "");
            }
            else
            {
                // Do the Node Window 
                node.Windowpos = GUI.Window(node.windowid, node.Windowpos, node.DoNodeWindow, node.name);
                // and blink link that not fit conditions 
                // here node output Type != parent input 

                bool canchange = CanChangeType(node.ReturnTye, node.Parent.slotspos[node.ParentFeedSlotInfo.index].T);
                bool entryfit =(node.ReturnTye == node.Parent.slotspos[node.ParentFeedSlotInfo.index].T);




                if (canchange || entryfit )    
                    fade = 0.8f;
                BBDrawing.curveFromTo(node.outputslotbutton, node.Parent.slotspos[node.ParentFeedSlotInfo.index].R, new Color(0.2f, fade * 0.8f, 0.2f ),node.Windowpos.width , out node.velocity );

                

            }
            // the node stuff that might be done out of the window ( slot etc )
            // the link draw should be done also there as soon as i redo the spline draw 
            // for something faster and more accurate 
            node.DoNode();

            
            foreach (BBCtrlNode BBC in node.Childrens.Values)
                GUI.Box(BBC.Windowpos, "");


      
        }
        EndWindows();
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