using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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



public  class BBControllManager
{
    // link between the controllers and the movepad texture target 
    private static List<BBControll> BBControllersAray = new List<BBControll>();

    // layer of movepads interfaces 
    // tricky layer but using dic is really convenient 
    // i believe that 
    public static Dictionary<string,Dictionary<int, BBControll > > MVP_LAYERS = new Dictionary<string,Dictionary<int, BBControll>>() ;
    public static Dictionary<string, List<Texture2D>> TEXTURES = new Dictionary<string, List<Texture2D>>();
    
  

        

    //public static Dictionary<int, int> layerentry = new Dictionary<int, int>();



    static private int gridsize ;
    private static int buttonsize = 0;
    private static int texturesize = 0;
    private static int gridcelnumber = 0;


    private static int count = 0;
    public static int MVPGSZ { get { return gridsize; } }
    public static int MVPBSZ { get { return  buttonsize  ; } }
    public static int MVPCNT { get { return count; } }


    public static int MVPTXTSZ { get { return ((int)gridsize * (int)buttonsize); } }
    public static int MVPCELLNB { get { return ((int)gridsize * (int)buttonsize); } }



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
            Debug.Log("returned " + layername + " controls dictionary ");
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

        Debug.Log("pos " + pos.ToString() + " gsz :" + gridsize.ToString() + " bsz :" + buttonsize.ToString());
        return (int)pos.y / gridsize * buttonsize + (int)pos.x / buttonsize; // base index 1 
        
    }

    public static int[] CalcRectFromIndex (int index)
    {
        int px = ((index) *(int) MVPBSZ) % MVPTXTSZ;// base index 0
        int py = ((((index) * (int)MVPBSZ / MVPTXTSZ)) * (int)MVPBSZ);
        int[] ret = new int[] { px, py, (int)MVPBSZ, (int) MVPBSZ };
        return ret;
    }



    public static bool  RenderSingleButton (string layername,TXTINDEX textureindex ,int index )
    {
        

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
        int[] i4 = CalcRectFromIndex((int)textureindex);  
        //Color[] pix = TXTBUF[(int)textureindex].GetPixels(i4[0], y - i4[1], i4[2], i4[3]);
        int[] R = CalcRectFromIndex(index);
        //TXTBUF[(int)TXTINDEX.TARGET].SetPixels(R[0], y - R[1], R[2], R[3], pix);
        //TXTBUF[(int)TXTINDEX.TARGET].Apply();
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
            
            Texture2D tex = null;
            int SZ = MVPCELLNB * (int)MVPBSZ;
            byte[] fileData = (returnclearbuffer) ? new byte[SZ] : File.ReadAllBytes(filePath)  ;
            tex = new Texture2D(SZ, SZ);
            if (returnclearbuffer) 
                fileData.Initialize();
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            tex.EncodeToPNG();
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
        Dictionary<int, BBControll> L = new Dictionary<int, BBControll>();
        MVP_LAYERS.Add(layername,L);
        // and the associated textures 
        LoadTextures(layername, TEXLIST);
    }

    /// <summary>
    /// associate the texture array to the layer name 
    /// pass a array formated in the proper order nornal clicked flyover
    /// need at least one texture  
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="FileArray"></param>
    /// <returns></returns>
    public static int LoadTextures( string layer , List<string> FileArray  )
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
                Debug.Log(T.name + " LOADED ");
            return TXTLIST.Count;
        }
        else
        {
            Debug.Log("no texture found check name you sent ");
            return 0;
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
        int linearmax = BBControllManager.MVPCELLNB;
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

    public bool invoke (Actor A)
    {
        if (FunctionName == null)
        {
            Debug.Log("Invoke function name is null ");
            return false;
        }
        Debug.Log(FunctionName);
        System.Reflection.MethodInfo Minfo = A.GetType().GetMethod(FunctionName);
        if (Minfo == null)
        {
            Debug.Log("reflection cannot get method for " + A.GetType().ToString() +" function : " +FunctionName );
            return false;
        }
        Minfo.Invoke( A , null ) ;
        return true;
    }
    public bool invoke(BBehavior B)
    {
        System.Reflection.MethodInfo Minfo = B.GetType().GetMethod(FunctionName);
        Minfo.Invoke(B, B.argsbuff.ToArray());
        return true;
    }


}
