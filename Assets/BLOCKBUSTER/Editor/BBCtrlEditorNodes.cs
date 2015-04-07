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
    public int id;
    public System.Type T;
    public Rect R;
    public SlotInfo(int zid)
    {
        id = zid;
    }
}


/// <summary>
/// this node is a function entry node 
/// i ll top it with an mother class 
/// to manage diferent types ( need a Dataset Block anyway )
/// </summary>
[System.Serializable]
public class BBCtrleditorNode
{
    public Rect Windowpos;

    public string name; // name used for cosmetic purpose 
    private System.Guid Guid; // this is the real identifier 
    public int windowid; // initialized with hashcode of the guid 
    public ParameterInfo Paraminfo; // for slot creation of a child node (this node output ) 
    public int slotYoffset = 0; // offset for slot creation ++ is size of node 
    private List<ParameterInfo> ArgsList; // this Function args 
    private int WINDOWHEADOFFSET = 20; // ofset from head of windo bar 

    // ------------------------------           LOOKUP STORAGE GUI 
    private int LookupClassindex;           //  index in the ddlist 
    private string lookupclassname;         //  class name  
    private int userindex = 0;              //  multi purpose index 
    private int LookupMethodindex;          //  index in ddl for methods 
    private string LookupMethodName;        //  method name 
    private System.Type  lookupclasstype;   //  class type        
    public System.Type ReturnTye;           //  return type 
    // --------------------------------------------------------------

    public BBCtrleditorNode Parent; // handle on parent node 

    // ------------------------------------------------------------------- CHILDRENS of this one 
    public Dictionary<int, BBCtrleditorNode> Childrens = new Dictionary<int, BBCtrleditorNode>();
    // slot informations ( mainly pos name type  ) 
    public List<SlotInfo> slotspos = new List<SlotInfo>();
    // the parent input of this output 
    public SlotInfo ParentFeedSlotInfo ;
    // all parent input not used now 
    // public KeyValuePair<int,System.Type> parentslotindex;
    public bool isroot = false; // is it a rootnode (final )

    // ---------------------- GUI STUFF 
    public Rect outputslotbutton;           // output slot button 
    public Rect RectCurrent = new Rect();   // the current ghost rect ( popup anim ) 
    public Rect RectDest = new Rect();      // popup anim TO
    public Rect Rectorg = new Rect();       // popup anim FROM

    // store one shot event that already occured to this node and dont need a second 
    // iteration like warning messages open event etc 
    private List<int> iknowit = new List<int>();

    // the node timer for timed event like poping effect 
    public EditorTimer Timer = new EditorTimer();
    // check box fo displaying node info 
    public bool nodedebug;

    // ======================================================================== CREATE DESTRUCT 
    ~ BBCtrleditorNode()
    {
        // should be called to remove children from root 
        BBCtrlEditor.RemoveChildrenFromRoot(Guid.GetHashCode());
        Parent.Childrens.Remove(Guid.GetHashCode()); // not sure it s working .. C# suck at this 
        Flood("DESTRUCT  " + name);
    }
    public BBCtrleditorNode(Rect initpos,bool root =false)
    {
        Guid = System.Guid.NewGuid();
        Windowpos = initpos;
        windowid = Guid.GetHashCode();
        ArgsList = new List<ParameterInfo>();
        Flood("Constructor called " + Guid.ToString());
        //EditorUtility.DisplayDialog("creator", "create ", "ok");
        isroot  = root;
        if (!root)
        {
            BBCtrlEditor.AddChildrenToRoot(Guid.GetHashCode(), this); 
        }
    }
    // ======================================================================== CREATE DESTRUCT 




    /// <summary>
    /// node placement ( could be better )
    /// </summary>
    /// <param name="nb"></param>
    /// <returns></returns>
    public Rect GetNewPosFromThisNode(int nb )
    {
        float zoomfactor = (isroot) ? 0.8f : 1.0f;
        Rect Newrect = new Rect();
        float width = Windowpos.width * zoomfactor;
        float height = Windowpos.height * zoomfactor;
        float px = Windowpos.x - ( width *1.8f)  ;
        float py = Windowpos.y + ((height *1.2f) * nb);
        Newrect.Set(px, py, width, height);
        return Newrect;
    }

    /// <summary>
    /// POPUP ANIMATION ( COSMETIC ) 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    public  void deploychildren(Rect parent, Rect child )
    {
        if (Timer.run)
        {
            Timer.Update(false);
            float rt = Timer.timeremaining;
            float max = Timer.s;
            RectCurrent  = Drawing.Rectinterpolate(parent, child, max - rt, max);
            Flood(RectCurrent.ToString());  
        }
    }

     /// <summary>
    /// flood is useless logout .. for debug
     /// </summary>
     /// <param name="S"></param>
     /// <param name="forced"></param>
    public void Flood (string S , bool forced = false )
    {
        if (BBCtrlEditor.showdebuginfo || forced )
            Debug.Log(S);
    }
         
