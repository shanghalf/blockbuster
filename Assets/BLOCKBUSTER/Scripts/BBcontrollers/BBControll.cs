using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
//using BlockbusterControll;



[System.Serializable]
public enum TXTINDEX 
{
    NORMAL = 0,
    CLICKED = 1,
    FLYOVER = 2,
    TARGET = 3,
    CLEAR =4
}
[System.Serializable]
public enum MVPGRIDSISE 
{
    MVP1 = 1,
    MVP2 = 2,
    MVP4 = 4,
    MVP8 = 8,
    MVP16 = 16,
    MVP32 = 32,
    MVP64 = 64,
    MVP128 = 128,
    MVP256 = 256,
    MVP512 = 512,
    MVP1024 = 1024
}
[System.Serializable]
public enum MVPBUTTONSIZE 
{
    MVP16 = 16,
    MVP32 = 32,
    MVP64 = 64,
    MVP128 = 128,
    MVP256 = 256,
    MVP512 = 512,
    MVP1024 = 1024
}

public class BBTextEnumattribute : System.Attribute
{
    private string _value;
    public BBTextEnumattribute(string value)
    {
        _value = value;
    }
    public string Value
    {
        get { return _value; }
    }
}

public class BBCtrlVisible : System.Attribute
{
    private static  bool bbvisible;

    public BBCtrlVisible()
    {
        bbvisible = true;
    }
    public bool  IsBBVisible
    {
        get { return bbvisible; }
    }
}

public enum BBpath
{

    [BBTextEnumattribute("/BLOCKBUSTER/Editor/")]
    EDITOR = 1,
    [BBTextEnumattribute("/BLOCKBUSTER/Editor/BBResources/256/")]
    RES = 2,
    [BBTextEnumattribute("/BLOCKBUSTER/XML/")]
    XML = 3,
    [BBTextEnumattribute("/BLOCKBUSTER/XML/blockbustersetings/")]
    SETING = 4,
    [BBTextEnumattribute("/BLOCKBUSTER/XML/paramblock/")]
    DATASET = 5,
    [BBTextEnumattribute("/BLOCKBUSTER/XML/preset/")]
    PRESET = 6,
    [BBTextEnumattribute("/BLOCKBUSTER/XML/Replays/")]
    REPLAY = 7,
    [BBTextEnumattribute("/BLOCKBUSTER/XML/scenes/")]
    SCENE = 8,
    [BBTextEnumattribute("/BLOCKBUSTER/Scripts/")]
    SCRIPTS = 9,
    [BBTextEnumattribute("/BLOCKBUSTER/Scripts/Actors/")]
    ACTORSCRIPTS = 10,
    [BBTextEnumattribute("/BLOCKBUSTER/Scripts/BBehaviors/")]
    BBEHAVIORSCRIPTS = 11,
    [BBTextEnumattribute("/BLOCKBUSTER/BBGBASE/")]
    BBGBASE = 12,
    [BBTextEnumattribute("Assets/BLOCKBUSTER/BBGBASE/")]
    ROOTGBASE = 13

}

public static class BBDir
{
    public static string Get(Enum value, bool root = false)
    {
        string output = null;
        Type type = value.GetType();
        FieldInfo fi = type.GetField(value.ToString());
        BBTextEnumattribute[] attrs = fi.GetCustomAttributes(typeof(BBTextEnumattribute), false) as BBTextEnumattribute[];
        if (attrs.Length > 0)
            output = attrs[0].Value;
        if (root)
            return output;
        else
            return Application.dataPath + output;
    }


}


public static class BBdebug
{
    
    public static void SaveMovepadTarget(String filename, Texture2D Txt)
    {
        

        FileStream fs = new FileStream(BBDir.Get(BBpath.RES) + filename, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(Txt.EncodeToPNG());
        bw.Close();
        fs.Close();
    }

}

public class EditorTimer
{
    public bool run;
    private float timeOut;
    public float timeremaining;
    public float s;
   


    public  void StartCountdown( float seconds )
    {
        timeOut = Time.realtimeSinceStartup + seconds;
        s = seconds;
        run = true;
    }
 
    public float Update(bool loop)
    {
        timeremaining = timeOut - Time.realtimeSinceStartup;

        if (Time.realtimeSinceStartup > timeOut)
        {
            if (!loop)
            {
                run = false;
                timeremaining = 0.0f;
            }
            else
            {
                timeOut = Time.realtimeSinceStartup + s;
            }
        }
        return timeremaining;
    }

}


public  class BBCtrl
{

    
    
