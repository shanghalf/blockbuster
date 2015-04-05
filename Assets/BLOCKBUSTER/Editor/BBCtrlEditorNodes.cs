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


[System.Serializable]
public class BBCtrleditorNode
{
    public Rect Windowpos;
    public object o;
    public string name;
    private System.Guid Guid;
    public int windowid;
    public System.Type T;
    public ParameterInfo Paraminfo;

    private List<ParameterInfo> paramlist;

    private int WINDOWHEADOFFSET = 20;
    private int LookupClassindex;
    private string lookupclassname;

    private int userindex = 0;
    private int LookupMethodindex;
    private string LookupMethodName;
    private int idcount;

    private System.Type  lookupclasstype;
    public System.Type ReturnTye;
    public Matrix4x4 zoom;

    public BBCtrleditorNode Parent;

    public Dictionary<int, BBCtrleditorNode> Childrens = new Dictionary<int, BBCtrleditorNode>();
    
    public List<SlotInfo> slotspos = new List<SlotInfo>();

    public SlotInfo ParentFeedSlotInfo = new SlotInfo(0);


    public KeyValuePair<int,System.Type> parentslotindex;


    public bool isroot = false;



    [BBCtrlVisible] // define a function visible for BBControl 
    public int ADD(int a, int b)
    {
        return (a + b);

    }



    public BBCtrleditorNode(Rect initpos,bool root =false)
    {
        Guid = System.Guid.NewGuid();

        Windowpos = initpos;
        windowid = Guid.GetHashCode();
        paramlist = new List<ParameterInfo>();
        Debug.Log("Constructor called " + Guid.ToString());
        //EditorUtility.DisplayDialog("creator", "create ", "ok");
        isroot = root;
        if (!root)
        {
            BBCtrlEditor.AddChildrenToRoot(Guid.GetHashCode(), this); 
        }
    }




    public Rect GetNewPosFromThisNode(int nb, float zoomfactor = 1f)
    {
        Rect Newrect = new Rect();
        float width = Windowpos.width * zoomfactor;
        float height = Windowpos.height * zoomfactor;
        float px = Windowpos.x - ( width *1.8f)  ;
        float py = Windowpos.y + ((height *1.2f) * nb);
        Newrect.Set(px, py, width, height);
        return Newrect;
    }

    int slotcount = 0;

    public void DoNode()
    {
        if (paramlist == null)
            return;

        GUIStyle textonly = new GUIStyle();
        textonly.imagePosition = ImagePosition.ImageOnly;
        int BZ = (int)MVPBUTTONSIZE.MVP32;
        Rect outputslotbutton = new Rect(Windowpos.width + Windowpos.position.x + 8,
            Windowpos.height / 2 + Windowpos.position.y,
            BZ, BZ);
        if (Parent != null)
        {
            if (GUI.Button(outputslotbutton, Textureloader.slot_ok, textonly))
            {
                Parent.Childrens.Remove(Guid.GetHashCode());
                Parent.slotcount--;

                Parent = null;
                foreach (KeyValuePair<int, BBCtrleditorNode> node in Childrens)
                    node.Value.Parent = null;
            }
            else
            {
                if (ReturnTye == ParentFeedSlotInfo.T )

                    Drawing.curveFromTo(outputslotbutton, Parent.slotspos[ParentFeedSlotInfo.id].R, new Color(0.2f, 0.8f, 0.2f));
                else
                    Drawing.curveFromTo(outputslotbutton, Parent.slotspos[ParentFeedSlotInfo.id].R, new Color(0.8f, 0.5f, 0.6f));
            }
        }
        else
        {
            if (isroot)
            {
                if (GUI.Button(outputslotbutton, Textureloader.slot_main_output_txt, textonly))
                    Debug.Log("button info for example ");
            }
            else
            {
                foreach (KeyValuePair<int, BBCtrleditorNode> node in Childrens)
                    node.Value.Parent = null;
                Childrens.Clear();
            }

        }
        


        slotspos.Clear();

        for (int c = 0; c < paramlist.Count; c++)
        {


            if (paramlist[c] == null)
                continue;
            Rect entryslotbutton = new Rect(Windowpos.position.x - (BZ / 1.5f),
                                Windowpos.position.y + ((BZ) * c),
                                BZ, BZ);
            Textureloader.slot_questionmark.tooltip = (paramlist[c].Name + " " + paramlist[c].ParameterType.ToString());

            SlotInfo S = new SlotInfo(c);
            S.R = entryslotbutton;
            S.T = paramlist[c].ParameterType;

            slotspos.Add(S);
            

            if (GUI.Button(entryslotbutton, Textureloader.slot_questionmark, textonly))
            {
                float zoom = (isroot) ? 0.5f : 1.0f ;


                Rect NR = GetNewPosFromThisNode( slotcount , zoom ); //new Rect(10, 50, 80, 80);
                slotcount++;

                


                BBCtrleditorNode Child = new BBCtrleditorNode(NR);
                Child.Parent = this;
                Child.name = paramlist[c].Name;
                Child.T = paramlist[c].ParameterType;
                Child.Paraminfo = paramlist[c];

                //Child.parentslotindex = new KeyValuePair<int,Type>( c,paramlist[c].ParameterType)  ;

                Child.ParentFeedSlotInfo.T = paramlist[c].ParameterType;
                Child.ParentFeedSlotInfo.id = c;
                Debug.Log(c.ToString());


                //Child.GetNewPosFromThisNode(-Vector2.right);
                Debug.Log("add children...");
                Childrens.Add(Child.Guid.GetHashCode(), Child);
                //if ( ! BBCtrlEditor.AllNodes.ContainsKey(Child.Guid.ToString()) )
                 //   BBCtrlEditor.AllNodes.Add(Child.Guid.ToString(), Child);

            }

            

        }















        //GUI.skin.button = save;

    }



