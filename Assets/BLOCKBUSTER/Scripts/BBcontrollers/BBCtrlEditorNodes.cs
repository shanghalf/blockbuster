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

    public int index= 0 ;
    //public  System.Type T;
    public int linkedto;
    //public BBCtrlNode TargetNode;
    //public int parentslotindex ;
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



/// <summary>
/// the graphnode 
/// under a digest form for 
/// save and load 
/// </summary>
[System.Serializable]
public class NodeGraph
{
    public bool GraphOK = true;

    public static bool autoload = false;
    public static bool autosave = false;
    public static string  autosavefilename;


    public BBCtrlNode ROOTNODE;
    public List<BBCtrlNode> Nodes = new List<BBCtrlNode>();
    public List<int> nodekeys = new List<int>();
    public Guid Guid = Guid.NewGuid();



    public BBCtrlNode  GetnodeFromID (int ID)
    {
        foreach (BBCtrlNode BBC in Nodes)
            if (BBC.Guid.GetHashCode() == ID)
                return BBC;
        Debug.Log("cant find node " + ID.ToString());
        return null;
    }
    public  void FlushBuffer()
    {
        BBCtrlNode.THEGRAPH.Nodes.Clear();
        BBCtrlNode.THEGRAPH.nodekeys.Clear();
    }

    public void Save(bool saveas=false)
    {
        string path ;
        if (autosave && !saveas)
            path = NodeGraph.autosavefilename;
        else
            path = EditorUtility.SaveFilePanel("filename to save", BBDir.Get(BBpath.SETING), "graph", "xml");
        
        System.Type T = typeof(NodeGraph); 
        System.Type[] extraTypes = { };
        XmlSerializer serializer = new XmlSerializer(T, extraTypes);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
    }



    public void  Load(string path )
    {


        if (autoload )
            path = NodeGraph.autosavefilename ;
        else
            path =  EditorUtility.OpenFilePanel("load scene", BBDir.Get(BBpath.SETING), "xml");

        if (!System.IO.File.Exists(path))
        {
            EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
            autoload = false;
            return;
        }
        XmlSerializer serializer = new XmlSerializer(typeof(NodeGraph));
        Stream stream = new FileStream(path, FileMode.Open);
        BBCtrlNode.THEGRAPH = serializer.Deserialize(stream) as NodeGraph;
        stream.Close();

        // A create a list to process the hierarchy 
        List<BBCtrlNode> processnodelist = new List<BBCtrlNode>();
        processnodelist.Add(BBCtrlNode.THEGRAPH.ROOTNODE);
        foreach (BBCtrlNode bbc in BBCtrlNode.THEGRAPH.Nodes)
            processnodelist.Add(bbc);
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


        // flush the buffers 
        BBCtrlNode.THEGRAPH.Nodes.Clear();
        BBCtrlNode.THEGRAPH.nodekeys.Clear();

        // split the list ROOT / NOROOT 
        foreach (BBCtrlNode node in processnodelist)
        {
            if (node.isroot)
                BBCtrlNode.THEGRAPH.ROOTNODE = node;
            else
            {
                BBCtrlNode.THEGRAPH.Nodes.Add(node);
                BBCtrlNode.THEGRAPH.nodekeys.Add(node.Guid.GetHashCode());
            }

        }
        BBCtrlNode.dirty = true;
        }

