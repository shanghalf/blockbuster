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



public enum ACTIVEBASENAME
{
	HIGHTECH = 0, 
	JUNGLE = 1, 
	TEMPLE = 2,
	SANDBOX = 3

   
}

// add test comment


/// <summary>
/// nice stuff found on the net i m not using right now but keepa as ref 
/// thanks to Linusmartensson from unity comunauty 
/// to share such simple handy and precious info 
/// </summary>

/*
    public class Movepad : EditorWindow
    {
        public string name = "MOVEPAD";
        [MenuItem("Window/movepad")]
        static void init()
        {
            string uitex = Application.dataPath + "/BLOCKBUSTER/Editor/BBResources/movepad_interface.png";
            //t = LoadPNG(uitex);
            EditorWindow.GetWindow<Movepad>();
        }
        Rect windowpos = new Rect(5, 100, 200, 150);
        Rect movepadpos = new Rect(5, 5, 200, 150);
        string result = "no result";
        
        Texture2D t; //new Texture2D(int.Parse( r.width.ToString()) ,int.Parse(  r.height.ToString()) );
        Texture2D LoadPNG(string filePath)
        {
            Rect picsize = new Rect(5, 100, 200, 150);
            Texture2D tex = null;
            byte[] fileData;
            if ( File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(int.Parse(picsize.width.ToString()), int.Parse(picsize.height.ToString()));
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
        void OnGUI()
        {
            BeginWindows();
            windowpos=GUI.Window(1, windowpos, doWindow, "d");
            GUI.Button(new Rect(0, 30, 100, 50), "Wee!");

            GUI.Button(new Rect(windowpos.x, windowpos.y, 100, 100), "delegated button");

            EndWindows();
        }

        void doWindow(int i)
        {
            //GUI.Button(new Rect(1,1, 100,20), "delegated button");
            //movepadpos.position = Event.current.mousePosition;
            //result = Event.current.mousePosition.ToString();//+themovepad.GetInstanceID().ToString() + "/" + mouseOverWindow.GetInstanceID().ToString();
            //GUI.DrawTexture(movepadpos, t);
  
            GUI.DragWindow();
        }

        public  string Getname()
        {
            return name;
        }
    }
*/







public static class ROOTDEF
{
    public static string projectname = "BLOCKBUSTER";
}






public static class BBTools
{
    // this class is a Helper to manage Actor Behaviors 
    // private  Dictionary<string, string> BehaviorDic = new Dictionary<string, string>();

    public static string ROOTFOLDER= "BLOCKBUSTER";
    public static bool debugmode = false;
    public static bool showgrid = false;
    public static bool buttonpage = false;
   




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


    public static void  Deserialize(string path , object o , System.Type T)
    {
        if (!System.IO.File.Exists(path))
        {
            //debug.Log("file not exist");
            return ;
        }
        XmlSerializer serializer = new XmlSerializer(T);
        Stream stream = new FileStream(path, FileMode.Open);
        o = serializer.Deserialize(stream) as object ;
        stream.Close();
    }

  


    public static System.Type castenum()
    {
        System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        AssemblyName aName = new AssemblyName("blockbusterbehavior_a");
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

        try
        {
            System.Type T = eb.CreateType();
            ab.Save(aName.Name + ".dll");
        }
        catch
        { }
        System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFrom("blockbusterbehavior_a.dll");
        System.Type castenum = ass.GetType("blockbusterbehavior_a");
        return castenum;
    }

    public static System.Type GetClassDataset(string behaviorclassclass)
    {
        AssemblyName assembly = new AssemblyName("ALFTEST");

        System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        //System.AppDomain appDomain = System.Threading.Thread.GetDomain();
        AssemblyName aName = new AssemblyName(behaviorclassclass);
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(aName.Name);
        //create the class
        TypeBuilder typeBuilder = moduleBuilder.DefineType("BBdataset", TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                            TypeAttributes.BeforeFieldInit, typeof(Behaviour));
        System.Type TT = typeBuilder.CreateType();
        //lastNamePropertySetter.CreateMethodBody(
        return TT;
    }




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



