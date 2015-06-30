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
/// THIS is the NodeEditor Window 
/// the editorwindow dont do a lot of thing 
/// except the Node Display loop and window managememt 
/// </summary>
public class BBCtrlEditor : EditorWindow
{

    // the ofset from window title bar to drawing area 
    int WINDOWHEADOFFSET = 20; 
    // list of timers used in the editor view 
    public static Dictionary<string, EditorTimer> BBCtrlEditortimerList = new Dictionary<string, EditorTimer>();


    // the node editor show up when the following menu item is selected 
    // the init sequence set 2 timers mainly used for ghost popup effect on node creation 
    // and another running permanently for spring effect on the link 
    // those effects are useless but bring a little bit of fun in the final look of the tool 
    // why so serious ? 
    [MenuItem("BlockBuster/BBControllEditor")]
    public static void init()
    {
        EditorWindow E = EditorWindow.GetWindow<BBCtrlEditor>();
        MethodInfo[] MethodInfoList = typeof(BBCtrlNode).GetMethods();
        // 2 TIMERS FOR EDITOR PURPOSES 
        BBCtrlEditortimerList.Clear();
        BBCtrlEditortimerList.Add("T1", new EditorTimer());
        BBCtrlEditortimerList.Add("T2", new EditorTimer());
        BBCtrlEditortimerList["T2"].StartCountdown(1.0f);

        // dont remember exactly why the movepad is initialized here ? 
        // but probably for the case where the editor is open and the 
        // mainwindow closed since it need some value from movepad .. i ll check later 
        BBMovepad.Init();

        // static instance of BBControll NodeGraph.EditedControll is the Controll used to hold 
        // the Graph actually edited 
        // if the controll allready have a graph associated we load it right now 
        if (System.IO.File.Exists(NodeGraph.EditedControll.Graphfilename))
            NodeGraph.LoadGraph(NodeGraph.EditedControll,null);
        
    }





    /// <summary>
    /// unity callback when window is opened or closed 
    /// the editor open automatically in some case ..hum most case 
    /// editoropen need to be checked before otherwise the editor will blink 
    /// continuously and all init redo as well 
    /// </summary>
    void OnEnable()
    {
        NodeGraph.editoropen = true;
    }
    /// <summary>
    /// same thing 
    /// </summary>
    void OnDisable ()
    {
        NodeGraph.editoropen = false;
    }

    
    /// <summary>
    ///  Repaint on external event 
    /// </summary>
    void OnInspectorUpdate()
    {
        Repaint();
        return;
    }
    

    /// <summary>
    /// Do the Editor Node Layout 
    /// draw a grid in debug mode 
    /// place the nodes etc ..
    /// </summary>
    void DoEditorLayout ()
    {
        Rect SCR = new Rect(0, 0, Screen.width, Screen.height);
        if (NodeGraph.EditedControll.thisgraph.ROOTNODE == null)
        {

            return;
            

        }
        int NodeSize = (int) NodeGraph.EditedControll.thisgraph.ROOTNODE.Windowpos.width;
        BBDrawing.BBDoGridLayout( SCR, NodeSize);
    }
 

    void OnUpdate()
    {
        BBDrawing.CheckInput();
    }

