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



/// <summary>
/// enumerate the asset data base
/// in a way to switch environment asset dynamically during gameplay 
/// the naming convention of datas is defined by A 3dsmax Tool and name and folder structure have to be consistent 
/// along the production pipeline the project folder might include the 3dsmax tool in 3DSMAXTOOL folder 
/// name is blockbuster.ms and it provide a lot of function to generate a proper asset 
/// </summary>
public enum ACTIVEBASENAME
{
	HIGHTECH = 0, 
	JUNGLE = 1, 
	TEMPLE = 2,
	SANDBOX = 3
}


/// <summary>
/// static helper class to use in EDITOR scope 
/// </summary>
public static class BBTools
{

    public static string ROOTFOLDER= "BLOCKBUSTER";
    public static bool debugmode = false;
  
    // a log util function to output in console or in dialog modal message 
    // default is console when parameter are not specified can also return a user yesnocancel switch
    public static int  BBdebug (String message , bool Dial = false , bool yesnocancel=false ) 
    {
        if (Dial || debugmode)
        {
            if (yesnocancel)
                return EditorUtility.DisplayDialogComplex("BBdebug", message, "yes", "no", "cancel");
            else
                EditorUtility.DisplayDialog("class", message, "yes");
        }
        else
            Debug.Log(message);
        return -1;
    }

    /// <summary>
    /// this function create an assembly that define an enum Type with all BBhaviors in script folder by reading a 
    /// specific tag in CS file header : <autoenum> ENUM_LABEL bbhaviorclassname </autoenum>
    /// this is a little bit tricky and a popup based on string array could do the same job i keep this function as valuable code sample 
    /// that took me a while to figure out and could be extended to generate other dll component ( with a little extra effort ) 
    /// </summary>
    /// <returns></returns>
    public static System.Type castenum()
    {
        System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        AssemblyName aName = new AssemblyName("blockbusterbehavior_a"); // an assembly name 
        AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave); 
        ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll"); 
        EnumBuilder eb = mb.DefineEnum("blockbusterbehavior_a", TypeAttributes.Public, typeof(int));
        int i = 0;
        foreach (var entry in GetEnumFromScriptFolder())
        {
            string s = entry.Key;
            eb.DefineLiteral(s, i);
            i++;
        }
        // try catch cause during the dev , sometime the dll generation fail but do not need to get built every single time 
        // and probably the previous generated dll is right enough and anyway easy to spot if the enum is not complete by checking the 
        // blockbuster >> Dataset >> Behaviour Dropdownlist ( all of this is mainly designed to generate this EnumPopup)
        try
        {
            System.Type T = eb.CreateType();
            ab.Save(aName.Name + ".dll");
        }
        catch
        {
            // me if you can 
        }
        System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFrom("blockbusterbehavior_a.dll");
        System.Type castenum = ass.GetType("blockbusterbehavior_a");
        return castenum;
    }

    /// <summary>
    /// this function return a dictionary made of all BBhaviour Script Classes Name 
    /// it s used by the Dynamic Enum Creation Above that catch Class with tag <autoenum> ENUM_LABEL bbhaviorclassname </autoenum>
    /// like explained in castenum function description this could be replaced by a simple String Array but kept in place 
    /// cause it s a good entry point for further Dynamic stuff
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetEnumFromScriptFolder()
    {
        Dictionary<string, string> behaviorenumbuilderlist = new Dictionary<string, string>();
        string[] files = Directory.GetFiles(BBDir.Get(BBpath.BBEHAVIORSCRIPTS), "*.cs");
        int i = 0;
        foreach (string f in files)
        {
            System.IO.StreamReader file = new StreamReader(f);
            string line;
            while ((line = file.ReadLine()) != null)
                if (line.Contains("<autoenum>"))
                {
                    var a = line.Split(char.Parse(" "));
                    behaviorenumbuilderlist.Add(a[3], a[2]);
                }
            i++;
        }
        return behaviorenumbuilderlist;
    }

    /// <summary>
    /// return all childrens of a game object 
    /// prety simple 
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static GameObject[] getchildrens(GameObject o)
    {
        List<GameObject> L = new List<GameObject>();
        for (int c = 0; c < o.transform.childCount; c++)
            L.Add(o.transform.GetChild(c).gameObject);
        return L.ToArray();
    }


    /// <summary>
    ///  Return The Type According To Class name String 
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static System.Type GetTypeFromClassName(String typeName)
    {
        foreach (Assembly currentassembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = currentassembly.GetType(typeName, false, true);
            if (t != null)
                return t;
        }
        return null;
    }



}