    public static void Save(string path, System.Type type, object o)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, o);
        stream.Flush();
        stream.Close();
    }

    public static GameObject[] getchildrens(GameObject o)
    {
        List<GameObject> L = new List<GameObject>();
        for (int c = 0; c < o.transform.childCount; c++)
            L.Add(o.transform.GetChild(c).gameObject);
        return L.ToArray();
    }


}





    public class blockbuster : EditorWindow  
    {

        public enum GridIndexType
        {
            LINERAR = 0,
            FUNCTION = 1,
            R = 2,
            G = 3,
            B = 4,
            A = 5
        }



        // editortick is used instead of Time.Delta in editor LIVE mode
        //static float EditorTick = 0.1f;
        Actor m_actor = null;
        bool
            b_front_X,          // determine if editor camera point roughly along X axis ( to keep a viewport relative block move ) 
            b_fixedstepedit,    // manipulate actor on defined step offset instead of using it block size 
            b_groupselectmode   // used to 
            ;
        float
            stepvalue = 0.0f,
            ofset = 0.0f
            ;
        int
            i = 0,
            //APX = 0,
            assetbaseindex = 0,
            assetsliderindex = 0
            //BBiteratemaxobjects = 10
            ;
        public static GridIndexType bbGrdtype;
        public int toolbarInt;
        private int currentpreset;
        
        Vector3 BlockSize = new Vector3(0, 0, 0);
        Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);
        //====================== FOR MOUSE EVENT 

       
        

        private static int mvpd_grsz = 8 ;
        private static int mvpd_bsz = 32;
        private static int mvpd_txtsz = mvpd_bsz * mvpd_grsz ;
        static Rect movepadpos = new Rect(0, 0, mvpd_txtsz, mvpd_txtsz);


        int methodindex ;
        int buttonnumber;



        int oldindexstorage;
        ACTIVEBASENAME activebasename;
        List<GameObject> hidenobjectlist = new List<GameObject>();
        string selectedbasename = BBDir.Get(BBpath.BBGBASE)+ "/HIGHTECH/";
        public List<string> data = new List<string>();
        private int pathindex;

        private bool Staticfunctiononly;

        private int levelID;
        public GameObject m_RePlayerObject;
        public List<GameObject> m_replayactors = new List<GameObject>();
        public List<string> xmlfoldercontent = new List<string>();
        private bool b_applyfilter;
        private BBRePlayer m_replayer;
        private float replayspeed;
        public static System.Enum behaviourenum;
        Vector2 MovepadMousePos = new Vector2();
        public bool showmovepad = true;

        [MenuItem("BlockBuster/BBmainWindow")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(blockbuster));
            InitGUIValues();
        }

        void Start()
        {

        }

        public void UpdateDatasetList(GameObject G)
        { 
        

        }

        public bool savescene(bool preset = false, string scenename = null)
        {
            string repo = "";
            string defname = "";
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
                
                //string assetpath = ("Assets/BLOCKBUSTER/"+ basename+"/"+blk.assetname + ".fbx");

                // asset are loaded that way by passing root without full datapath ( thanks unity )  
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

        public Actor GetNewObject(Type t)
        {
            try
            {
                return (Actor)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            catch
            {
                BBTools.BBdebug ("Object do not have proper Constructor");
                return null;
            }
        }

        // cast origin actor to desired behavior 



        void AddActorComponent(GameObject obj,  Actor originalactor)
        {
            // ****************************************************************************
            // add a custom script on gameobject 
            // this function should be called for every new block in the scene
 
            MeshFilter M;
            obj.AddComponent(typeof(Actor)); 				// refer to block setup script ( block behaviour ) 
            m_actor = (Actor)obj.GetComponent(typeof(Actor));		// bs is at global scope 
            M = (MeshFilter)obj.GetComponent(typeof(MeshFilter));			// get some info from mesh to custom script 
            // -------------------------------------------------------------------------------------- init the block properties  ( parameterblock ) 
            m_actor.Actorprops.block_size = M.renderer.bounds.size;
            //m_actor.block_transform = obj.transform;	 	// transform from source	
            m_actor.Actorprops.orig_rotation = obj.transform.rotation; // origin transform help to re initialize a block in static mode 
            m_actor.Actorprops.orig_pos = obj.transform.position;		// same for pos ( sound weird could be done in oneline have to see that 	
            //m_actor.scenerefobj = obj;

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

            // ui block ( should get the rid of this intermediate value and use directly one ref <TODO>
            //UIPB = m_behavior.paramblock;
            //UIAP = m_actor.Actorprops;



        }

        /*
        int GetNearPathnode()
        {

            float v;
            int index = 0;
            float max = Mathf.Infinity;
            for (int it = 0; it < m_behavior.paramblock.m_pathnodes.Count; it++)
            {
                v = Vector3.Distance(m_behavior.paramblock.m_pathnodes[it].pos, Selection.activeGameObject.transform.position);
                if (v < max)
                {
                    max = v;
                    index = it;
                }

            }

            return index;
        }
        */

        void DoBlockMove(bool instanciate, Vector3 dir, bool moveallpath = true)
        {
            //************************************************************************************************
            // perform block manipulation in move block section of the tool 
            // there s 2 diferent way to move a block during the runtime ( dynamicaly ) have to pay atention to 
            // rotation blocks where there s a bunch of generated sub component ( root node and looktarget ) 
            // rotation blocks use a lookat function which is more convenient to handle INSTANCIATE if the block 
            // nedd to be duplicated 

            string str;


            //if (b_followpath)
            // bs.paramblock.pathnodes[bs.paramblock.targetindex] = Selection.activeGameObject.transform.position;

            // iterate on the selection 
            foreach (GameObject go in Selection.gameObjects)
            {
                //GameObject ts = (GameObject)Selection.gameObjects.GetValue(i);

                m_actor = (Actor)go.GetComponent(typeof(Actor));						// should be there 
                m_actor.Actorprops.last_pos = go.transform.position; // make sure the pos is right 
                if (m_actor == null)
                    continue;


                if (instanciate)
                {	// ------------------------- MOVE AND DUPLICATE

                    
                    GameObject obj = (GameObject)Instantiate(go, go.transform.position, go.transform.rotation);
                    
                    str = go.name;										// change the name  
                    string[] strarray = str.Split(new char[] { '-' });
                    obj.name = strarray[0] + obj.GetInstanceID();								// final name is block original name plus unique id 
                    if (go.transform.parent)
                        obj.transform.parent = go.transform.parent;
                    
                    Guid g;
                    g = Guid.NewGuid();
                    if (m_actor)
                    {
                        m_actor.Actorprops.guid = g.ToString();//obj.GetInstanceID().ToString();
                        m_actor.Actorprops.parentgui = m_actor.Actorprops.guid;
                        m_actor.Actorprops.orig_pos = go.transform.position;
                        m_actor.Actorprops.orig_rotation = go.transform.rotation;



                        m_actor.Actorprops.BehaviorListID.Clear();
                        // refresh dataset guiid list 
                        BBehavior[] BHL = obj.GetComponents<BBehavior>();
                        foreach (BBehavior B in BHL)
                        {
                            Dataset localdataset = B.GetDataset();
                            string newguid  = System.Guid.NewGuid().ToString();
                            localdataset.SetGuid(newguid.ToString());
                            m_actor.Actorprops.BehaviorListID.Add(newguid);

                            // translate pathnodes 
                            List<Pathnode> pnodes = localdataset.GetPathNodes();
                            if (pnodes== null )
                                continue;
                            if (pnodes.Count == 0)
                                continue;
                            // in edit sub mode we move only the current pathnode 
                            if (!localdataset.editsub)
                                foreach (Pathnode pn in pnodes)
                                    pn.pos += dir; // else move closest to obj 
                            else
                                pnodes[localdataset.targetindex].pos += dir;

                        }
                        

                     }

                    //AddPlatformComponent(obj, null);
                }
                go.transform.position += dir;

              

                // static block

                BBehavior[] blist = go.GetComponents<BBehavior>();
                foreach (BBehavior B in blist)
                {
                    Dataset localdataset = B.GetDataset();
                    List<Pathnode> pnodes = localdataset.GetPathNodes();
                    if (pnodes == null)
                        continue;
                    // in edit sub mode we move only the current pathnode 
                    foreach (Pathnode pn in pnodes)
                            pn.pos += dir; // else move closest to obj 
                }


                

                Repaint();


            }

        }


        void GetDir()
        {
            // **************************************************************************************
            // make sure that the direction buttons of the move tool are related to the camera 
            // did not search a better way to manage this but i m sure there s something more 
            // simple to do directly in the moveblock function 
            // todo this function work fine but it s definitely not the correct method have to define proper transformation 
            // for all cases stick in world coordinate and cook on the fly sorry for this it s one of the first function defined in this tool 
            // but actually makeit complicated to evolve  especially cause its picky to know what would come out of 3dsmax 
            // need to ensure a consistent transform along the tool chain < huge > 

            if (SceneView.currentDrawingSceneView == null) return;
            Transform cam = SceneView.currentDrawingSceneView.camera.transform;
            Vector3 flatcamvector = new Vector3(cam.forward.x, 0.0f, cam.forward.z);
            float AF = Vector3.Angle(flatcamvector, Vector3.forward);
            float AB = Vector3.Angle(flatcamvector, Vector3.back);
            float AL = Vector3.Angle(flatcamvector, Vector3.left);
            float AR = Vector3.Angle(flatcamvector, Vector3.right);

            float[] anglearray = new float[] { AF, AB, AL, AR };

            Array.Sort(anglearray);
            if (AF == anglearray[0])
            {
                front = Vector3.forward;
                back = Vector3.back;
                left = Vector3.right;
                right = Vector3.left;
                b_front_X = false;
            }
            if (AB == anglearray[0])
            {
                front = Vector3.back;
                back = Vector3.forward;
                left = Vector3.left;
                right = Vector3.right;
                b_front_X = false;
            }
            if (AL == anglearray[0])
            {
                front = Vector3.left;
                back = Vector3.right;
                left = Vector3.forward;
                right = Vector3.back;
                b_front_X = true;
            }
            if (AR == anglearray[0])
            {
                front = Vector3.right;
                back = Vector3.left;
                left = Vector3.back;
                right = Vector3.forward;
                b_front_X = true;
            }

            // inform a potential movig platform for direction of the camera 
            MovingPlatform  B = (MovingPlatform)Selection.activeGameObject.GetComponent(typeof(MovingPlatform));
            if (B != null)
                B.paramblock.b_front_x = b_front_X;
            m_actor.b_front_X = b_front_X;


        }



        Vector3 CalculateSelectionSize(GameObject[] gameobjects)
        {
            // **************************************************************************************
            // return the global size of given obj array used in block move section   
            // 

            if (gameobjects == null) 										// called on GUI event may be for nothing 
                return Vector3.zero;



            ArrayList xar = new ArrayList();									// vect array for bbox
            ArrayList yar = new ArrayList();
            ArrayList zar = new ArrayList();


            for (var i = 0; i < gameobjects.GetLength(0); i++)
            {
                GameObject TGO = (GameObject)gameobjects[i];


                MeshFilter M = (MeshFilter)TGO.GetComponent("MeshFilter");				// push all bbox in minmax array 
                if (M == null) continue;									// at least if the object got a mesh render 

                xar.Add((float)M.renderer.bounds.max.x);
                xar.Add((float)M.renderer.bounds.min.x);
                yar.Add((float)M.renderer.bounds.max.y);
                yar.Add((float)M.renderer.bounds.min.y);
                zar.Add((float)M.renderer.bounds.max.z);
                zar.Add((float)M.renderer.bounds.min.z);

            }
            if (xar.Count == 0) 											// nothing come >>exit 
                return Vector3.zero;

            xar.Sort();													// tidy it up 
            yar.Sort();
            zar.Sort();

            Vector3 tv = new Vector3(0.0f, 0.0f, 0.0f);				// global bbox

            tv.x = (float)xar[xar.Count - 1] - (float)xar[0];
            tv.y = (float)yar[xar.Count - 1] - (float)yar[0];
            tv.z = (float)zar[xar.Count - 1] - (float)zar[0];

            return tv;
        }




        public bool ReadAssetBase()
        {
            // **************************************************************************
            // the asset base xml file is generated by 3dsmax exporter in asset folder 
            // its basically a list of all fbx under that specific path 
            // should be replaced soon by a multi base selector 
            // this function populate the global data array ( name of asset ) 

            //debug.Log(EditorWindow.focusedWindow.title.ToString()+ " " + caller );

            if (data != null)
                data.Clear();  // clear the base 

            // next statment is a little patch should evolve to a multi base management 
            String filepath = BBDir.Get(BBpath.BBGBASE) + selectedbasename + "AssetList.xml";
            // use xml parser
            //debug.Log(filepath);


            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(filepath))
            {

                xmlDoc.Load(filepath);
                if (xmlDoc == null)
                    return false;

                // populate the base 
                XmlNodeList Asset_list = xmlDoc.GetElementsByTagName("AssetsList");

                if (Asset_list == null)
                {
                    //debug.Log("asset list file is corrupted"); 
                    return false;
                }
                XmlNodeList Item_list = Asset_list.Item(0).ChildNodes;

                for (i = 0; i < Item_list.Count; i++)
                {
                    XmlNodeList l = Item_list.Item(i).ChildNodes;
                    if (data != null)
                        data.Add(l[0].InnerText);
                }
                ////debug.Log( data.length.ToString()  + " nb of elements " ) ;
                return true;
            }
            else
                Debug.Log(data.Count.ToString() + " no XML database");

            return false;
        }


        public void BrowseAsset(int next)
        {
            // ***************************************************************
            // browse asset from base todo  should be changeed to manage multiple base 

            if (b_groupselectmode) return;
            GameObject tgo = (GameObject)Selection.activeObject;
            if (tgo == null)
                return;
            ReadAssetBase();  																	// read the base definition ( generated by 3dsmax during  export ) 

            Actor A = (Actor)tgo.GetComponent(typeof(Actor));
            BaseActorProperties BAP = A.Actorprops; ;

            BBehavior[] blist = tgo.GetComponents<BBehavior>(); 

            int index;
            if (Mathf.Abs(next) == 1)
            {
                assetbaseindex = Mathf.Abs(assetbaseindex + next);								// loop index todo check index base seems to have a small bug here 
                index = assetbaseindex % data.Count;
                assetsliderindex = index;
                assetbaseindex = index;
            }
            else
            {
                index = assetsliderindex;
                BBTools.BBdebug(next.ToString());
            }
            string assetname = (data[index]);													// load and swap 
            GameObject prefab = (GameObject)Resources.LoadAssetAtPath(("Assets" + selectedbasename + assetname + ".fbx"), typeof(GameObject));
            if (prefab == null)
            {
                BBTools.BBdebug("prefab load fail " + ("Assets" + selectedbasename + assetname + ".fbx"));
                return;
            }
            // the new instance need a block controller as well and get whatever it can grab on the 
            // original object // in AddPlatformComponent funbtion ( add new fresh script ut can also take props from source 
            GameObject instance = (GameObject)Instantiate(prefab, tgo.transform.position, tgo.transform.rotation);
            instance.name = prefab.name + instance.GetInstanceID();
            instance.AddComponent(typeof(Actor));
            Actor swapactor = instance.GetComponent<Actor>();
            swapactor.Actorprops = BAP;
            swapactor.Actorprops.assetname = assetname;

            foreach ( BBehavior B in blist ) 
            {
                System.Type T = B.GetType();
                instance.AddComponent(T.Name);
                BBehavior SB =(BBehavior) instance.GetComponent(T.Name);
                //SB = B;
                Dataset D = B.GetDataset();
                SB.SetDataset( D ) ;

            }

            DestroyImmediate(Selection.activeObject);
            Selection.activeGameObject = instance;

        }



        public void placefromxmlfile()
        {
            // **************************************************************************
            // the asset base xml file is generated by 3dsmax exporter in asset folder 
            // this function place assets from unity base related to the assettransfert.xml generated by max tool 
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
                    XmlNodeList l = Item_list.Item(i).ChildNodes;
                    //debug.Log(l[0].InnerText);
                    GameObject prefab = (GameObject)Resources.LoadAssetAtPath(("Assets" + selectedbasename + l[0].InnerText + ".fbx"), typeof(GameObject));
                    if (prefab == null)
                    {
                        //debug.Log(  ( "Assets/" +selectedbasename + l[0].InnerText +".fbx") );
                        return;
                    }
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
                    if (b_groupselectmode)
                    {
                        tempobjarray.Add(instance);
                        ////debug.Log( tempobjarray.length.ToString());
                    }
                }
                if (b_groupselectmode)
                {
                    ////debug.Log( b_groupselectmode.ToString() ) ;
                    for (int i = 0; i < tempobjarray.Count; i++)
                        Selection.objects.SetValue(tempobjarray[i], i);
                    GameObject tgo = (GameObject)Selection.activeGameObject;
                    //debug.Log(tgo.name);
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





        // movepad interface composition in diferent state 
        public static Dictionary<int, int> mvp_dic_normal= new Dictionary<int, int>();
        // movepad interface normal state 
        static Texture2D mvp_texture_button_clic = new Texture2D(mvpd_txtsz, mvpd_txtsz);
        // movepad interface normal state 
        static Texture2D mvp_texture_button_normalstate = null;
        // mask to identify the button on rgba Red is for function id 
        static Texture2D mvp_texture_func_id = new Texture2D(mvpd_txtsz, mvpd_txtsz);
        // target for composition 
        static Texture2D mvp_texture_target = new Texture2D(mvpd_txtsz, mvpd_txtsz);

        static List<Texture2D> mvp_textures_array = new List<Texture2D>();

        

        static bool InitGUIValues()
        {
            //  todo fill the array with all png in editor folder 
            // and texture would be avaiable on name  T.B.C 
            //**************************************************
            System.Type T = (System.Type)BBTools.castenum();
            behaviourenum = (System.Enum)System.Activator.CreateInstance(T);
            //System.Type TT = behaviorManager.GetClassDataset();

            List<string> tlist = new List<string>() ;
            tlist.Add(BBDir.Get(BBpath.RES) + "movepad_i.png");
            tlist.Add(BBDir.Get(BBpath.RES) + "movepad_i_cl.png");
            tlist.Add(BBDir.Get(BBpath.RES) + "movepad_id.png");
            tlist.Add(BBDir.Get(BBpath.RES) + "");
            tlist.Add(BBDir.Get(BBpath.RES) + "outclear_256.png");


            string layername = "bbmain";
            BBCtrl.RegisterLayer(layername, tlist, 8, 32);



            


            BBCtrl.RegisterButton(layername, 1, 0, "");
            BBCtrl.RegisterButton(layername, 8, 2, "");
            BBCtrl.RegisterButton(layername, 10, 3, "");
            BBCtrl.RegisterButton(layername, 17, 1, "");
            BBCtrl.RegisterButton(layername, 19, 9, "");
            BBCtrl.RegisterButton(layername, 3, 8, "");
            BBCtrl.RegisterButton(layername, 5, 4, "");
            BBCtrl.RegisterButton(layername, 13, 5, "");
            BBCtrl.RegisterButton(layername, 21, 6, "");
            //********* test with parameters to pass 
            BBCtrl.RegisterButton(layername, 7, 7, "");

            BBCtrl.RenderLayer(layername, TXTINDEX.NORMAL);

            BBCtrl.Init();


            return true;
        }





        void Update()
        {
            if (Selection.activeGameObject == null)
                return;
        }

        void OnInspectorUpdate()
        {
            //***********************************************************************************
            // update inspector sheet according to the tool value 
            Repaint();
            return;


            if (Selection.gameObjects.Length == 0 || EditorWindow.focusedWindow == null)
                return; 																// out of Unity nothing to refresh 
            var gg = Selection.activeGameObject.transform.parent;

            if (gg != null && b_groupselectmode)
            {
                Selection.activeObject = gg;
            }


            if ((EditorWindow.focusedWindow.title == "blockbuster"))
            {				// Block Buster got the focus: so play that funky music white boy  
 
                if (assetsliderindex - 1 != oldindexstorage)
                    BrowseAsset(assetsliderindex - 1);												// till you die ....:::..::...::...::.::.:.:.:::
                oldindexstorage = assetsliderindex - 1;
                Repaint();
                //SceneView.RepaintAll();
                //DebugUtils.Log(Core.LogCategory.Gamelogic, R.replayspeed.ToString());
                if (m_replayer != null)
                    m_replayer.Update();
            }

            GameObject tg = (GameObject)Selection.activeGameObject;
            if (b_groupselectmode && tg.transform.parent)
            {
                ////debug.Log("select");
                //Selection.activeObject = Selection.activeObject.transform.parent;
            }

            
            ////debug.Log(bs.parent.name) ;
        }



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

        void OnLevelWasLoaded(int level)
        {
            m_replayactors.Clear();

        }
        void OnDestroy()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (var it in allObjects)
            {
                if (it.name == "REPLAYERINSTANCE")
                    DestroyImmediate(it);
            }
        }




        void SwitchReplayerPath(string filename)
        {
            foreach (BBReplay ri in m_replayer.replaylist)
                if (ri.m_xmlfilename.Contains(filename))
                    m_replayer.m_playerreplay = ri;
            m_replayer.targetindex = 0;
        }




        int selectedtab = 0;
        float replayerdtab = 0;

        Vector2 scrollPos = new Vector2();


    

        private void ichangeEditorFlag(bool bstatic)
        {
            StaticEditorFlags flags = new StaticEditorFlags();
            if (bstatic)
                flags = (StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.NavigationStatic);
            foreach (GameObject G in Selection.gameObjects)
                GameObjectUtility.SetStaticEditorFlags(G, flags);
        }


        public delegate void spinner(int i);


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




        public void refreshActorDatasetList(  GameObject o , BaseActorProperties BAP)
        {
            BBehavior[] BHL = (BBehavior[])o.GetComponents<BBehavior>();
            BAP.BehaviorListID.Clear();
            foreach (BBehavior B in BHL )
                BAP.BehaviorListID.Add( B.GetDataset().GetGuid());
        }


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

        void DisplayToolPannel(bool showobjectactionbuttons)
        {
                b_groupselectmode = EditorGUILayout.Toggle("GrpMode", b_groupselectmode);

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


                if (showobjectactionbuttons) // object seleted in the scene
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("LOAD PRESET"))
                        loadscene(true);
                    if (GUILayout.Button("SAVE PRESET"))
                        savescene(true);
                    GUILayout.EndHorizontal();



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

        public static int[] MVP_RectFromIndex(int index, int celsize, int gridsize)
        {

            int px = ((index) * celsize) % mvpd_txtsz;// base index 0
            int py = ((((index) * celsize / mvpd_txtsz)) * celsize);
            int[] ret = new int[] { px, py, celsize, celsize };
            return ret;
        }
        public static void MVP_ComposeMovepad(int mousestate)
        {

            BBdebug.SaveMovepadTarget("test.png", mvp_texture_target);
        }

        public static void MVP_dClearTarget (Texture2D txt )
        {
            int txtsz =(int)  Math.Pow(mvpd_grsz * mvpd_bsz , 2) ;
            mvp_texture_target = txt;
        }

        //public static void MVP_UpdateSingleButton(int state, int index = -1, int cx = 0, int cy = 0)
        //{

        //    int i;
        //    if (index < 0)
        //        i = MVP_returngridindex(cx, cy);
        //    else
        //        i = index;

        //    if (state > mvp_textures_array.Count)
        //    {
                
        //        Debug.Log( "no texture at index : "+state.ToString());
        //        return;
        //    }

        //    int y = mvpd_txtsz - BBControllManager.MVPBSZ;

        //    if (!BBControllManager.layerentry.ContainsKey(i))
        //    {
        //        Debug.Log("no entry key at : " + i.ToString());
        //        return;
        //    }

        //    int[] i4 = MVP_RectFromIndex(BBControllManager.layerentry[i], BBControllManager.MVPBSZ, BBControllManager.MVPGSZ);  // { (entry.Value * bsz) % texturesize, texturesize - ((1 + entry.Value / gsz) * bsz), bsz, bsz };
            
        //    Color[] pix = mvp_textures_array[state].GetPixels(i4[0],y-i4[1], i4[2], i4[3]);
        //    int[] R = MVP_RectFromIndex(i, BBControllManager.MVPBSZ, BBControllManager.MVPGSZ);
        //    mvp_texture_target.SetPixels(R[0], y-R[1], R[2], R[3], pix);
        //    mvp_texture_target.Apply();


            


        //}



        


        public void ShowMovePadGrid()
        {
            for (int ic = 0; ic < Math.Pow( mvpd_grsz ,2 ); ic++)
            {
                int index = 0;
                switch (bbGrdtype)
                {
                    case GridIndexType.LINERAR:
                        index = ic;
                        break;
                    case GridIndexType.FUNCTION:
                        Vector2 mpos = Event.current.mousePosition - BBCtrl.mvpd_rect.position;
                        Color32 C = mvp_texture_func_id.GetPixel((int)mpos.x, mvpd_grsz - (int) mpos.y);
                        index = C.r;
                        break;
                }

                int[] I = MVP_RectFromIndex(ic, mvpd_bsz, mvpd_grsz);  //Rect(px, py, bsz, bsz);
                Rect NR = new Rect(I[0], I[1], I[2], I[3]);
                NR.position += BBCtrl.mvpd_rect.position;
                GUI.TextField(NR, (index).ToString());
            }
        }
        /// <summary>
        ///  MOVEPAD the mouse management should be redo
        ///  but works fine right now i ll probably merge this with nodes logic 
        ///  to share more stuff it s basically the same 
        /// </summary>
        /// <param name="i"></param>
        void MVP_DoMovepad(int i)
        {
            BBDrawing.CheckInput();
            if (!BBCtrl.INITITIALIZED)
                InitGUIValues();
            Vector2 mpos = Event.current.mousePosition - BBCtrl.mvpd_rect.position;
            // define a output area for the movepad 
            BBCtrl.mvpd_rect.Set(Screen.width / 2 - (BBCtrl.MVPTXTSZ / 2), 30, BBCtrl.MVPTXTSZ, BBCtrl.MVPTXTSZ);
            GUI.DrawTexture(BBCtrl.mvpd_rect, BBCtrl.GetTextureFromLayer("bbmain", TXTINDEX.TARGET)); // draw the target 
            if (!BBDrawing.GetRectFocus(BBCtrl.mvpd_rect))
            {
                GUI.DragWindow();
                return;
            }
            if (BBDrawing.mousedown)
            {
                BBCtrl.RenderSingleButton("bbmain", TXTINDEX.CLICKED, mpos);
                Actor A = Selection.activeGameObject.GetComponent<Actor>();
                object[] result; // returned by function ( not yet ) 

                if (BBDrawing.leftmouseclickeventnumber == 0) // crappy mouse management 
                {
                    BBCtrl.RenderSingleButton("bbmain", TXTINDEX.CLICKED, mpos);
                    BBCtrl.InvokeCtrlMethod("bbmain", mpos, (object)A, null, out result);
                    BBDrawing.lastmousepos = mpos;
                }

                BBDrawing.leftmouseclickeventnumber++; // single event 
            }
            else if (BBDrawing.mouseup)
            {
                // hold the last mouse pos 
                BBCtrl.RenderSingleButton("bbmain", TXTINDEX.NORMAL, BBDrawing.lastmousepos);
                BBDrawing.leftmouseclickeventnumber=0;
            }
            if (BBTools.showgrid)
                ShowMovePadGrid();
        }
 

        void OnGUI()
        {

            if (Selection.activeGameObject == null)
            {
                DisplayToolPannel(false);
                return;
            }

            EditorWindow W = EditorWindow.GetWindow(typeof(blockbuster));
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(W.position.width), GUILayout.Height(W.position.height));


            m_actor = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
            if (m_actor == null)
                AddActorComponent(Selection.activeGameObject, null);
            BBehavior[] blist = Selection.activeGameObject.GetComponents<BBehavior>();
            BlockSize = CalculateSelectionSize(Selection.gameObjects);
            m_actor.Actorprops.block_size = BlockSize;


            GUILayout.BeginHorizontal(EditorStyles.toolbar);

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



            if (selectedtab != 1)  // button from dataset ...   
            {
                BBTools.showgrid = false;
                BBTools.buttonpage = false;

            }
            


            showmovepad = GUILayout.Toggle(showmovepad, "");

            GUILayout.EndHorizontal();

            if (selectedtab == 2)
                DisplayToolPannel(true);
        
            if (selectedtab == 1)
            {
            }

            if (selectedtab == 4)
            {
                String filepath;
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
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(" add  "))
                {
                    string s = blockbuster.behaviourenum.ToString();
                    System.Type T = GetTypeFromClassName(s);
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
                    System.Type T = GetTypeFromClassName(s);
                    bool haveit = false;
                    foreach (BBehavior B in blist)
                        if (B.GetType() == T)
                            haveit = true;
                    if (haveit)
                    {
                        Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
                        BBehavior tbv = (BBehavior) Selection.activeGameObject.GetComponent(T.ToString());
                        DestroyImmediate(tbv);
                        refreshActorDatasetList(Selection.activeGameObject, A.Actorprops);
                    }

                }
                GUILayout.EndHorizontal();

                if (blockbuster.behaviourenum == null)
                    InitGUIValues();

                blockbuster.behaviourenum = (System.Enum)EditorGUILayout.EnumPopup("behaviour:", blockbuster.behaviourenum, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                foreach (BBehavior B in blist)
                {
                    B.DoGUILoop((Rect)W.position);
                    //Repaint();
                }
            }




            // desactivate replay tab
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



            GetDir(); // Dir defined bu 4 global vector coocked related to the camera 

            // safely assign a game object for GUI loop

            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                m_actor = (Actor)go.GetComponent(typeof(Actor));
                if (m_actor == null)
                {
                    // at this point we add an actor component this component is base for the system and should manipulate 
                    // every single actor of the game ( could be unactivated at runtime and activated for edition ) 
                    // actor is only related to BlockBuster Editor and store only edition stuff
                    // behavior is the game component and is optional for static stuff

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
            // prevent iteration over 10 objects selected

            if (selectedtab == 0)
            {
                b_groupselectmode = EditorGUILayout.Toggle("GrpMode", b_groupselectmode);
                b_fixedstepedit = EditorGUILayout.Toggle("fixed predefined move  ", b_fixedstepedit );
                  

                //------------------------------------------------------------------ SLIDER FOR FIXED OFSET MOVE #2
                stepvalue = EditorGUILayout.Slider("fixed step ", stepvalue, 0, 4);
                //------------------------------------------------------------------ NEXT ASSET BUTTON #3
                activebasename = (ACTIVEBASENAME)EditorGUILayout.EnumPopup("base:", activebasename);
                switch (activebasename)
                {
                    case ACTIVEBASENAME.HIGHTECH:
                        selectedbasename = "/BLOCKBUSTER/HIGHTECH/";
                        break;
                    case ACTIVEBASENAME.JUNGLE:
                        selectedbasename = "/BLOCKBUSTER/JUNGLE/";
                        break;
                    case ACTIVEBASENAME.TEMPLE:
                        selectedbasename = "/BLOCKBUSTER/TEMPLE/";
                        break;
                    case ACTIVEBASENAME.SANDBOX:
                        selectedbasename = "/BLOCKBUSTER/SANDBOX/";
                        break;
                }
                assetsliderindex = (int)EditorGUILayout.Slider("quick select", assetsliderindex, 0, data.Count );
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("PREV ASSET"))
                {
                    assetsliderindex = assetbaseindex;
                    BrowseAsset(-1);
                }
                if (GUILayout.Button("NEXT ASSET"))
                {
                    assetsliderindex = assetbaseindex;
                    BrowseAsset(1);
                }
                GUILayout.EndHorizontal();
                assetbaseindex = assetsliderindex;

                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("NEXT PRESET"))
                        BrowsePresset(1);
                    if (GUILayout.Button("PREV PRESET"))
                        BrowsePresset(-1);
                GUILayout.EndHorizontal();

                m_actor.bbeditor_fixedstep = b_fixedstepedit;


                /*
                if (GUI.Button(new Rect(bsz * 3, bsz * 12, bsz, bsz), "")) //-------------- 	BACK BUTTON
                    DoBlockMove(false, (back * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 2, bsz * 11, bsz, bsz), "")) //-------------- 	LEFT BUTTON
                    DoBlockMove(false, (right * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 4, bsz * 11, bsz, bsz), "")) //-------------- 	RIGHT BUTTON
                    DoBlockMove(false, (left * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 10, bsz, bsz), "")) //-------------- 	FRONT BUTTON
                    DoBlockMove(false, (Vector3.up * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 12, bsz, bsz), "")) //-------------- 	DOWN BUTTON
                    DoBlockMove(false, (Vector3.down * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                if (GUI.Button(new Rect(bsz * 3, bsz * 9, bsz, bsz), "")) //-------------- FRONT BUTTON
                    DoBlockMove(true, (front * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 3, bsz * 13, bsz, bsz), "")) //-------------- BACK BUTTON
                    DoBlockMove(true, (back * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz, bsz * 11, bsz, bsz), "")) //-------------- 	LEFT BUTTON
                    DoBlockMove(true, (right * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 5, bsz * 11, bsz, bsz), "")) //-------------- RIGHT BUTTON
                    DoBlockMove(true, (left * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 9, bsz, bsz), "")) //-------------- UP BUTTON
                    DoBlockMove(true, (Vector3.up * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 13, bsz, bsz), "")) //-------------- DOWN BUTTON
                    DoBlockMove(true, (Vector3.down * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                if (GUI.Button(new Rect(bsz * 9, bsz * 11, bsz, bsz), "Y")) //---------------------- ROTATE Y BUTTON  ( ZUP ?? ) 
                    for (int c = 0; c < Selection.gameObjects.GetLength(0); c++)
                        Selection.gameObjects[c].transform.Rotate(Vector3.up * 90, Space.Self);
                if (GUI.Button(new Rect(bsz * 11, bsz * 11, bsz, bsz), "X")) //----------------- ROTATE X BUTTON
                    for (int c = 0; c < Selection.gameObjects.GetLength(0); c++)
                        Selection.gameObjects[c].transform.Rotate(Vector3.left * 90, Space.Self);
                if (GUI.Button(new Rect(bsz * 13, bsz * 11, bsz, bsz), "Z")) ///---------------- ROTATE Z BUTTON
                    for (int c = 0; c < Selection.gameObjects.GetLength(0); c++)
                        Selection.gameObjects[c].transform.Rotate(Vector3.forward * 90, Space.Self);
                 */
            }
        

     

            if (showmovepad)
            {

                //EditorGUILayout.BeginFadeGroup(10.0f);
                b_groupselectmode = EditorGUILayout.Toggle("GrpMode", b_groupselectmode);
                //EditorGUILayout.EndFadeGroup();

                movepadpos.width = Screen.width;
                movepadpos.height= Screen.height;
                BeginWindows();
                Rect R = new Rect();

                R = GUI.Window(1, movepadpos, MVP_DoMovepad, MovepadMousePos.ToString());
                float ylimitation = (Screen.height - 320);
                float ymin  = (Screen.height - 40);
                
                if ( R.y < ylimitation )
                    R.y =ylimitation ;
                if (R.y > ymin)
                    R.y = ymin;




                movepadpos.Set(movepadpos.xMin, R.y, movepadpos.width, 0);
                EndWindows();
                
            }

            EditorGUILayout.EndScrollView();
        }
    }














