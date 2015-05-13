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
using UnityEditor;

//using BlockbusterControll;

public class BBDebugLog
{
    static List<int> iknowit = new List<int>();

    public static bool singleWarning (string message )
    {
        int h = message.GetHashCode();
        if (!iknowit.Contains(h))
        {
            Debug.Log(message);
            iknowit.Add(h);
            return false ;
        }
        else 
            return true ;
    }




    public static void SaveMovepadTarget(String filename, Texture2D Txt)
    {
        FileStream fs = new FileStream(BBDir.Get(BBpath.RES) + filename, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(Txt.EncodeToPNG());
        bw.Close();
        fs.Close();
    }



}


/// <summary>
/// state of the interface 
/// each should be provided with a dedicated bitmap 
/// right now normal and mandatory 
/// target is generated and 
/// clear is to bitblit instead of calculate erasing
/// </summary>
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
public enum OUTSLOTTYPE
{
    NORMAL = 1,
    EMITTER = 2,
    REMOVE = 3
}





/// <summary>
/// size of grids and button allowed 
/// to add some control on exec level 
/// using only allowed 
/// right now square from one to 1024 / power of 2
/// </summary>
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
/// <summary>
/// could use gridsize as well 
/// but button 1 have no use 
/// and slow down the target rendering 
/// </summary>
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
/// <summary>
/// this custom attribute define if 
/// a function is listed by the function reflector
/// this could be improved and first of all 
/// assign a persistent index calculated on 
/// function name not order that could change each 
/// time to do >> 
/// </summary>
/// 

[System.Serializable]
public class BBCtrlVisible : System.Attribute
{
    private static  bool bbvisible;
    public Guid guid ;

    public BBCtrlVisible()
    {
        guid = Guid.NewGuid();
        bbvisible = true;
    }
    public bool  IsBBVisible
    {
        get { return bbvisible; }
    }
}




public class BBCtrlProp : System.Attribute
{
    public Guid guid;
    public Type T;
    public object Value;
    public string name;
    private bool _needinvoke;

    public int id { get { return guid.GetHashCode(); } }
    public bool needinvoke { get { return _needinvoke; } }

    public BBCtrlProp()
    {
        guid = Guid.NewGuid();
    }
    public BBCtrlProp(bool needinvoke)
    {
        _needinvoke = needinvoke;
        guid = Guid.NewGuid();
    }
    
}





/// <summary>
/// custom attribute used to get text field 
/// in path enum for BBPATH ( really convenient for developping )
/// no need to remember path good practice thanks 
/// to the author of the trick in unity forum 
/// sorry guy i dont remember your name 
/// but you rocks 
/// </summary>
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
/// <summary>
/// read BBTextEnumattribute to get the point
/// </summary>
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
    ROOTGBASE = 13,
    [BBTextEnumattribute("Assets/BLOCKBUSTER/Resources/")]
    RESOURCESFOLDER = 14

}
/// <summary>
/// read BBTextEnumattribute
/// BBdir is a convenient way to lay down a 
/// folder name with enum popup in visual 
/// </summary>
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

/// <summary>
/// need to work this out and 
/// write more convenient function 
/// for debug hey .. you know that !
/// </summary>

/// <summary>
/// timer for editor cosmetic 
/// like slack efffect on nodelinks 
/// </summary>
public class EditorTimer
{
    public bool run;
    private float timeOut;
    public float timeremaining;
    public float s;
    public float timefromstart ()
    {
       return  Time.realtimeSinceStartup;
    }
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

/// <summary>
/// for serialisation of a single movepad Layer
/// </summary>
[System.Serializable]
public class BBMovepadLayerDescriptor
{
    // hope it wil be enough 

