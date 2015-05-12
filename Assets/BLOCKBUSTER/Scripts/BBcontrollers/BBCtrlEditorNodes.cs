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
/// a class to store the slot informations and may be perform stuff on it 
/// ( later ) 
/// </summary>
[System.Serializable]
public class SlotInfo 

{
    public string paramname;
    public bool UIControllparamslot = false;
    public int index= 0 ;
    public int linkedto;
    public Rect R;
    public string TypeFullString;
    SlotInfo()
    {
    }
    public SlotInfo(int zid)
    {
        index = zid;
    }
}


// need a class to manage the editor handle on the active graph that could change often 
// depending on the context of graph use ( as active gameobject component or interface ) 




/// <summary>
/// the graphnode 
/// under a digest form for 
/// save and load layer is a step above and could 
/// manage multiple graph
/// </summary>
[System.Serializable]
[XmlInclude(typeof(Vector3))]
[XmlInclude(typeof(TXTINDEX))]
[XmlInclude(typeof(AnimationCurve))]
[XmlInclude(typeof(BBGameObjectHandle))]

public class NodeGraph
{
    public static bool gamemode = true;
    public static bool editoropen;
    public static BBControll EditedControll = new BBControll();

    public bool GraphOK = true;

    public string filename;

    public BBCtrlNode ROOTNODE;
    public List<BBCtrlNode> Nodes = new List<BBCtrlNode>();
    public List<int> nodekeys = new List<int>();
    public Guid Guid = Guid.NewGuid();
    public BBCtrlNode  GetnodeFromID (int ID)
    {
        foreach (BBCtrlNode BBC in Nodes)
            if (BBC.Guid.GetHashCode() == ID)
                return BBC;
        return null;
    }
    public  void FlushBuffer()
    {
        Nodes.Clear();
        nodekeys.Clear();
    }
    public void Save(bool saveas=false)
    {
        string path ;
        if (!saveas)
            EditedControll.Graphfilename = BBDir.Get(BBpath.SETING) + EditedControll.guid.ToString() + ".bbxml";
        else
        {
            EditedControll.Graphfilename = EditorUtility.SaveFilePanel("filename to save", BBDir.Get(BBpath.SETING), "graph", "xml");
            BBDebugLog.singleWarning("save graph on " + EditedControll.Graphfilename); 
        }
        System.Type T = typeof(NodeGraph); 
        System.Type[] extraTypes = { };
        XmlSerializer serializer = new XmlSerializer(T, extraTypes);
        Stream stream = new FileStream(EditedControll.Graphfilename, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
    }


    [BBCtrlVisible]
    public static NodeGraph LoadGraph(BBControll bbc,string path =null)
    {
        if (bbc.Graphfilename==null || ! File.Exists( bbc.Graphfilename  ) )
        {
            if (path == null)
            {
                EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
                return null;
            }

        }

        string pathtoload = (path == null) ? bbc.Graphfilename : path;

        NodeGraph NG = new NodeGraph();
        XmlSerializer serializer = new XmlSerializer(typeof(NodeGraph));
        Stream stream = new FileStream(pathtoload, FileMode.Open);
        NG = serializer.Deserialize(stream) as NodeGraph;
        stream.Close();

        // A create a list to process the hierarchy 
        List<BBCtrlNode> processnodelist = new List<BBCtrlNode>();
        processnodelist.Add(NG.ROOTNODE);
        foreach (BBCtrlNode B in NG.Nodes)
            processnodelist.Add(B);
        // add the coresponding number of node to match the key ref list 
        
        foreach (BBCtrlNode node in processnodelist)
            for (int c = 0; c < node.SUBNodesKEY.Count; c++)
            {
                BBCtrlNode fillnode = new BBCtrlNode();
                node.SUBNodes.Add(fillnode);
            }
        

        // push the child node in the correct index in the subnodearray 
        foreach (BBCtrlNode node in processnodelist)
            for (int c = 0; c < node.SUBNodesKEY.Count; c++)
                foreach (BBCtrlNode BBC in processnodelist)
                    if (BBC.Guid.GetHashCode() == node.SUBNodesKEY[c])
                        node.SUBNodes[c] = BBC;


        NG.Nodes.Clear();
        NG.nodekeys.Clear();


        // split the list ROOT / NOROOT 
        foreach (BBCtrlNode node in processnodelist)
        {
            if (node.isroot)
                NG.ROOTNODE = node;
            else
            {
                NG.Nodes.Add(node);
                NG.nodekeys.Add(node.Guid.GetHashCode());
            }

        }

        if (NG.ROOTNODE == null)
            BBDebugLog.singleWarning("rootnode is null after load graph");

        BBCtrlNode.dirty = true;

        //NodeGraph.EditedControll.thisgraph = NG;
        BBDebugLog.singleWarning("loaded graph is " + pathtoload);

        return NG;
    }



    }

    /// <summary>
    /// node option 
    /// </summary>
    public class nodeoption
    {
        public List<ParameterInfo> paraminfos = new List<ParameterInfo>();
        public List<object> values = new List<object>();
    }



    public class BBCtrlNode
    {
        // statics props 
        public static bool dirty;
        public static  string Nodeinfos = "";

