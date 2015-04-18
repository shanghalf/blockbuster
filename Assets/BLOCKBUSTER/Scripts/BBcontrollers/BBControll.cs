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
using BBBLocks;

//using BlockbusterControll;


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
public class BBCtrlVisible : System.Attribute
{
    private static  bool bbvisible;
    private Guid guid ;

    public int id { get { return guid.GetHashCode(); } }

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
    ROOTGBASE = 13

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




    public int GridSize;
    public int ButtonSize;
    public int TextureSize;
    public Guid guid;
    public int id;
    public string name;
    public static bool autoload = false;
    public static string bbmainfilename = "bbmain";

    public BBMovepadLayerDescriptor(string newname , int initgridsize , int initbutonsize , int inittexturesize)
    {
        guid = new Guid();
        id = guid.GetHashCode();
        string name = newname;
        GridSize = initgridsize;
        ButtonSize = initbutonsize;
        TextureSize = inittexturesize;
    }




    public BBMovepadLayerDescriptor Load(string path , BBMovepadLayerDescriptor layer)
    {
        if (autoload)
            path = NodeGraph.autosavefilename;
        else
            path = EditorUtility.OpenFilePanel("load Movepad Layer", BBDir.Get(BBpath.SETING), "MVPL");

        if (!System.IO.File.Exists(path))
        {
            // open the layer editor when it will be done 
            return null;
        }
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
            path = BBDir.Get(BBpath.SETING) + savepath + ".MVPL";
        }