    public BBMovepadLayerDescriptor()
    { 
    }
    [XmlIgnore]
    public Dictionary<int, BBControll> DicCtrl = new Dictionary<int, BBControll>();
    public List<BBControll> BBControllersAray = new List<BBControll>();
    public List<string> Texturelistname = new List<string>();
    [XmlIgnore]
    public List<Texture2D> TEXTURES = new List<Texture2D>();

    [XmlIgnore]
    public Texture2D RtTxt ;
    [XmlIgnore]
    public Material MovepadMat;


    public int GridSize;
    public int ButtonSize;
    public int TextureSize;
    public Guid guid;
    public int id;
    public string name;
    public static bool autoload = false;
    public static string bbmainfilename = "bbmain";
    
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [BBCtrlVisible]
    public static string GetLayerame ()
    {
        return BBMovepad.Mainlayer.name;
    }
    [BBCtrlVisible]
    public static BBMovepadLayerDescriptor GetMainLayer()
    {
        return BBMovepad.Mainlayer;
    }
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    
    
    
    public BBMovepadLayerDescriptor(string newname , int initgridsize , int initbutonsize , int inittexturesize)
    {
        guid = new Guid();
        id = guid.GetHashCode();
        string name = newname;
        GridSize = initgridsize;
        ButtonSize = initbutonsize;
        TextureSize = inittexturesize;
        RtTxt = new Texture2D(inittexturesize, inittexturesize);
    }

    public void AssignMovepadTextureToMaterial(string matname)
    {
      MovepadMat = Resources.Load(matname, typeof(Material)) as Material;
    }



    public BBMovepadLayerDescriptor Load(string path , BBMovepadLayerDescriptor layer)
    {

        if (!System.IO.File.Exists(path))
            path = EditorUtility.OpenFilePanel("load Movepad Layer", BBDir.Get(BBpath.SETING), "MVPL");

        XmlSerializer serializer = new XmlSerializer(typeof(BBMovepadLayerDescriptor));
        Stream stream = new FileStream(path, FileMode.Open);
        BBMovepadLayerDescriptor Movepadlayer = serializer.Deserialize(stream) as BBMovepadLayerDescriptor;
        stream.Close();
        // reconstruct the dictionary ( more convenient than 2 separated list )
        // i have to do the same for reflector nodes editor
        DicCtrl.Clear();
        foreach (BBControll button in Movepadlayer.BBControllersAray )
            DicCtrl.Add(button.linearindex, button);
        BBMovepad.RegisterLayer(layer, Movepadlayer.Texturelistname, Movepadlayer.GridSize, Movepadlayer.ButtonSize, layer.name);
        BBMovepad.RenderLayer(layer, TXTINDEX.NORMAL);
        return Movepadlayer;
    }



    public void Save(string savepath = null)
    {
        string path;
        if (savepath == null)
        {
            path = BBDir.Get(BBpath.SETING) + guid.ToString() + ".MVPL";
        }
        else
        {
            //path = BBDir.Get(BBpath.SETING) + savepath + ".MVPL";

            path = EditorUtility.SaveFilePanel("load Movepad Layer", BBDir.Get(BBpath.SETING),"bbmain", "MVPL");

        }

        //BBControllersAray.
        BBControllersAray.Clear();
        // populate serialized list 
        foreach (int key in DicCtrl.Keys) // key as linear index 
        {

            BBControll bbc;
            if (DicCtrl.TryGetValue(key, out bbc))
                BBControllersAray.Add(bbc);
            else
                Debug.Log("serialisation error ");

        }


        System.Type T = typeof(BBMovepadLayerDescriptor);
        System.Type[] extraTypes = { typeof(BBControll) };
        XmlSerializer serializer = new XmlSerializer(T, extraTypes);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
    }

}

    /// <summary>
    ///  The Movepad is a layered 
    ///  texture that is generated and linked to a graph action 
    ///  the graph action is a node based editor 
    ///  that define a Reflection GUI to handle almost everything that expose 
    ///  something for interop ... that the point of all this crap
    /// </summary>

    public class BBMovepad
    {

