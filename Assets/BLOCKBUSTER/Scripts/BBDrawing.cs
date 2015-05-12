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





//using BlockbusterControll;
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


public static class BBDrawing
{

    /// <summary>
    ///  FREE LUNCH 
    /// </summary>
    public static Vector2 mousepos = new Vector2();
    public static bool mouseclic;
    public static int mousebttn = 0;
    public static int leftmouseclickeventnumber = 0;
    public static bool isfocused;
    public static bool mousedrag;
    public static bool mousemove;
    public static bool mouseup;
    public static bool mousedown;
    public static Vector2 lastclicdown;
    public static Vector2 lastclicdup;
    public static Vector2 offst = new Vector2();
    public static Vector2 offstcunul  = new Vector2();
    public static bool griddraglock = false;
    public static int whellzoomdelta;
    public static Texture2D aaLineTex = null;
    public static Texture2D lineTex = null;
    public static EditorTimer Zoomtimer = new EditorTimer();

    public static EditorTimer looptimer = new EditorTimer();

    public static  float zoomdir;
    public static Vector2 lastmousepos;

  



    /// <summary>
    /// DO THE MOUSE CHECK AND STORE VALUE IN CLASS 
    /// STATIC MEMBERS 
    /// </summary>
    public static void CheckInput()
    {
        Zoomtimer.Update(false);
        string focusedwindow;
        if (EditorWindow.mouseOverWindow != null)
        {
            focusedwindow = EditorWindow.mouseOverWindow.name;
            if (focusedwindow == "BBCtrlEditor")
                BBCtrlNode.editorfocused = true;
            else
                BBCtrlNode.editorfocused = false;
        }
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        mousepos = Event.current.mousePosition;
        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.scrollWheel:
                Zoomtimer.StartCountdown(1.0f);
                zoomdir = (Event.current.delta.y > 0f) ? 1.0f : -1.0f;
                break;
            case EventType.MouseDown:
                if (Event.current.button == 0)
                    mouseclic = true;
                mouseup = false;
                mousedown = (true);
                lastclicdown = mousepos;
                mousebttn = 1;
                break;
            case EventType.DragExited:
                break;
            case EventType.dragPerform:
                break;
            case EventType.dragUpdated:
                break;
            case EventType.MouseMove:
                break;
            case EventType.MouseDrag:
                mousedrag = true;
                break;
            case EventType.MouseUp:
                mouseclic = false;
                mousedrag = false;
                //Debug.Log("mouseup"); 
                lastclicdup = mousepos;
                griddraglock = false;
                mouseup = true;
                mousedown = (false);
                //Debug.Log("mouseup = " + mouseup.ToString() + "mousedown = " + mousedown.ToString());
                mousebttn = 0;
                leftmouseclickeventnumber = 0;
                break;
        }
    }

    /// <summary>
    /// global local node zoom 
    /// </summary>
    /// <param name="posarray"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    /// 




    public static Vector2[] ZoomNodeGraph ( Vector2[] posarray , float scale )
    {
        List<BBCtrlNode> LBBCN = new List<BBCtrlNode>();
        foreach (BBCtrlNode n in NodeGraph.EditedControll.thisgraph.Nodes)
            LBBCN.Add(n);
        float xmax = 0;
        float xmin = Screen.width;
        float ymax = 0;
        float ymin = Screen.height;
        foreach (BBCtrlNode n in LBBCN)
        {
            if (n.Windowpos.position.x < xmin)
                xmin = n.Windowpos.position.x;

            if (n.Windowpos.position.x > xmax  )
                xmax= n.Windowpos.position.x;

            if (n.Windowpos.position.y < ymin)
                ymin = n.Windowpos.position.y;
                    
            if (n.Windowpos.position.y > ymax )
                ymax = n.Windowpos.position.y;
        }

        Rect ARV = new Rect(xmin, ymin,xmax-xmin, ymax-ymin);
        float delta = BBDrawing.Zoomtimer.timeremaining * BBDrawing.zoomdir;
        /*
        ARV.width += (-delta);
        ARV.height += (-delta);
        ARV.x += (-delta) / 2;
        ARV.y += (-delta) / 2;
        */
        bool nodezoom = false;
        foreach (BBCtrlNode node in LBBCN)
        {
            if ( BBCtrlNode.RMAX.width - node.Windowpos.width < BBCtrlNode.RMAX.width *0.1 )
                node.maximized = true;
            else
                node.maximized = false;
            if (node.Windowpos.width-BBCtrlNode.RMIN.width < BBCtrlNode.RMIN.width * 0.1)
                node.minimized = true;
            else
                node.minimized = false;
            if (Math.Abs( delta ) > 0.01f)
                if (GetRectFocus(node.Windowpos))
                {
                    nodezoom = true;
                    node.Windowpos.width += (-delta);
                    node.Windowpos.height = node.Windowpos.width;
                    node.Windowpos.x += delta / 2;
                    node.Windowpos.y += delta / 2;
                }
        }
        if (nodezoom == false ) 
            foreach (BBCtrlNode n in LBBCN)
            {
                n.Windowpos.x = (n.Windowpos.x - ARV.center.x);
                n.Windowpos.y = (n.Windowpos.y - ARV.center.y);
                n.Windowpos.x += (n.Windowpos.x * (-delta / 100));
                n.Windowpos.y += (n.Windowpos.y * (-delta / 100));
                n.Windowpos.x = (n.Windowpos.x + ARV.center.x);
                n.Windowpos.y = (n.Windowpos.y + ARV.center.y);
                //n.Windowpos.width += (-delta);
                n.Windowpos.height = n.Windowpos.width;
            }
        if (BBCtrlNode.scrolllock)
        {
            GUI.Box(ARV, "");
            GUI.Box(new Rect(ARV.center.x, ARV.center.y, 20, 20), "");

        }
        // limitation on nodes 
        foreach (BBCtrlNode node in LBBCN)
        {
            if (node.Windowpos.width + (-delta) > BBCtrlNode.RMAX.width)
                node.Windowpos.width = BBCtrlNode.RMAX.width;
            if (node.Windowpos.width + (-delta) < BBCtrlNode.RMIN.width)
                node.Windowpos.width = BBCtrlNode.RMIN.width;
        }
        return null;
    }


    /// <summary>
    ///  node editor 2d space managment and grid 
    /// </summary>
    /// <param name="SCRSZ"></param>
    /// <param name="NSZ"></param>
    public static void  BBDoGridLayout(Rect SCRSZ, Vector2 NSZ)
    {
        // check th window focused 
        string focusedwindow ;
        if (EditorWindow.mouseOverWindow != null)
        {
            focusedwindow = EditorWindow.mouseOverWindow.title;
            if (focusedwindow != "BBCtrlEditor")
                return;
        }
        mousepos = Event.current.mousePosition;
        bool nofocus = true;
        foreach (BBCtrlNode n in NodeGraph.EditedControll.thisgraph.Nodes)// want to knoe if node is clicked 
        if (BBDrawing.GetRectFocus(n.Windowpos , true))
        {
            griddraglock = true;
            nofocus = false;
            break;
        }
        // is grid focused and no node under the mouse clic
        if (mousedrag && nofocus && !griddraglock  )
        {
            // scroll the grid and nodes follow 
            offst = mousepos - lastclicdown;
            offstcunul += offst;
            lastclicdown = mousepos;
            
            NodeGraph.EditedControll.thisgraph.ROOTNODE.Windowpos.position += offst;
            foreach (BBCtrlNode n in NodeGraph.EditedControll.thisgraph.Nodes)
            {
                if (! n.isroot) // crappy patch ( some conf get the root listed in nodes some dont to fix ) 
                    n.Windowpos.position += offst;
            }
        }
        // draw the grid
        if (BBCtrlNode.scrolllock)
        {
            Rect D = new Rect();
            for (int x = (int)offstcunul.x % (int)NSZ.x; x < SCRSZ.width; x += (int)NSZ.x)
            {
                D.x = x + offst.x;
                D.y = SCRSZ.y;
                D.width = 2;
                D.height = SCRSZ.height;
                if (BBCtrlNode.scrolllock)
                    GUI.Box(D, "");
            }
            for (int y = (int)offstcunul.y % (int)NSZ.y; y < SCRSZ.height; y += (int)NSZ.y)
            {
                D.x = SCRSZ.x;
                D.y = y + offst.y;
                D.width = SCRSZ.width;
                D.height = 2;
                if (BBCtrlNode.scrolllock)
                    GUI.Box(D, "");
            }
        }

        ZoomNodeGraph(null, 0);


    }
    /// <summary>
    /// check if a rect is under the mouse cursor
    /// with an option to return true on mouse clic 
    /// </summary>
    /// <param name="R"></param>
    /// <param name="checkclick"></param>
    /// <returns></returns>
    public static bool GetRectFocus(Rect R , bool checkclick=false )
    {
        Rect MouseCursor = new Rect();
        BBDrawing.mousepos = Event.current.mousePosition;
        BBDrawing.CheckInput();
        MouseCursor.Set(BBDrawing.mousepos.x, BBDrawing.mousepos.y, 1, 1);
        if (MouseCursor.Overlaps(R))
        {
            if (!checkclick)
            {
                if (BBCtrlNode.scrolllock)
                    GUI.Box(MouseCursor, ""); // for debug
                return true;
            }
            if (mouseclic)
                return true;
            else
                return false;
        }
        else
            return false;
    }
    /// <summary>
    /// THE CRAPPY CURVE DRAWING 
    /// probably not the best part but i ll replace it with another more 
    /// interesting solution right now it do the job 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="color"></param>
    /// 


    

    public static  Vector2 Flex (Vector2 V, float f , float t , float speed )
    {
        float u = 1 / (f);
        float dist = 1 - (u * (t));
        if (speed <= 0) speed = 0;
        looptimer.s = 1.0f; // feed the timer 
        looptimer.Update(true);
        float a = ((speed) * Mathf.Deg2Rad) * looptimer.timefromstart() * dist + (Mathf.Deg2Rad * 90.0f);
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(f * ca - f * sa, f, f * sa + f * ca).normalized;
        return (new Vector2 ( RV.x,RV.z*t) * f);//+ pos) ;
    }



    public static void curveFromTo(Vector2 from, Vector2 to, Color color, float W, out Vector2 V)
    {
        //calcuate handle tension on box distance and output velocity for nodes 
        // with arbitrary W = node children width 

        Vector2 Dir = (from - to).normalized;
        //float ease = 3; // ease prevent node touch 
        float D = Vector2.Distance(from, to); // distance 

        float t = 1 - (D / W  );
        t =  (t<0) ? 0 : t  ;

        
        Vector2 HandleA = new Vector2(from.x + Mathf.Abs(to.x - from.x ) *t, from.y + W * t  );
        Vector2 HandleB = new Vector2( to.x - Mathf.Abs(to.x - from.x ) *t , to.y + W *t);

        float ampl = 50; // *3 is an arbitrary speed based on amplitude 
        HandleA.y += Flex(new Vector2(0, HandleA.y), ampl * t, t, ampl*3).y;
        HandleB.x += Flex(new Vector2(0, HandleB.x), ampl * t, t, ampl*3).x;

        // a simple velocity pushed on node calling 

        if (BBCtrlNode.unfiltered)
            V = Dir * (1* (1 / W *D )  - t);
        else
            V = Vector2.zero;



        Handles.DrawBezier(new Vector2(from.x+8, from.y),
                            new Vector2(to.x-8, to.y ), 
                            HandleA,

                            HandleB,


                            color, null, 3);
        /*
        // finaly draw the spline 
        BBDrawing.bezierLine
            (   new Vector2(from.center.x, from.center.y - 8 ), //(from.x + from.width, from.y + from.height / 2),
                HandleA,
                new Vector2(to.x, to.y + (to.height/2) -8) ,
                HandleB, 
                color, 2, true, 1);*/

    }

    /// <summary>
    /// rect interpolation 0 to max with val 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="val"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Rect Rectinterpolate(Rect A, Rect B, float val, float max)
    {
        float v = ((1 / max) * val);
        float x = ((B.x - A.x) * v) + A.x;
        float y = ((B.y - A.y) * v) + A.y;
        float width = ((B.width) * v);
        float height = ((B.height) * v);
        Rect N = new Rect();
        N.x = x;
        N.y = y;
        N.width = width;
        N.height = height;
        return N;
    }
    /// <summary>
    /// rotate a point on 45deg
    /// by default or free angle 
    /// </summary>
    /// <param name="lookatindex"></param>
    /// <param name="radius"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public static Vector2 RotatePoint(int lookatindex, float radius, int step = 8)
    {
        float a = ((360.0f / step) * Mathf.Deg2Rad) * lookatindex + (Mathf.Deg2Rad * 45.0f);
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.0f, radius * sa + radius * ca);
        return (RV);//+ pos) ;
    }
    /// <summary>
    /// draw a line Fancy function 
    /// that rotate gui matrix on a box to simulate a free line 
    /// smart but crap anyway 
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <param name="antiAlias"></param>
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
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
            GUI.matrix = translationMatrix(dz) * GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(m, width), new Vector3(0, 0, 0));
            GUI.matrix = translationMatrix(-dz) * GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, Vector2.zero);
            GUI.matrix = translationMatrix(dz + new Vector3(width / 2, -m / 10) * Mathf.Cos(angle * Mathf.Deg2Rad)) * GUI.matrix;
            if (!antiAlias)
                GUI.DrawTexture(new Rect(0, 0, 1, 1), lineTex);
            else
                GUI.DrawTexture(new Rect(0, 0, 1, 1), aaLineTex);
        }
        GUI.matrix = savedMatrix;
        GUI.color = savedColor;
    }
    /// <summary>
    /// bezier line 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="startTangent"></param>
    /// <param name="end"></param>
    /// <param name="endTangent"></param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <param name="antiAlias"></param>
    /// <param name="segments"></param>
    public static void bezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments)
    {
        if (BBCtrlNode.scrolllock)
        {
            GUI.Box(new Rect(startTangent.x, startTangent.y, 5, 5), "");
            GUI.Box(new Rect(endTangent.x, endTangent.y, 5, 5), "");
        }

        Vector2 lastV = cubeBezier(start, startTangent, end, endTangent, 0);
        for (int i = 1; i <= segments; ++i)
        {
            Vector2 v = cubeBezier(start, startTangent, end, endTangent, i / (float)segments);

            BBDrawing.DrawLine(
                lastV,
                v,
                color, width, antiAlias);
            lastV = v;
        }
    }
    /// <summary>
    /// calculate bezier interpolation 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="st"></param>
    /// <param name="e"></param>
    /// <param name="et"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static Vector2 cubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
    {
        float rt = 1 - t;
        float rtt = rt * t;
        return rt * rt * rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t * t * t * e;
    }
    /// <summary>
    /// for line draw 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static Matrix4x4 translationMatrix(Vector3 v)
    {
        return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);
    }
}