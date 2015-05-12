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
///  The editor window and prety much all the conection with unity 
///  everything else is mainly easy to split 
///  just removing the dep with the Blockbuster Game and goddamned GUI 
///  that bring more problem than it fix 
/// </summary>
public class BBCtrlEditor : EditorWindow
{
    [MenuItem("BlockBuster/BBControllEditor")]
    public static void init()
    {
        EditorWindow E = EditorWindow.GetWindow<BBCtrlEditor>();
        MethodInfo[] MethodInfoList = typeof(BBCtrlNode).GetMethods();
        /*
        foreach (MethodInfo i in MethodInfoList)
            if (i.Name == "onhwcalback")
            {
                MethodBody B=  i.GetMethodBody();
            }*/

        // 2 TIMERS FOR EDITOR PURPOSES 
        BBCtrlEditortimerList.Clear();
        BBCtrlEditortimerList.Add("T1", new EditorTimer());
        BBCtrlEditortimerList.Add("T2", new EditorTimer());
        BBCtrlEditortimerList["T2"].StartCountdown(1.0f);
        // ROOT IS THE FIRST AND MANDATORY NODE FOR THE VIEW 

        /*
        if (NodeGraph.EditedControll.thisgraph.ROOTNODE == null)
        {
            NodeGraph.EditedControll.thisgraph.ROOTNODE = new BBCtrlNode(BBCtrlNode.ROOTPOS, "ROOT");
            NodeGraph.EditedControll.thisgraph.ROOTNODE.isroot = true;
            NodeGraph.EditedControll.thisgraph.ROOTNODE.name = "ROOT";
            NodeGraph.EditedControll.thisgraph.ROOTNODE.isroot = true;

        }*/
        
        // init BBCTRL 
        // since this view is linked to Movepad ( BBCTRL is the movepad ) 
        

        BBMovepad.Init();
        /*
        if (NodeGraph.autoload)
        {
            if (System.IO.File.Exists(NodeGraph.autofilename))
                GraphHandle.ActiveGraph = NodeGraph.LoadGraph(NodeGraph.autofilename);
        }*/
        
    }
    // the ofset from window title bar to drawing area 
    int WINDOWHEADOFFSET = 20;
    // list of timers used in the editor view 
    public static Dictionary<string,EditorTimer> BBCtrlEditortimerList = new Dictionary<string,EditorTimer>();
    public static bool BBCTRLEditorFocused = true;
    public static bool showdebuginfo;
    public static BBCtrlNode RootNode;
    public static BBMovepadControll MovepadButtonEdited;



    void OnEnable()
    {
        NodeGraph.editoropen = true;
    }

    void OnDisable ()
    {
        NodeGraph.editoropen = false;


    }


    /// <summary>
    ///  Repaint on external event 
    /// </summary>
    void OnInspectorUpdate()
    {
        // i believe my lock UI problem come from this repaint 
        // may be not the right place to put a repaint but since it 
        // should play in editor mode i need a update of unfocused window 
        Repaint();
        return;
    }
    

    /// <summary>
    /// debug grid 
    /// </summary>
    void DrawGrid ()
    {
        Rect SCR = new Rect(0, 0, Screen.width, Screen.height);
        if (NodeGraph.EditedControll.thisgraph.ROOTNODE == null)
            return;
        List<BBCtrlNode> L = new List<BBCtrlNode>();
        Vector2 NodeSize = new Vector2(NodeGraph.EditedControll.thisgraph.ROOTNODE.Windowpos.width, NodeGraph.EditedControll.thisgraph.ROOTNODE.Windowpos.height);
        BBDrawing.BBDoGridLayout( SCR, NodeSize);
    }
    /*
    void Update()
    {
        if (!BBControll.editgraph )
        EditorWindow.GetWindow<BBCtrlEditor>().Close();
    }
     */

    /// <summary>
    /// that the main purpose of the Editor 
    /// call and display node hierarchy 
    /// </summary>
    void OnGUI()
    {
        BBCtrlNode.hierarchy = "";
        if (BBCtrlNode.dirty)
        {
            BBCtrlNode.dirty = false;
            BBDebugLog.singleWarning("flush buffer et iterate graph on " + NodeGraph.EditedControll.guid.GetHashCode().ToString());
            NodeGraph.EditedControll.thisgraph.FlushBuffer();
            NodeGraph.EditedControll.thisgraph.ROOTNODE.REcusiveCollectNodes();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("APPLY"))
        {
            BBDebugLog.singleWarning("save graph on " + NodeGraph.EditedControll.guid.GetHashCode().ToString()  ); 
            NodeGraph.EditedControll.thisgraph.Save();
            EditorWindow E = EditorWindow.GetWindow<BBCtrlEditor>();
            E.Close();
        }
        if (GUILayout.Button("SAVE"))
        {
            NodeGraph.EditedControll.thisgraph.Save(true);
        }
        if (GUILayout.Button("LOAD"))
        {
            string path=null ;
            if (NodeGraph.EditedControll.thisgraph != null)
                path = BBDir.Get(BBpath.SETING) + NodeGraph.EditedControll.thisgraph.Guid.ToString() + ".xml";
            if (! File.Exists(path))
                NodeGraph.EditedControll.Graphfilename = EditorUtility.OpenFilePanel("open graph ", BBDir.Get(BBpath.SETING), "xml");
            NodeGraph.EditedControll.thisgraph = NodeGraph.LoadGraph(NodeGraph.EditedControll);
            return;
        }
        GUILayout.EndHorizontal();
        BBDrawing.CheckInput();  // update inputs 
        // perform the layout routine 
        if (NodeGraph.EditedControll.thisgraph != null)
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
        /*
        foreach (BBCtrlNode n in NodeGraph.EditedControll.thisgraph.Nodes)
        {
            str += n.name + "\n";
            str += "velocity" + n.velocity.ToString() + "\n";
            n.Windowpos.position -= n.velocity;
        }
        */
        str += "\n\n\n\n";
        GUILayout.Label(str);
        BeginWindows(); // ---------------------------------------------------------------------------------------------- START WINDOWS LOOP
        GUILayout.Label(BBCtrlNode.hierarchy);
        if (NodeGraph.EditedControll.thisgraph!= null)
            if (NodeGraph.EditedControll.thisgraph.ROOTNODE != null ) 
                NodeGraph.EditedControll.thisgraph.ROOTNODE.DoNode();
        EndWindows();
        Repaint();
        BBCtrlNode.Nodeinfos = "";
    }
}


   