        //private static Dictionary<string, Dictionary<int, BBControll>> MVP_LAYERS = new Dictionary<string, Dictionary<int, BBControll>>();

        private static Dictionary<string, BBMovepadLayerDescriptor> MVP_LAYERS = new Dictionary<string, BBMovepadLayerDescriptor>();


        public static BBMovepadLayerDescriptor Mainlayer = new BBMovepadLayerDescriptor("bbmain",8,32,256);
 
        public static GUIStyle BBGuiStyle = new GUIStyle();
        private static bool initialized = false;
        private static int count;
        public static int MVPTXTSZ { get { return (Mainlayer.GridSize * Mainlayer.ButtonSize); } }
        public static int MVPCELLNB { get { return (Mainlayer.GridSize * Mainlayer.GridSize); } }
        public static int MVPGSZ { get { return Mainlayer.GridSize; } }
        public static int MVPBSZ { get { return Mainlayer.ButtonSize; } }
        public static int MVPCNT { get { return count; } }
        public static bool INITITIALIZED { get { return initialized; } }

        //public List<BBMovepadLayerDescriptor>  new List<BBMovepadLayerDescriptor>();




        public static void Init()
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

        public static Rect mvpd_rect = new Rect(0, 30, MVPTXTSZ, MVPTXTSZ);

        /// <summary>
        ///  render a layer in target 
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="type"></param>
        /// 

    
        /// <summary>
        /// process a index grid 
        /// for button assign 
        /// i ll change this for a more visual 
        /// editor later 
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="pos"></param>
        /// <param name="linear"></param>
        /// 

        [BBCtrlVisible]
        public static Vector2 GetMovePadPos()
        {
            return mvpd_rect.position;
        }
        [BBCtrlVisible]
        public TXTINDEX indexbase() { return TXTINDEX.NORMAL; }
        [BBCtrlVisible]
        public TXTINDEX indexactive() { return TXTINDEX.CLICKED; }
        [BBCtrlVisible]
        public TXTINDEX indextarget() { return TXTINDEX.TARGET; }            


        [BBCtrlVisible]
        public static void ShowMovePadGrid(BBMovepadLayerDescriptor layer, Vector2 pos, bool linear , TXTINDEX type)
        {
            // call inside a gui event draw 
            for (int ic = 0; ic < Math.Pow(layer.GridSize, 2); ic++)
            {
                int index = 0;
                switch (linear)
                {
                    case true:
                        index = ic;
                        break;
                    case false:
                        Vector2 mpos = Event.current.mousePosition - BBMovepad.mvpd_rect.position;

                        Color32 C = layer.TEXTURES[(int)type].GetPixel((int)mpos.x, layer.GridSize - (int)mpos.y);
                        index = C.r;
                        break;
                }
                int[] I = CalcRectFromIndex(ic);  //Rect(px, py, bsz, bsz);
                Rect NR = new Rect(I[0], I[1], I[2], I[3]);
                NR.position += pos;
                GUI.TextField(NR, (index).ToString());
            }
        }

        /// <summary>
        /// Dictionary for control 
        /// dic are nice for such purpose 
        /// i like it but a way to extend serialisation 
        /// more confortably would be appreciated 
        /// </summary>
        /// <param name="layername"></param>
        /// <returns></returns>
        /// 


        [BBCtrlVisible]
        public static BBMovepadLayerDescriptor GetControlLayer(BBMovepadLayerDescriptor layer)
        {
            BBMovepadLayerDescriptor CTRLIST;
            if (MVP_LAYERS.TryGetValue(layer.name, out CTRLIST))
            {
                //Debug.Log("returned " + layername + " controls dictionary ");
                return CTRLIST;
            }
            else
            {
                Debug.Log("no control dictionary for " + layer.name);
                return null;
            }
        }


        [BBCtrlVisible]
        public static BBMovepadLayerDescriptor GetMovepadLayer ()
        {
            return Mainlayer;
        }