        public string nodedebuglog = "";


        public static string hierarchy = "";
        public static Rect ROOTPOS = new Rect(Screen.width, Screen.height / 2, 200, 200);
        public static Rect RMIN = new Rect(Screen.width, Screen.height / 2, 100, 100);
        public static Rect RMAX = new Rect(Screen.width, Screen.height / 2, 400, 400);
        public static bool editordebugmode = false;
        public static float debugfloat1;
        public static float debugfloat2;
        public static float debugfloat3;
        public static float debugfloat4;
        public static bool scrolllock = false;
        public static bool unfiltered = false;
        public static int slotYoffset = 0; // offset for slot creation ++ is size of node 
        private static int WINDOWHEADOFFSET = 20; // ofset from head of windo bar 
        public static bool editorfocused;
        public static bool draglock = false;
        static GUIStyle textonly = new GUIStyle();
        static int BZ = (int)MVPBUTTONSIZE.MVP32;
        Color linkcolor = Color.green;
        // should be handled separately in serialization 

        static Dictionary<string, object> ControllsArgs = new Dictionary<string, object>(); // for ui nodes 

        //public KeyValuePair<string, object> ControllKVP = new KeyValuePair<string, object>(); // for ui nodes 

        [XmlIgnore]
        public object objtoinvoke;
        public object controllarg;
        public string controllargname;
        public bool needinvoke;


        





        [XmlIgnore] // cannot serialize that 
        public List<BBCtrlNode> SUBNodes = new List<BBCtrlNode>();
        // but keep a key list instead and rebuild on deserialize 
        public List<int> SUBNodesKEY = new List<int>();
        public int NodeId;
        public int ParentID;



        [XmlIgnore]
        MethodInfo[] filteredmethods;
        [XmlIgnore]
        public Vector2 velocity = Vector2.zero;

        public Rect Windowpos;
        public string name; // name used for cosmetic purpose 
        public string ClassnameFQ;
        public string classnameshort; // store the short name to avoid a string manip on node invoke ( optim ) 
        public string FunctionnameFQ;
        public System.Guid Guid; // this is the real identifier 
        public String ParamFQ;

        public bool iscontroll = false;

        public bool checknodevalid = true;
        public bool nodewarning = false;

        [XmlIgnore]
        public object m_OutputObj;

        // important info that mainly where the system 
        // get info on method and class to invoke 

        //**********************************
        public int LookupClassindex;           
        public string lookupclassname;
        // inform this field on deserializattion to store the original method name and find it again if the source has been changed
        public string savedclassname; // graph comes from file and this is the place to store the saved class name         
        public int Lookupmethodindex;          
        public string LookupMethodName;
        // inform this field on deserializattion to store the original method name and find it again if the source has been changed
        public string savedMethodName;// graph comes from file and this is the place to store the saved method name 
        //**********************************


        //public List<string> ArglistFQ = new List<string>();

        public List<ParameterInfo> Arglist = new List<ParameterInfo>();


        public List<SlotInfo> slotspos = new List<SlotInfo>();
        public SlotInfo ParentFeedSlotInfo;
        MethodInfo selectedmethodinfo;
        // this is the tag for Rootnode 
        public bool isroot = false;
        

        // fill up to keep function updated ( system not done yet ) 
        List<string> classnamelist = new List<string>();
        List<string> methodnamelist = new List<string>();


        // serialization issue use fq name instead 
        [XmlIgnore]
        private System.Type lookupclasstype;   
        [XmlIgnore]
        public System.Type ReturnTye;
        [XmlIgnore]
        List<System.Type> NodeClassTypeArray = new List<Type>();
        [XmlIgnore]
        public static List<System.Type> MandatoryNodeClassTypeArray = new List<Type>();

        
        // member used for exec loop but not needed for serialization
        // to replace with private and get/set
        [XmlIgnore]
        public bool m_gotfocus = false;
        [XmlIgnore]
        public Rect outputslotbutton;           
        [XmlIgnore]
        public Rect RectCurrent = new Rect();    
        [XmlIgnore]
        public Rect RectDest = new Rect();      
        [XmlIgnore]
        public Rect Rectorg = new Rect();       
        [XmlIgnore]
        public bool maximized;
        [XmlIgnore]
        public bool minimized;
        [XmlIgnore]
        public EditorTimer Timer = new EditorTimer();
        [XmlIgnore]
        public bool nodedebug;
        [XmlIgnore]
        public bool FlushNode = false;


   
        ~BBCtrlNode()
        {
            //Debug.Log("DESTRUCT  " + name+ " " + NodeId );
   
        }
        /// <summary>
        /// rebuild node hierarchy before each 
        /// render tick 
        /// </summary>
        public void REcusiveCollectNodes()
        {
            foreach (BBCtrlNode N in SUBNodes)
                N.REcusiveCollectNodes();
            NodeGraph.EditedControll.thisgraph.Nodes.Add(this);
            NodeGraph.EditedControll.thisgraph.nodekeys.Add(Guid.GetHashCode());
        }
        /// <summary>
        /// stupid solution to use 2 separated list in place of a dic 
        /// did this to solve serialization issue but 
        /// should definitelly reuse a dic and create list at serialization time 
        /// that quite risky to maintain this structure where a dic 
        /// provide safe access 
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="child"></param>
        public void NodeAddChild(BBCtrlNode Parent, BBCtrlNode child)
        {
            Parent.SUBNodesKEY.Add(child.Guid.GetHashCode());
            Parent.SUBNodes.Add(child);
        }

