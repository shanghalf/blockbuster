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
///  we start with a class used to describe input / output of a node 
///  and slot connection 
/// </summary>
[System.Serializable]
public class SlotInfo 

{
    public string paramname;                        // name of the node output parameter 
    public bool UIControllparamslot = false;        // a node controller is special node that get it s input parameter from the GUI element executed in it s window 
    public int index= 0 ;                           // index of the slot ( parameter index in fact used only in input slots 
    public int linkedto;                            // ID of the parent slot 
    public Rect R;                                  // slot position
    public string TypeFullString;                   // full qualified string of the output parameter 
    public bool isoutputslot = false;               // is slot an output or input 
    public bool isemiter = false;                   // emiter is necessary output slot and can be linked to multiple input slot 
    public bool isreceiver = false;                 // receiver is a input slot linked to an emiter slot 
    public object EmitedObject = null;              // the object returned by the node ( may be not necessary ) 

    public int ownerID ;

    /// <summary>
    ///  default constructor for serialization
    /// </summary>
    public SlotInfo()
    {
    }

    public SlotInfo(bool isoutput , int islinkedto , int ownerid )
    {
        isoutputslot = isoutput;
        linkedto = islinkedto; 

    }


    /// <summary>
    /// overide constructor with a predefined index 
    /// </summary>
    /// <param name="zid"></param>
    public SlotInfo(int zid)
    {
        index = zid;
    }
}



/// <summary>
/// hold the link EmiterNode / receiverslotinfo for this graph 
/// </summary>
[System.Serializable]
public class EmiterDesc
{
    int NodeID;
    public List<int> ReceiverID = new List<int>();
    public List<SlotInfo> ReceiverSlotInfo = new List<SlotInfo>();

    public EmiterDesc()
    {
    }

    public static void UnregisterReceiver (BBCtrlNode n )
    {
        foreach (BBCtrlNode bbc in NodeGraph.EditedControll.thisgraph.Nodes)
            foreach (SlotInfo si in n.slotspos)
                bbc.broadcast.ReceiverSlotInfo.Remove(si);

    }

    
    public void AddReceiver (int ID , SlotInfo SI )
    {
        
        bool flag = false ;
        foreach (SlotInfo S in ReceiverSlotInfo)
            if (SI == S)
            {
                flag = true;
                break;
            }
        if (!flag)
        {
            ReceiverSlotInfo.Add(SI);
        }

    }

    public void RemoveReceiver(int ID, SlotInfo SI)
    {
        
        ReceiverID.Remove(ID);
        ReceiverSlotInfo.Remove(SI);
    }



}



/// <summary>
/// the graphnode class with extra serializable attribute 
/// this class describe the full graph and provide methods for load and save 
/// </summary>
[System.Serializable]
[XmlInclude(typeof(Vector3))]
[XmlInclude(typeof(TXTINDEX))]
[XmlInclude(typeof(AnimationCurve))]
[XmlInclude(typeof(BBGameObjectHandle))]
[XmlInclude(typeof(EmiterDesc))]
public class NodeGraph
{

   


    // list of nodes broadcasting their result into this graph 
    public List<int> EmiterIDlist = new List<int>();
    public static bool gamemode = true;                                                              // the special UI nodes have to do different thing depending on the context ( editor / runtime )
    public static bool editoropen;                                                                   // is editor open ?
    public static BBControll EditedControll = new BBControll();                                      // the BBControll linked to this graph 
    public bool GraphOK = true;                                                                      // validate the graph if no error are spotted in logic 
    public string filename;                                                                          // name of the XML descriptor file to save and load this graph
    public  BBCtrlNode ROOTNODE;                                                                             // rootnode is the entry point for all the nodetree manipulation ( edit or execute )
    public  BBCtrlNode EMITERROOTNODE = new BBCtrlNode();       // Handle Emiters
    public List<BBCtrlNode> Nodes = new List<BBCtrlNode>();                                          // the list of nodes used in this graph
    public List<int> nodekeys = new List<int>();                                                     // separated ID of the Node List ( cannot serialize a Dictionary ) 
    public Guid Guid = Guid.NewGuid();                                                               // like everywhere else the GUID is the main ID of the component ( graph file name graph id etc  )



  


    /// <summary>
    /// return the node matching the id 
    /// id is the GUI Hashcode enought for solid identifier and more convenient to manipulate than a GUID string 
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public BBCtrlNode  GetnodeFromID (int ID)
    {
        foreach (BBCtrlNode BBC in Nodes)
            if (BBC.Guid.GetHashCode() == ID)
                return BBC;
        return null;
    }

    /// <summary>
    /// clear the couple of array KEY and NODE
    /// i use 2 separated arrays instead of one dictionnary cause a dic is not easily serializable 
    /// could use a dic and split in 2 array for serialisation .. same .. 
    /// </summary>
    public  void FlushBuffer()
    {
        Nodes.Clear();
        nodekeys.Clear();
    }
    /// <summary>
    /// Save the graph .. 
    /// </summary>
    /// <param name="saveas"></param>
    public void Save(bool saveas=false)
    {

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

    /// <summary>
    /// loading is a liitle more complex 
    /// the node buffer have to be restored in the right order 
    /// to fit the key buffer 
    /// </summary>
    /// <param name="bbc"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [BBCtrlVisible]
    public static NodeGraph LoadGraph(BBControll bbc,string path =null)
    {
        // opening a BBControll without graph file 
        // will open automatically the node editor to create one 
        if (bbc.Graphfilename==null || ! File.Exists( bbc.Graphfilename  ) )
        {
            if (path == null)
            {
                EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
                return null;
            }

        }

        // without specified path we open the graph file name stored in the BBControll 
        // can cause some troubles : the pathe is defined automatically , allways exist but 
        // the file could be actually not saved ( in such case the statment above should open the graph editor 
        // but i think we ll need to change this a little cause the editor wont open in game mode  
        string pathtoload = (path == null) ? bbc.Graphfilename : path;
        // deserialize the graph 
        NodeGraph NG = new NodeGraph();
        XmlSerializer serializer = new XmlSerializer(typeof(NodeGraph));
        Stream stream = new FileStream(pathtoload, FileMode.Open);
        NG = serializer.Deserialize(stream) as NodeGraph;
        stream.Close();

        // A create a list to process the hierarchy 
        List<BBCtrlNode> processnodelist = new List<BBCtrlNode>();
        //processnodelist.Add(NG.ROOTNODE);
        //processnodelist.Add(NG.EMITERROOTNODE);
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
        // rootnode should not be moved to the nodes buffer 
        foreach (BBCtrlNode node in processnodelist)
        {
            if (node.isroot)
            {
                NG.ROOTNODE = node;
            }
            else if (node.isemiterroot)
            {
                NG.EMITERROOTNODE = node;
            }
            else if (! node.isemiterroot && !node.isroot)
            {
                if ((NG.nodekeys.FindIndex(node.Guid.GetHashCode().Equals) < 0))
                {
                    NG.nodekeys.Add(node.Guid.GetHashCode());
                    NG.Nodes.Add(node);
                }
            }
        }

        // such case would be terrible and this log is for debug
        if (NG.ROOTNODE == null)
            BBDebugLog.singleWarning("rootnode is null after load graph");

        // rise a dirty flag ( may be this flag should be moved to graphnode classe 
        // dont be confused the full graph is dirty not only a node 
        BBCtrlNode.dirty = true;

        //NodeGraph.EditedControll.thisgraph = NG;
        BBDebugLog.singleWarning("loaded graph is " + pathtoload);

        NodeGraph.EditedControll.thisgraph = NG;

        foreach (BBCtrlNode b in NG.EMITERROOTNODE.SUBNodes)
        {
            foreach (SlotInfo si in b.broadcast.ReceiverSlotInfo)
                for (int c = 0; c < b.broadcast.ReceiverSlotInfo.Count; c++ )
                    b.broadcast.ReceiverSlotInfo[c] = NodeGraph.EditedControll.thisgraph.GetnodeFromID(si.ownerID).slotspos[si.index]; 
        }

        return NG;
    }

    

}