        [BBCtrlVisible]
        public static BBControll GetControlLayerAtMousepos (BBMovepadLayerDescriptor layer,bool lastmousepos = false )
        {
            Vector2 pos ;
            if (lastmousepos)
                pos = BBDrawing.lastmousepos;
            else
                pos =  Event.current.mousePosition;

            int index = GetGridIndexFromXY(layer,pos  );
            return  GetControll(layer, index);
        }

        /// <summary>
        /// to move in Draw Class could be usefull for other purpose 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// 

        [BBCtrlVisible]
        public Vector2 GetMousepos() { return Event.current.mousePosition;  }
        [BBCtrlVisible]
        public static int GetGridIndexFromXY(BBMovepadLayerDescriptor layer, Vector2 pos)
        {
            //return(int)  y / mvpd_bsz * mvpd_grsz   + x / mvpd_bsz; // base index 1 
            int x = (int)pos.x;
            int y = (int)pos.y;
            int res = (y / layer.ButtonSize * layer.GridSize) + (x / layer.ButtonSize); // base index 1 
            //Debug.Log("pos " + pos.ToString() + " gsz :" + gridsize.ToString() + " bsz :" + buttonsize.ToString() + "index is :"+ res );
            return res;
        }
        /// <summary>
        /// to move in draw as well
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// 
        [BBCtrlVisible]
        public static int[] CalcRectFromIndex(int index)
        {
            int px = ((index) * (int)MVPBSZ) % MVPTXTSZ;// base index 0
            int py = ((((index) * (int)MVPBSZ / MVPTXTSZ)) * (int)MVPBSZ);
            int[] ret = new int[] { px, py, (int)MVPBSZ, (int)MVPBSZ };
            return ret;
        }
        /// <summary>
        /// invoke the graph action 
        /// of the controll standing under the mouse pos 
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="frompos"></param>
        /// <param name="In"></param>
        /// <param name="args"></param>
        /// <param name="Out"></param>
        /// <returns></returns>
        /// 
        [BBCtrlVisible]
        public static void InvokeCtrlMethod(BBMovepadLayerDescriptor layer, Vector2 frompos)
        {
            int index = GetGridIndexFromXY(layer, frompos);
            BBControll MPC = GetControll(layer, index);
            if (MPC == null)
                return ;
            MPC.BBinvoke();
        }
        /// <summary>
        /// render a button in movepad 
        /// according to it s state
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="textureindex"></param>
        /// <param name="frompos"></param>
        /// <returns></returns>
        /// 
        [BBCtrlVisible]
        public static Color[] ClearButton ( BBMovepadLayerDescriptor layer )
        {
            List<Color> CB = new List<Color>();
            for (int c = 0; c < layer.ButtonSize * layer.ButtonSize; c++)
                CB.Add(new Color(0, 0, 0, 0));
            return CB.ToArray();
        }

        /// <summary>
        /// render a single control on movepad texture 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="textureindex"></param>
        /// <param name="frompos"></param>
        /// <returns></returns>
        [BBCtrlVisible]
        public static bool RenderSingleButton(BBMovepadLayerDescriptor layer, TXTINDEX textureindex, Vector2 frompos)
        {
            int index = GetGridIndexFromXY(layer, frompos);
            BBControll BBC = GetControll(layer, index);
            if (BBC == null)
                return false;
            if ((int)textureindex > layer.TEXTURES.Count)
            {
                Debug.Log("no texture at index : " + textureindex.ToString());
                return false;
            }
            int y = MVPTXTSZ - (int)MVPBSZ;
            int[] i4 = CalcRectFromIndex(BBC.iconindex);
            Color[] pix = layer.TEXTURES[(int)textureindex].GetPixels(i4[0], y - i4[1], i4[2], i4[3]);
            int[] R = CalcRectFromIndex(BBC.linearindex);
            layer.TEXTURES[(int)TXTINDEX.TARGET].SetPixels(R[0], y - R[1], R[2], R[3], pix);
            layer.TEXTURES[(int)TXTINDEX.TARGET].Apply();
            if ( BBControll.RenderToMaterial)
                RenderTargetoMaterial(Mainlayer);
            return true;
        }