    private static  List<BBControll> BBControllersAray = new List<BBControll>();
    private static Dictionary<string, Dictionary<int, BBControll>> MVP_LAYERS = new Dictionary<string, Dictionary<int, BBControll>>();
    private static Dictionary<string, List<Texture2D>> TEXTURES = new Dictionary<string, List<Texture2D>>();
    






    public static GUIStyle BBGuiStyle = new GUIStyle();

    private static bool initialized = false;

    private static int      gridsize;
    private static int      buttonsize;
    private static int      texturesize;
    private static int      gridcelnumber;
    private static int      count;

    private static bool     isfocused;
    private static int      mousebttn;
    private static bool     mousemove;
    private static bool     mouseclic;
    private static bool     mousedrag;
    
    private static int      leftmouseclickeventnumber;
    private static Vector2  lastmousepos;
    private static Vector2 mousepos;


    public static bool      GOTFOCUS()  
    {
        CheckInput();
        return isfocused;  
    }

    public static int       MOUSECLIC()
    {
        CheckInput();

        if (mouseclic)
            return mousebttn ;
        else 
            return -1;

    }

    public  static int      MVPTXTSZ            { get { return ((int)gridsize * (int)buttonsize); } }
    public  static int      MVPCELLNB           { get { return ((int)gridsize * (int)buttonsize); } }
    public  static int      MVPGSZ              { get { return gridsize; } }
    public  static int      MVPBSZ              { get { return buttonsize; } }
    public  static int      MVPCNT              { get { return count; } }
    public static bool      INITITIALIZED       { get { return initialized; } }


    public static void  Init()
    {
        // do init 
        BBGuiStyle.fontStyle = FontStyle.Normal;
        Font bbfont = Resources.Load("digistrip", typeof(Font)) as Font;
        Color C = new Color(255, 255, 255, 255);
        Material M = Resources.Load("BBFONTMAT", typeof(Material)) as Material;
        M.color = C;
        bbfont.material = M;
        BBGuiStyle.font = bbfont;
        BBGuiStyle.name = "bb";
        initialized = true;
    }


    public static void Flush()
    {
        initialized = false;
        // do cleaning task 
    }


    public static int MVPMOUSECLIC      
    { 
        get 
        {
            CheckInput();
            return mousebttn; 
        } 
    }
    
    public  static bool     MVPMOUSEMOVE        { get { return mousemove; } }
    //public  static bool     MVPMOUSECLIC        { get { return mouseclic; } }
    public  static bool     MVPMOUSEDRAG        { get { return mousedrag; } }

    // where the final txt stand 

    public static Rect mvpd_rect = new Rect(0, 30, MVPTXTSZ, MVPTXTSZ);

    public static void RenderLayer(string layername, TXTINDEX type)
    {
        foreach (KeyValuePair<int, BBControll> kvp in GetControlDic(layername))
        {
            int[] r = CalcRectFromIndex(kvp.Key);
            Vector2 V = new Vector2(r[0], r[1]);
            RenderSingleButton("bbmain", type, V);
            Debug.Log(kvp.Key.ToString());
            
        }

    }


    public static void ShowMovePadGrid(string layername  ,Vector2 pos , bool linear )
    {
        // call inside a gui event draw 
        for (int ic = 0; ic < Math.Pow(gridsize, 2); ic++)
        {
            int index = 0;
            switch (linear)
            {
                case true:
                    index = ic;
                    break;
                case false:
                    Vector2 mpos = Event.current.mousePosition - BBCtrl.mvpd_rect.position;
                     Texture2D T =  GetTextureFromLayer(layername , TXTINDEX.NORMAL); 
                    Color32 C =  T.GetPixel((int)mpos.x, gridsize - (int)mpos.y);
                    index = C.r;
                    break;
            }
            int[] I = CalcRectFromIndex (ic);  //Rect(px, py, bsz, bsz);
            Rect NR = new Rect(I[0], I[1], I[2], I[3]);
            NR.position += pos;



            GUI.TextField(NR, (index).ToString() , BBGuiStyle);
        }
    }



    private static void CheckInput()
    {
        Rect MouseCursor = new Rect();
        // Get a unique ID for your control.
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        mousepos = Event.current.mousePosition;
        MouseCursor.Set(mousepos.x, mousepos.y, 1, 1);
        GUI.Box(MouseCursor, "");
        if (MouseCursor.Overlaps(mvpd_rect))
            isfocused = true;
        else
        {
            isfocused = false;
            return;
        }

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (Event.current.button == 0)
                {

                    mouseclic = true;
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                    mousebttn = 1;
                }
                break;
            case EventType.MouseMove:
                
                break;
            case EventType.MouseDrag:
                break;
            case EventType.MouseUp:
                // If this control is currently active.
                if (GUIUtility.hotControl == controlID)
                {
                    // Release lock on it :)
                    mouseclic = false;
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    mousebttn = 0;
                    leftmouseclickeventnumber = 0;
                }
                break;
        }
    }