    /// <summary>
    /// This iss the implementation of a Node 
    /// there s basically 2 bigs things happen here 
    /// first is DoNode() this function do the External function of the Node ( links , slots , Comunication with it s BBControll 
    /// and 
    /// DonodeWindow , which is the internal node Function ( a node is roughly a window and Donodewindow is the associated Window Callback for this node )
    /// internal method ( DonodeWindow ) will process the node function according to it s input and outputs 
    /// a UI controll node ( BBUInode class ) do not expose it s unique input parameter and will generate it s output from the encapsulated 
    /// UI controll , it s getting more difficult to keep the value ( external to the window function ) when the graph is executed in game mode 
    /// where the UI is not accessible , the point to remember is A UI NODE GET IT S INPUT NOT FROM A CHILD BUT FRON THE EMBEDED UI CONTROLL
    /// </summary>
    /// 

    public class BBCtrlNode
    {
        // statics props 
        public static bool dirty;
        public static  string Nodeinfos = "";
        public EmiterDesc broadcast = new EmiterDesc();

        public  bool flyover = false;


        [XmlIgnore]
        public float RandomSeed = UnityEngine.Random.Range(1f, 3f);

        [XmlIgnore]
        public Vector2 BestPos = new Vector2();

        public bool pined = false;


        // static are not serialized ( implicitely ) and [XmlIgnore] attribute prevent the value to be serialized 
        // cause it s meanless or simply not serializable 
        // a BBctrlNode have to store a lot of datas ... may be i will dedicate a class to manipulate then in a more convenient way 
        // that would be nice 

        public String GraphPerf="not evalued yet";                                              // benchnmark node or part of it 
        public static int CheckPerfIterationNumber;                                             // number of loop to perfonm in invoke perf test 
        public static string hierarchy = "";                                                    // subnode list 
        public static Rect ROOTPOS = new Rect(Screen.width, Screen.height / 2, 200, 200);       // defaut pos for a new root window 
        public static Rect RMIN = new Rect(Screen.width, Screen.height / 2, 100, 100);          // for popup ghost anim
        public static Rect RMAX = new Rect(Screen.width, Screen.height / 2, 400, 400);          // for popup ghost anim
        public static bool editordebugmode = false;                                             // global flag to switch editor in debug mode 
        public static bool showgrid = false;                                                    // display debug grid and helpers          
        public static bool unfiltered = false;                                                  // define if methodinfo list is limited to bbvisible 
        public static bool autopos = false;                                                  // autopos
        public static int slotYoffset = 0;                                                      // offset for slot creation ++ is size of node       
        private static int WINDOWHEADOFFSET = 20;                                               // ofset from head of windo bar 
        public static bool editorfocused;                                                       // is editor on focus 
        static GUIStyle textonly = new GUIStyle();                                              // gui button style for slots
        static int BZ = (int)MVPBUTTONSIZE.MVP32;                                               // movepad button size (could be found in layer desc  test res )
        static Dictionary<string, object> ControllsArgs = new Dictionary<string, object>();     // for ui nodes <unused> <todo> change single handle for dic
        [XmlIgnore]
        public object objtoinvoke;                                                              // store the GameObject that holding the BBctrl
        public object controllarg;                                                              // single handle for controll arg to replace with dic
        public string controllargname;                                                          // controll arg name for serialization
        public bool needinvoke;                                                                 // some GUIcontroll need invoke some dont ( use directly the stored object..depending )
        [XmlIgnore] // every node should keep a handle on it s own graph 
        public NodeGraph NodeGraphHandle;                                                       // the Graph associated with this node 
        [XmlIgnore] // cannot serialize that 
        public List<BBCtrlNode> SUBNodes = new List<BBCtrlNode>();                              // handle on this node sub hierarchy
        // but keep a key list instead and rebuild on deserialize 
        public List<int> SUBNodesKEY = new List<int>();                                         // nodes cannot be serialized just id to rebuild on load 
        public int NodeId;                                                                      // the node id ( GUID hashtag )
        public int ParentID;                                                                    // it s parent ID
        [XmlIgnore]
        MethodInfo[] filteredmethods;                                                           // method list ( not allways filtered for perf optim )
        [XmlIgnore]
        public Vector2 velocity = Vector2.zero;                                                 // for node physic ( FUN FACTOR )
        public Rect Windowpos;                                                                  // position of this node 
        public Rect debugWindowpos;
        public string name;                                                                     // for window title 
        public string ClassnameFQ;                                                              // full qualified string of the class used for this function
        public string classnameshort;                                                           // store the short name to avoid a string manip on node invoke ( optim ) 
        public string FunctionnameFQ;                                                           // full qualified function name 
        public System.Guid Guid;                                                                // this is the real identifier 
        public String ParamFQ;                                                                  // full qualified name of the returned param     
        public bool iscontroll = false;                                                         // guiNodes use a diferent flow 
        private bool checknodevalid = true;                                                      // is this node ready to invoke 
        public bool nodewarning = false;                                                        // warning turn links to grey but are not error just missmatch on obj selection for instance 
        [XmlIgnore]
        public object m_OutputObj;                                                              // object generated after invoke 
        public int LookupClassindex;                                                            // for ui popup ddlist in donodewindow callback 
        public string lookupclassname;                                                          // short name ??? may be duplicated @@@@
        public int Lookupmethodindex;                                                           // for gui popup ddlist in  donodewindow callback        
        public string LookupMethodName;                                                         // may be duplicated .. short name of the method @@@@
        public List<ParameterInfo> Arglist = new List<ParameterInfo>();                         // list of args used by this function
        public List<SlotInfo> slotspos = new List<SlotInfo>();                                  // slot info for args (input)
        public SlotInfo ParentFeedSlotInfo;                                                     // return (output) info 