        //public static  RenderTexture renderTexture = new RenderTexture(Mainlayer.TextureSize, Mainlayer.TextureSize,32);
        //public static Texture2D movepadtexture = new Texture2D(Mainlayer.TextureSize, Mainlayer.TextureSize, TextureFormat.ARGB32, true);

        [BBCtrlVisible]
        public static  void RenderTargetoMaterial(BBMovepadLayerDescriptor layer)
        {
            Byte[] buf = layer.TEXTURES[(int)TXTINDEX.TARGET].EncodeToPNG();
            layer.RtTxt.LoadImage(buf);
            layer.RtTxt.Apply();
            Graphics.Blit(layer.RtTxt,layer.MovepadMat, 1);
        }


        [BBCtrlVisible]
        public static void SaveTargetToFile(BBMovepadLayerDescriptor layer )
        {

            //Byte[] buf  = layer.TEXTURES[(int)TXTINDEX.TARGET].EncodeToPNG();
            //Stream file = File.Open(  BBDir.Get(BBpath.RES)+"Targout.png", FileMode.Create );
            //BinaryWriter  B =  new BinaryWriter(file);
            //B.Write(buf);
            //file.Close();
        }

        [BBCtrlVisible]
        public static bool RenderLayer(BBMovepadLayerDescriptor layer, TXTINDEX textureindex)
        {
            if ((int)textureindex > layer.TEXTURES.Count)
            {
                Debug.Log("no texture at index : " + textureindex.ToString());
                return false;
            }
            int y = layer.TextureSize - layer.ButtonSize;

            for (int c = 0; c < layer.GridSize * layer.GridSize; c++)
            {

                BBControll bbc;
                if (layer.DicCtrl.TryGetValue(c, out bbc))
                {
                    int[] i4 = CalcRectFromIndex(bbc.iconindex);
                    Color[] pix = layer.TEXTURES[(int)textureindex].GetPixels(i4[0], y - i4[1], i4[2], i4[3]);
                    int[] R = CalcRectFromIndex(bbc.linearindex);
                    //GUI.Box(new Rect(i4[0], i4[1], i4[3] - i4[0], i4[1] - i4[3]), "A");
                    layer.TEXTURES[(int)TXTINDEX.TARGET].SetPixels(R[0], y - R[1], R[2], R[3], pix);
                    layer.TEXTURES[(int)TXTINDEX.TARGET].Apply();
                }
                else
                {
                    int[] i4 = CalcRectFromIndex(c);
                    Color[] pix = layer.TEXTURES[(int)TXTINDEX.CLEAR].GetPixels(i4[0], y - i4[1], i4[2], i4[3]);
                    int[] R = CalcRectFromIndex(c);
                    layer.TEXTURES[(int)TXTINDEX.TARGET].SetPixels(R[0], y - R[1], R[2], R[3], pix);
                    layer.TEXTURES[(int)TXTINDEX.TARGET].Apply();
                    //GUI.Box(new Rect(i4[0],i4[1],i4[3]-i4[0],i4[1]-i4[3] ),"B");

                }
                
            }
            return true;

        }