        /// <summary>
        /// check if conversion is possible 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static bool CanConvert(Type Tin ,Type Tout)
        {

            try
            {
                object objin = (object)Activator.CreateInstance(Tin);
                object objout = (object)Activator.CreateInstance(Tout);
                object res = Convert.ChangeType(objin, objout.GetType());
                if (res == null)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }

        }


        /// <summary>
        /// same comment as nodeaddchild
        /// </summary>
        public void NodeRemoveThis()
        {
            NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).SUBNodesKEY.Remove(Guid.GetHashCode());
            NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).SUBNodes.Remove(this);
            Debug.Log("remove this " + this.NodeId + " from parent " + NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).NodeId);
        }
        /// <summary>
        /// same comment as nodeaddchild
        /// </summary>
        public void NodeRemoveChild(BBCtrlNode pParent, BBCtrlNode child)
        {
            List<BBCtrlNode> todelete = new List<BBCtrlNode>();
            foreach (BBCtrlNode n in child.SUBNodes)
                todelete.Add(n);
            foreach (BBCtrlNode n in todelete)
                n.NodeRemoveThis();
            pParent.SUBNodesKEY.Remove(child.Guid.GetHashCode());
            pParent.SUBNodes.Remove(child);
            Debug.Log("remove child " + child.NodeId + " from parent " + pParent.NodeId);

        }
        /// <summary>
        /// the constructor to use for node creation 
        /// look at the bottom of file to pick up a copy constructor 
        /// this copy constructor is not used but could be interesting 
        /// for debug and inspect how list are passed the copy constructor is commented 
        /// and desactivated right now
        /// </summary>
        public BBCtrlNode(Rect initpos, string newname, BBCtrlNode nodeParent = null)
        {

            Guid = System.Guid.NewGuid();
            NodeId = Guid.GetHashCode();
            Windowpos = initpos;
            //ArgsList = new List<ParameterInfo>();
            name = newname;
            textonly.imagePosition = ImagePosition.TextOnly;
            textonly.clipping = TextClipping.Clip;

            Flood("Constructor called " + Guid.ToString());
            //EditorUtility.DisplayDialog("creator", "create ", "ok");
            if (nodeParent == null)
                isroot = true;
            else
            {
                //AddChildrenToGRAPH(Guid.GetHashCode(), this);
                ParentID = nodeParent.NodeId;
                NodeAddChild(NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID), this);
            }
            if (NodeGraph.EditedControll.thisgraph != null)
            {
                NodeGraph.EditedControll.thisgraph.Nodes.Add(this);
                NodeGraph.EditedControll.thisgraph.nodekeys.Add(Guid.GetHashCode());
            }
            else
                Debug.Log("global crap graph is null");

        }
        /// <summary>
        /// return this index in parent child buffer
        /// based on slot list 
        /// </summary>
        /// <returns></returns>
        public int GetThisNodeIndexInParentSlots ()
        {
            BBCtrlNode parent = NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID);
            if (parent == null)
                return -1;

            for (int i = 0 ; i < parent.slotspos.Count ; i++ )
                if (name.Contains(  parent.slotspos[i].paramname ) )
                    return i;
            BBDebugLog.singleWarning("Parameter Name incorrect flush this and recreate" + this.name);
            return -1;
        }
        /// <summary>
        /// check if the connection can be done 
        /// need to add a transtyping method to figure out where 
        /// object could be casted in a valid result 
        /// this would improve the interop 
        /// </summary>
        /// <returns></returns>
        public bool CheckNodesconection ()
        {

            if (nodedebug)
                nodedebug = true;


            if (isroot || iscontroll) // check is from children to parent root have no parent
                return true;
            BBCtrlNode parent = NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID);
            int I = GetThisNodeIndexInParentSlots();
            if (I < 0 || I > parent.slotspos.Count)
                return false ;
            string A = parent.slotspos[I].TypeFullString;
            string B = ParamFQ;
            if (A == B)
                return true;
            else
                return false;
        }
        /// <summary>
        /// create a fresh void node 
        /// with minimal info ( for future extention of the action graph editor )
        /// </summary>
        public BBCtrlNode()
        {
            // default constructor for serialization 
            Guid = System.Guid.NewGuid();
            Windowpos = BBCtrlNode.ROOTPOS;
            //ArgsList = new List<ParameterInfo>();
            name = "NEW NODE";
            textonly.imagePosition = ImagePosition.ImageOnly;
            Flood("Constructor called " + Guid.ToString());
            //EditorUtility.DisplayDialog("creator", "create ", "ok");
            isroot = true;
        }
        /// <summary>
        /// node placement ( could be better )
        /// </summary>
        /// <param name="nb"></param>
        /// <returns></returns>
        public Rect GetNewPosFromThisNode(int nb)
        {
            float zoomfactor = (isroot) ? 1.0f : 1.0f;
            Rect Newrect = new Rect();
            float width = Windowpos.width * zoomfactor;
            float height = Windowpos.height * (zoomfactor + ((nodedebug) ? 1f : 0f)); // big debug for text
            float px = Windowpos.x - (width * 1.8f);
            float py = Windowpos.y + ((height * 1.2f) * nb);
            Newrect.Set(px, py, width, height);
            return Newrect;
        }
        /// <summary>
        /// POPUP ANIMATION ( COSMETIC ) 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void DrawDeploychildren(Rect parent, Rect child)
        {
            if (Timer.run)
            {
                Timer.Update(false);
                float rt = Timer.timeremaining;
                float max = Timer.s;

                RectCurrent = BBDrawing.Rectinterpolate(parent, child, max - rt, max);
                Flood(RectCurrent.ToString());
            }
        }
        /// <summary>
        /// flood is useless logout .. for debug
        /// </summary>
        /// <param name="S"></param>
        /// <param name="forced"></param>
        public void Flood(string S, bool forced = false)
        {
            /* use another bool 
            if (BBCtrlEditor.showdebuginfo || forced )
                Debug.Log(S);
             * */
        }
        /// <summary>
        /// check slot connection consistency 
        /// probably duplicated logic with checkconnection 
        /// need to merge both 
        /// </summary>
        /// <returns></returns>
        public bool Checkslot()
        {
            if (slotspos.Count != Arglist.Count)
                return false;
            for (int c = 0; c < Arglist.Count; c++)
                if (Arglist[c].ParameterType != null)
                    if (slotspos[c].TypeFullString != Arglist[c].ParameterType.AssemblyQualifiedName)
                        return false;
            return true;
        }
        /// <summary>
        /// slot consistency check failed 
        /// rebuild the slot list 
        /// </summary>
        public void REbuildSlots()
        {
            // lets say that the first parameter of a controll node 
            // is the gui feed parameter managed by internal UI and not by external 
            // input 


            slotspos.Clear();
            for (int c = 0; c < Arglist.Count; c++)
            {
                int BZ = (int)MVPBUTTONSIZE.MVP32;
                SlotInfo S = new SlotInfo(c);
                Rect slotbuttonpos = new Rect(Windowpos.position.x - (BZ / 1.5f),
                        Windowpos.position.y + ((BZ) * c), // y got c offset
                        BZ, BZ); // size 
                S.linkedto = 0;
                S.linkedto = this.Guid.GetHashCode();
                // cannoit serialize type so i store full class  qualified name 
                S.TypeFullString = Arglist[c].ParameterType.AssemblyQualifiedName ; // to store type as full qualified classname
                S.index = c;
                ParameterInfo[] p = selectedmethodinfo.GetParameters();
                S.paramname = p[c].Name;
                S.R = slotbuttonpos;
                if (iscontroll)
                    S.UIControllparamslot = true;
                slotspos.Add(S);
            }
        }
        /// <summary>
        /// NODE ROUTINE THE IN AND OUT WINDOWS LOGIC  
        /// </summary>
        public virtual void DoNode()
        {
            m_gotfocus = BBDrawing.GetRectFocus(Windowpos, true);
            // collect the nodes that need to be deleted 
            List<BBCtrlNode> toremove = new List<BBCtrlNode>();

            // do the node and add to the trashbin if it s not relevant anymore
            // carefull it s a recursive call to execute or trash 
            foreach (BBCtrlNode N in SUBNodes)
            {
                N.DoNode(); // do the call 
                if (N.FlushNode)
                {
                    BBDebugLog.singleWarning("MARK FOR DELETE "+N.NodeId);
                    toremove.Add(N);
                }
            }

            foreach (BBCtrlNode N in toremove)
            {
                NodeRemoveChild(this, N);
                dirty = true;
            }
            // Perform Inside Action (REFLECTOR)
            /// ===================================================== OUTPUT SLOT 
            if (Timer.run && !isroot) // prevent the out slot to show up during popup anim 
            {
                // node timer is running the node is not ready to display 
                DrawDeploychildren(Rectorg, RectDest);
                GUI.Box(RectCurrent, "");
                return;
            }

            outputslotbutton = new Rect(Windowpos.width + Windowpos.position.x + 8,
                Windowpos.height / 2 + Windowpos.position.y,
                BZ, BZ);



            if (GUI.Button(outputslotbutton, Textureloader.slot_ok, textonly))
                FlushNode = true;
            // FUCTION DO NOT HAVE PARAMETERS 
            if (Arglist.Count == 0)
                slotspos.Clear();
            //****************************************************** CREATE NEW SLOTS
            for (int c = 0; c < Arglist.Count; c++)
            {
                if (!Checkslot())
                    REbuildSlots();
                //ParamFQ = ArglistFQ[c];

                slotspos[c].R.Set(
                                    Windowpos.position.x - (BZ / 1.5f),
                                    Windowpos.position.y + ((BZ) * c),
                                    BZ, BZ
                                   );
                // do not draw a UI parameter on controllnode
                if ( ! slotspos[c].UIControllparamslot ) 
                    if (GUI.Button(slotspos[c].R, Textureloader.slot_questionmark, textonly))
                    {
                        //float zoom = (isroot) ? 0.5f : 1.0f; // root childs smallers 
                        Rect NR = GetNewPosFromThisNode(slotYoffset); //new Rect(10, 50, 80, 80);
                        ParameterInfo[] p  =  selectedmethodinfo.GetParameters();
                        BBCtrlNode Child = new BBCtrlNode(NR,p[c].ParameterType.Name+ " " +  p[c].Name, this);
                        Child.ParentFeedSlotInfo = new SlotInfo(c); // but i put on recently created Slot classs
                        Child.ParentFeedSlotInfo.TypeFullString = Arglist[c].ParameterType.AssemblyQualifiedName; // Parent input type 
                        Child.ParentFeedSlotInfo.index = c; // the parent slot index 
                        // for serialization 
                        Child.RectDest.Set(NR.xMin, NR.yMin, NR.width, NR.height); // for popup anim
                        Child.Rectorg = slotspos[c].R;    // for popup anim 
                        // launch the node popup anim 
                        Child.Timer.StartCountdown(0.5f);
                        slotspos[c].linkedto = Child.Guid.GetHashCode();
                        dirty = true;

                    }
            }
            Windowpos = GUI.Window(Guid.GetHashCode(), Windowpos, DoNodeWindow, name);
            if (!isroot)
            {
                BBCtrlNode P = NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID);
                if (P == null)
                    return;


                if (P.slotspos.Count > ParentFeedSlotInfo.index)
                {
                    if (!CheckNodesconection() || !checknodevalid)
                    {
                        if (iscontroll)
                            checknodevalid = true;
                        else
                        {
                            //Debug.Log(name + " Fail b");
                            linkcolor = Color.red;
                        }
                    }
                    else
                        linkcolor =  (nodewarning ) ? Color.gray  : Color.green;

                    Vector2 A=  outputslotbutton.center;
                    Vector2 B=  NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).slotspos[ParentFeedSlotInfo.index].R.center;

                   

                    // temp the spline is too expensive 
                    //BBDrawing.DrawLine(A, B, linkcolor, 3f, true);
                    BBDrawing.curveFromTo(outputslotbutton, NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).slotspos[ParentFeedSlotInfo.index].R, linkcolor, Windowpos.width * 2, out velocity);
                }
            }
        }
        /// <summary>
        ///  check if the graph could be invoked 
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        /// 
        public void CheckSubchain (BBCtrlNode N, out bool valid)
        {
          foreach (BBCtrlNode Sub in N.SUBNodes)
              CheckSubchain(Sub, out valid);
          valid = checknodevalid;
        }
        /// <summary>
        /// inspect recursively the full graph
        /// check connection is for a single and it s result is part of this
        /// check it basically define if the graph is ready to invoke the 
        /// tree
        /// </summary>
        /// <param name="N"></param>
        /// <param name="valid"></param>
        public void CheckfullGraph(BBCtrlNode N , out bool valid)
        {

            if (NodeGraph.EditedControll.thisgraph.GraphOK) // stop check at first invalid node connection
                foreach (BBCtrlNode Sub in N.SUBNodes)
                    CheckfullGraph(Sub, out valid);

            if (!NodeGraph.EditedControll.thisgraph.GraphOK)
            {
                // stop at the first problem useless to perform more check 
                valid = false;
                N.checknodevalid = false;
                //Debug.Log( name + "Fail c");
                return;
            }

            if (N.slotspos.Count != N.SUBNodes.Count  && !N.iscontroll )
            {
                valid = false;
                N.checknodevalid = false;
                //Debug.Log(name + " IS CONTROL : " + iscontroll.ToString() + " Fail d");
                return;
            }
            BBCtrlNode parent = NodeGraph.EditedControll.thisgraph.GetnodeFromID(N.ParentID);
            if (parent == null) // only root allowed to have no parent so it is root
            {
                bool subcheck = true;
                CheckSubchain (N, out subcheck);
                valid =  subcheck;
                return;
            }
            int I = N.GetThisNodeIndexInParentSlots();
            if (I < 0 || I >= parent.slotspos.Count)
                valid = false;
            else
            {
                string A = parent.slotspos[I].TypeFullString;
                string B = N.ParamFQ;
                if (A == B)
                    valid = true;
                else
                    valid = false;

            }

            N.checknodevalid = valid;





        }

        /// <summary>
        /// process nodes GUI
        /// </summary>



        


        
        // List<string> argnamelist = new List<string>();

        /// <summary>
        /// the window callback associated with this node this is basically a 
        /// Reflection Inspector that associate Function specificly tagged with a custom atribute
        /// and listed in the look up list >>>> to the node hierarchy once it s done 
        /// invoke a buffered sequence shoud execute ( i hope )
        /// </summary>
        /// <param name="id"></param>
        /// 




        public virtual void DoNodeWindow(int id)
        {
            // clear debug log
            checknodevalid = true;
            nodewarning = false;
            string prevclassname = lookupclassname;
            string prevmethodname  = LookupMethodName;
            string[] localclassarray = FillClassArray(Selection.activeGameObject).ToArray();
            // reassign the class index before use cause things changing all the time 
            // depending on selection and behaviors assigned 
            for (int c = 0; c < localclassarray.GetLength(0); c++)
                if (lookupclassname == localclassarray[c])
                    LookupClassindex = c;
            List<string> localmethodarray = new List<string>();
            // user action to select which one to inspect 
            LookupClassindex = EditorGUILayout.Popup(LookupClassindex, localclassarray);
            if (LookupClassindex >= localclassarray.GetLength(0)) 
            {
                checknodevalid = false;
                BBDebugLog.singleWarning("DoNodeWindow node fail : LookupClassindex is above localclassarray value:  " + id.ToString() );
                GUILayout.Label("classindex out of range");
                return;
            }
            lookupclassname = localclassarray[LookupClassindex];
            localmethodarray.Clear();
            filteredmethods = BuildFilteredMethodArray(LookupClassindex);
            foreach (MethodInfo mi in filteredmethods)
                localmethodarray.Add(mi.Name);
            // reassign index before doing popup action 
            for (int c = 0; c < filteredmethods.GetLength(0); c++)
                if (filteredmethods[c].Name == LookupMethodName)
                    Lookupmethodindex = c;
            Lookupmethodindex = EditorGUILayout.Popup(Lookupmethodindex, localmethodarray.ToArray());
            if (filteredmethods.Length <= Lookupmethodindex || Lookupmethodindex < 0)
            {
                checknodevalid = false;
                BBDebugLog.singleWarning("DoNodeWindow node fail : method index point out of range :  " + id.ToString());
                GUILayout.Label("method index out of range");
                GUI.DragWindow();
                return;
            }
            string STR = "";
            if (ParamFQ != null)
                STR = NodeId.ToString();   //Paraminfo.ParameterType.ToString();
            if (BBCtrlNode.scrolllock)  
                EditorGUILayout.LabelField(STR);
            // store the fullQF class name
            if (NodeClassTypeArray.Count <= LookupClassindex)
            {
                // this is the process to output string debug in node 
                GUI.DragWindow(); // anyway
                string msg = " LookupClassindex out of NodeClassTypeArray  range : ()  " + id.ToString() ;
                BBDebugLog.singleWarning(msg);
                GUILayout.Label("index out of range");
                nodewarning = true;
                return;
            }
            ClassnameFQ = NodeClassTypeArray[LookupClassindex].AssemblyQualifiedName;
            classnameshort = NodeClassTypeArray[LookupClassindex].AssemblyQualifiedName.Split(char.Parse(","))[0];


            //--------------------------------------- GUI ELEMENT 

            NodeGraph.EditedControll.thisgraph.GraphOK = true;

            CheckfullGraph(this, out NodeGraph.EditedControll.thisgraph.GraphOK);
            if (NodeGraph.EditedControll.thisgraph.GraphOK && checknodevalid && !iscontroll)
                if (GUILayout.Button("INVOKE"))
                        Nodeinvoke(0);
            nodedebug = GUILayout.Toggle(nodedebug, "debugnode");
            string subnodesname = "";
            if (selectedmethodinfo != null)
            {
                Nodeinfos = "\nReturn :" + selectedmethodinfo.ReturnParameter.ParameterType.Name;
                ParamFQ = selectedmethodinfo.ReturnParameter.ParameterType.AssemblyQualifiedName;
            }
            foreach (BBCtrlNode N in SUBNodes)
                subnodesname += "child " + N.NodeId + "\n";
            if (!isroot)
            {
                if (nodedebug)
                {
                    GUILayout.Label(subnodesname);
                    GUILayout.Label(Nodeinfos);
                }
                

            }
            selectedmethodinfo = filteredmethods[Lookupmethodindex];
            LookupMethodName = selectedmethodinfo.Name;
            ReturnTye = selectedmethodinfo.ReturnType; // return type ys stored in the node 
            Arglist.Clear();
            foreach (ParameterInfo pi in selectedmethodinfo.GetParameters())
                Arglist.Add(pi);
            checknodevalid = true;
            // this node is a ui controll 
            // have to perform the user input to store the value 
            iscontroll = false;
            object[] atributelist = selectedmethodinfo.GetCustomAttributes(true);
            foreach (object o in atributelist)
            {
                if (o.GetType() == typeof(BBCtrlProp))
                {
                    BBCtrlProp bbctrltag = (BBCtrlProp)o;
                    needinvoke = bbctrltag.needinvoke; 
                    iscontroll = true;
                    ControllInvoke(selectedmethodinfo);
                }
            }
            GUI.DragWindow(); // anyway
        }


        public void ControllInvoke(MethodInfo M)
        {
            // cache the object since createinstance cost memory and fps 
            // better to reuse even if this instance need extra management 
            // actually set to null if class change during edition , but not likely to happen on runtime 

            BBUInodes controllnode ;
            if (objtoinvoke == null || !NodeGraph.gamemode )
                controllnode = (BBUInodes)Activator.CreateInstance(typeof(BBUInodes));
            else
                controllnode = (BBUInodes)objtoinvoke;

            controllnode.Rwindow = Windowpos;
            ParameterInfo pi = M.GetParameters()[0];

            if (controllarg == null || controllarg.GetType() != pi.ParameterType)
                controllarg = Activator.CreateInstance(pi.ParameterType);
            if (controllarg == null)
            {
                BBDebugLog.singleWarning("cannot create new ui controll during Gameplay yet stop play to assign a new UI controll node ");
                return;
            }

            if (nodedebug)
                BBDebugLog.singleWarning("break");


                controllarg = M.Invoke(controllnode, new object[] { controllarg });
            m_OutputObj = controllarg;
        }




        

    
        /// <summary>
        ///  Node invoke recursively the full graph chain starting from this 
        ///  node 
        /// </summary>
        /// <param name="returnindex"></param>
        /// <returns></returns>


        public object Nodeinvoke(int returnindex)
        {
            if (!NodeGraph.EditedControll.thisgraph.GraphOK)
                return null;

            Dictionary<string, object> Args = new Dictionary<string, object>();
            List<object> objlist = new List<object>();


            object classInstance = null;
            if (Selection.activeGameObject != null)
                classInstance = Selection.activeGameObject.GetComponent(NodeClassTypeArray[LookupClassindex].Name);
            if (classInstance == null)
                classInstance = Activator.CreateInstance(NodeClassTypeArray[LookupClassindex]);
            if (classInstance == null)
            {
                Debug.Log("cannot get or create an instance of " + NodeClassTypeArray[LookupClassindex].Name);
                return null;
            }
            for (int c = 0; c < SUBNodes.Count; c++)
            {
                BBCtrlNode N = SUBNodes[c];
                if (N.iscontroll) // controll is invoked in edit loop on user input
                    N.ControllInvoke(N.selectedmethodinfo);
                else
                    N.Nodeinvoke(c);
                if (N.m_OutputObj == null)
                    Debug.Log("no result for node " + N.NodeId.ToString());
                Args.Add(N.name, N.m_OutputObj);
            }

            // push args in right order fo the call 
            for (int c = 0; c < slotspos.Count; c++)
                foreach (KeyValuePair<string, object> kvp in Args)
                    if (kvp.Key.Contains(slotspos[c].paramname))
                        objlist.Add(kvp.Value);

            try
            {
                m_OutputObj = selectedmethodinfo.Invoke(classInstance, objlist.ToArray());
            }

            catch
            {
                //Debug.Log("method " + selectedmethodinfo.Name + "throw : error  on node " + name + " set a default value for return " );
                m_OutputObj = (object)Activator.CreateInstance(selectedmethodinfo.ReturnParameter.GetType());
            }

            return m_OutputObj;
        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public void MandatoryClassType ()
        {
            MandatoryNodeClassTypeArray.Add(typeof(BBBLocks));
            MandatoryNodeClassTypeArray.Add(typeof(BBMovepad));
            MandatoryNodeClassTypeArray.Add(typeof(BBUInodes));
            MandatoryNodeClassTypeArray.Add(typeof(BBMovepadLayerDescriptor));
            MandatoryNodeClassTypeArray.Add(typeof(BBControll));
            MandatoryNodeClassTypeArray.Add(typeof(BBMathnodes));
        }


        
        /// <summary>
        /// here is the place to add class to inspect by the reflectornode editor
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public List<string> FillClassArray(GameObject go = null)
        
        {


            NodeClassTypeArray.Clear();
            List<String> L = new List<string>();
            if (go != null)
            {
                MonoBehaviour[] scripts = Selection.activeGameObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour o in scripts)
                    NodeClassTypeArray.Add(o.GetType());
            }

            // BBcontroll default nodes

            if (MandatoryNodeClassTypeArray.Count == 0)
                    MandatoryClassType();


            foreach (Type T in MandatoryNodeClassTypeArray)
                NodeClassTypeArray.Add(T); 



            foreach (System.Type T in NodeClassTypeArray)
            {
                L.Add(T.Name);
            }
            if (ClassnameFQ == null)
                return L;
            foreach (Type T in NodeClassTypeArray)
                if (T.AssemblyQualifiedName == ClassnameFQ)
                    return L;
            Type missingtype = Type.GetType(ClassnameFQ);
            L.Add(missingtype.Name);
            return L;
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        //public MethodInfo[] filteredmethods;




        public  MethodInfo[] BuildFilteredMethodArray(int classindex, MethodInfo[] allmethods = null)
        {

            MethodInfo[] MethodInfoList ;
            if (allmethods == null) // can pass directly the method array when it s called from graph node player (MOVEPAD for instance) 
            {
                if (NodeClassTypeArray.Count <= classindex)
                    classindex = 0;
                MethodInfoList = NodeClassTypeArray[classindex].GetMethods();
            }
            else
                MethodInfoList = allmethods;

            List<MethodInfo> L = new List<MethodInfo>();
            for (int mc = 0; mc < MethodInfoList.GetLength(0); mc++)
            {
                object[] atributelist = MethodInfoList[mc].GetCustomAttributes(true);
                foreach (object o in atributelist)
                {
                    if (o.GetType() == typeof(BBCtrlVisible) || BBCtrlNode.unfiltered)
                        L.Add(MethodInfoList[mc]); // store the index on the name 
                }
                // try to get those anyway to replace by a customizable list of predefined methods 
                if (MethodInfoList[mc].Name == "GetDataset" || MethodInfoList[mc].Name == "GetActorProps")
                    L.Add(MethodInfoList[mc]);
            }
     
            return L.ToArray();
        }


        /// <summary>
        /// compose the GUI BOX debug text for this node 
        /// when the node log out in maximized mode
        /// </summary>
        /// <returns></returns>
        public string NodeDebugLogOutput()
        {
            string debugout = "";
            if (!isroot)
            {
                if (this.m_OutputObj != null) // 
                {
                    debugout += ("RETURNED " + m_OutputObj.ToString()) + "\n";
                    debugout += ("type expected " + ParentFeedSlotInfo.TypeFullString) + "\n";
                }
                else
                {
                    debugout += ("returntype " + ReturnTye.ToString()) + "\n";
                    debugout += ("paraminfo " + ParamFQ + "\n");
                    debugout += ("outfeed  type " + ParentFeedSlotInfo.TypeFullString + "id in parent " + ParentFeedSlotInfo.index.ToString() + "\n");
                    foreach (SlotInfo s in slotspos)
                        debugout += ("slotinfo id " + s.index + " Type " + s.TypeFullString + "\n");
                }
            }
            return debugout;
        }
        /// <summary>
        ///  update the node info 
        /// </summary>
        /// <param name="CLIDX"></param>
        /// <param name="MTHIDX"></param>
        public int  UpdateNodeEntry(int CLIDX, int MTHIDX)
        {
            for (int c = 0; c < filteredmethods.GetLength(0); c++)
                if (filteredmethods[c].Name == LookupMethodName)
                    MTHIDX = c;

            if (filteredmethods.Length <= MTHIDX || MTHIDX < 0 )
            {
                GUILayout.Label("graph not apply to this selection");
                checknodevalid = false;
                //Debug.Log("Fail A");
                return MTHIDX; // FAIL 
            }

            Lookupmethodindex = MTHIDX;
            selectedmethodinfo = filteredmethods[MTHIDX];
            LookupMethodName = selectedmethodinfo.Name;
            ReturnTye = selectedmethodinfo.ReturnType; // return type ys stored in the node 
            Arglist .Clear();
            foreach (ParameterInfo pi in selectedmethodinfo.GetParameters())
            {
                Arglist.Add(pi);
            }
            checknodevalid = true;
            return MTHIDX;
        }
    }



