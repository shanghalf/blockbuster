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








public class Drawing
{
    public static Texture2D aaLineTex = null;
    public static Texture2D lineTex = null;

    //public List<Rect> childrenslot= new List<Rect>();
    public static  void curveFromTo(Rect from, Rect to, Color color)
    {
        Drawing.bezierLine(

            new Vector2(from.center.x, from.center.y - 8), //(from.x + from.width, from.y + from.height / 2),
            new Vector2(from.x + from.width + Mathf.Abs(to.x - (from.x + from.width)) / 2, from.y + from.height / 2),
            new Vector2(to.x, to.y + ( to.height / 2) -8 ),
            new Vector2(to.x - Mathf.Abs(to.x - (from.x + from.width)) / 2, to.y + to.height / 2), color, 2, true, 30);
    }

    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias )
    {
        
        Color savedColor = GUI.color;
        Matrix4x4 savedMatrix = GUI.matrix;

        if (!lineTex)
        {
            lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
            lineTex.SetPixel(0, 1, Color.white);
            lineTex.Apply();
        }
        if (!aaLineTex)
        {
            aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
            aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
            aaLineTex.SetPixel(0, 1, Color.white);
            aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
            aaLineTex.Apply();
        }
        if (antiAlias) width *= 3;
        float angle = Vector3.Angle(pointB - pointA, Vector2.right) * (pointA.y <= pointB.y ? 1 : -1);
        float m = (pointB - pointA).magnitude;
        if (m > 0.005f)
        {
            Vector3 dz = new Vector3(pointA.x, pointA.y, 0);

            GUI.color = color;
            GUI.matrix = translationMatrix(dz) * GUI.matrix ;

            GUIUtility.ScaleAroundPivot(new Vector2(m, width), new Vector3(-0.5f, 0, 0));
            GUI.matrix = translationMatrix(-dz) * GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, Vector2.zero);
            GUI.matrix = translationMatrix(dz + new Vector3(width / 2, -m / 2) * Mathf.Sin(angle * Mathf.Deg2Rad)) * GUI.matrix;

            if (!antiAlias)
                GUI.DrawTexture(new Rect(0, 0, 1, 1), lineTex);
            else
                GUI.DrawTexture(new Rect(0, 0, 1, 1), aaLineTex);
        }
        GUI.matrix = savedMatrix;
        GUI.color = savedColor;
    }

    public static void bezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments )
    {
        


        Vector2 lastV = cubeBezier(start, startTangent, end, endTangent, 0);
        for (int i = 1; i <= segments; ++i)
        {
            Vector2 v = cubeBezier(start, startTangent, end, endTangent, i / (float)segments);

            Drawing.DrawLine(
                lastV,
                v,
                color, width, antiAlias  );
            lastV = v;
        }
    }

    private static Vector2 cubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
    {
        float rt = 1 - t;
        float rtt = rt * t;
        return rt * rt * rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t * t * t * e;
    }

    private static Matrix4x4 translationMatrix(Vector3 v)
    {
        return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);
    }
}

public static class Textureloader
{
    // TEXTURES 
    //------------------------------------------------------------------------------------------------------
    public static Texture2D slot_main_output_txt = Resources.Load("slot_main_output", typeof(Texture2D)) as Texture2D;
    public static Texture2D slot_ok_txt = Resources.Load("slot_ok", typeof(Texture2D)) as Texture2D;
    public static Texture2D slot_error_txt = Resources.Load("slot_error", typeof(Texture2D)) as Texture2D;
    public static Texture2D slot_questionmark_txt = Resources.Load("slot_questionmark", typeof(Texture2D)) as Texture2D;
    public static GUIContent slot_main_output = new GUIContent(slot_main_output_txt);
    public static GUIContent slot_ok = new GUIContent(slot_ok_txt);
    public static GUIContent slot_error = new GUIContent(slot_error_txt);
    public static GUIContent slot_questionmark = new GUIContent(slot_questionmark_txt);
    //------------------------------------------------------------------------------------------------------

}





[System.Serializable]
public class BBCtrlEditor : EditorWindow
{
    [MenuItem("BlockBuster/BBControllEditor")]
    static void init()
    {
        EditorWindow.GetWindow<BBCtrlEditor>();


        BBCtrlEditortimerList.Clear();
        BBCtrlEditortimerList.Add("T1", new EditorTimer());
        BBCtrlEditortimerList.Add("T2", new EditorTimer());
        BBCtrlEditortimerList["T2"].StartCountdown(1.0f);

        ROOT = new BBCtrleditorNode(ROOTPOS,true);
        ROOT.name = "ROOT";
        ROOT.isroot = true;

        ROOT.windowid = 1;


        BBCtrl.Init();
    }