        public List<SlotInfo> ReceiverSlotInfo = new List<SlotInfo>();                          // slot that take this node parameter remotely 


        
        public int inspectedinputslotindex;                                                     // the slot index actually inspected ( for popup nemu as far as i remember ) 
        public static SlotInfo slotselectorhandle = new SlotInfo();                             // slot handle for slotselector 
        [XmlIgnore]
        public static bool outputslotselector = false;                                                 // flag used to popup a dropdown list on slot to change the slot type 
        [XmlIgnore]
        public static bool inputslotselector = false;                                                  // popup menu on input 
        [XmlIgnore]
        public static bool  EmiterSelector=false;                                                      // when nodes are ready to emit the popu menu might display a list of node to connect 
        public int selectedemiter;                                                              // selected emmiter 
        public SlotInfo outputslot;                                                             // the output slot info 
        MethodInfo selectedmethodinfo;                                                          // current method standing 
        public bool isroot = false;                                                             // this node is a rootnode 
        public bool isemiterroot = false;                                                             // this node is a rootnode 

        [XmlIgnore]
        private System.Type lookupclasstype;                                                    // keep a handle on type <unused but should to save a lot of gettype>
        [XmlIgnore]
        public System.Type ReturnTye;                                                           // handle on returned param type 
        [XmlIgnore]
        List<System.Type> NodeClassTypeArray = new List<Type>();                                // list of function type
        [XmlIgnore]
        public static List<System.Type> MandatoryNodeClassTypeArray = new List<Type>();         // user defined class to add on lookup
        [XmlIgnore]
        public bool m_gotfocus = false;                                                         // if this node is focused or not 
        [XmlIgnore]
        public Rect outputslotbutton;                                                           // output slot position
        [XmlIgnore]
        public Rect RectCurrent = new Rect();                                                   // for popup creation ghost effect 
        [XmlIgnore]
        public Rect RectDest = new Rect();                                                      // popup ghost effect 
        [XmlIgnore]
        public Rect Rectorg = new Rect();                                                       // same 
        [XmlIgnore]
        public bool maximized=false;                                                                  // will be used to define the node content to display on size 
        [XmlIgnore]
        public bool minimized=true;                                                                  // will be used to define the node content to display on size 
        [XmlIgnore]
        public EditorTimer Timer = new EditorTimer();                                           // a timer for everything that move 
        [XmlIgnore]
        public bool nodedebug;                                                                  // local node debug info
        [XmlIgnore]
        public bool FlushNode = false;                                                          // if a node get corrupted it s removed from graph
        [XmlIgnore]
        public OUTSLOTTYPE outslottype;                                                         // to hold the Slot DropDownList out index
        [XmlIgnore]
        public INSLOTTYPE inslottype;                                                           // to hold the Slot DropDownList in index


