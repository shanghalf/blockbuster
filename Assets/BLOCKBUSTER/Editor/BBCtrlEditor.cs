using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using UnityEditor;
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
//using BlockbusterControll;








public class Drawing
{
    public static Texture2D aaLineTex = null;
    public static Texture2D lineTex = null;
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

    public static void bezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments)
    {
        Vector2 lastV = cubeBezier(start, startTangent, end, endTangent, 0);
        for (int i = 1; i <= segments; ++i)
        {
            Vector2 v = cubeBezier(start, startTangent, end, endTangent, i / (float)segments);

            Drawing.DrawLine(
                lastV,
                v,
                color, width, antiAlias);
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



public class BBCtrlEditor : EditorWindow
{
    [MenuItem("BlockBuster/BBControllEditor")]
    static void init()
    {
        EditorWindow.GetWindow<BBCtrlEditor>();
        G.fontStyle = FontStyle.Normal;
        Font bbfont = Resources.Load("digistrip", typeof(Font)) as Font;
        Color C = new Color(255, 255, 255, 255);
        Material M = Resources.Load("BBFONTMAT", typeof(Material)) as Material;
        M.color = C;
        bbfont.material = M;
        bbfont.
        G.font = bbfont;
         
        G.name = "bb"; 

    }

    public  static GUIStyle G = new GUIStyle();

    private int BBCTRLID = 1; // rndm id can change 
    private int WFUNCID = 2;
    private int WPARAMID = 3;

    int WINDOWHEADOFFSET = 20;
    
 

    Rect FunctionInspectorWindowRect = new Rect(10, 50, 200, 300);
    Rect BBctrlWindowRect = new Rect(100, 50, BBCtrl.mvpd_rect.width, BBCtrl.mvpd_rect.height + 20);
    Rect ParameterInspectorWindowRect = new Rect(300, 50, 200, 300);

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


            GUI.TextField(NR, (index).ToString(),G);
        }
    }

    void DoBBCtrlWindow(int id)
    {
        Rect R = new Rect(0, WINDOWHEADOFFSET, BBCtrl.mvpd_rect.width, BBCtrl.mvpd_rect.height );
        Texture2D T = BBCtrl.GetTextureFromLayer("bbmain", TXTINDEX.TARGET);
        GUI.DrawTexture( R , T); // draw the target 
        ShowMovePadGrid("bbmain", new Vector2(0, WINDOWHEADOFFSET), true);
        GUI.DragWindow();
    }



    void doParameterInspectorWindow(int id)
    {
        GUI.DragWindow();
    }

    void doFunctionInspectorWindow(int id)
    {
        GUI.DragWindow();
    }

    void curveFromTo(Rect wr, Rect BBctrlWindowRect, Color color, Color shadow)
    {

        Drawing.bezierLine(
            new Vector2(wr.x + wr.width, wr.y + wr.height / 2),
            new Vector2(wr.x + wr.width + Mathf.Abs(BBctrlWindowRect.x - (wr.x + wr.width)) / 2, wr.y + wr.height / 2),
            new Vector2(BBctrlWindowRect.x, BBctrlWindowRect.y + BBctrlWindowRect.height / 2),
            new Vector2(BBctrlWindowRect.x - Mathf.Abs(BBctrlWindowRect.x - (wr.x + wr.width)) / 2, BBctrlWindowRect.y + BBctrlWindowRect.height / 2), color, 2, true, 30);
    }

    void OnGUI()
    {
        Color s = new Color(0.4f, 0.4f, 0.5f);
        curveFromTo(FunctionInspectorWindowRect, BBctrlWindowRect, new Color(0.3f, 0.7f, 0.4f), s);
        curveFromTo(BBctrlWindowRect, ParameterInspectorWindowRect, new Color(0.7f, 0.2f, 0.3f), s);
        BeginWindows();
        FunctionInspectorWindowRect = GUI.Window(WFUNCID, FunctionInspectorWindowRect, doFunctionInspectorWindow, "FUNCTION");
        BBctrlWindowRect = GUI.Window(BBCTRLID, BBctrlWindowRect, DoBBCtrlWindow, "UI");
        ParameterInspectorWindowRect = GUI.Window(WPARAMID, ParameterInspectorWindowRect, doParameterInspectorWindow, "PARAMETER");

        EndWindows();




    }
}