    public void DoNodeWindow(int id)
    {
        // after a build or repopen window 
        //List<BBCtrleditoParameterbox> paramlist = new List<BBCtrleditoParameterbox>();

        if (Selection.activeGameObject == null)
        {   // exit and clear param list ( to kill prev windows ) 
            GUI.TextField(new Rect(0, WINDOWHEADOFFSET, Windowpos.width, Windowpos.height - WINDOWHEADOFFSET), "Select an object To inspect ");
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
            TLIST.Add(o.GetType());

        TLIST.Add(typeof(BBCtrleditorNode));    

        foreach ( System.Type T in TLIST )
            genericstringlist.Add(T.Name);





        // store ispected class index 
        LookupClassindex = EditorGUILayout.Popup(LookupClassindex, genericstringlist.ToArray());
        lookupclasstype = TLIST[LookupClassindex];

        

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
        Dictionary<string, int> Methodindexdic = new Dictionary<string, int>();
        for (int mc = 0; mc < MethodInfoList.GetLength(0); mc++)
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

        MethodInfo selectedmethod; // the method selected in combobox 
        selectedmethod = MethodInfoList[LookupMethodindex];//*/
        ReturnTye = selectedmethod.ReturnType;

        // ready to parse Args for this method 

        System.Reflection.ParameterInfo[] argstypes = selectedmethod.GetParameters();
        paramlist.Clear();
        foreach (ParameterInfo pi in argstypes)
            paramlist.Add(pi);

        genericstringlist.Clear();
        foreach (ParameterInfo argTypepinfo in argstypes)
            genericstringlist.Add(argTypepinfo.ParameterType.Name);

        //int argnnb = 0; 
        //EditorGUILayout.Popup(argnnb, genericstringlist.ToArray());

        if (methodchanged || classchanged)
        {
            paramlist.Clear();

            // remove node from global list 
            //foreach (KeyValuePair<string, BBCtrleditorNode> kvp in Childrens)
            //    if ( BBCtrlEditor.AllNodes.ContainsKey(kvp.Key))
            //        BBCtrlEditor.AllNodes.Remove(kvp.Key);


            Childrens.Clear();

            idcount = 10; // base index for window ID 
            //************************************************
            //BBCtrlEditortimerList["T1"].StartCountdown(0.5f);
            EditorTimer T = new EditorTimer();
            T.StartCountdown(0.8f);
            //************************************************



            foreach (ParameterInfo pi in argstypes)
                paramlist.Add(pi);




            if (GUILayout.Button("invoke"))
            {
                Debug.Log("class " + lookupclassname + " method " + LookupMethodName);

            }



        }




        //DrawSlots();
        GUI.DragWindow();


    }


}