        /// <summary>
        /// destructor defaut 
        /// </summary>
        ~BBCtrlNode()
        {
   
        }
        /// <summary>
        /// Refresh the Nodes and Key buffer ( no dict for serialisation reason ) 
        /// </summary>
        public void REcusiveCollectNodes()
        {
            foreach (BBCtrlNode N in SUBNodes)
                N.REcusiveCollectNodes();
            NodeGraph.EditedControll.thisgraph.Nodes.Add(this);
            NodeGraph.EditedControll.thisgraph.nodekeys.Add(Guid.GetHashCode());
        }
        /// <summary>
        /// use external function to be sure that i ll update 
        /// both node buffer and key buffer 
        /// a good thing would be to use a dictionary for it s safe access 
        /// and plit in 2 array only at serialize / deserialise 
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="child"></param>
        public void NodeAddChild(BBCtrlNode Parent, BBCtrlNode child)
        {
            Parent.SUBNodesKEY.Add(child.Guid.GetHashCode());
            Parent.SUBNodes.Add(child);
        }
        /// <summary>
        /// same comment as nodeaddchild
        /// remove this node from parent buffers 
        /// </summary>
        public void NodeRemoveThis()
        {
           NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).SUBNodesKEY.Remove(Guid.GetHashCode());
           NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).SUBNodes.Remove(this);
           Debug.Log("remove this " + this.NodeId + " from parent " + NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).NodeId);
            // unregister node slot from all emiters 
           foreach (BBCtrlNode bbc in NodeGraph.EditedControll.thisgraph.Nodes)
               if (bbc.outputslot.isemiter)
                   foreach (SlotInfo si in slotspos)
                       bbc.broadcast.ReceiverSlotInfo.Remove(si);

           NodeGraph.EditedControll.thisgraph.Nodes.Remove(this);
           NodeGraph.EditedControll.thisgraph.nodekeys.Remove(Guid.GetHashCode());
        }
        /// <summary>
        /// same comment as nodeaddchild
        /// i think both function areuseless i should use only this static one 
        /// </summary>
        public static void NodeRemoveChild(BBCtrlNode pParent, BBCtrlNode child)
        {
            
            foreach (BBCtrlNode n in child.SUBNodes)
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
            name = newname;
            // a node without parent is necessary the rootnode 
            // the graph system do not manage multiroot (pointless but may be for future extenssion)
            if (nodeParent == null)
                isroot = true;
            else
            {
                ParentID = nodeParent.NodeId;
                NodeAddChild(NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID), this);
            }
            // the node created in editor is obviously the current edited node 
            if (NodeGraph.EditedControll.thisgraph != null)
            {
                NodeGraph.EditedControll.thisgraph.Nodes.Add(this);
                NodeGraph.EditedControll.thisgraph.nodekeys.Add(Guid.GetHashCode());
            }
            else
                BBDebugLog.singleWarning("ERROR DURING NODE CREATION");
        }
        /// <summary>
        /// return this index in parent child buffer (same as slot index)
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
        /// this would improve the node interop and avoid anoying use of convert nodes 
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
        /// with minimal info ( placeholder ) 
        /// </summary>
        public BBCtrlNode()
        {
            // default constructor for serialization 
            Guid = System.Guid.NewGuid();
            Windowpos = BBCtrlNode.ROOTPOS;
            //ArgsList = new List<ParameterInfo>();
            name = "NEW NODE";
            textonly.imagePosition = ImagePosition.ImageOnly;
            //EditorUtility.DisplayDialog("creator", "create ", "ok");
            isroot = true;
        }
        /// <summary>
        /// node placement ( could be better )
        /// the grid display is a early step of a better node placement 
        /// actually just an ofset left to the parent node 
        /// </summary>
        /// <param name="nb"></param>
        /// <returns></returns>
        public Rect GetNewPosFromThisNode(int nb)
        {
            Rect Newrect = new Rect();
            float width = Windowpos.width;
            float height = Windowpos.height ; 
            float px = Windowpos.x - (width * 1.8f);
            float py = Windowpos.y + ((height * 1.2f) * nb); // y ofset 
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
            }
        }
        /// <summary>
        /// check slot connection consistency 
        /// probably duplicated logic with checkconnection 
        /// need to merge both 
        /// </summary>
        /// <returns></returns>
        public bool Checkslot()
        {
            // first thing to check is if the slot number 
            // match with the arg number required by the function of the node 
            if (slotspos.Count != Arglist.Count)
            {
                BBDebugLog.singleWarning("slot number do not match arg list on " + name + " " + Guid.GetHashCode().ToString());
                checknodevalid = false;
                return false;
            }
            // then if the type match as well 
            for (int c = 0; c < Arglist.Count; c++)
                if (Arglist[c].ParameterType != null)
                    if (slotspos[c].TypeFullString != Arglist[c].ParameterType.AssemblyQualifiedName)
                    {
                        BBDebugLog.singleWarning("slot type do not match parameter on " + name + " " + Guid.GetHashCode().ToString());
                        checknodevalid = false;
                        return false;
                    }
            return true;
        }
        /// <summary>
        /// slot consistency check failed 
        /// rebuild the slot buffer 
        /// </summary>
        public void REbuildSlots()
        {

            if (Arglist.Count > 0 )
                slotspos.Clear();
            BBDebugLog.Log("Rebuild Slots Info Buffer ");


            for (int c = 0; c < Arglist.Count; c++)
            {
                int BZ = (int)MVPBUTTONSIZE.MVP32;// i use a custom enum for buttonsize just cosmetic stuff
                SlotInfo S = new SlotInfo(c); // create a slot with C index 
                Rect slotbuttonpos = new Rect(Windowpos.position.x - (BZ / 1.5f),
                        Windowpos.position.y + ((BZ) * c), // y got c offset
                        BZ, BZ); // size 
                
                S.ownerID =this.Guid.GetHashCode();

                // cannoit serialize type so i store full class  qualified name 
                if (Arglist[c].ParameterType!= null )
                    S.TypeFullString = Arglist[c].ParameterType.AssemblyQualifiedName ; // to store type as full qualified classname
                S.index = c;
                // important : the slot order have to match the parameter index 
                ParameterInfo[] p = selectedmethodinfo.GetParameters();
                S.paramname = p[c].Name;
                S.R = slotbuttonpos;
                // a slot for BUInode should not display 
                if (iscontroll)
                    S.UIControllparamslot = true;
                slotspos.Add(S);
            }
        }

    



        /// <summary>
        ///  contextual menu on output node button
        ///  this action define the type of the output ( linked or emiter )
        ///  or delete the node 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool DoOutputslotselector()
        {

            if (nodedebug) // to place a breakpoint only in inspected for debug 
                BBDebugLog.singleWarning("inspect_" + Guid.GetHashCode().ToString());

            GUIStyle s = new GUIStyle(); // blank to display only the bitmap 
            GUI.SetNextControlName("Outputslotselector"); // this statment hole the name of the next controll 
            outslottype = (OUTSLOTTYPE)EditorGUI.EnumPopup(outputslotbutton, outslottype, s); // enumddlist

            if (outputslot == null) // for compatibility with previous xml formal 
                outputslot = new SlotInfo();

            // need an handle on the node parent ( to connect slots and nodes ) 
            BBCtrlNode parent;
            // compose an easy to parse string 
            //string emiterid = outputslot.paramname + "_" + name + "_" + Guid.GetHashCode().ToString();

            // if the node is not linked ( could happen in future version for temporary edition purpose )
            // should deploy another popup to pick a unlinked node action ( linkto / delete / emit ) 
            int I = GetThisNodeIndexInParentSlots();  // return -1 if this node is not linked 
            parent = NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID); // catch this node  parent
                switch (outslottype)
                {
                    case OUTSLOTTYPE.NORMAL: // setup a regular output slot 

                        // this node switch from emiter to normal 
                        // need a function to select parent slot assignation 
                        // meanwhile  delete 
                        outputslot.isemiter = false;
                        outputslot.isoutputslot= true;
                        NodeGraph.EditedControll.thisgraph.EmiterIDlist.Remove(Guid.GetHashCode());

                        foreach (SlotInfo si in broadcast.ReceiverSlotInfo)
                            si.linkedto = -1;
                        broadcast.ReceiverSlotInfo.Clear();
                        BBCtrlNode.dirty = true;
                        break;
                    case OUTSLOTTYPE.EMITTER:
                        outputslot.isoutputslot = true; 
                        outputslot.isemiter = true; // no comment 
                        if (broadcast == null) // need a valid instance  of emiterdesc ( link list parentslot/node )
                            broadcast = new EmiterDesc();
                        if (NodeGraph.EditedControll.thisgraph.EmiterIDlist.FindIndex(Guid.GetHashCode().Equals) < 0)
                            NodeGraph.EditedControll.thisgraph.EmiterIDlist.Add(Guid.GetHashCode());
                        // remove this node from parent subs and assign EMITERROOT as parent 
                        NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).SUBNodes.Remove(this);
                        NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).SUBNodesKEY.Remove(Guid.GetHashCode());
                        if (NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodesKEY.FindIndex(Guid.GetHashCode().Equals) < 0)
                        {
                            NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodes.Add(this);
                            NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodesKEY.Add(Guid.GetHashCode());
                        }
                        ParentID = NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.Guid.GetHashCode();
                        NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.isemiterroot = true;
                        ParentFeedSlotInfo = null;
                        pined = true;
                        BBCtrlNode.dirty = true;
                        break;
                    case OUTSLOTTYPE.REMOVE:
                        // iterate on all nodes and removve this inputslots from receivers lists 
                        EmiterDesc.UnregisterReceiver(this); 
                        // remove from graph emiternodelist
                        NodeGraph.EditedControll.thisgraph.EmiterIDlist.Remove(Guid.GetHashCode());
                        // physicaly remove from EMITERROOT
                        if (outputslot.isemiter)
                            NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodes.Remove(this);
                        outputslot.isemiter = false;
                        BBCtrlNode.dirty = true;
                        return false; // if this function return false the node is pushed in node garbage 
                    case OUTSLOTTYPE.PIN:
                        // pined is to prevent the node to react to 
                        // tree drag/move 
                        pined = true;
                        break;
                    case OUTSLOTTYPE.UNPIN:
                        pined = false;
                        break;
                }
            outslottype = default(OUTSLOTTYPE);
            BBDrawing.popupmenuopen = false;
            return true;

        }

    


        /// <summary>
        /// 
        /// </summary>
        public void DoEmiterSelector()
        {
            // to prevent any fail while method s not selected 
            // a controll ( GUI controll ) node have an hiden slot 
            // the value is coming from the node body ( the controll ) 
            // but for the edition loop it use an invisible slot , so there s no point to display nor 
            // tweak the slot entry 
            if (selectedmethodinfo == null || iscontroll)
                return;
            // emiter list is the dropdown list created with fixed element for 
            // the node flow ( create / cancel .. ) the bottom of list is made of the name of 
            // actives emiters in a formated string that make easy to parse and isolate 
            // component values A_B_C  string>> split("_")[A,B,orC]
            List<string> EmiterList = new List<string>();

            // fixed part 
            EmiterList.Add("CLEAR");
            EmiterList.Add("CREATE");

            // the emiter ddlist enum 
            foreach (int i in NodeGraph.EditedControll.thisgraph.EmiterIDlist)
            {
                BBCtrlNode bbc = NodeGraph.EditedControll.thisgraph.GetnodeFromID(i) ;
                if ( bbc != null )
                if (bbc.Guid != Guid) // a node inpuc cannot use it s own output 
                    EmiterList.Add(NodeGraph.EditedControll.thisgraph.GetnodeFromID(i).name + "_" + NodeGraph.EditedControll.thisgraph.GetnodeFromID(i).Guid.GetHashCode());
            }
            // check if inspectedinputslotindex is in the slot buffer scope 
            // inspectedinputslotindex is defined on mousedown by getting the slot index and 
            // stand in a global value .. should be accurate since the ddlist show up only on a slot clic 
            // but i strongly believe that kind of check could save your day when you change the things around and impact that logic 
            if (slotspos.Count <= inspectedinputslotindex)
            {
                // to prevent console to get flooded i use a debug output that keep track of messages to avoid a per tick string output 
                BBDebugLog.singleWarning("the inspected slot do not fit with node slot buffer " + name + Guid.GetHashCode().ToString());
                return;
            }
            // the GUI dropdown list for emiter selection 
            selectedemiter = EditorGUI.Popup(slotspos[inspectedinputslotindex].R, selectedemiter, EmiterList.ToArray());

            // perform the ddlist routine 
            switch (selectedemiter)
            {

                case 0:

                    foreach (BBCtrlNode bbc in NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodes)
                        bbc.broadcast.ReceiverSlotInfo.Remove(slotspos[inspectedinputslotindex]);

                    slotspos[inspectedinputslotindex].isreceiver = false;
                    slotspos[inspectedinputslotindex].linkedto = -1;
                    selectedemiter = -1;
 


                    break;
                case 1:
                    // CREATE a new node and plug to this slot 
                    // <todo> for debug i keep creating another node but multiple node entry have no point and should be unalowwed 
                    // in final version 

                    // calculate the node position from this node 
                    Rect NR = GetNewPosFromThisNode(slotYoffset); 
                    // selected method is defined in DonodeWindow ( the node internal callback function ) 
                    // now we create the input slots for this new node 
                    // it have to be initialized correctly in any case even with default method / class selected 
                    ParameterInfo[] p = selectedmethodinfo.GetParameters();
                    // name according to this node parameter 
                    BBCtrlNode Child = new BBCtrlNode(NR, p[inspectedinputslotindex].ParameterType.Name + " " + p[inspectedinputslotindex].Name, this);
                    
                    Child.ParentFeedSlotInfo = slotspos[inspectedinputslotindex];           // link parent / child 
                    Child.RectDest.Set(NR.xMin, NR.yMin, NR.width, NR.height);              // for popup anim
                    Child.Rectorg = slotspos[inspectedinputslotindex].R;                    // for popup anim 
                    Child.outputslot = new SlotInfo(true , Guid.GetHashCode() , Child.Guid.GetHashCode() );                                  // the output slot info 
                    Child.outputslot.R = new Rect(0, 0, 16, 16);                            // sise of the slot button
                    Child.outputslot.isoutputslot = true;                                   // look weird to specify this probably useless but ..
                    Child.Timer.StartCountdown(0.5f);                                       // start a ghost effect timer ( why make it simple ) 
                    slotspos[inspectedinputslotindex].linkedto = Child.Guid.GetHashCode();  // later i ll just rely on slot id ( for potential memory issue ) 
                    //dirty = true;                                                           // mark nodetree as dirty to trigger a nodegraph rebuild 
                    slotspos[inspectedinputslotindex].isreceiver = false;                   // not a remote input 
                    selectedemiter = 0;
                    break;
                    
                default:
                    if (selectedemiter < 0)
                        break;

                    //foreach (BBCtrlNode bbc in NodeGraph.EditedControll.thisgraph.EMITERROOTNODE.SUBNodes)
                    //bbc.broadcast.ReceiverSlotInfo.Remove(slotspos[inspectedinputslotindex]);




                    Debug.Log(EmiterList[selectedemiter]);
                    string idstring = EmiterList[selectedemiter].Split(char.Parse("_"))[1];
                    int ID = (int) Convert.ToInt32(idstring);
                    BBCtrlNode emiternode = NodeGraph.EditedControll.thisgraph.GetnodeFromID(ID);
                    int hi =  emiternode.broadcast.ReceiverSlotInfo.FindIndex(slotspos[inspectedinputslotindex].Equals);
                    if (hi<0)
                        emiternode.broadcast.ReceiverSlotInfo.Add ( slotspos[inspectedinputslotindex] );
                    slotspos[inspectedinputslotindex].isreceiver = true;
                    slotspos[inspectedinputslotindex].ownerID = Guid.GetHashCode();
                    slotspos[inspectedinputslotindex].linkedto = emiternode.Guid.GetHashCode();
                    BBCtrlNode.dirty = true;


                break;
            }



        }



        /// <summary>
        /// add and remove node shake the nodes buffer info 
        /// this function rearange in the arglist order 
        /// this is mainly for node placement in draw function that use the slot index to 
        /// calculate ofset btwn nodes 
        /// </summary>
        public void REorganiseNodeInfo()
        {
            List<BBCtrlNode> Rnodes = new List<BBCtrlNode>();
            List<int> Rkey = new List<int>();

            int i, c;

            for (c = 0; c < Arglist.Count; c++)
            {
                for (i = 0; i < SUBNodes.Count; i++)
                    if (Arglist[c].Name == SUBNodes[i].name)
                    {
                        Rnodes.Add(SUBNodes[c]);
                        Rkey.Add(SUBNodesKEY[c]);
                    }
            }

            SUBNodes.Clear();
            SUBNodesKEY.Clear();
            SUBNodes = Rnodes;
            SUBNodesKEY = Rkey;

        }



        /// <summary>
        /// DO NODE EXTERNAL 
        /// </summary>
        /// <returns></returns>
        public virtual bool DoNode()
        {
            BBDrawing.CheckInput();
            // action on output slot return false is a delete node code 

            if (nodedebug) // to place a breakpoint only in inspected for debug 
                BBDebugLog.singleWarning("inspect_" + Guid.GetHashCode().ToString());

            if (!DoOutputslotselector())
                return false;

            DoEmiterSelector();

            //GUI.SetNextControlName("donode");
            Color linkcolor = Color.green;                                                          // color of the connecting line ( change according to
            GUIContent sloticon; // to draw invisible frame 
            if (isroot)
                sloticon = Textureloader.slot_main_output;
            else
                // this next statment is for compatibility with old xml that not handle the outputslotinfo 
                if (outputslot != null)
                    sloticon = (outputslot.isemiter) ? Textureloader.slot_emitter : Textureloader.slot_ok;
                else
                    sloticon = Textureloader.slot_ok; 
            // this node is foccused actualy 
            if ( ! BBDrawing.mousedrag )
                m_gotfocus = BBDrawing.GetRectFocus(Windowpos, out flyover, true);

            //BBDrawing.mousedrag = false;
            //BBDrawing.mousedown = false;

            //EditorGUILayout.LabelField((Event.current.mousePosition.y.ToString() + " : " + (Windowpos.y + WINDOWHEADOFFSET).ToString()));
            // should be the current edited graph ;
            
            NodeGraphHandle = NodeGraph.EditedControll.thisgraph;
            if (NodeGraphHandle == null)
                BBDebugLog.singleWarning("cannot find NodeGraphHandle ");

            // collect the nodes that need to be deleted 
            List<BBCtrlNode> toremove = new List<BBCtrlNode>();

            // RECURSIVE CALL 
            foreach (BBCtrlNode N in SUBNodes)
                if (! N.DoNode())
                    toremove.Add(N);
            foreach (BBCtrlNode N in toremove)
            {
                NodeRemoveChild(this, N);
            }
            // Perform Inside Action (REFLECTOR)
            /// ===================================================== OUTPUT SLOT 
            if (Timer.run && !isroot) // prevent the out slot to show up during popup anim 
            {
                DrawDeploychildren(Rectorg, RectDest);
                GUI.Box(RectCurrent, "");
                return true; // normal thing 
            }
            outputslotbutton = new Rect(Windowpos.width + Windowpos.position.x ,
                Windowpos.center.y,
                16, 16);



            /// CONTEXT MENU ON NODE INPUT OUTPUT CLIC

            /* definitely work fine to get last focused controll 
            if (GUI.GetNameOfFocusedControl().Contains("outputslotddlist"))
                Debug.Log("popup under focus ");
            if (outputslot == null) // for old xml 
                outputslot = new SlotInfo();
            */
            


            if (GUI.Button(outputslotbutton, sloticon, textonly))
                outputslotselector = true;
            /*
            if ( selectedmethodinfo != null )
                GUI.Label(new Rect(outputslotbutton.x, outputslotbutton.y-40, 300, 80), selectedmethodinfo.ReturnType.ToString());
            */
            // FUCTION DO NOT HAVE PARAMETERS 
            if (Arglist.Count != slotspos.Count)
            {
                BBDebugLog.singleWarning("arg number do not fit the slot number >> rebuild slot" + name + Guid.GetHashCode().ToString());
                REbuildSlots();
            }




            Windowpos = GUI.Window(Guid.GetHashCode(), Windowpos, DoNodeWindow, name);

            //****************************************************** CREATE NEW SLOTS
            for (int c = 0; c < Arglist.Count; c++)
            {
                if (!Checkslot())
                {
                    BBDebugLog.singleWarning("Checkslot returned false on " + name + Guid.GetHashCode().ToString() + " >> REbuildSlots");
                    REbuildSlots();
                }
                slotspos[c].R.Set(
                                    Windowpos.position.x - (16 ),
                                    Windowpos.position.y + ((18) * c),
                                    16, 16
                                   );



                // do not draw a UI parameter on controllnode
                if (!slotspos[c].UIControllparamslot)
                {
                    sloticon = (slotspos[c].isreceiver) ? Textureloader.slot_emitter : Textureloader.slot_ok;
                    if (GUI.Button(slotspos[c].R, sloticon, textonly))
                    {
                        inspectedinputslotindex = c;
                        inputslotselector = true;
                        BBDrawing.mousedown = false;
                        //BBDrawing.mousedrag = false;
                    }
                }
            }
      



            //REorganiseNodeInfo();

            BBDrawing.Calculatenodepos( this ,SUBNodes, Windowpos.position);


            // temporary draw link emiter / receiver
            if (outputslot.isemiter)
            {
                linkcolor = Color.white;
                foreach (SlotInfo si in broadcast.ReceiverSlotInfo)
                {
                    Vector2 V = NodeGraph.EditedControll.thisgraph.GetnodeFromID(si.ownerID).slotspos[si.index].R.center;

                    BBDrawing.curveFromTo(this, outputslotbutton.center, V, linkcolor, 0, out velocity);
                }
            }



            if (!isroot && !outputslot.isemiter )
            {
                // acting on node parent 
                BBCtrlNode P =  NodeGraphHandle.GetnodeFromID(ParentID);
                if (P == null || ParentFeedSlotInfo == null)
                    BBDebugLog.singleWarning("node :" + name +" have no parent >> Trash ");
                else
                    if (P.slotspos.Count > ParentFeedSlotInfo.index )
                    {
                        linkcolor = (pined) ? Color.cyan : Color.green;
                        if ( !checknodevalid)
                        {
                            BBDebugLog.singleWarning("node :" + name + Guid.GetHashCode().ToString()+ " have missconnection ");
                            linkcolor = Color.red;
                        }
                        if (nodewarning) linkcolor =   Color.gray ;
                        if (outputslot != null ) // old xml do not handle the outputslot info 
                            if (outputslot.isemiter)
                                linkcolor = Color.yellow;
                        Vector2 A = outputslotbutton.center;
                        Vector2 B = P.slotspos[ParentFeedSlotInfo.index].R.center;
                        //if ( !( P.slotspos[ParentFeedSlotInfo.index].isreceiver ) )
                        BBDrawing.curveFromTo(this,outputslotbutton.center, NodeGraph.EditedControll.thisgraph.GetnodeFromID(ParentID).slotspos[ParentFeedSlotInfo.index].R.center, linkcolor, Windowpos.width*1.5f , out velocity);
                        BestPos.x-=Windowpos.width;
                        BestPos.y-=Windowpos.width/2;
                    }
                }
            return true;
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
                //N.checknodevalid = false;
                //Debug.Log( name + "Fail c");
                return;
            }

            if (N.slotspos.Count != N.SUBNodes.Count  && !N.iscontroll )
            {
                valid = false;
                N.checknodevalid = false;
                BBDebugLog.singleWarning (name + " IS CONTROL : " + iscontroll.ToString() + " Fail d");
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
                /*
                if (parent.slotspos[I].TypeFullString != null)
                    GUI.Label(new Rect(parent.slotspos[I].R.x, parent.slotspos[I].R.y + 40, 300, 80), parent.slotspos[I].TypeFullString);
                */
                string A = parent.slotspos[I].TypeFullString;
                string B = N.ParamFQ;
                if (A == B)
                    valid = true;
                else
                    valid = false;

            }

            N.checknodevalid = valid;





        }





        




  



        public void BBDragWindow(Rect WP)
        {

            if (BBDrawing.mousedrag && BBDrawing.lastclicdown.y < WINDOWHEADOFFSET && m_gotfocus  )
                BBDrawing.movesubbranchnode(this, Event.current.delta); // the /2 is a fix cause something weird i dont get right now move subbranch twice or delta get double value ?? 

            GUI.DragWindow(); 
        }


        public int slotindexddl;
        public Vector2 nodedebugscrollpos;
        public int debugsection;


        

        /// <summary>
        /// display a windo with all node informations for Debug 
        /// </summary>
        /// 

        public string  DoNodeDebuginfos ()
        {

            if (!maximized)
                return null;

            List<string> debugsectionlist = new List<string>();
            List<string> LS = new List<string>();

            debugsectionlist.Add("NODE INFOS"); // 0
            debugsectionlist.Add("OUTPUT INFOS"); // 1 
            debugsectionlist.Add("INPUT INFOS"); // 2
            debugsectionlist.Add("POSITION INFOS");// 3 
            debugsectionlist.Add("EMITER INFOS");// 4
            debugsectionlist.Add("Child List"); // 5


            nodedebugscrollpos= GUILayout.BeginScrollView(nodedebugscrollpos);
            string Str = "";

            debugsection = EditorGUILayout.Popup(debugsection, debugsectionlist.ToArray());

            switch (debugsection)
            {

                case 0:
                    EditorGUILayout.LabelField("NODE ID:" + Guid.GetHashCode().ToString());
                    EditorGUILayout.LabelField("PARENT ID:" + this.ParentID);
                    EditorGUILayout.LabelField("Return :" + selectedmethodinfo.ReturnParameter.ParameterType.Name);
                    EditorGUILayout.LabelField("Nodevalid = " + checknodevalid.ToString());
                    EditorGUILayout.LabelField("method return : "+ selectedmethodinfo.ReturnParameter.ParameterType.AssemblyQualifiedName );
                    EditorGUILayout.LabelField("graph perf : " + GraphPerf) ;
                    EditorGUILayout.LabelField("iscontroll : " + this.iscontroll.ToString());
                    EditorGUILayout.LabelField("isroot : " + this.isroot.ToString());
                    EditorGUILayout.LabelField("lookupclassindex : " + this.LookupClassindex.ToString());
                    EditorGUILayout.LabelField("lookupclassname : " + this.lookupclassname);
                    if (this.lookupclasstype != null)
                        EditorGUILayout.LabelField("lookupclasstype : " + this.lookupclasstype.ToString());
                    EditorGUILayout.LabelField("Lookupmethodindex : " + this.Lookupmethodindex.ToString());
                    EditorGUILayout.LabelField("LookupMethodName : " + this.LookupMethodName);
                    EditorGUILayout.LabelField("m_gotfocus : " + this.m_gotfocus.ToString());
                    if ( this.m_OutputObj != null ) 
                        EditorGUILayout.LabelField("outputobject : " + this.m_OutputObj.ToString());
                    EditorGUILayout.LabelField("BestPos: " + this.BestPos.ToString());
                    EditorGUILayout.LabelField("FlyOver: " + this.flyover.ToString());

                    break;
                case 1:
                    EditorGUILayout.LabelField("Output slot");
                    EditorGUILayout.LabelField("---------------------------");
                    EditorGUILayout.LabelField("is emiter : " + outputslot.isemiter.ToString());
                    EditorGUILayout.LabelField("is output: " + outputslot.isoutputslot.ToString());
                    EditorGUILayout.LabelField("linked to : " + outputslot.linkedto.ToString());
                    EditorGUILayout.LabelField("paramname: " + outputslot.paramname);
                    EditorGUILayout.LabelField("Rect : " + outputslot.R.ToString());
                    EditorGUILayout.LabelField("typefullstring : " + outputslot.TypeFullString);
                    EditorGUILayout.LabelField("UIControllparamslot : " + outputslot.UIControllparamslot.ToString());
                    EditorGUILayout.LabelField("---------------------------");

                    break;
                case 2:
                if (slotspos.Count > 0)
                {
                    foreach (SlotInfo s in slotspos)
                        LS.Add("slot index : " + s.index.ToString());
                    slotindexddl = EditorGUILayout.Popup(slotindexddl, LS.ToArray());
                    EditorGUILayout.LabelField("Input slot" + slotspos[slotindexddl].index.ToString());
                    EditorGUILayout.LabelField("---------------------------");
                    EditorGUILayout.LabelField("slot index : " + slotspos[slotindexddl].index.ToString());
                    EditorGUILayout.LabelField("paramname: " + slotspos[slotindexddl].paramname);
                    EditorGUILayout.LabelField("Rect : " + slotspos[slotindexddl].R.ToString());
                    EditorGUILayout.LabelField("typefullstring : " + slotspos[slotindexddl].TypeFullString);
                    EditorGUILayout.LabelField("UIControllparamslot : " + slotspos[slotindexddl].UIControllparamslot.ToString());
                    EditorGUILayout.LabelField("Isreceiver : " + slotspos[slotindexddl].isreceiver.ToString());
                    EditorGUILayout.LabelField("linkedto : " + slotspos[slotindexddl].linkedto.ToString());
                    EditorGUILayout.LabelField("paramname : " + slotspos[slotindexddl].paramname);
                    EditorGUILayout.LabelField("ownerid : " + slotspos[slotindexddl].ownerID);
                }
                break;
                case 4:
                    EditorGUILayout.LabelField("this node broadcast it s result to : ");
                    
                    for (int c = 0; c < broadcast.ReceiverSlotInfo.Count; c++)
                        EditorGUILayout.LabelField("broadcast to : " + broadcast.ReceiverSlotInfo[c].ownerID + " on slot index : " +  broadcast.ReceiverSlotInfo[c].index.ToString());

                    foreach ( int i in NodeGraph.EditedControll.thisgraph.EmiterIDlist )
                        EditorGUILayout.LabelField("EMITERLIST: " + i.ToString());

                break;
                case 5:
                    foreach ( BBCtrlNode bbc in SUBNodes ) 
                        EditorGUILayout.LabelField("SUBNODE : " + bbc.name + "_" + bbc.Guid.GetHashCode().ToString());
                break;


        }

            GUILayout.EndScrollView();

            

            return Str;
        }

        public  void DoNodeWindow(int id)
        {



            // default validation flag 
            checknodevalid = true;
            nodewarning = false;
            // mouse event in bbdrawing statics 
            BBDrawing.CheckInput();
            // compose the class list 
            string[] localclassarray = FillClassArray(Selection.activeGameObject).ToArray();
            // reassign the class index before use cause things changing all the time 
            // depending on selection and behaviors assigned 
            // note : reading custom attribute [bbvisible] on function is very time consuming 
            // so filtering the method list occur only when the a node is focused and mousedown active 
            for (int c = 0; c < localclassarray.GetLength(0); c++)
                if (lookupclassname == localclassarray[c])
                    LookupClassindex = c;
            // user action to select which one to inspect 
            LookupClassindex = EditorGUILayout.Popup(LookupClassindex, localclassarray);
            if (LookupClassindex >= localclassarray.GetLength(0))
            {
                // this mainly occurs when the graph try to access a monobehavior from a gameobject 
                // selection and selection has been changed so the graph can nnot apply to this selected object 
                checknodevalid = false;
                BBDebugLog.singleWarning("DoNodeWindow node fail : LookupClassindex is above localclassarray value:  " + id.ToString());
                GUILayout.Label("WRONG SELECTION 0");
                BBDragWindow(Windowpos); // anyway
                nodewarning = true;
                return;
            }
            lookupclassname = localclassarray[LookupClassindex];
            // huge optim .. avoid to parse [customatribute if not necessary]
            // just the node under the window get a filtered list 
            //BBDrawing.CheckInput();
            if (flyover )//BBDrawing.mousedown && !BBDrawing.mousedrag)
                filteredmethods = BuildFilteredMethodArray(LookupClassindex);
            else
                if (NodeClassTypeArray.Count > LookupClassindex)
                    filteredmethods = NodeClassTypeArray[LookupClassindex].GetMethods();
                else
                {
                    nodewarning = true;
                }
            
            List<string> MethodnameList = new List<string>();
            
            // no selection on monobehavior class function 
            if (filteredmethods == null)
                return;
            foreach (MethodInfo mi in filteredmethods)
                MethodnameList.Add(mi.Name);

            // reassign index before doing popup action 
            for (int c = 0; c < filteredmethods.GetLength(0); c++)
                if (filteredmethods[c].Name == LookupMethodName)
                {
                    Lookupmethodindex = c;
                    selectedmethodinfo = filteredmethods[c];
                    break;
                }
            
       
            if (filteredmethods.Length <= Lookupmethodindex || Lookupmethodindex < 0)
            {
                checknodevalid = false;
                BBDebugLog.singleWarning("DoNodeWindow node fail : method index point out of range :  " + id.ToString());
                GUILayout.Label("WRONG SELECTION 1");
                nodewarning = true;
                //BBDragWindow(Windowpos); // anyway
                //return;
            }
            Lookupmethodindex = EditorGUILayout.Popup(Lookupmethodindex, MethodnameList.ToArray());
            // store the fullQF class name
            
            if (NodeClassTypeArray.Count <= LookupClassindex)
            {
                // this is the process to output string debug in node 
                string msg = " LookupClassindex out of NodeClassTypeArray  range : ()  " + id.ToString();
                BBDebugLog.singleWarning(msg);
                GUILayout.Label("WRONG SELECTION 2");
                nodewarning = true;
                BBDragWindow(Windowpos); // anyway
                return;
            }

            ClassnameFQ = NodeClassTypeArray[LookupClassindex].AssemblyQualifiedName;
            classnameshort = NodeClassTypeArray[LookupClassindex].AssemblyQualifiedName.Split(char.Parse(","))[0];

            NodeGraph.EditedControll.thisgraph.GraphOK = true;
            
            CheckfullGraph(this, out NodeGraph.EditedControll.thisgraph.GraphOK);

            if (NodeGraph.EditedControll.thisgraph.GraphOK && checknodevalid && !iscontroll )
                if (GUILayout.Button("INVOKE"))
                {
                    if (nodedebug )
                    {
                        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                        watch.Start();
                        // the code that you want to measure comes here
                        for (int c = 0; c < CheckPerfIterationNumber; c++)
                            Nodeinvoke(0);
                        watch.Stop();
                        TimeSpan ts = watch.Elapsed;
                        GraphPerf = String.Format("number: {0}  {1} {2:00}:{3:00}:{4:00}.{5:00}", CheckPerfIterationNumber, (float)ts.TotalMilliseconds / 100,
                                  ts.Hours, ts.Minutes, ts.Seconds,
                                   ts.Milliseconds / 10);
                        Debug.Log(GraphPerf);
                    }
                    else
                        Nodeinvoke(0);
                }


            // switch button to open close node window 
            string buttontext = (nodedebug) ? "CLOSE" : "OPEN";
            if (GUILayout.Button(buttontext))
                nodedebug = !nodedebug; // open window display iner node ui 







            selectedmethodinfo = filteredmethods[Lookupmethodindex];

            LookupMethodName = selectedmethodinfo.Name;
            ReturnTye = selectedmethodinfo.ReturnType; // return type ys stored in the node 
            outputslot.TypeFullString= selectedmethodinfo.ReturnParameter.ParameterType.AssemblyQualifiedName;
            Arglist.Clear();
            foreach (ParameterInfo pi in selectedmethodinfo.GetParameters())
                Arglist.Add(pi);
            checknodevalid = true;
            // this node is a ui controll 
            // have to perform the user input to store the value 
            iscontroll = false;
            object[] atributelist = selectedmethodinfo.GetCustomAttributes(true);

            // during edition a ui control need to keep it s value updated 
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

            /*
            DoOutputslotselector();
            DoEmiterSelector();
            DoInputslotselector();
            */
            //string subnodesname = "";
            //foreach (BBCtrlNode N in SUBNodes)
            //  subnodesname += "child " + N.NodeId + "\n";
            if (nodedebug)
            {
                if (!maximized)
                {
                    Windowpos.width += NodeGraph.EditedControll.thisgraph.nodekeys.Count;
                    Windowpos.height = Windowpos.width;
                }
                CheckPerfIterationNumber = EditorGUILayout.IntSlider(CheckPerfIterationNumber, 1, 1000);
                DoNodeDebuginfos();
            }
            else
            {
                if (!minimized)
                {
                    Windowpos.width -= NodeGraph.EditedControll.thisgraph.nodekeys.Count;
                    Windowpos.height = Windowpos.width;
                }
            }



            GUILayout.Label(GraphPerf);



            BBDragWindow(Windowpos); // anyway
        }

   

    /// <summary>
    ///  
    /// </summary>
    /// <param name="M"></param>
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
                object[] atributelist = MethodInfoList[mc].GetCustomAttributes(false);
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
                BBDebugLog.singleWarning ("filtermethod flse "+ name   );
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