/// <summary>
/// blockbuster is the main window of this Framework a editorwindow class 
/// that group all the functionality of the tool 
/// </summary>
    public class blockbuster : EditorWindow  
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////  VALUE MEMBER SECTION  OF BLOCKBUSTER MAIN EDITOR WINDOW ////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // the enum created ( see function in BBTool ) for the dropdown list in Blockbuster >> DataSet >> behaviour: 
        // related function is castenum()
        public static System.Enum behaviourenum;


        // this enum is used in MOVEPAD window to define what is the current state of the grid 
        // in several section Linear is the Sprite page where all icons are aligned 
        // used in ShowMovePadGrid() that display a grid number layer on top of the movepad 
        public enum GridIndexType
        {
            LINERAR = 0,
            FUNCTION = 1,
            R = 2,
            G = 3,
            B = 4,
            A = 5
        }

        public static GridIndexType bbGrdtype; // type of the active grid 

        // this hold the main window area for several UI updates 
        // it s initialized in ShowWindow() function when you open Blockbuster 
        // and in ONGUI callback to perform the sub GUI loop on each BBhaviour class registered 
        // getting the window pos on the fly cause unity to refresh child window and close all sub window of it s child (node editor)
        // better to keep a static handle on it 
        public static Rect MAINWINDOWRECT;

        // the actor class is the base layer for Blockbuster game object 
        // the mainwindow interact with the selected actor this instance is the handle of 
        // the active selected object 
        Actor m_actor = null;

        // ------------------------------------------------------------------------------------------------------------- a bunch of bool 
        bool
            b_front_X,                                      // determine if editor camera point roughly along X axis ( to keep a viewport relative block move ) 
            b_fixedstepedit                                 // manipulate actor on defined step offset instead of using it block size 
            ;

        // ------------------------------------------------------------------------------------------------------------- a bunch of float 
        float
            stepvalue = 0.0f,                               // used to move a gameobject with fixed step translation value 
            ofset = 0.0f                                    // unused <remove>
            ;

        // ------------------------------------------------------------------------------------------------------------- a bunch of integer 
        int
            assetbaseindex = 0,                             // hold the current index of the (graphic) asset used in move section of blockbuster browse asset
            assetsliderindex = 0 ,                          // same for the slider asset selection 
            currentpreset                                   // the current index of preset when you browse a preset and not a single graphic object
            ;

        // ------------------------------------------------------------------------------------------------------------- a bunch of vector3 
        Vector3 BlockSize = new Vector3(0, 0, 0);           // hold the size of the currrent asset or selection 
        Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);      // used to swap othogonal translation related to the camera on block move  
        Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);      // right now the doblockmove in camera space call GetDir() function to redefine what is front ..right left back 
        Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);      // definitelly not optimal but do the job right now , it s open for better implementation 
        Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);      // but actually there s higher priority 

        //-------------------------------------------------------------------------------------------------------------------------------------------
        // MovePad is Part Of the blockbuster Window it s basically the texture generated in slide window in blockbuster main window 
        // or <WIP> on interactive Material for ingame interaction 
        //-------------------------------------------------------------------------------------------------------------------------------------------
        
        // the MovePad Texture is mainly driven by those figures size of the icon number of cell all calculated from this 
        // the size of generated texture is important for all 2d calculation 
        // for example the helper grid , the index of icon focussed etc .. 
        private static int mvpd_grsz = 8 ;
        private static int mvpd_bsz = 32;
        private static int mvpd_txtsz = mvpd_bsz * mvpd_grsz ;
        // movepad position the first couple is the position and might change on DragWindow event ( slide )
        static Rect movepadpos = new Rect(0, 0, mvpd_txtsz, mvpd_txtsz);
        // hold the mouse position during MovePad Slide in BlockBuster main window 
        Vector2 MovepadMousePos = new Vector2();
        // display Movepad or not 
        public bool showmovepad = true; 


        // the Asset Database currently used with browse asset function 
        ACTIVEBASENAME activebasename;
        // the tool section of Blockbuster got hide unhide function this is the list of hiden objects 
        List<GameObject> hidenobjectlist = new List<GameObject>();

        // this string is added to base string to get the path of the selected base objects 
        // this could be simplified by using a string array popup and read directly the index value 
        // using an enum make it a little bit more uselessly complicated 
        string selectedbasename = BBDir.Get(BBpath.BBGBASE)+ "HIGHTECH/";

        // 3dsMax Tool Generate Name Fbx and Database XML file to list 
        // all objects of the base this xml is parsed in ReadAssetBase() to fill an array of 
        // asset path used by browse asset function 
        public List<string> data = new List<string>();
        
        // used By Replayer section of the tool <inactive>
        // this replay is actually not active but integrated from a previous project 
        // and will be used as base for replayable actor , a gameplay idea where you drive a platform in a first time then use the 
        // reccorded path of this platform to move it with game character on it 

        private int pathindex;                                              // to switch from one to another reccorded path by the replay system
        public GameObject m_RePlayerObject;                                 // replay object is the Game object moved by the replay system 
        public List<GameObject> m_replayactors = new List<GameObject>();    // list of actor manipulated by replay system one actually but could be extended 
        private bool b_applyfilter;                                         // in editor replay system draw a spline of the replay path this filter by category
        private BBRePlayer m_replayer;                                      // Replayer is a monobehavior class to add on a gameobject to generate the replay path 
        private float replayspeed;                                          // speed of the replay in editor 
        private float replayerdtab = 0;                                     // sub tab pannel for replayer tool 
        private int selectedtab = 0;                                        // hold the active index of the Selected Tab Pannel ( Blockbuster tool section ( move .. ui editor etc ) )

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////  CODE SECTION  OF BLOCKBUSTER MAIN EDITOR WINDOW ////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// menu item in main editor window 
        /// </summary>
        [MenuItem("BlockBuster/BBmainWindow")]
        static void ShowWindow()
        {
            EditorWindow  E= EditorWindow.GetWindow<blockbuster>();
            // hold the main blockbuster window pos to pass to DoGuiLoop() in Behaviour 
            MAINWINDOWRECT = E.position;
            InitGUIValues();
        }

        /// <summary>
        /// save the scene to XML Format 
        /// this function is used in 2 differents context 
        /// save the full scene or save Preset 
        /// a preset is an arbitrary selection of blocks (gameobjects/Actors) stored in 
        /// predefined Folder ( see BBPATH and Project Structure related topics ) 
        /// </summary>
        /// <param name="preset"> is this a full scene or a preset selection</param> 
        /// <param name="scenename">filename of the scene to save </param>
        /// <returns></returns>
        public bool savescene(bool preset = false, string scenename = null)
        {
            string repo = "";
            string defname = "";
            // BBscene is the main scene container class for serialisation 
            BBScene scenetosave = new BBScene();
            GameObject go;
            object[] obj;
            if (!preset) 
            {
                obj = GameObject.FindObjectsOfType(typeof(GameObject));
                repo = "scenes";
                defname = "scene.xml";
            }
            else
            {
                obj = Selection.gameObjects;
                repo = "preset";
                defname = "preset.xml";
            }
            var sfolder = BBDir.Get(BBpath.XML)+ repo;
            string path = "";
            if (scenename == null)
                path = EditorUtility.SaveFilePanel("filename to save", sfolder, defname, "xml");
            else
                path = sfolder + scenename;
            foreach (object o in obj)
            {
                go = (GameObject)o;
                Actor localactor = (Actor)go.GetComponent(typeof(Actor));
                if (localactor == null)
                    continue;
                BaseActorProperties baseactorprop = localactor.Actorprops;
                if (localactor != null)
                {
                    // do not forget to update paramblock value before saving 
                    var filepath = path + "/" + localactor.Actorprops.guid + ".xml";
                    localactor.Actorprops.last_pos = go.transform.position; // make sure the pos is right 
                    localactor.Actorprops.orig_rotation = go.transform.rotation;
                    scenetosave.cluster.baseassetproplist.Add(baseactorprop);
                    BBehavior[] bblist = go.GetComponents<BBehavior>();
                    foreach ( BBehavior  B in bblist )
                        B.GetDataset().Save(BBDir.Get(BBpath.DATASET) + B.GetDataset().GetGuid() + ".xml", B.GetDataset().GetType());
                }
            }
            scenetosave.Save(path);
            return true;
        }

        public void BrowsePresset(int dir)
        {
            GameObject original = Selection.activeGameObject;
            currentpreset+=dir;
            string[] files = Directory.GetFiles(BBDir.Get(BBpath.PRESET), "*.xml");
            int i = currentpreset % files.Length;
            BBTools.BBdebug(i.ToString());
            List<GameObject> L = loadscene(true, System.IO.Path.GetFileName(files[i]));
            Selection.objects = L.ToArray();
            Selection.activeGameObject = GroupSelection(Selection.gameObjects[0], true);
            Selection.activeGameObject.transform.position = original.transform.transform.position;
            DestroyImmediate(original);
        }


        /// <summary>
        /// simple function that Group or ungroup a set of 
        /// gameobjects ( set a hierarchy )
        /// </summary>
        /// <param name="go"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public GameObject GroupSelection(GameObject go , bool group)
        {
            GameObject[] glist = (group) ? Selection.gameObjects :BBTools.getchildrens(Selection.activeGameObject);

            foreach (GameObject tgo in glist)
            {
                if (go != tgo)
                {
                    if ( group )
                        tgo.transform.parent = go.transform;
                    Actor destactor = tgo.GetComponent<Actor>();
                    Actor originalactor = go.GetComponent<Actor>();
                    if (destactor == null || originalactor ==null)
                        continue;
                    destactor.Actorprops.parentgui = (group) ?  m_actor.Actorprops.guid.ToString() : null;
                    destactor.Actorprops.grouped = (group) ? true : false ;
                }
                if (!group)
                    Selection.activeGameObject.transform.DetachChildren();
            }
            return go;
        }

   
        /// <summary>
        /// Load an XML scene descriptor 
        /// the scene is a little bit tricky to serialize ( some Gameobjects properties such as transform are not serializable )
        /// the game features require to load and unload dynamically scene composition so the scene save / load system is 
        /// a custom feature that work roughly like :
        /// 1 the scene file serialize a SceneCluster class 
        /// a Scene Cluster manage a list of BaseActorProperties, each BaseActorPropertie define a list of BBhaviours 
        /// the purpose of this is to save and load only the serializable properties of an actor or a behaviour 
        /// Actor meaning information are stored in BaseActorProperties and BBhaviour Serializable properties are handled by Dataset class 
        /// datasets are serialized in XML/Blockbustersettings folder as Meta Data Soup identified by the Gui loading  a scene 
        /// Load the Scene >> load list of BaseActorProperties >> each BaseActorProperties Load the related BBhaviour XML and the Object is reCreated 
        /// Same For Saving in The Other Direction 
        /// Beside The Load Scene Function is also used to load Preset ( preset Are Subscene set of game objects used to manage scene not only by game object but 
        /// also by subset of several game objects ) 
        /// </summary>
        /// <param name="preset"></param>
        /// <param name="scenename"></param>
        /// <returns></returns>
        public List<GameObject> loadscene(bool preset = false, string scenename = null)
        {
            List<GameObject> merged = new List<GameObject>();

            string repo = "";
            if (!preset)
                repo = "scenes";
            else
                repo = "preset";
            var sfolder = BBDir.Get(BBpath.XML) + repo;
            string path = "";
            scenecluster S = new scenecluster();
            if (scenename == null)
                path = EditorUtility.OpenFilePanel("load scene", sfolder, "xml");
            else
                path = sfolder + "/" + scenename;
            S = BBScene.Load(path);
            foreach (BaseActorProperties blk in S.baseassetproplist)
            {
                string basename = blk.assetname.Split(char.Parse("_"))[0] +"/";
                string assetpath = (BBDir.Get(BBpath.ROOTGBASE, true) + basename + blk.assetname + ".fbx"); 
                BBTools.BBdebug(assetpath);
                GameObject tgo = (GameObject)Resources.LoadAssetAtPath(assetpath, typeof(GameObject));
                GameObject instance = (GameObject)Instantiate(tgo, blk.last_pos, blk.orig_rotation);
                instance.name = blk.assetname + instance.GetInstanceID();
                instance.AddComponent(typeof(Actor));
                Actor tbs = (Actor)instance.GetComponent(typeof(Actor));
                tbs.Actorprops = blk;
                LoadBBhaviorfromActorlist(instance);
                merged.Add(instance);
            }
            return merged;
        }



        /// <summary>
        ///  to manipulate a game object , BlockBuster ReQuire an Actor component ( script Actor ) 
        ///  the actor class define A serializable BaseActorProperties that hold all information required by the tool 
        ///  so selecting an object in the viewport call this function if the game object selected do not have this component 
        ///  then the function add a default Actor component with minimal values such as BlockSize position etc 
        ///  the function is also called when an object is duplicated and take the same properties as originalactor parameter 
        ///  just change GUI / original pos / rot / name to make it unique 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="originalactor"></param>
        void AddActorComponent(GameObject obj,  Actor originalactor)
        {
            MeshFilter M;
            obj.AddComponent(typeof(Actor)); 				                // refer to block setup script ( block behaviour ) 
            m_actor = (Actor)obj.GetComponent(typeof(Actor));		        // bs is at global scope 
            M = (MeshFilter)obj.GetComponent(typeof(MeshFilter));			// get some info from mesh to custom script 
            // -------------------------------------------------------------------------------------- init the block properties  ( parameterblock ) 
            m_actor.Actorprops.block_size = M.renderer.bounds.size;
            m_actor.Actorprops.orig_rotation = obj.transform.rotation;      // origin transform help to re initialize a block in static mode 
            m_actor.Actorprops.orig_pos = obj.transform.position;		    // same for pos ( sound weird could be done in oneline have to see that 	

            if (originalactor != null)
            {
                BBehavior[] blist = originalactor.GetComponents<BBehavior>();
                foreach (BBehavior b in blist)
                {
                    string s = b.GetType().ToString();
                    obj.AddComponent(s) ;
                }
            }
            string str = obj.name;										// change the name  
            string[] strarray = str.Split(new char[] { '-' });
            m_actor.Actorprops.assetname = strarray[0];								// final name is block original name plus unique id 
            Guid g;
            g = Guid.NewGuid();
            m_actor.Actorprops.guid = g.ToString();//obj.GetInstanceID().ToString();
        }








        /// <summary>
        /// simple function to return the global size of a gameobject selection 
        /// </summary>
        /// <param name="gameobjects"></param>
        /// <returns></returns>
        Vector3 CalculateSelectionSize(GameObject[] gameobjects)
        {
            if (gameobjects == null) 										    // called on GUI event may be for nothing 
                return Vector3.zero;
            ArrayList xar = new ArrayList();									// vect array for bbox
            ArrayList yar = new ArrayList();
            ArrayList zar = new ArrayList();
            for (var i = 0; i < gameobjects.GetLength(0); i++)
            {
                GameObject TGO = (GameObject)gameobjects[i];
                MeshFilter M = (MeshFilter)TGO.GetComponent("MeshFilter");	    // push all bbox in minmax array 
                if (M == null) continue;									    // at least if the object got a mesh render 
                xar.Add((float)M.renderer.bounds.max.x);
                xar.Add((float)M.renderer.bounds.min.x);
                yar.Add((float)M.renderer.bounds.max.y);
                yar.Add((float)M.renderer.bounds.min.y);
                zar.Add((float)M.renderer.bounds.max.z);
                zar.Add((float)M.renderer.bounds.min.z);
            }
            if (xar.Count == 0) 											    // nothing come >>exit 
                return Vector3.zero;
            xar.Sort();													        // tidy it up 
            yar.Sort();
            zar.Sort();
            Vector3 tv = new Vector3(0.0f, 0.0f, 0.0f);				            // global bbox
            tv.x = (float)xar[xar.Count - 1] - (float)xar[0];
            tv.y = (float)yar[xar.Count - 1] - (float)yar[0];
            tv.z = (float)zar[xar.Count - 1] - (float)zar[0];
            return tv;
        }



        /// <summary>
        /// the asset base xml file is generated by 3dsmax exporter in asset folder 
        /// its basically a list of all fbx under that specific path 
        /// this function populate the global data array ( name of asset ) 
        /// </summary>
        /// <returns></returns>
        public bool ReadAssetBase()
        {
            if (data != null)
                data.Clear();  // clear the base 
            String filepath = BBDir.Get(BBpath.BBGBASE) + selectedbasename + "AssetList.xml";
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(filepath))
            {
                int i;
                xmlDoc.Load(filepath);
                if (xmlDoc == null)
                    return false;
                XmlNodeList Asset_list = xmlDoc.GetElementsByTagName("AssetsList");
                if (Asset_list == null)
                {
                    Debug.Log("asset list file is corrupted"); 
                    return false;
                }
                XmlNodeList Item_list = Asset_list.Item(0).ChildNodes;
                for (i = 0; i < Item_list.Count; i++)
                {
                    XmlNodeList l = Item_list.Item(i).ChildNodes;
                    if (data != null)
                        data.Add(l[0].InnerText);
                }
                return true;
            }
            else
                Debug.Log(data.Count.ToString() + " no XML database");
            return false;
        }


        /// <summary>
        /// This function is the pending of the 3dsmaxTool Function TO UNITY 
        /// 3dsmaxtool generate a temp xml descriptor in BBGBASE/activebasename 
        /// this file describe what and how to place instance of gameobjects to redo 
        /// the composition identically as 3dsmax viewport 
        /// there s actually a lot of bugs in this workflow , mainly because i did not 
        /// maintain the 3dsmax tool recently after changing a huge amount of the 
        /// unity code .. but the final workflow might include this feature and some of the gameplay features 
        /// require this system to work ... TBC
        /// </summary>
        public void placefromxmlfile()
        {
            String filepath = BBDir.Get(BBpath.BBGBASE) + selectedbasename + "assettransfert" + ".xml";
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(filepath))
            {
                xmlDoc.Load(filepath);
                if (xmlDoc == null)
                {
                    //debug.Log( "xml load fail : " + filepath ) ;
                    return;
                }
                XmlNodeList objlist = xmlDoc.GetElementsByTagName("ObjectList");
                XmlNodeList Item_list = objlist.Item(0).ChildNodes;
                ArrayList tempobjarray = new ArrayList();
                for (int vi = 0; vi < Item_list.Count; vi++)
                {
                    XmlNodeList l = Item_list.Item(vi).ChildNodes;
                    GameObject prefab = (GameObject)Resources.LoadAssetAtPath(("Assets" + selectedbasename + l[0].InnerText + ".fbx"), typeof(GameObject));
                    if (prefab == null)
                        return;
                    // --------------------------------------------------------- MAX >> UNITY translate roughly 
                    Vector3 pos;
                    pos.z = float.Parse(l[1].InnerText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    pos.x = float.Parse(l[2].InnerText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    pos.y = float.Parse(l[3].InnerText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    pos.z = (-pos.z); // flip on z 
                    var xa = float.Parse(l[4].InnerText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + 270;
                    var ya = float.Parse(l[6].InnerText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    var za = float.Parse(l[5].InnerText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + 270;
                    var rot = Quaternion.Euler(xa, ya, za);
                    GameObject instance = (GameObject)Instantiate(prefab, pos, rot);
                    instance.name = prefab.name + instance.GetInstanceID();
                    AddActorComponent(instance,  null);
                    if (Actor.b_groupselectmode)
                        tempobjarray.Add(instance);
                }
                if (Actor.b_groupselectmode)
                {
                    for (int i = 0; i < tempobjarray.Count; i++)
                        Selection.objects.SetValue(tempobjarray[i], i);
                    GameObject tgo = (GameObject)Selection.activeGameObject;
                    for (var c = 0; c < Selection.gameObjects.GetLength(0); c++)
                    {
                        Actor bs = (Actor)Selection.gameObjects[c].GetComponent(typeof(Actor));
                        Selection.gameObjects[c].transform.parent = tgo.transform;
                        bs.transform.parent = tgo.transform;
                        bs.Actorprops.grouped = true;
                    }
                }
            }
        }

        

        /// <summary>
        /// initialize the BlockBuster GUI component 
        /// Load Textures and Compose initial state of UI 
        /// </summary>
        /// <returns></returns>
        public static bool   InitGUIValues()
        {
            System.Type T = (System.Type)BBTools.castenum();
            behaviourenum = (System.Enum)System.Activator.CreateInstance(T);
            //System.Type TT = behaviorManager.GetClassDataset();

            List<string> tlist = new List<string>() ;
            tlist.Add(BBDir.Get(BBpath.RES) + "movepad_i.png");
            tlist.Add(BBDir.Get(BBpath.RES) + "movepad_i_cl.png");
            tlist.Add(BBDir.Get(BBpath.RES) + "movepad_id.png");
            tlist.Add(BBDir.Get(BBpath.RES) + "");
            tlist.Add(BBDir.Get(BBpath.RES) + "outclear_256.png");
            // movepad implement a Layer System to be able to compose or preload different interfaces 
            // then switch the active one easily 
            // actually only one layer is in use ( mainlayer ) 
            BBMovepad.RegisterLayer( BBMovepad.Mainlayer, tlist, 8, 32,"layermain");
            BBMovepad.Mainlayer.DicCtrl.Clear();
            BBMovepad.Init();
            BBMovepad.Mainlayer.Load( BBDir.Get(BBpath.SETING)+ "movepadmain.MVPL", BBMovepad.Mainlayer);
            return true;
        }


        /// <summary>
        /// unity callback to refresh UI on scene event 
        /// </summary>
        void OnInspectorUpdate()
        {
            Repaint();
        }
        

        /// <summary>
        /// component of the replay system 
        /// not fully implemented yet it  s just a rough port of 
        /// another project replay system 
        /// this function read an xml file and return the tag related to 
        /// the current scene 
        /// </summary>
        /// <returns></returns>
        string[] ReturnXmlContent()
        {
            string[] files = Directory.GetFiles(BBDir.Get(BBpath.REPLAY), "*.xml");
            List<string> l = new List<string>();
            string[] path = EditorApplication.currentScene.Split(char.Parse("/"));
            string currenteditormapname = path[path.Length - 1].Split(char.Parse("."))[0];
            foreach (var f in files)
            {
                string[] mapname = f.Split(char.Parse("\\"));
                if (mapname[mapname.Length - 1].Contains(currenteditormapname))
                {
                    string[] spath = f.Split(char.Parse("-"));
                    string fn = spath[spath.Length - 1].Split(char.Parse("."))[0];
                    l.Add(fn);
                }
            }
            if (l.Count == 0)
            {
                l.Add("NO REPLAY FOR THIS MAP");
                m_replayactors.Clear();
            }
            return l.ToArray();
        }

        /// <summary>
        /// another component of the unactivated Replay System 
        /// this function add a replayer Gameobject that redo the path recorded during 
        /// the runtime session 
        /// </summary>
        /// <returns></returns>
        bool AddReplayerObject()
        {
            m_replayactors.Clear();
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (var it in allObjects)
            {
                if (it.name == "REPLAYERINSTANCE")
                    m_replayactors.Add(it);
            }
            if (m_replayactors.Count > 0)
            {
                return true;
            }
            else
            {
                GameObject ReplayerObjectBody = (GameObject)Resources.LoadAssetAtPath(("Assets/ReplayerObjects/ReplayerObjectBody.FBX"), typeof(GameObject));
                GameObject O = (GameObject)Instantiate(ReplayerObjectBody, Vector3.zero, Quaternion.identity);
                O.AddComponent(typeof(BBRePlayer));
                BBRePlayer m_replayer = (BBRePlayer)O.GetComponent(typeof(BBRePlayer));
                m_replayer.m_replayfiletag = ".xml";
                O.name = "REPLAYERINSTANCE";
                m_replayactors.Add(O);
                return true;
            }
        }

        /// <summary>
        /// other Replay Component 
        /// this is a Unity Callback 
        /// to notify the script when the scene is changed 
        /// in this case we clear the replayactor buffer 
        /// </summary>
        /// <param name="level"></param>
        void OnLevelWasLoaded(int level)
        {
            m_replayactors.Clear();
        }
        /// <summary>
        /// Unity Callback Destroy The Replay Actor 
        /// when the tool window is closed 
        /// </summary>
        void OnDestroy()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (var it in allObjects)
            {
                if (it.name == "REPLAYERINSTANCE")
                    DestroyImmediate(it);
            }
        }

        /// <summary>
        /// move the replay object on another track 
        /// not activated yet 
        /// </summary>
        /// <param name="filename"></param>
        void SwitchReplayerPath(string filename)
        {
            foreach (BBReplay ri in m_replayer.replaylist)
                if (ri.m_xmlfilename.Contains(filename))
                    m_replayer.m_playerreplay = ri;
            m_replayer.targetindex = 0;
        }



        /// <summary>
        /// called from load event on Actor 
        /// to reload all BBhaviour Properties 
        /// </summary>
        /// <param name="o"></param>
        public void LoadBBhaviorfromActorlist(GameObject o )
        {
            Actor A = o.GetComponent<Actor>();
            foreach (string S in A.Actorprops.BehaviorListID)
            {
                string filepath = BBDir.Get(BBpath.DATASET) + S + ".xml";
                XmlDocument xmlDoc = new XmlDocument();
                if (!File.Exists(filepath))
                {
                    BBTools.BBdebug(string.Format("file {0} do not exist", filepath));
                    continue;
                }
                xmlDoc.Load(filepath);
                XmlNode node = xmlDoc.GetElementsByTagName("fullqualifiedclassname")[0];
                string classname = node.InnerText;
                node = xmlDoc.GetElementsByTagName("suportedclassname")[0];
                string suportedclassname = node.InnerText;
                BBTools.BBdebug(classname);
                System.Type T = System.Type.GetType(suportedclassname);
                BBTools.BBdebug(T.ToString());
                GameObject.DestroyImmediate(o.GetComponent(T.ToString()));
                BBehavior TEMPB = (BBehavior)o.AddComponent(T.ToString());
                Dataset DS = TEMPB.GetDataset().Load(filepath);
                TEMPB.SetDataset(DS);
            }
        }
        /// <summary>
        /// Called During The OnGUI() callback to display the tool pannel 
        /// i moved this to a separated function cause a OnGUI() could get quickly unreadable 
        /// if you keep all the code inside and the tool pannel is a specific case that not 
        /// require a lot of value defined during the OnGui loop 
        /// the tool pannel show up few buttons if no objects are selected in the viewport 
        /// LOAD SAVE SCENE  >> MAX IN AND OUT 
        /// refer to the function called on GUI action for further information 
        /// </summary>
        /// <param name="showobjectactionbuttons"></param>
        void DisplayToolPannel(bool showobjectactionbuttons)
        {
            Actor.b_groupselectmode = EditorGUILayout.Toggle("GrpMode", Actor.b_groupselectmode);

            GUILayout.BeginHorizontal();
                if (GUILayout.Button(">> 3ds MAX"))
                    placefromxmlfile();
                if (GUILayout.Button("3ds MAX >> "))
                    BBTools.BBdebug("not implemented",true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                if (GUILayout.Button("SAVE SCENE"))
                    savescene();
                if (GUILayout.Button("LOAD SCENE"))
                    loadscene();
            GUILayout.EndHorizontal();


            if (showobjectactionbuttons) 
            {
                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("LOAD PRESET"))
                        loadscene(true);
                    if (GUILayout.Button("SAVE PRESET"))
                        savescene(true);
                GUILayout.EndHorizontal();

                // reset the position to the original one 
                if (GUILayout.Button("reset Position"))
                    for (int ti = 0; ti < Selection.gameObjects.GetLength(0); ti++)
                    {
                        Actor temactor = (Actor)Selection.gameObjects[ti].GetComponent(typeof(Actor)); // associated script 
                        if (temactor != null)
                        { // pull back at original place ( where the go  has been spotted for the first time 
                            Selection.gameObjects[ti].transform.rotation = temactor.Actorprops.orig_rotation;
                            Selection.gameObjects[ti].transform.position = temactor.Actorprops.orig_pos;
                        }
                    }
                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("  GROUP  " ))
                        GroupSelection(Selection.activeGameObject,true);
                    if (GUILayout.Button(" UNGROUP "))
                        GroupSelection(Selection.activeGameObject, false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Hide Group"))
                {
                    Selection.activeGameObject.SetActive(false);
                    hidenobjectlist.Add(Selection.activeGameObject);
                }
                if (GUILayout.Button("Unhide All"))
                {
                    for (int count = 0; count < hidenobjectlist.Count; count++)
                    {
                        GameObject tgo = (GameObject)hidenobjectlist[count];
                        tgo.SetActive(true);
                    }
                    hidenobjectlist.Clear();
                }
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                if (GUILayout.Button("select same"))
                {
                    GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                    ArrayList oblist = new ArrayList();
                    var sname = Selection.activeGameObject.name;
                    var tab1 = sname.Split('-');
                    for (var c = 0; c < allObjects.Length; c++)
                    {
                        var itname = allObjects[c].name;
                        var tab2 = itname.Split('-');
                        if (tab2[0] == tab1[0])
                            oblist.Add(allObjects[c]);
                    }
                    GameObject[] s = new GameObject[oblist.Count];
                    oblist.CopyTo(s, 0);
                    Selection.objects = s;
                }
                if (GUILayout.Button("remove col"))
                    for (var c = 0; c < Selection.gameObjects.Length - 1; c++)
                        DestroyImmediate(Selection.gameObjects[c].collider);
                GUILayout.EndHorizontal();
            }

            

        }


        



        /// <summary>
        /// The MovePad Function executed by the BlockBuster Movepad window 
        /// the movepad procedural texture is designed to be used in both Editor and Gameplay 
        /// the implementation is actually hard coded in Blockbuster but will be replaced asap 
        /// by simples and clear Movepad Class calls 
        /// right now it just make the link on Mouse Event with the Node Editor 
        /// the node editor is a big part of this project and is implemented in classes BBCtrlnode , BBControll , BBCtrlEditor
        /// its basically a nice node editor to implement logic on a controll in movepad , each icon of the texture is associated 
        /// with a nodeGraph that use BBCtrlEditor to edit graphically the logic refer to thoses classes for further information 
        /// </summary>
        /// <param name="i"></param>
        void MVP_DoMovepad(int i)
        {
            BBDrawing.CheckInput();


            if (BBMovepad.Mainlayer.TEXTURES.Count == 0)
                InitGUIValues();
            TXTINDEX active = (BBControll.unlayered) ? TXTINDEX.NORMAL : TXTINDEX.TARGET; 
            Vector2 mpos = Event.current.mousePosition - BBMovepad.mvpd_rect.position;
            // define a output area for the movepad 
            BBMovepad.mvpd_rect.Set(Screen.width / 2 - (BBMovepad.MVPTXTSZ / 2), 30, BBMovepad.MVPTXTSZ, BBMovepad.MVPTXTSZ);

            if (BBMovepad.Mainlayer.TEXTURES[(int)active] == null)
                InitGUIValues();

            GUI.DrawTexture(BBMovepad.mvpd_rect, BBMovepad.Mainlayer.TEXTURES[(int)active]); // draw the target 
            
            bool flyover;

            if (!BBDrawing.GetRectFocus(BBMovepad.mvpd_rect, out flyover ))
            {
                GUI.DragWindow();
                return;
            }
            if (BBDrawing.mousedown)
            {
                //BBMovepad.RenderSingleButton(BBMovepad.Mainlayer, TXTINDEX.CLICKED, mpos);
                Actor A = Selection.activeGameObject.GetComponent<Actor>();
                if (BBDrawing.leftmouseclickeventnumber == 0) // crappy mouse management 
                {
                    BBMovepad.RenderSingleButton(BBMovepad.Mainlayer, TXTINDEX.CLICKED, mpos);
                    if (!BBControll.editgraph)
                        BBMovepad.InvokeCtrlMethod(BBMovepad.Mainlayer, mpos);
                    else
                    {
                        int index = BBMovepad.GetGridIndexFromXY(BBMovepad.Mainlayer, mpos);
                        BBControll MPC = BBMovepad.GetControll(BBMovepad.Mainlayer, index);
                        if (MPC != null)
                        {
                            NodeGraph.EditedControll = MPC;
                            if (MPC.thisgraph == null)
                                MPC.thisgraph = NodeGraph.LoadGraph(MPC);
                            NodeGraph.EditedControll.thisgraph.filename = MPC.Graphfilename;
                            NodeGraph.EditedControll.thisgraph = MPC.thisgraph;
                            EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
                        }
                    }
                    BBDrawing.lastmousepos = mpos;
                }

                BBDrawing.leftmouseclickeventnumber++; // single event 
            }
            else if (BBDrawing.mouseup)
            {
                // hold the last mouse pos 
                //BBMovepad.RenderLayer(BBMovepad.Mainlayer, TXTINDEX.NORMAL);

                BBMovepad.RenderSingleButton(BBMovepad.Mainlayer, TXTINDEX.NORMAL, BBDrawing.lastmousepos);
                BBDrawing.leftmouseclickeventnumber = 0;
            }
        }
 
        int controlposfrom ;
        int controlposto ;



        /// <summary>
        /// OnGUI callback perform the BlockBuster User Interface functions 
        /// all element of the ui are not necessary within the function body 
        /// but might be called from this context 
        /// for instance All BBhaviors got their own GUI procecessed from this GUI CALL 
        /// </summary>
        void OnGUI()
        {

            // no game objects selected in the scene we just display 
            // the tool pannel without Selection related action 
            if (Selection.activeGameObject == null)
            {
                DisplayToolPannel(false);
                return;
            }

            // then catch the actor component of the active game object to perform 
            // on this object or iterate trough it s hierarchy 
            m_actor = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
            if (m_actor == null)
                AddActorComponent(Selection.activeGameObject, null);
            // grab the list of BBhaviours attached to the game object 
            BBehavior[] blist = Selection.activeGameObject.GetComponents<BBehavior>();
            // calculate the selection size for moves actions 
            BlockSize = CalculateSelectionSize(Selection.gameObjects);
            m_actor.Actorprops.block_size = BlockSize;

            // first line TabPannel Selection and Toggle for Movepad
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                // header of the window display the TabPannels section and change the selectedtab accordingly 
                if (GUILayout.Button("MOVE", EditorStyles.toolbarButton))
                    selectedtab = 0;
                if (GUILayout.Button("UI EDITOR", EditorStyles.toolbarButton))
                    selectedtab = 1;
                if (GUILayout.Button("TOOLS", EditorStyles.toolbarButton))
                    selectedtab = 2;
                if (GUILayout.Button("REPLAY", EditorStyles.toolbarButton))
                    selectedtab = 3;
                if (GUILayout.Button("DATASET", EditorStyles.toolbarButton))
                    selectedtab = 4;
                // beside the tabpannel selection we put a toggle to 
                // hide the movepad slider window ( for debug purposes ) 
                showmovepad = GUILayout.Toggle(showmovepad, "");
            GUILayout.EndHorizontal();

            // tab pannel with all gameobject selection related buttons activated 
            if (selectedtab == 2)
                DisplayToolPannel(true);
        

            if (selectedtab == 1)
            {
                // second line SAVE AND LOAD Layer 
                // a layer is serialized as *.MVP xml file that define the
                // icon position in Movepad Texture and  BBcontroll associated to this button 
                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save Layer"))
                        BBMovepad.Mainlayer.Save("bbmain.layer");
                    if (GUILayout.Button("Load Layer"))
                        BBMovepad.Mainlayer.Load("bbmain.layer", BBMovepad.Mainlayer);
                GUILayout.EndHorizontal();

                // Edit Mode Toggle , when the edit mode is activated 
                // we display the movepad gridd reference and a cllic on the icon will popup 
                // the nodegraph editor to modify or create the button logic ( action ) 
                BBControll.editgraph = GUILayout.Toggle(BBControll.editgraph, "Edit Mode");
                // unlayered toggle switche the movepad texture to its linear state 
                // ( all icons aligned to check the linear index of the icon in the sprite page 
                BBControll.unlayered= GUILayout.Toggle(BBControll.unlayered, "Unlayered");

                // <todo> render the movepad texture to material for Ingame Use ( unfinished yet ) 
                BBControll.RenderToMaterial = GUILayout.Toggle(BBControll.RenderToMaterial, "Render To material");
                // to be continued 
                if (BBControll.RenderToMaterial) 
                    EditorGUILayout.SelectableLabel("mat");

                // fill up a table with integers strings from 0 to movepad celnumber * movepad cellnumber
                // defined above in initialization of the movepad texture
                List<string> indexstrings  = new List<string> () ;
                for (int i = 0; i < BBMovepad.Mainlayer.GridSize * BBMovepad.Mainlayer.GridSize; i++)
                    indexstrings.Add(i.ToString());

                // next Line is the ( minimalist ) interface 
                // to assign a BBControll to a movepad Icon 
                GUILayout.BeginHorizontal();
                    // 2 dropdown list first is the control position in Movepad texture
                    controlposfrom = EditorGUILayout.Popup(controlposfrom, indexstrings.ToArray());
                    // next is the linear index of the icon in the unlayered sprite page 
                    controlposto = EditorGUILayout.Popup(controlposto, indexstrings.ToArray());
                    // if the couple of value is allready defined the action button beside 
                    // will clear the Dictionary element that define the link Movepad Button / BBcontroll action 
                    string buttonstring; 
                    BBControll bbc ;
                    if (BBMovepad.Mainlayer.DicCtrl.TryGetValue(controlposto, out bbc))
                        buttonstring = "REMOVE";
                    else
                        buttonstring = "ASSIGN";

                    // register the Movepad Button To TheBBcontroll dictionary 
                    // or remove the element according to button state 
                    if (GUILayout.Button(buttonstring))
                    {
                        if (buttonstring == "ASSIGN")
                        {
                            BBMovepad.RegisterButton(BBMovepad.Mainlayer, controlposto, controlposfrom);
                            BBMovepad.RenderLayer(BBMovepad.Mainlayer, TXTINDEX.NORMAL);
                        }
                        else
                        {
                            BBMovepad.Mainlayer.DicCtrl.Remove(controlposto);
                            BBMovepad.RenderLayer(BBMovepad.Mainlayer, TXTINDEX.NORMAL);
                        }
                    }
                GUILayout.EndHorizontal();

                BBDrawing.debugfloat1 = EditorGUILayout.Slider(BBDrawing.debugfloat1, 0.0f, 10.0f);
                
                BBDrawing.mouseclic = GUILayout.Toggle(BBDrawing.mouseclic, "mouseclic");
                BBDrawing.mousedown = GUILayout.Toggle(BBDrawing.mousedown, "mousedown");
                BBDrawing.mousedrag = GUILayout.Toggle(BBDrawing.mousedrag, "mousedrag");
                BBDrawing.mousemove= GUILayout.Toggle(BBDrawing.mousemove, "mousemove");
                BBDrawing.mouseup = GUILayout.Toggle(BBDrawing.mouseup, "mouseup");
                BBDrawing.popupmenuopen = GUILayout.Toggle(BBDrawing.popupmenuopen, "popupmenuopen");
                BBDrawing.griddraglock = GUILayout.Toggle(BBDrawing.griddraglock, "griddraglock");
                BBCtrlNode.EmiterSelector = GUILayout.Toggle(BBCtrlNode.EmiterSelector, "emiterselector");
                BBCtrlNode.inputslotselector = GUILayout.Toggle(BBCtrlNode.inputslotselector, "inputslotselector");
                BBCtrlNode.outputslotselector = GUILayout.Toggle(BBCtrlNode.outputslotselector, "outputslotselector");
               // GUILayout.Label(BBDrawing.lastclicdown.ToString(),"lastclicdown");

          





            }
            // NExt TabPannel is related to the list of 
            // BBhaviours attached to the GameObject ( add remove and change their values )
            if (selectedtab == 4)
            {
                String filepath;
                // first button Line Save Or Load a set of BBhaviours on the gameobject selected 
                // it s mainly used to run the game , make a fine tuning of the parameter , save the values 
                // then recall the setup when the game stop and the valuye return to their original state 
                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("SAVE"))
                    {
                        if (Selection.activeGameObject == null)
                            return;
                        filepath = BBDir.Get(BBpath.DATASET) + m_actor.Actorprops.guid + ".xml";
                        m_actor.Actorprops.Save(filepath, typeof(BaseActorProperties));
                        BBehavior[] BHL = Selection.activeGameObject.GetComponents<BBehavior>();
                        foreach (BBehavior tmp in BHL)
                        {
                            filepath = BBDir.Get(BBpath.DATASET) + tmp.GetDataset().GetGuid() + ".xml";
                            tmp.GetDataset().Save(filepath, tmp.GetDataset().GetType());
                        }
                    }
                    if (GUILayout.Button("LOAD"))
                    {
                        if (Selection.activeGameObject == null)
                            return;
                        filepath = BBDir.Get(BBpath.DATASET) + m_actor.Actorprops.guid + ".xml";
                        //Selection.activeObject.GetComponent(Platform).paramblock.Load(filepath);
                        if (!System.IO.File.Exists(filepath))
                            return;
                        Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
                        BaseActorProperties B = BaseActorProperties.Load(filepath); // deserialise pblock 
                        A.Actorprops = B;
                        LoadBBhaviorfromActorlist(Selection.activeGameObject);
                    }
                GUILayout.EndHorizontal();
                // next line Add or Remove a BBhaviour script to the selected gameobject 
                // the bbhaviour script added or removed is the one currently selected in the DropDownList below 
                GUILayout.BeginHorizontal();
                    if (GUILayout.Button(" add  "))
                    {
                        string s = blockbuster.behaviourenum.ToString();
                        System.Type T = BBTools.GetTypeFromClassName(s);
                        bool add = true;
                        foreach (BBehavior B in blist)
                            if (B.GetType() == T)
                                add = false;
                        if (add)
                        {
                            //Behavior b = new Behavior();
                            Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
                            BBehavior b = (BBehavior)Selection.activeGameObject.AddComponent(T) ;
                            System.Guid g =System.Guid.NewGuid();
                            Dataset d = b.GetDataset();
                            d.guid = g.ToString();
                            d.fullqualifiedclassname = d.GetFullQualifiedClassName();
                            m_actor.Actorprops.BehaviorListID.Add (g.ToString());
                            d.suportedclassname = T.AssemblyQualifiedName;
                        }
                    }
                    if (GUILayout.Button("remove"))
                    {
                        string s = blockbuster.behaviourenum.ToString();
                        System.Type T = BBTools.GetTypeFromClassName(s);
                        bool haveit = false;
                        foreach (BBehavior B in blist)
                            if (B.GetType() == T)
                                haveit = true;
                        if (haveit)
                        {
                            Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
                            BBehavior tbv = (BBehavior) Selection.activeGameObject.GetComponent(T.ToString());
                            DestroyImmediate(tbv);
                            BBehavior[] BHL = (BBehavior[])Selection.activeGameObject.GetComponents<BBehavior>();
                            A.Actorprops.BehaviorListID.Clear();
                            foreach (BBehavior B in BHL)
                                A.Actorprops.BehaviorListID.Add(B.GetDataset().GetGuid());
                        }

                    }
                GUILayout.EndHorizontal();

                // call the dynamic Enum creation ( above in BBTools ) 
                // to generate the list of actives BBhaviours scripts 
                // if the list got flushed for example after a script compile 
                if (blockbuster.behaviourenum == null)
                    InitGUIValues();
                // the Enum Dropdownlist to select the bbhaviour to add or remove
                blockbuster.behaviourenum = (System.Enum)EditorGUILayout.EnumPopup("behaviour:", blockbuster.behaviourenum, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));

                // the next statment call the custom GUI loop defined in each BBhaviour script 
                foreach (BBehavior B in blist)
                    B.DoGUILoop(MAINWINDOWRECT);
                   
            }




            // desactivated replay tab , no chance to have a 666 value here 
            // if you get it anyway .. call an exorcist :) 
            // actually the replayer could be activated by replacing 666 by 3 
            // but since its just a draft port of another context it wont work properly without 
            // few modification , i tried it but right now there is no point to save a replay on procedural moves 
            if (selectedtab == 666)
            {
                if (m_replayactors.Count == 0) // ||  m_replayactors[0] == null)
                {
                    pathindex = 0;
                    AddReplayerObject();
                }
                if (m_replayactors[0] == null) // ||  m_replayactors[0] == null)
                {
                    pathindex = 0;
                    AddReplayerObject();
                }
                m_replayer = (BBRePlayer)m_replayactors[0].GetComponent(typeof(BBRePlayer));
                m_replayer.RefreshXmlBase();
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                    if (GUILayout.Button("remote", EditorStyles.toolbarButton))
                        replayerdtab = 0;
                    if (GUILayout.Button("filter", EditorStyles.toolbarButton))
                        replayerdtab = 1;
                GUILayout.EndHorizontal();
                if (replayerdtab == 0)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.TextField(m_replayactors[0].transform.position.ToString());
                    m_replayer.bviewportcamfollowing = EditorGUILayout.Toggle("viewport camera follow ", m_replayer.bviewportcamfollowing);
                    replayspeed = EditorGUILayout.Slider("speed", replayspeed, 0, 1);
                    m_replayer.replayspeed = replayspeed;
                    if (GUILayout.Button("RUN SCENE", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                        return;
                    EditorGUILayout.EndVertical();
                }
                if (replayerdtab == 1)
                {
                    string[] L = ReturnXmlContent();
                    if (!b_applyfilter)
                    {
                        m_replayer = (BBRePlayer)m_replayactors[0].GetComponent(typeof(BBRePlayer));
                        if (m_replayer.m_replayfiletag != L[pathindex] + ".xml")
                        {
                            m_replayer.m_replayfiletag = L[pathindex] + ".xml";
                            SwitchReplayerPath(m_replayer.m_replayfiletag);
                        }
                    }
                    else
                        m_replayer.m_replayfiletag = ".xml";
                    GUI.BeginGroup(new Rect(0, 0, 600, 600));
                    pathindex = EditorGUILayout.Popup(pathindex, L);
                    b_applyfilter = EditorGUILayout.Toggle("Display All Path", b_applyfilter, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                    GUI.EndGroup();
                }
            }



  
            // catch the selected game object
            // check if it have an actor component and add in case of no 
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                m_actor = (Actor)go.GetComponent(typeof(Actor));
                if (m_actor == null)
                {
                    AddActorComponent(go,  null);
                    m_actor = (Actor)go.GetComponent(typeof(Actor));
                    BlockSize = CalculateSelectionSize(Selection.gameObjects);
                    if (m_actor == null)
                    {
                        GUI.Label(new Rect(0, 20, 200, 200), "SELECT A VALID GAME OBJECT");
                        return;
                    }
                }

            }

            // the Move Section of the TabPannel 
            // ( not defined in the same order in the ongui body . but it doesnt matter ) 
            if (selectedtab == 0)
            {
                Actor.b_groupselectmode = EditorGUILayout.Toggle("GrpMode", Actor.b_groupselectmode);
                b_fixedstepedit = EditorGUILayout.Toggle("fixed predefined move  ", b_fixedstepedit);
                  

                //------------------------------------------------------------------ SLIDER FOR FIXED OFSET MOVE #2
                stepvalue = EditorGUILayout.Slider("fixed step ", stepvalue, 0, 4);
                //------------------------------------------------------------------ NEXT ASSET BUTTON #3
                activebasename = (ACTIVEBASENAME)EditorGUILayout.EnumPopup("base:", activebasename);
                switch (activebasename)
                {
                    case ACTIVEBASENAME.HIGHTECH:
                        selectedbasename = "HIGHTECH/";
                        break;
                    case ACTIVEBASENAME.JUNGLE:
                        selectedbasename = "JUNGLE/";
                        break;
                    case ACTIVEBASENAME.TEMPLE:
                        selectedbasename = "TEMPLE/";
                        break;
                    case ACTIVEBASENAME.SANDBOX:
                        selectedbasename = "SANDBOX/";
                        break;
                }
                assetsliderindex = (int)EditorGUILayout.Slider("quick select", assetsliderindex, 0, data.Count );
                assetbaseindex = assetsliderindex;

                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("NEXT PRESET"))
                        BrowsePresset(1);
                    if (GUILayout.Button("PREV PRESET"))
                        BrowsePresset(-1);
                GUILayout.EndHorizontal();

                m_actor.bbeditor_fixedstep = b_fixedstepedit;
  
            }
            // movepad can be desactivated for debug 
            if (showmovepad)
            {
                movepadpos.width = Screen.width;
                movepadpos.height= Screen.height;
                // do the movepad window 
                BeginWindows();
                    Rect R = new Rect();
                    // Do the Movepad Window callback 
                    R = GUI.Window(1, movepadpos, MVP_DoMovepad, "SLIDE ME : MOVEPAD V 0.01 ALF (c) 2015 " + R.y.ToString());
                    float ylimitation = (Screen.height - 320);
                    float ymin  = (Screen.height - 40);
                    if ( R.y < ylimitation )
                        R.y =ylimitation ;
                    if (R.y > ymin)
                        R.y = ymin;
                    Vector2 VMP = new Vector2(movepadpos.xMin, R.y);
                    movepadpos.Set(movepadpos.xMin, R.y, movepadpos.width, 0);
                EndWindows();
                if (BBControll.editgraph)
                    BBMovepad.ShowMovePadGrid(BBMovepad.Mainlayer, BBMovepad.mvpd_rect.position + VMP, true,BBControll.textureddlist);
            }

            Repaint();
           // EditorGUILayout.EndScrollView();
        }
    }