/////////////////////////////////////////////////////////////////////////////////////////////////////

// copy constructor for debug 

        //public BBCtrlNode(BBCtrlNode from)
        //{
        //    foreach (String S in from.ArglistFQ)
        //        this.ArglistFQ.Add(S);
        //    this.ClassnameFQ = from.ClassnameFQ;
        //    this.classnamelist = new List<string>();
        //    foreach (string s in from.classnamelist )
        //        this.classnamelist.Add(s);
        //    List<MethodInfo> milist = new List<MethodInfo>();
        //    if (from.filteredmethods != null)
        //    {
        //        foreach (MethodInfo mi in from.filteredmethods)
        //            milist.Add(mi);
        //        this.filteredmethods = milist.ToArray();
        //    }
        //    this.FlushNode = false;
        //    this.FunctionnameFQ = from.FunctionnameFQ;
        //    this.Guid = from.Guid;
        //    this.isroot= from.isroot;
        //    this.LookupClassindex = from.LookupClassindex;
        //    this.lookupclassname = from.lookupclassname;
        //    this.lookupclasstype = from.lookupclasstype;
        //    this.Lookupmethodindex = from.LookupMethodindex;
        //    this.LookupMethodName = from.LookupMethodName;
        //    this.methodnamelist = new List<string>();
        //    foreach ( String s in from.methodnamelist)
        //        this.methodnamelist.Add(s);
        //    this.name = from.name;
        //    this.NodeClassTypeArray = new List<Type>();
        //    foreach ( Type T in from.NodeClassTypeArray)
        //        this.NodeClassTypeArray.Add( T );
        //    this.NodeId = from.NodeId;
        //    this.m_OutputObj = from.m_OutputObj;
        //    this.outputslotbutton = from.outputslotbutton;
        //    this.ParamFQ = from.ParamFQ;
        //    this.ParentID = from.ParentID;
        //    this.ParentFeedSlotInfo = from.ParentFeedSlotInfo;
        //    this.RectCurrent= from.RectCurrent;
        //    this.RectDest = from.RectDest;
        //    this.Rectorg = from.Rectorg;
        //    this.ReturnTye = from.ReturnTye;
        //    this.slotspos = from.slotspos;
        //    this.slotYoffset = from.slotYoffset;
        //    this.SUBNodes = new List<BBCtrlNode> () ;
        //    foreach ( BBCtrlNode bbc in from.SUBNodes )
        //    {
        //        BBCtrlNode sub = new BBCtrlNode(bbc);
        //        this.SUBNodes.Add(sub);
        //    }
        //    this.SUBNodesKEY = new List<int>();
        //    foreach (int key in from.SUBNodesKEY)
        //        this.SUBNodesKEY.Add(key);
        //    this.Timer = from.Timer;
        //    this.velocity = from.velocity;
        //    this.Windowpos =from.Windowpos;

        //}