    public static Texture2D GetTextureFromLayer (string layer , TXTINDEX type   )
    {
        List<Texture2D> TXTLIST;
        if (! TEXTURES.TryGetValue("bbmain", out TXTLIST))
        {
            Debug.Log("no texture in this layer entry");
            return null;
        }
        Texture2D Txt = TXTLIST[(int)type]; 
        return Txt;
    }



    public static bool InitBBControllManager(int gridsizefrominit , int buttonsizefrominit)
    {
        gridsize = gridsizefrominit;
        buttonsize = buttonsizefrominit;
        return true;
        // add some check here 
    }

    public static Dictionary<int, BBControll> GetControlDic (string layername)
    {
        Dictionary<int, BBControll> CTRLIST;
        if (MVP_LAYERS.TryGetValue(layername, out CTRLIST))
        {
            //Debug.Log("returned " + layername + " controls dictionary ");
            return CTRLIST;

        }
        else
        {
            Debug.Log("no control dictionary for " + layername );
            return null;
        }


    }



    public static int GetGridIndexFromXY (Vector2 pos)
    {
        //Debug.Log("pos " + pos.ToString() + " texture size  :" + MVPTXTSZ.ToString() + " pos :" +  pos.ToString() );
        
        
        //return(int)  y / mvpd_bsz * mvpd_grsz   + x / mvpd_bsz; // base index 1 
        int x = (int)pos.x ; 
        int y = (int)pos.y ;

        int res = ( y / buttonsize * gridsize ) +(  x / buttonsize) ; // base index 1 
        
        //Debug.Log("pos " + pos.ToString() + " gsz :" + gridsize.ToString() + " bsz :" + buttonsize.ToString() + "index is :"+ res );
        return res;
    }

    public static int[] CalcRectFromIndex (int index)
    {
        int px = ((index) *(int) MVPBSZ) % MVPTXTSZ;// base index 0
        int py = ((((index) * (int)MVPBSZ / MVPTXTSZ)) * (int)MVPBSZ);
        int[] ret = new int[] { px, py, (int)MVPBSZ, (int) MVPBSZ };
        return ret;
    }

    
    public static object[]  InvokeCtrlMethod (string layername, Vector2 frompos , object In , object[] args, out object[] Out  )
    {

        Out =null;
        int index = GetGridIndexFromXY(frompos);

        BBControll BBC = GetControll(layername, index);
        if (BBC == null)
            return null;


        BBC.BBinvoke ( In ,args );


        return Out;
    }



    public static bool  RenderSingleButton (string layername,TXTINDEX textureindex , Vector2 frompos )
    {
        int index = GetGridIndexFromXY(frompos);

        BBControll BBC = GetControll("bbmain", index);
        if (BBC == null)
            return false;

        // max number of controls in layer  
        int max = GetControlDic(layername).Count ;

        List<Texture2D> TXTBUF ;
        if ( ! TEXTURES.TryGetValue(layername , out TXTBUF ))
        {
            Debug.Log("no texture has been initialized for layer " + layername ) ;
            return false ;
        }

        if ((int) textureindex > TXTBUF.Count)
        {
            Debug.Log("no texture at index : " + textureindex.ToString());
            return false;
        }

        int y = MVPTXTSZ - (int) MVPBSZ;
        int[] i4 = CalcRectFromIndex(BBC.iconindex);

        //Rect A = new Rect(i4[0], i4[1], i4[2], i4[3]);
        //A.position += mvpd_rect.position;
        //GUI.Box(A,"A");

        Color[] pix = TXTBUF[(int)textureindex].GetPixels(i4[0], y - i4[1], i4[2], i4[3]);
        //Debug.Log("texture size for" + textureindex.ToString() + " >>> " + TXTBUF[(int)TXTINDEX.TARGET].width.ToString()); 

        int[] R = CalcRectFromIndex(BBC.linearindex);

        //Rect B = new Rect(i4[0], i4[1], i4[2], i4[3]);
        //B.position += mvpd_rect.position;
        //GUI.Box(B, "B");

        TXTBUF[(int)TXTINDEX.TARGET].SetPixels(R[0], y - R[1], R[2], R[3], pix);
        TXTBUF[(int)TXTINDEX.TARGET].Apply();
        return true;


    }