        /// <summary>
        /// called on texture load 
        /// to generate a void byte array 
        /// if texture name is not provided 
        /// and texture mandatory anyway 
        /// for clear and first iteration of the target 
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="T"></param>
        /// <param name="size"></param>
        protected static void GenerateTexture(TXTINDEX ti, out Texture2D T, int size)
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
        /// <summary>
        /// load a png for the 
        /// movepad interface
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected static Texture2D LoadPNG( BBMovepadLayerDescriptor layer,string filePath)
        {
            bool returnclearbuffer = false;
            if (!File.Exists(filePath) || filePath == null)
            {
                BBDebugLog.singleWarning("no file to load , init a blank texture for this entry " + filePath);
                returnclearbuffer = true;
            }
            try
            {
                Texture2D tex;//= new Texture2D( texturesize,texturesize );
                byte[] tData;
                if (returnclearbuffer)
                {
                    GenerateTexture(TXTINDEX.CLEAR, out tex, layer.TextureSize);
                    tex.EncodeToPNG();
                    tex.alphaIsTransparency = true;
                    BBDebugLog.SaveMovepadTarget(Directory.GetDirectoryRoot(filePath) + "outputtargetclear.png", tex);
                    return tex;
                }
                else
                {
                    tData = new byte[layer.TextureSize * layer.TextureSize];
                    tData = File.ReadAllBytes(filePath);
                    //..this will auto-resize the texture dimensions.
                }
                tex = new Texture2D(layer.TextureSize, layer.TextureSize);
                tex.LoadImage(tData);
                tex.EncodeToPNG();
                BBDebugLog.SaveMovepadTarget(Directory.GetDirectoryRoot(filePath) + "outputtargetclear.png", tex);

                //Debug.Log("load texture done: " + filePath + "byte size : " + texturesize);
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

        /// <summary>
        /// pick up a control in a layer 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// 
        [BBCtrlVisible]
        public static BBControll GetControll(BBMovepadLayerDescriptor layer, int index)
        {
            BBControll BBC;
            if (layer.DicCtrl.TryGetValue(index, out BBC))
                return BBC;
            else
                return null;
        }

        /// <summary>
        /// register a layer and load textures 
        /// next step generate a set of template texture to init layers without textures 
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="TEXLIST"></param>
        ///   BBControllManager.RegisterLayer(layername, tlist, MVPGRIDSISE.MVP8, MVPBUTTONSIZE.MVP32);
        [BBCtrlVisible]
        public static void RegisterLayer(BBMovepadLayerDescriptor layer, List<string> TEXLIST, int gsz, int bsz, string layername)
        {
            // add layer the size should be moved in another dic 
            // right now size s global for all ( bad )
            MVP_LAYERS.Clear();
            layer.TEXTURES.Clear();
            layer.name = layername;

            layer.GridSize= gsz;
            layer.ButtonSize = bsz;
            layer.TextureSize= gsz * bsz;


            //Dictionary<int, BBControll> L = new Dictionary<int, BBControll>();
            //BBMovepadLayerDescriptor L = new BBMovepadLayerDescriptor(layername);

            MVP_LAYERS.Add(layer.name, layer);
            // and the associated textures 

            // load texture set for this layer 
            layer.TEXTURES.Clear();
            layer.Texturelistname.Clear();
            layer.Texturelistname = TEXLIST;

            layer.TEXTURES.Add(LoadPNG(layer,TEXLIST[(int)TXTINDEX.NORMAL]));
            layer.TEXTURES.Add(LoadPNG(layer,TEXLIST[(int)TXTINDEX.CLICKED]));
            layer.TEXTURES.Add(LoadPNG(layer,TEXLIST[(int)TXTINDEX.FLYOVER]));
            layer.TEXTURES.Add(LoadPNG(layer,null)); // init with 0x00 
            layer.TEXTURES.Add(LoadPNG(layer,TEXLIST[(int)TXTINDEX.CLEAR]));



            
        }



        /// <summary>
        /// probably duplicated Unused to remove  
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="linearindex"></param>
        /// <returns></returns>
        [BBCtrlVisible]
        public static BBControll GetButon(string layername, int linearindex)
        {
            BBMovepadLayerDescriptor L;
            if (MVP_LAYERS.TryGetValue(layername, out L))
            {
                BBControll BBC;
                if (L.DicCtrl.TryGetValue(linearindex, out BBC))
                {
                    Debug.Log(string.Format("returned control {0} \n control stored index : {1} \n icon index: {2} \n function: {3}", linearindex, BBC.linearindex, BBC.iconindex, BBC.Graphfilename));
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

        /// <summary>
        /// used to setup how the 
        /// target texture is made of 
        /// from imput source it also 
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="linearindex"></param>
        /// <param name="iconindex"></param>
        /// <returns></returns>
        /// 
        [BBCtrlVisible]
        public static bool RegisterButton(BBMovepadLayerDescriptor layer, int linearindex, int iconindex)
        {
            int linearmax = BBMovepad.MVPCELLNB;
            if (iconindex > linearmax || iconindex < 0 || linearindex > linearmax || linearindex < 0)
            {
                Debug.Log(string.Format("you try to acccess index out of movepad scope \n movepad linear max :{0} \n iconindex: {1} \n linear :{2}", linearmax, iconindex.ToString(), linearindex.ToString()));
                return false;
            }
            if (MVP_LAYERS.TryGetValue(layer.name, out Mainlayer))
            {
                BBControll MPC = new BBControll();
                MPC.linearindex = linearindex;
                MPC.iconindex = iconindex;
                Mainlayer.DicCtrl.Add(linearindex, MPC);
                return true;
            }
            else
            {
                Debug.Log("no layer " + layer.name);
                return false;
            }

           

        }

    }
    /// <summary>
    /// Button descriptor in the movepad 
    /// lot of thing have to be moved 
    /// in a top class to manage diferent type of controlls 
    /// actually just button are handled by the movepad
    /// sliders combobox would follow after the Unite in bkk
    /// in the 
    /// </summary>
    [System.Serializable]
    public class BBControll
    {



        [XmlIgnore]
        public NodeGraph thisgraph;
        [XmlIgnore]
        public object[] args;
        // placement in interface composition
        public int linearindex;
        public int iconindex;
        public string Graphfilename;
        public Guid guid;
        public int id;

        public static bool editgraph = false;
        public static bool unlayered = false;
        public static bool RenderToMaterial = false;

        public static TXTINDEX textureddlist;




        /// <summary>
        /// default constructor that just assign a file name to this action
        /// </summary>
        public BBControll()
        {
            guid = Guid.NewGuid();
            id = guid.GetHashCode();
            //Graphfilename = BBDir.Get(BBpath.SETING) + guid.ToString() + ".bbxml";
        }

        public void Clean()
        {
        }
        /// <summary>
        /// function load a graph for execution the graphnode method 
        /// do it slightly diferently it would be nice to unify both 
        /// right now a load change need to be handled in both editor and exec mode
        /// i ll do it once back from thailand
        /// </summary>
        /// <param name="path"></param>
        /// 



        [BBCtrlVisible]
        public void Save()
        {
            string path;
            path = BBDir.Get(BBpath.SETING) + guid.ToString() + ".MVPB";
            System.Type T = typeof(BBControll);
            System.Type[] extraTypes = { };
            XmlSerializer serializer = new XmlSerializer(T, extraTypes);
            Stream stream = new FileStream(path, FileMode.Create);
            serializer.Serialize(stream, this);
            stream.Flush();
            stream.Close();
        }



        [BBCtrlVisible]
        public object InvokeGraph(BBCtrlNode NCaller , GameObject o =null)
        {
            if (NCaller == null)
                return null ;

            Dictionary<string, object> Args = new Dictionary<string, object>();
            List<object> objlist = new List<object>();
            string debuglog="";

            // in edition mode we want the graph to 
            // keep the object up to date with editor action ;
            if (!NodeGraph.gamemode)
                NCaller.objtoinvoke = null;



            if (NCaller.nodedebug)
                debuglog += "startdebug \n"; // right now just to setup a breakpoint 
            if (NCaller.ClassnameFQ == null)
                return null;
            Type T = Type.GetType(NCaller.ClassnameFQ);

            // if the node object allready have an object assigned 
            if (NCaller.objtoinvoke == null )
            {
                //string classname = NCaller.ClassnameFQ.Split(char.Parse(","))[0];
                if (o == null)
                {
                    if (Selection.activeGameObject == null)
                        return null;
                    NCaller.objtoinvoke = Selection.activeGameObject.GetComponent(NCaller.classnameshort);
                }
                else
                    NCaller.objtoinvoke = o.GetComponent(NCaller.classnameshort);

                if (NCaller.objtoinvoke == null)
                    try
                    {
                        NCaller.objtoinvoke = T.Assembly.CreateInstance(T.FullName);            // (T.Assembly.) Activator.CreateInstance( T);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                if (NCaller.objtoinvoke == null)
                {
                    Debug.Log("something wrong in class instanciate");
                    return null;
                }
            }



            for (int c = 0; c < NCaller.SUBNodes.Count; c++)
            {
                BBCtrlNode N = NCaller.SUBNodes[c];
                NCaller.SUBNodes[c].m_OutputObj = InvokeGraph(N , o);
                    Args.Add(N.name + N.Guid.GetHashCode(), N.m_OutputObj);
            }
            // push args in right order fo the call 
            for (int c = 0; c < NCaller.slotspos.Count; c++)
                foreach (KeyValuePair<string, object> kvp in Args)
                    if (kvp.Key.Contains(NCaller.slotspos[c].paramname))
                        objlist.Add(kvp.Value);


            MethodInfo[] MI = T.GetMethods();

            // reassign index before doing popup action 
            for (int c = 0; c < MI.GetLength(0); c++)
                if (MI[c].Name == NCaller.LookupMethodName)
                {
                    NCaller.Lookupmethodindex = c;
                    break; // small optim useless to check for more but have to try another way to get index oof function 
                }
            if (NCaller.iscontroll) // a controll hold the parameter set manualy on graph and saved no need to invoke 
            {
                if (NCaller.needinvoke)
                    NCaller.ControllInvoke(MI[NCaller.Lookupmethodindex]);
                else
                    NCaller.m_OutputObj = NCaller.controllarg;

            }
            else
            {

                try
                {
                    NCaller.m_OutputObj = MI[NCaller.Lookupmethodindex].Invoke(NCaller.objtoinvoke, objlist.ToArray());
                }

                catch 
                {
                    if (NCaller.nodedebug)

                        BBDebugLog.singleWarning("method " + MI[NCaller.Lookupmethodindex].Name + "throw : error  on Controllnode " + NCaller.name + " set a default value for return ");
                        
                    NCaller.m_OutputObj = T.Assembly.CreateInstance(MI[NCaller.Lookupmethodindex].ReturnParameter.GetType().FullName);  
                }

            }


            return NCaller.m_OutputObj;
        }

        /// <summary>
        /// invoke the graph action on button clic or 
        /// open the graph editor if  no action file is bound to this 
        /// </summary>
        /// <returns></returns>
        [BBCtrlVisible]
        public bool BBinvoke(GameObject o =null)
        {
            // to inform that inoke comes from a game object and 
            // to not perform GUI node edition related stuff
            if (o != null)
                NodeGraph.gamemode = true;
            else
                NodeGraph.gamemode = false;


            if (thisgraph == null)
            {
                if (!File.Exists(BBDir.Get(BBpath.SETING)+guid.ToString()+".bbxml"))
                {
                    if (!NodeGraph.editoropen)
                        EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
                    NodeGraph.EditedControll = this;
                    return false;
                }
                else
                    thisgraph = NodeGraph.LoadGraph(this);
            }
            InvokeGraph(thisgraph.ROOTNODE , o);
            return true;
        }


    }