    public void ForceLoad()
    {

        // patch a la con 
        string path = EditorUtility.OpenFilePanel("load scene", BBDir.Get(BBpath.SETING), "xml");

        if (!System.IO.File.Exists(path))
        {
            EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
            autoload = false;
            return;
        }
        XmlSerializer serializer = new XmlSerializer(typeof(NodeGraph));
        Stream stream = new FileStream(path, FileMode.Open);
        BBCtrlNode.THEGRAPH = serializer.Deserialize(stream) as NodeGraph;
        stream.Close();

        // A create a list to process the hierarchy 
        List<BBCtrlNode> processnodelist = new List<BBCtrlNode>();
        processnodelist.Add(BBCtrlNode.THEGRAPH.ROOTNODE);
        foreach (BBCtrlNode bbc in BBCtrlNode.THEGRAPH.Nodes)
            processnodelist.Add(bbc);
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


        // flush the buffers 
        BBCtrlNode.THEGRAPH.Nodes.Clear();
        BBCtrlNode.THEGRAPH.nodekeys.Clear();

        // split the list ROOT / NOROOT 
        foreach (BBCtrlNode node in processnodelist)
        {
            if (node.isroot)
                BBCtrlNode.THEGRAPH.ROOTNODE = node;
            else
            {
                BBCtrlNode.THEGRAPH.Nodes.Add(node);
                BBCtrlNode.THEGRAPH.nodekeys.Add(node.Guid.GetHashCode());
            }

        }
        BBCtrlNode.dirty = true;
    }  


    }




    public class BBCtrlNode
    {
        // statics props 
        public static bool dirty;
        public static  string NodeDebuginfos = "";
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
        static Color linkcolor = Color.green;


        [XmlIgnore]
        public static  NodeGraph THEGRAPH = new NodeGraph();
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
        public string FunctionnameFQ;
        public System.Guid Guid; // this is the real identifier 
        public String ParamFQ;

        //[XmlIgnore]
        //public List<ParameterInfo> ArgsList; // this Function args 

        public bool checknodevalid = true;

        [XmlIgnore]
        public object m_OutputObj;

        // important info that mainly where the system 
        // get info on method and class to invoke 
        public int LookupClassindex;           
        public string lookupclassname;         
        public int Lookupmethodindex;          
        public string LookupMethodName;
        public List<string> ArglistFQ = new List<string>();
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
        private List<int> iknowit = new List<int>();
        [XmlIgnore]
        public EditorTimer Timer = new EditorTimer();
        [XmlIgnore]
        public bool nodedebug;
        [XmlIgnore]
        public bool FlushNode = false;


   
        ~BBCtrlNode()
        {
            Debug.Log("DESTRUCT  " + name+ " " + NodeId );
   
        }
        /// <summary>
        /// rebuild node hierarchy before each 
        /// render tick 
        /// </summary>
        public void REcusiveCollectNodes()
        {
            foreach (BBCtrlNode N in SUBNodes)
                N.REcusiveCollectNodes();
            BBCtrlNode.THEGRAPH.Nodes.Add(this);
            BBCtrlNode.THEGRAPH.nodekeys.Add(Guid.GetHashCode());
            //display the node hierarchy ( for debug ) 
            //BBCtrlNode.hierarchy += "\n node: " + this.NodeId;
            //foreach ( BBCtrlNode child in this.SUBNodes)
            //    BBCtrlNode.hierarchy += "\n     child: " + child.NodeId;
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
            BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID).SUBNodesKEY.Remove(Guid.GetHashCode());
            BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID).SUBNodes.Remove(this);
            Debug.Log("remove this " + this.NodeId + " from parent " + BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID).NodeId);
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
            textonly.imagePosition = ImagePosition.ImageOnly;
            Flood("Constructor called " + Guid.ToString());
            //EditorUtility.DisplayDialog("creator", "create ", "ok");
            if (nodeParent == null)
                isroot = true;
            else
            {
                //AddChildrenToGRAPH(Guid.GetHashCode(), this);
                ParentID = nodeParent.NodeId;
                NodeAddChild(BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID), this);
            }
            BBCtrlNode.THEGRAPH.Nodes.Add(this);
            BBCtrlNode.THEGRAPH.nodekeys.Add(Guid.GetHashCode());
        }
        /// <summary>
        /// return this index in parent child buffer
        /// based on slot list 
        /// </summary>
        /// <returns></returns>
        public int GetThisNodeIndexInParentSlots ()
        {
            BBCtrlNode parent =  THEGRAPH.GetnodeFromID(ParentID);

            for (int i = 0 ; i < parent.slotspos.Count ; i++ )
                if (name.Contains(  parent.slotspos[i].paramname ) )
                    return i;
            BBCtrlNode.NodeDebuginfos += "\nParameter Name incorrect flush this and recreate";
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
            if (isroot) // check is from children to parent root have no parent
                return true;



            BBCtrlNode parent = THEGRAPH.GetnodeFromID(ParentID);
            int I = GetThisNodeIndexInParentSlots();
            if (I < 0 || I > parent.slotspos.Count)
                return false ;
            string A = parent.slotspos[I].TypeFullString;
            string B = ParamFQ;
            if (A == B)
                return true;
            else
            {
                return false;
                // too slow to test conversion each frame
                /*
                if (A == null || B == null)
                    return false;

                Type Tin = Type.GetType(parent.slotspos[I].TypeFullString). ;
                Type Tout= Type.GetType(ParamFQ);

                if (CanConvert(Tin, Tout))
                {
                    checknodevalid = true;
                    return true;
                }
                else
                {
                    // could check if transtyping could be applied ( later ) 
                    checknodevalid = false;
                    return false;
                }*/
            }
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
            if (slotspos.Count != ArglistFQ.Count)
                return false;
            for (int c = 0; c < ArglistFQ.Count; c++)
                if (slotspos[c].TypeFullString != ArglistFQ[c])
                    return false;
            return true;
        }
        /// <summary>
        /// slot consistency check failed 
        /// rebuild the slot list 
        /// </summary>
        public void REbuildSlots()
        {
            slotspos.Clear();
            for (int c = 0; c < ArglistFQ.Count; c++)
            {
                int BZ = (int)MVPBUTTONSIZE.MVP32;
                SlotInfo S = new SlotInfo(c);
                Rect slotbuttonpos = new Rect(Windowpos.position.x - (BZ / 1.5f),
                        Windowpos.position.y + ((BZ) * c), // y got c offset
                        BZ, BZ); // size 
                S.linkedto = 0;
                S.linkedto = this.Guid.GetHashCode();
                // cannoit serialize type so i store full class  qualified name 
                S.TypeFullString = ArglistFQ[c] ; // to store type as full qualified classname
                S.index = c;
                ParameterInfo[] p = selectedmethodinfo.GetParameters();
                S.paramname = p[c].Name;
                S.R = slotbuttonpos;
                slotspos.Add(S);
            }
        }





        /// <summary>
        /// do the external node routine ( inside node editor view )
        /// </summary>
        /// 
        public virtual void DoNode()
        {
            
            m_gotfocus = BBDrawing.GetRectFocus(Windowpos, true);
            // collect the nodes that need to be deleted 
            List<BBCtrlNode> toremove = new List<BBCtrlNode>();
            // do the node and add to the trashbin if it s not relevant anymore
            foreach (BBCtrlNode N in SUBNodes)
            {
                N.DoNode();
                if (N.FlushNode)
                {
                    Debug.Log("MARK FOR DELETE "+N.NodeId);
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
            if (ArglistFQ.Count == 0)
                slotspos.Clear();



            //****************************************************** CREATE NEW SLOTS
            for (int c = 0; c < ArglistFQ.Count; c++)
            {
                if (!Checkslot())
                    REbuildSlots();
                //ParamFQ = ArglistFQ[c];

                slotspos[c].R.Set(
                                    Windowpos.position.x - (BZ / 1.5f),
                                    Windowpos.position.y + ((BZ) * c),
                                    BZ, BZ
                                   );
                // here the button is clicked and children is created 
                if (GUI.Button(slotspos[c].R, Textureloader.slot_questionmark, textonly))
                {
                    //float zoom = (isroot) ? 0.5f : 1.0f; // root childs smallers 
                    Rect NR = GetNewPosFromThisNode(slotYoffset); //new Rect(10, 50, 80, 80);
                    ParameterInfo[] p  =  selectedmethodinfo.GetParameters();
                    BBCtrlNode Child = new BBCtrlNode(NR,p[c].ParameterType.Name+ " " +  p[c].Name, this);
                    Child.ParentFeedSlotInfo = new SlotInfo(c); // but i put on recently created Slot classs
                    Child.ParentFeedSlotInfo.TypeFullString = ArglistFQ[c]; // Parent input type 
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

            // bool entryfit =(ReturnTye == Parent.slotspos[ParentFeedSlotInfo.index].T);



            if (!isroot)
            {
                if (BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID).slotspos.Count > ParentFeedSlotInfo.index)
                {

                    // can draw connection but check if the conection is valid 
                    if (!CheckNodesconection() || !checknodevalid)
                    {
                        checknodevalid = false;
                        linkcolor = Color.red;
                    }
                    else
                        linkcolor = Color.green;
                    BBDrawing.curveFromTo(outputslotbutton, BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID).slotspos[ParentFeedSlotInfo.index].R, linkcolor, Windowpos.width * 2, out velocity);
                }

            }


            // the node stuff that might be done out of the window ( slot etc )




        }





        /// <summary>
        ///  check if the graph could be invoked 
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        /// 


        public bool CheckfullGraph(BBCtrlNode N)
        {
            
            if ( BBCtrlNode.THEGRAPH.GraphOK ) // stop check at first invalid node connection
                foreach (BBCtrlNode Sub in N.SUBNodes)
                {
                    if (!Sub.checknodevalid)
                        BBCtrlNode.THEGRAPH.GraphOK = false;
                    else
                        CheckfullGraph(Sub);
                }

            if (!checknodevalid)
                BBCtrlNode.THEGRAPH.GraphOK = false;

            return BBCtrlNode.THEGRAPH.GraphOK;


        }


        
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
            //GUILayout.Label(gotfocus.ToString());
            // get Monobehavior and derived on the selected object 
            // if the node is related  to scene content 
            string[] localclassarray = FillClassArray(Selection.activeGameObject).ToArray();
            List<string> localmethodarray = new List<string>();
            // user action to select which one to inspect 
            LookupClassindex = EditorGUILayout.Popup(LookupClassindex, localclassarray);
            filteredmethods = BuildFilteredMethodArray(LookupClassindex);
            foreach (MethodInfo mi in filteredmethods)
                localmethodarray.Add(mi.Name);
            Lookupmethodindex = EditorGUILayout.Popup(Lookupmethodindex, localmethodarray.ToArray());

            string STR = "";
            if (ParamFQ != null)
                STR = NodeId.ToString();   //Paraminfo.ParameterType.ToString();

            EditorGUILayout.LabelField(STR);
            // store the fullQF class name
            if (NodeClassTypeArray.Count < LookupClassindex)
            {
                // EXIT FAIL 
                GUI.DragWindow(); // anyway
                checknodevalid = false;
                return;
            }

            if (NodeClassTypeArray.Count <= LookupClassindex)
            {

                GUILayout.Label(  "wrong selection for the node setup \n need a Game object ?  " ) ;
                selectedmethodinfo = null;
                checknodevalid = false;
                GUI.DragWindow(); // anyway
                return;
            }

            ClassnameFQ = NodeClassTypeArray[LookupClassindex].AssemblyQualifiedName;

            if (isroot)
            {
                THEGRAPH.GraphOK = true;
                if ( CheckfullGraph(this))
                    if (GUILayout.Button("INVOKE"))
                        Nodeinvoke(0);
            }

            string subnodesname = "";
            if (selectedmethodinfo != null)
            {
                // @@
                NodeDebuginfos += "\nReturn :" + selectedmethodinfo.ReturnParameter.ParameterType.Name;
                ParamFQ = selectedmethodinfo.ReturnParameter.ParameterType.AssemblyQualifiedName;
                
            }

            //if (! isroot) // root have no parent 
            //    foreach (SlotInfo si in BBCtrlNode.THEGRAPH.GetnodeFromID(ParentID).slotspos)
            //    NodeDebuginfos += "\n @slot " + si.index.ToString() + " pos :" + si.R.position.ToString() + "\ntype" + si.TypeFullString; 

            

            foreach (BBCtrlNode N in SUBNodes)
            {
                subnodesname += "child " + N.NodeId + "\n";
            }

            if (!isroot)
            {
                GUILayout.Label(subnodesname);
                GUILayout.Label(NodeDebuginfos);
            }
            UpdateNodeEntry(LookupClassindex, Lookupmethodindex);
            
            
            GUI.DragWindow(); // anyway

        }






        public object Nodeinvoke(int returnindex )
        {

            Dictionary<string, object> Args = new Dictionary<string, object>();

            List<object> objlist = new List<object>();

            object classInstance = Selection.activeGameObject.GetComponent(NodeClassTypeArray[LookupClassindex].Name); 
            if ( classInstance == null )
                classInstance = Activator.CreateInstance(NodeClassTypeArray[LookupClassindex]);


            if (classInstance == null)
            {
                Debug.Log("cannot get or create an instance of " + NodeClassTypeArray[LookupClassindex].Name);
                return null;
            }



            for (int c = 0 ; c< SUBNodes.Count;c++)
            {
                BBCtrlNode N = SUBNodes[c];

                N.Nodeinvoke(c);
                if (N.m_OutputObj==null)
                    Debug.Log("no result for node "+ N.NodeId.ToString() );
                Args.Add(N.name,N.m_OutputObj);
            }

            // push args in right order fo the call 
            for (int c = 0; c < slotspos.Count; c++)
                foreach ( KeyValuePair<string,object> kvp in Args )
                    if (kvp.Key.Contains(slotspos[c].paramname))
                        objlist.Add(kvp.Value);



            


            m_OutputObj = selectedmethodinfo.Invoke(classInstance, objlist.ToArray());


            return m_OutputObj;
        }

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
            NodeClassTypeArray.Add(typeof(BBBLocks.BBBLocks));
            foreach (System.Type T in NodeClassTypeArray)
            {
                L.Add(T.Name);
            }
            return L;
        }


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

            // descriptor for serialization 

            // for nodedesc 
            List<int> filteredmethodindex = new List<int>();

            List<MethodInfo> L = new List<MethodInfo>();
            for (int mc = 0; mc < MethodInfoList.GetLength(0); mc++)
            {
                object[] atributelist = MethodInfoList[mc].GetCustomAttributes(true);
                foreach (object o in atributelist)
                    if (o.GetType() == typeof(BBCtrlVisible) ||   BBCtrlNode.unfiltered )
                    {
                        L.Add(MethodInfoList[mc]); // store the index on the name 
                        filteredmethodindex.Add(mc);
                    }
                // try to get those anyway to replace by a customizable list of predefined methods 
                if (MethodInfoList[mc].Name == "GetDataset" || MethodInfoList[mc].Name == "GetActorProps")
                {
                    filteredmethodindex.Add(mc);
                    L.Add(MethodInfoList[mc]);
                }
            }

            // get an index on the methodes selected 


     
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
        public void UpdateNodeEntry(int CLIDX, int MTHIDX)
        {
            // CLIDX class index 
            // MTHIDX method index 
            // at this point we r supposed to have the class and method to inspect 

            // --------------------------------------------------------------------------------- 
            // real deal start here all the reflection part have to be done in this function 
            if (filteredmethods.Length < MTHIDX)
            {
                GUILayout.Label("selected method is out of range");
                checknodevalid = false;
                return; // FAIL 
            }

            selectedmethodinfo = filteredmethods[MTHIDX];
            
            LookupMethodName = selectedmethodinfo.Name;
            if (selectedmethodinfo != null)
                ReturnTye = selectedmethodinfo.ReturnType; // return type ys stored in the node 
            else
                GUILayout.Label(  BBCtrlNode.NodeDebuginfos );
            

            ArglistFQ .Clear();
            //Paraminfo = selectedmethodinfo.ReturnParameter;
            // we push the arg type table at node level 
            foreach (ParameterInfo pi in selectedmethodinfo.GetParameters())
                ArglistFQ.Add(pi.ParameterType.AssemblyQualifiedName);



            if (nodedebug)
                NodeDebugLogOutput();

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