    // init ( the texture generated for the current view should exend to multiple target ) 
    public static void  UnregisterLayer  (string layername )
    {
        // flush the layer 
        if ( MVP_LAYERS.ContainsKey(layername) ) 
        {
                
            Dictionary<int,BBControll> CTRLIST ;
            if  ( MVP_LAYERS.TryGetValue(layername,out CTRLIST)) 
            {
                foreach ( KeyValuePair<int,BBControll>  kvp in CTRLIST )
                    kvp.Value.Clean(); // make sure the controller clean all it s own mess 
                CTRLIST.Clear() ; // clear the controller list
                Debug.Log("clear the controller list for " + layername);
            }
            else 
            {
                Debug.Log ("layer " + layername + "not found "  );
                return ;
            }
        }
        

        Debug.Log("Layer "+ layername + "flusheed " );
    }


    protected static void  GenerateTexture (TXTINDEX ti ,out Texture2D T, int size )

    {
        T = new Texture2D(size, size, TextureFormat.RGBA32, false);

        switch (ti)
        {

            case TXTINDEX.CLEAR:
                Color32[] TBUF = T.GetPixels32();
                for (int i = 0; i < TBUF.Length; i++)
                {
                    TBUF[i].r = 0;
                    TBUF[i].g = 0;
                    TBUF[i].b = 0;
                    TBUF[i].a = 0;
                }
                break;
            case TXTINDEX.NORMAL:

                break;
        }
    }



    protected static Texture2D LoadPNG(string filePath )
    {
        //Rect picsize = new Rect(5, 100, 200, 150);

        bool returnclearbuffer = false;

        if (!File.Exists(filePath) || filePath== null )
        {
            Debug.Log("no file to load , init a blank texture for this entry " + filePath);
            // cannot find the name init wit black texture 
            returnclearbuffer = true ;
        }
        try
        {

            Texture2D tex;//= new Texture2D( texturesize,texturesize );
            byte[] tData;


            if (returnclearbuffer)
            {
                
                GenerateTexture(TXTINDEX.CLEAR, out tex, texturesize);
                tex.EncodeToPNG();
                tex.alphaIsTransparency = true;
                BBdebug.SaveMovepadTarget(Directory.GetDirectoryRoot(filePath) + "outputtargetclear.png", tex);
                return tex;
            }
            else
            {
                tData = new byte[texturesize * texturesize];
                tData = File.ReadAllBytes(filePath);
                 //..this will auto-resize the texture dimensions.
            }
            tex = new Texture2D(texturesize, texturesize);
            tex.LoadImage(tData);
            tex.EncodeToPNG();
            BBdebug.SaveMovepadTarget(Directory.GetDirectoryRoot(filePath) + "outputtargetclear.png", tex);

            Debug.Log("load texture done: " + filePath + "byte size : " + texturesize);
            return tex;
        }
        catch (IOException e)
        {
            // Extract some information from this exception, and then 
            // throw it to the parent method.
            if (e.Source != null)
                Debug.Log("load texture : " + e.Source);
            throw;
        }

    }


    public static BBControll  GetControll( string layer ,int index)
    {
        Dictionary<int,BBControll> BBCL ;
        if (MVP_LAYERS.TryGetValue(layer, out BBCL))
        {
            BBControll BBC;
            if (BBCL.TryGetValue(index, out BBC))
                return BBC;
            else
            {
                Debug.Log("cannot get a valid bbcontroller for layer " + layer + "at index " + index);
                return null;
            }
        }
        else
        {
            Debug.Log("layer :" + layer +" is not referenced in Movepad check your spelling" );
            return null;
        }
    }

    /// <summary>
    /// register a layer and load textures 
    /// next step generate a set of template texture to init layers without textures 
    /// </summary>
    /// <param name="layername"></param>
    /// <param name="TEXLIST"></param>
    ///   BBControllManager.RegisterLayer(layername, tlist, MVPGRIDSISE.MVP8, MVPBUTTONSIZE.MVP32);
    public static void RegisterLayer(string layername , List<string> TEXLIST ,int gsz, int bsz )
    {
        // add layer the size should be moved in another dic 
        // right now size s global for all ( bad )
        MVP_LAYERS.Clear();
        TEXTURES.Clear();

        gridsize = gsz;
        buttonsize = bsz;
        texturesize = gsz * bsz; 

        Dictionary<int, BBControll> L = new Dictionary<int, BBControll>();
        MVP_LAYERS.Add(layername,L);
        // and the associated textures 
        LoadTexturesAndRegisterTxtLayer(layername, TEXLIST) ;
    }