    /// <summary>
    /// the gui loop of the editor node 
    /// </summary>
    void OnGUI()
    {
        System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();

        if (BBCtrlNode.editordebugmode)
            perf.Start();


        BBDrawing.CheckInput();
        // for debug output 
        BBCtrlNode.hierarchy = "";

        // graph get dirty when the node hierarchy has been changed (node removed added or graph loaded )
        // then the node tree need to be parsed again to reflect the changes for optim reason its better to do as less as possible 
        if (BBCtrlNode.dirty)
        {
            BBCtrlNode.dirty = false;
            // singlewarning is a convenient function that memorize a message allready output for debug 
            // and popup only if it s a new message , convenient to get log feedback for debug without flooding the console
            BBDebugLog.singleWarning("flush buffer et iterate graph on " + NodeGraph.EditedControll.guid.GetHashCode().ToString());
            // flush graph buffer and collect new infos
            NodeGraph.EditedControll.thisgraph.FlushBuffer();
            NodeGraph.EditedControll.thisgraph.ROOTNODE.REcusiveCollectNodes();
            NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.REcusiveCollectNodes();
        }
        GUILayout.BeginHorizontal();
            
            // Save The BBControll Graph and close node editor 
            // the name used to save is GUI of the BBcontroll file name association is done that way 
            // and work quite well without any user inputs 
            if (GUILayout.Button("APPLY"))
            {
                BBDebugLog.singleWarning("save graph on " + NodeGraph.EditedControll.guid.GetHashCode().ToString()  ); 
                NodeGraph.EditedControll.thisgraph.Save();
                EditorWindow E = EditorWindow.GetWindow<BBCtrlEditor>();
                E.Close();
            }
            // save also the graph but with an xml format and arbitrary name (true parameter indicate to open file save dialog )
            // the purpose of this way to save is to reuse a graph on other controls , the load dialog display only named XML files
            // everything else is GUI based 
            if (GUILayout.Button("SAVE"))
            {
                NodeGraph.EditedControll.thisgraph.Save(true);
            }
            // Load a graph in node editor 
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
        // update user inputs
        //BBDrawing.CheckInput();   
        // perform the layout routine 
        if (NodeGraph.EditedControll.thisgraph != null)
            DoEditorLayout();
        if (BBCtrlNode.showgrid = GUI.Toggle(new Rect(10, Screen.height - 150, 100, 20), BBCtrlNode.showgrid, "show grid"))
        BBCtrlNode.unfiltered = GUI.Toggle(new Rect(10, Screen.height - 100, 100, 20), BBCtrlNode.unfiltered, "show all method");
        BBCtrlNode.autopos = GUI.Toggle(new Rect(10, Screen.height - 200, 100, 20), BBCtrlNode.autopos, "Autopos");
        BBCtrlNode.editordebugmode = GUI.Toggle(new Rect(10, Screen.height - 250, 100, 20), BBCtrlNode.editordebugmode, "edit perfs");


        if (NodeGraph.EditedControll.thisgraph != null)
          foreach (BBCtrlNode n in NodeGraph.EditedControll.thisgraph.Nodes)
              if (!n.isemiterroot && !n.isroot && n.velocity.magnitude > 0.1f && !n.m_gotfocus )
              { 
                n.Windowpos.position -= n.velocity * ( NodeGraph.EditedControll.thisgraph.Nodes.Count/2 * n.RandomSeed  ) ; // speed multiplicator more node more speed to compensate 

              }

            //if (n.Windowpos.y < NodeGraph.EditedControll.thisgraph.GetnodeFromID(n.ParentID).SUBNodes[n.ParentFeedSlotInfo.index - 1].Windowpos.y + n.Windowpos.width && !n.isroot)
            //  n.Windowpos.y += 1;
        
        
        //str += "\n\n\n\n";
        //GUILayout.Label(str);
        BeginWindows(); // ---------------------------------------------------------------------------------------------- START WINDOWS LOOP
        // the node graph display is maily the recursive execution of the node tree ... right here ...
        GUILayout.Label(BBCtrlNode.hierarchy);
        if (NodeGraph.EditedControll.thisgraph != null)
        {
            if (NodeGraph.EditedControll.thisgraph.ROOTNODE != null)
                NodeGraph.EditedControll.thisgraph.ROOTNODE.DoNode();
            if (NodeGraph.EditedControll.thisgraph.EMITERROOTNODE != null)
                NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.DoNode();
        }



        /*
            foreach ( BBCtrlNode bbc in NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodes )
                bbc.DoNode();
        */

            // keep display emiters 
            //foreach (int i in NodeGraph.EditedControll.thisgraph.EmiterIDlist)
            //{
            //    BBCtrlNode bbc = NodeGraph.EditedControll.thisgraph.GetnodeFromID(i);
            //    if (bbc != null)
            //        bbc.DoNode();
            //    else // emiter node do not exist anymore 
            //        NodeGraph.EditedControll.thisgraph.EmiterIDlist.Remove(i);

            //}


            // display ghost nodes not flushed from graph desc (debug function)
        EndWindows();
        Repaint();

        if (BBCtrlNode.editordebugmode)
        {
            if (BBCtrlNode.showgrid)
                foreach (BBCtrlNode bbc in NodeGraph.EditedControll.thisgraph.Nodes)
                    GUI.Box(bbc.Windowpos, "");

            perf.Stop();
            TimeSpan nts = perf.Elapsed;
            String GraphPerf = String.Format("perf :" + ((float)nts.TotalMilliseconds / 100).ToString());
            GUILayout.Label(GraphPerf);
        }


    }
}


   