    public void DoNode()
    {
        if (ArgsList == null) // no args do display 
            return;
        // for a frameless button (slots )
        GUIStyle textonly = new GUIStyle();
        textonly.imagePosition = ImagePosition.ImageOnly;
        int BZ = (int)MVPBUTTONSIZE.MVP32;
        outputslotbutton = new Rect(Windowpos.width + Windowpos.position.x + 8,
            Windowpos.height / 2 + Windowpos.position.y,
            BZ, BZ);
        //***************************************************************** REMOVE NODE
        if (! isroot ) 
        {
            if (!Timer.run) // prevent the out slot to show up during popup anim 
            if (GUI.Button(outputslotbutton, Textureloader.slot_ok, textonly))
            {
                // cut branch is recursive flush of all sub tree
                // to call carefully especially during loop with gui event ( i still have one ) 
                // caue it could remove controls used during a gui repaint and pop ERROR 
                CutBranch();
                return;
            }
        }
        else
        {
            // for root action on output slot button 
            if (GUI.Button(outputslotbutton, Textureloader.slot_main_output_txt, textonly))
                Flood("button info for example ");
        }
        // recreate slots on current state 
        slotspos.Clear();
        //****************************************************** CREATE NEW SLOTS
        for (int c = 0; c < ArgsList.Count; c++)
        {
            // fill up the slotspos list 
            Rect entryslotbutton = new Rect(Windowpos.position.x - (BZ / 1.5f),
                                Windowpos.position.y + ((BZ) * c),
                                BZ, BZ);
            SlotInfo S = new SlotInfo(c);
            S.R = entryslotbutton;
            S.T = ArgsList[c].ParameterType;
            slotspos.Add(S);

            if (GUI.Button(entryslotbutton, Textureloader.slot_questionmark, textonly))
            {
                // that where we create a new NODE and perform tree operation parent / children 
                // also init and launch the popup anim 
                float zoom = (isroot) ? 0.5f : 1.0f ;
                Rect NR = GetNewPosFromThisNode( slotYoffset ); //new Rect(10, 50, 80, 80);
                // create the child node 
                BBCtrleditorNode Child = new BBCtrleditorNode(NR);
                Child.Parent = this;
                Child.name = ArgsList[c].Name;
                Child.Paraminfo = ArgsList[c];// that mainly what the input will need from this node 
                Child.ParentFeedSlotInfo = new SlotInfo(c); // but i put on recently created Slot classs
                Child.ParentFeedSlotInfo.T = ArgsList[c].ParameterType; // Parent input type 
                Child.ParentFeedSlotInfo.id = c; // the parent slot index 
                Child.RectDest.Set(NR.xMin, NR.yMin, NR.width, NR.height); // for popup anim
                Child.Rectorg = entryslotbutton;    // for popup anim 

                // launch the node popup anim 
                Child.Timer.StartCountdown(0.5f);

            }

        }
    }

    /// <summary>
    /// FLUSH A BRANCH CALL TO REMOVE ALL FROM THIS 
    /// THI IS APPLYED TO ALL NODE IN SUB BRANCH 
    /// AND SHOULD CLEAN PROPERLY 
    /// ( that not the case i barrely see a Destructor Log Out ) 
    /// </summary>
    public void CutBranch ()
    {

        BBCtrleditorNode BBC;
        foreach (int k in Childrens.Keys)
        {
            if (Childrens.TryGetValue(k, out BBC))
                BBC.CutBranch();
            else
                continue;
        }

        Flood("slotspos clear " + windowid);
        slotspos.Clear();
        Flood("childen clear " + windowid);
        Childrens.Clear();
        Flood("parentfeed null" + windowid);
        ParentFeedSlotInfo = null;
        Flood("window id " + windowid);
        windowid = 0; // 0 id is rejected from dowindow loop in node editor 
    }