    /// <summary>
    /// associate the texture array to the layer name 
    /// pass a array formated in the proper order nornal clicked flyover
    /// need at least one texture  
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="FileArray"></param>
    /// <returns></returns>
    public static List<Texture2D> LoadTexturesAndRegisterTxtLayer(string layer, List<string> FileArray)
    {
        // load texture set for this layer 
        List<Texture2D> TXTLIST = new List<Texture2D>();
        TXTLIST.Add( LoadPNG(FileArray[ (int) TXTINDEX.NORMAL ]) );
        TXTLIST.Add( LoadPNG(FileArray[ (int) TXTINDEX.CLICKED ]) );
        TXTLIST.Add( LoadPNG(FileArray[ (int) TXTINDEX.FLYOVER ]) );
        TXTLIST.Add( LoadPNG( null ) ); // init with 0x00
        TXTLIST.Add( LoadPNG( null ) ); // init with 0x00


        if (TXTLIST.Count > 0)
        {
            TEXTURES.Add(layer, TXTLIST);
            foreach (Texture2D T in TXTLIST)
                Debug.Log(T.name + " LOADED");
            return TXTLIST;
        }
        else
        {
            Debug.Log("no texture found check name you sent ");
            return null;
        }

    }

    public static bool  FlushLayerTextures (string layer)
    {
        List<Texture2D> T ;

        if (TEXTURES.TryGetValue(layer, out T))
        {
            foreach (Texture2D t in T)
                Debug.Log(t.name + " removed from layer " + layer);
            return true;
        }
        else
        {
            Debug.Log("cant catch texture array from layer " + layer);
            return false;
        }
        
    }


    public static BBControll GetButon(string layername, int linearindex)
    {
        Dictionary<int, BBControll> CTRLIST;
        if (MVP_LAYERS.TryGetValue(layername, out CTRLIST))
        {
            BBControll BBC;
            if (CTRLIST.TryGetValue(linearindex, out BBC))
            {
                Debug.Log(string.Format("returned control {0} \n control stored index : {1} \n icon index: {2} \n function: {3}", linearindex, BBC.linearindex, BBC.iconindex, BBC.FunctionName));
                return BBC;
            }
            else
            {
                Debug.Log("no controll at " + linearindex);
                return null;
            }
        }
        else
        {
            Debug.Log("no layer " + layername);
            return null;
        }
  

    }


    public static bool  RegisterButton(string layername, int linearindex, int iconindex, string functionname = "void")
    {
        int linearmax = BBCtrl.MVPCELLNB;
        if (iconindex > linearmax  || iconindex < 0 ||  linearindex > linearmax || linearindex <0 )
        {
            Debug.Log(string.Format ( "you try to acccess index out of movepad scope \n movepad linear max :{0} \n iconindex: {1} \n linear :{2}", linearmax,iconindex.ToString(),linearindex.ToString()));
            return false;
        }

        Dictionary<int, BBControll> CTRLIST;
        if (MVP_LAYERS.TryGetValue(layername, out CTRLIST))
        {
            BBControll BBC = new BBControll();
            BBC.linearindex = linearindex;
            BBC.FunctionName = functionname;
            BBC.iconindex = iconindex;
            CTRLIST.Add(linearindex, BBC);
            return true;
        }
        else
        {
            Debug.Log("no layer " + layername);
            return false;
        }
    }



    public static void UnRegisterFunction( string layername ,  int linearindex  )
    {


    }





}

[System.Serializable]
public class BBControll 

{
    public object[] args ;
    public int linearindex;
    public int iconindex;
    public string FunctionName;
    // for args will see 
    public List<System.Type>  Tlist = new List<System.Type> ();
    protected  string GUID; // for serialisation 
    public BBControll()
    {
        System.Guid Guid = System.Guid.NewGuid();
        GUID = Guid.ToString();
    }
    public string GetGuid()
    {
        return GUID;
    }

    public void Clean () 
    {
        //--------------------- 
        Debug.Log ( "perform all before removing "+ GUID + " " + FunctionName + " " + linearindex + " " + iconindex );
    }



    public bool BBinvoke ( object obj ,  object[] args = null )
    {
         

        if (FunctionName == null)
        {
            Debug.Log("no function bound to this button");
            return false;
        }


        Debug.Log(FunctionName);
        System.Type T = obj.GetType();

        System.Reflection.MethodInfo Minfo = T.GetMethod(FunctionName);
        if (Minfo == null)
        {
            Debug.Log("cannot get method " + FunctionName + " for " + obj.GetType().ToString() );
            return false;
        }
        Minfo.Invoke(obj, args);
        return true;
    }


}