        //BBControllersAray.

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
        public static void RenderLayer(BBMovepadLayerDescriptor layer, TXTINDEX type)
        {
            foreach (BBControll kvp in GetControlLayer(layer).DicCtrl.Values)
            {
                int[] r = CalcRectFromIndex(kvp.linearindex);
                Vector2 V = new Vector2(r[0], r[1]);
                RenderSingleButton(layer, type, V);
                //Debug.Log(kvp.Key.ToString());
            }

        }
        /// <summary>
        /// process a index grid 
        /// for button assign 
        /// i ll change this for a more visual 
        /// editor later 
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="pos"></param>
        /// <param name="linear"></param>
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
        /// <summary>
        /// to move in Draw Class could be usefull for other purpose 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
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
        public static object[] InvokeCtrlMethod(BBMovepadLayerDescriptor layer, Vector2 frompos, object In, object[] args, out object[] Out)
        {
            Out = null;
            int index = GetGridIndexFromXY(layer, frompos);
            BBControll MPC = GetControll(layer, index);
            if (MPC == null)
                return null;
            MPC.BBinvoke();
            return Out;
        }
        /// <summary>
        /// render a button in movepad 
        /// according to it s state
        /// </summary>
        /// <param name="layername"></param>
        /// <param name="textureindex"></param>
        /// <param name="frompos"></param>
        /// <returns></returns>
        public static bool RenderSingleButton(BBMovepadLayerDescriptor layer, TXTINDEX textureindex, Vector2 frompos)
        {
            int index = GetGridIndexFromXY(layer,frompos);
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

            //Rect A = new Rect(i4[0], i4[1], i4[2], i4[3]);
            //A.position += mvpd_rect.position;
            //GUI.Box(A,"A");



            Color[] pix = layer.TEXTURES[(int)textureindex].GetPixels(i4[0], y - i4[1], i4[2], i4[3]);
            //Debug.Log("texture size for" + textureindex.ToString() + " >>> " + TXTBUF[(int)TXTINDEX.TARGET].width.ToString()); 

            int[] R = CalcRectFromIndex(BBC.linearindex);

            //Rect B = new Rect(i4[0], i4[1], i4[2], i4[3]);
            //B.position += mvpd_rect.position;
            //GUI.Box(B, "B");

            layer.TEXTURES[(int)TXTINDEX.TARGET].SetPixels(R[0], y - R[1], R[2], R[3], pix);
            layer.TEXTURES[(int)TXTINDEX.TARGET].Apply();
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
                Debug.Log("no file to load , init a blank texture for this entry " + filePath);
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
                    BBdebug.SaveMovepadTarget(Directory.GetDirectoryRoot(filePath) + "outputtargetclear.png", tex);
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
                BBdebug.SaveMovepadTarget(Directory.GetDirectoryRoot(filePath) + "outputtargetclear.png", tex);

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
        public static TXTINDEX textureddlist;

        /// <summary>
        /// default constructor that just assign a file name to this action
        /// </summary>
        public BBControll()
        {
            guid = Guid.NewGuid();
            id = guid.GetHashCode();
            Graphfilename = BBDir.Get(BBpath.SETING) + guid.ToString() + ".bbxml";
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
        public void LoadGraph(string path)
        {
            if (path != null || editgraph)
            {
                path = NodeGraph.autosavefilename;
                //NodeGraph.autoload = true;
            }
            else
                path = EditorUtility.OpenFilePanel("load scene", BBDir.Get(BBpath.SETING), "xml");

            if (!System.IO.File.Exists(path) || editgraph)
            {
                EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
                return;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(NodeGraph));
            Stream stream = new FileStream(path, FileMode.Open);
            thisgraph = serializer.Deserialize(stream) as NodeGraph;
            stream.Close();

            // A create a list to process the hierarchy 
            List<BBCtrlNode> processnodelist = new List<BBCtrlNode>();
            processnodelist.Add(thisgraph.ROOTNODE);
            foreach (BBCtrlNode bbc in thisgraph.Nodes)
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
                    thisgraph.ROOTNODE = node;
                else
                {
                    thisgraph.Nodes.Add(node);
                    thisgraph.nodekeys.Add(node.Guid.GetHashCode());
                }

            }
        }

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




        public object InvokeGraph(BBCtrlNode NCaller)
        {
            Dictionary<string, object> Args = new Dictionary<string, object>();
            List<object> objlist = new List<object>();
            string classname = NCaller.ClassnameFQ.Split(char.Parse(","))[0];
            Type T = Type.GetType(NCaller.ClassnameFQ);

            object classInstance = Selection.activeGameObject.GetComponent(classname);
            if (classInstance == null)
                try
                {
                    classInstance = T.Assembly.CreateInstance(T.FullName);            // (T.Assembly.) Activator.CreateInstance( T);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            if (classInstance == null)
            {
                Debug.Log("cannot get or create an instance of )" + T.Name);
                return null;
            }
            for (int c = 0; c < NCaller.SUBNodes.Count; c++)
            {
                BBCtrlNode N = NCaller.SUBNodes[c];
                NCaller.SUBNodes[c].m_OutputObj = InvokeGraph(N);
                if (N.m_OutputObj == null)
                    Debug.Log("no result for node " + N.NodeId.ToString());
                Args.Add(N.name, N.m_OutputObj);
            }
            // push args in right order fo the call 
            for (int c = 0; c < NCaller.slotspos.Count; c++)
                foreach (KeyValuePair<string, object> kvp in Args)
                    if (kvp.Key.Contains(NCaller.slotspos[c].paramname))
                        objlist.Add(kvp.Value);
            MethodInfo[] MI = T.GetMethods();
            // method filtered by customtag [need to add id to method since the index change and saved index get obsolete]
            // the filtering function is on caller that allow to overide and create later different kind of nodes
            MethodInfo[] filterlist = NCaller.BuildFilteredMethodArray(-1, MI); // pass -1 to fail and not return shit
            NCaller.m_OutputObj = filterlist[NCaller.Lookupmethodindex].Invoke(classInstance, objlist.ToArray());
            return NCaller.m_OutputObj;
        }

        /// <summary>
        /// invoke the graph action on button clic or 
        /// open the graph editor if  no action file is bound to this 
        /// </summary>
        /// <returns></returns>
        public bool BBinvoke()
        {

            if (Graphfilename == null)
                Debug.Log("no graphnode for " + Graphfilename + " setup one in editor ");

            // Open Graph editor behaviour 
            // load and save on graph action name or let user 
            // choose which file to edit 
            NodeGraph.autoload = true;
            NodeGraph.autosave = true;
            NodeGraph.autosavefilename = Graphfilename;

            LoadGraph(Graphfilename);
            if (thisgraph != null)
                InvokeGraph(thisgraph.ROOTNODE);
            else
                return false;

            return true;
        }


    }