    //public static Dictionary<System.Guid, BBCtrleditorNode> AllNodes = new Dictionary<System.Guid, BBCtrleditorNode>();


    public  static GUIStyle G = new GUIStyle();

    private int BBCTRLID = 1; // rndm id can change 
    private int WFUNCID = 2;
    private int WPARAMID = 3;

    int WINDOWHEADOFFSET = 20;

    private bool Staticfunctiononly;
    private static int idcount =2067;
    int userindex = 0;
    public static Dictionary<string,EditorTimer> BBCtrlEditortimerList = new Dictionary<string,EditorTimer>();

    public static int LookupClassindex;
    public static string lookupclassname;
    public static int LookupMethodindex;
    public static string LookupMethodName;

    // ROOT NODE COULD ONLY BE A FUNCTION NODE 
    private static Rect ROOTPOS = new Rect(Screen.width , Screen.height / 2, 200, 200);


    private static  BBCtrleditorNode ROOT;

    public  static void AddChildrenToRoot (int id ,  BBCtrleditorNode newkidontheblock)
    {

        
        ROOT.Childrens.Add(id, newkidontheblock);

    }


    
    //Rect BBctrlWindowRect = new Rect(100, 50, BBCtrl.mvpd_rect.width, BBCtrl.mvpd_rect.height + 20);
    //Rect ParameterInspectorWindowRect = new Rect(300, 50, 200, 300);

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

    private List<BBCtrleditorNode> paramlist = new List<BBCtrleditorNode>();


    void DoParamWindow(int id)
    {
        GUI.DragWindow();

    }


    void DoBBCtrlWindow(int id)
    {
        Rect R = new Rect(0, WINDOWHEADOFFSET, BBCtrl.mvpd_rect.width, BBCtrl.mvpd_rect.height );
        Texture2D T = BBCtrl.GetTextureFromLayer("bbmain", TXTINDEX.TARGET);
        GUI.DrawTexture( R , T); // draw the target 
        //ShowMovePadGrid("bbmain", new Vector2(0, WINDOWHEADOFFSET), true);
        GUI.DragWindow();
    }

    void OnInspectorUpdate()
    {
        //***********************************************************************************
        // update inspector sheet according to the tool value 
        Repaint();
        return;
    }


    void doParameterInspectorWindow(int id)
    {
        GUI.DragWindow();
    }

    public Vector2 RotatePoint(int lookatindex, float radius, int step = 8)
    {
        float a = ((360.0f / step) * Mathf.Deg2Rad) * lookatindex + (Mathf.Deg2Rad * 45.0f);
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.0f, radius * sa + radius * ca);
        return (RV);//+ pos) ;
    }


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


    void curveFromTo(Rect from, Rect to, Color color, Color shadow ,Matrix4x4 zoom )
    {

       // from.position = zoom.MultiplyPoint3x4(from.position);
       // to.position = zoom.MultiplyPoint3x4(to.position);


        Drawing.bezierLine(
            new Vector2(from.x + from.width, from.y + from.height / 2)  ,
            new Vector2(from.x + from.width + Mathf.Abs(to.x - (from.x + from.width)) / 2, from.y + from.height / 2),
            new Vector2(to.x, to.y + to.height / 2),
            new Vector2(to.x - Mathf.Abs(to.x - (from.x + from.width)) / 2, to.y + to.height / 2), color, 2, true, 30 );
    }

    Rect  ZoomRect ( Rect R , Matrix4x4 zoom  )
    {
        Rect R2 = new Rect();

        R2.position = R.position;//zoom.MultiplyPoint(R.position);
        Vector2 sz = new Vector2(  R.width , 0 )   ;
        R2.width= zoom.MultiplyVector(sz).magnitude;
        sz = new Vector2(R.height, 0);
        R2.height = zoom.MultiplyVector(sz).magnitude;

        return R2;

    }


    Vector2 scrollPosition = Vector2.zero;


    

    float zoom = 1f;
    void OnGUI()
    {
        BeginWindows();
        if (ROOT == null)
            return;


       

        /*
        ROOT.Windowpos = GUI.Window(ROOT.windowid, ROOT.Windowpos, ROOT.DoNodeWindow, ROOT.name);
        ROOT.DoNode();
        */
        ROOT.Windowpos = GUI.Window(ROOT.windowid, ROOT.Windowpos, ROOT.DoNodeWindow, ROOT.name);
        ROOT.DoNode();



        foreach (KeyValuePair<int, BBCtrleditorNode> kvp in ROOT.Childrens)
        {

            BBCtrleditorNode node = kvp.Value;
            if (node.Parent != null )
            {
                node.Windowpos = GUI.Window(node.windowid, node.Windowpos, node.DoNodeWindow, node.name);
                node.DoNode();


            }




        }
        EndWindows();


      
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

        


    }
}