    /// <summary>
    /// the window callback associated with this node this is basically a 
    /// Reflection Inspector that associate Function specificly tagged with a custom atribute
    /// and listed in the look up list >>>> to the node hierarchy once it s done 
    /// invoke a buffered sequence shoud execute ( i hope )
    /// </summary>
    /// <param name="id"></param>
    public void DoNodeWindow(int id)
    {
        // based on active game object ( list is auto populated with Monobehaviour and derivated 
        // like BBehaviour .. Actor and your own classes 
        if (Selection.activeGameObject == null)
        {   // exit and clear param list ( to kill prev windows ) 
            GUI.TextField(new Rect(0, WINDOWHEADOFFSET, Windowpos.width, Windowpos.height - WINDOWHEADOFFSET), "Select an object To inspect ");
            ArgsList.Clear();
            GUI.DragWindow();
            return;
            // nothing has been done yet 
        }

        // a few list for inspection and gui (GUI should be moved in Donode anyway ) 
        List<string> classnamelist = new List<string>();
        List<System.Type> TLIST = new List<Type>();
        List<string> methodnamelist = new List<string>();
        List<string> argnamelist = new List<string>();
        // a couple of dirty flag 
        bool classchanged = false;
        bool methodchanged = false;


        // catch all monobehaviours
        MonoBehaviour[] scripts = Selection.activeGameObject.GetComponents<MonoBehaviour>();
        // list of class to inspect for Node methods 
        // tagged with BBCtrlVisible custom attribute 
        foreach (MonoBehaviour o in scripts)
            TLIST.Add(o.GetType());
        // add this (extenssible custom function like add multiply maths misc stuff ) 
        TLIST.Add(typeof(BBControlEditorCustomFunctions));    
        // a bug here that why i should move GUI controls in Donode 
        foreach ( System.Type T in TLIST )
            classnamelist.Add(T.Name);
        // --------------------------------------------------------------------------------- GUI ( BAD ) 
        LookupClassindex = EditorGUILayout.Popup(LookupClassindex, classnamelist.ToArray());
        // store the currrent type of inspected class 
        lookupclasstype = TLIST[LookupClassindex];
        
        if (lookupclassname != classnamelist[LookupClassindex])
        {
            lookupclassname = classnamelist[LookupClassindex];
            classchanged = true; // to prevent reinitialisation in loop of parameterbox
            methodchanged = true; // method could not be the same 
            userindex = 0;
            Flood("CLASS CHANGED ");
            //CutBranch(); // dont do it here and now !! 
            // even if the full branch is dead 
        }

        // get the method list of the selected Class Monobehaviour 
        MethodInfo[] MethodInfoList = TLIST[LookupClassindex].GetMethods();

        // we want to list only methods standing under the custom attribute BBCtrlVisible
        // and store the index in a dictionary 
        Dictionary<string, int> Methodindexdic = new Dictionary<string, int>();
        for (int mc = 0; mc < MethodInfoList.GetLength(0); mc++)
        {
            object[] atributelist = MethodInfoList[mc].GetCustomAttributes(true);
            foreach (object o in atributelist)
                if (o.GetType() == typeof(BBCtrlVisible))
                {
                    // that actually a limitation 
                    // method name should be unique 
                    // a message box popup if duplicated names are used 
                    // the dic is used to prevent dead loop on message box warning 
                    if (!Methodindexdic.ContainsKey(MethodInfoList[mc].Name))
                        Methodindexdic.Add(MethodInfoList[mc].Name, mc); // store the index on the name 
                    else
                    {
                        string warningstring = "Use Unique Name For Methodes \n" + MethodInfoList[mc].Name + " is duplicated in " + lookupclassname;
                        if ( !iknowit.Contains( warningstring.GetHashCode()) ) 
                            EditorUtility.DisplayDialog("WARNING", warningstring, "ok");
                        iknowit.Add(warningstring.GetHashCode());
                        continue;
                    }
                    // finaly we have a candidate 
                    methodnamelist.Add(MethodInfoList[mc].Name);  // for the list that would be wrong otherwise
                }
        }
        // useless to go further if no methods are visible 
        if (Methodindexdic.Count == 0)
            return;
        
        // get the user selection index in the drop down list
        // ------------------------------------------------------------------------------------------- GUI ( BAD and i know it ) 
        userindex = EditorGUILayout.Popup(userindex, methodnamelist.ToArray());

        // and the index in the full method table 
        // but init first to stored index to spot user changes 
        int methodindexfromdic = LookupMethodindex;
        // check on the method name in dic 
        if (!Methodindexdic.TryGetValue(methodnamelist[userindex], out methodindexfromdic))
        {
            // a useless security 
            Flood("no entry in methodindexfromdic");
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
        MethodInfo selectedmethod; // the method selected in combobox 
        selectedmethod = MethodInfoList[LookupMethodindex];//*/
        ReturnTye = selectedmethod.ReturnType;
        // ready to parse Args for this method 
        System.Reflection.ParameterInfo[] argstypes = selectedmethod.GetParameters();
        ArgsList.Clear();
        foreach (ParameterInfo pi in argstypes)
            ArgsList.Add(pi);
        argnamelist.Clear();
        foreach (ParameterInfo argTypepinfo in argstypes)
            argnamelist.Add(argTypepinfo.ParameterType.Name);
        if (methodchanged || classchanged)
        {
            slotspos.Clear();
            slotYoffset = 0;
            Flood("method changed !!! ");
            // a fresh list 
            foreach (ParameterInfo pi in argstypes)
                ArgsList.Add(pi);
        }

        nodedebug = GUILayout.Toggle(nodedebug, "debug mode ");


        if (nodedebug)
        {
            string debugout = "";
            if (!isroot)
            {
                debugout += ("returntype " + ReturnTye.ToString()) + "\n";
                debugout += ("paraminfo " + Paraminfo.ToString() + "\n");
                debugout += ("outfeed  type " + ParentFeedSlotInfo.T.Name + "id in parent " + ParentFeedSlotInfo.id.ToString() + "\n");
            }
            foreach (SlotInfo s in slotspos)
            {
                debugout += ("slotinfo id " + s.id + " Type " + s.T.Name + "\n");
            }
            GUILayout.Box(debugout);
        }
      
        GUI.DragWindow();

    }


}