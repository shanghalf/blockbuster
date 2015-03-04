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


public enum ACTIVEBASENAME
{
	HIGHTECH = 0, 
	JUNGLE = 1, 
	TEMPLE = 2,
	SANDBOX = 3
}

// add test comment






    public class blockbuster : EditorWindow
    {
        // editortick is used instead of Time.Delta in editor LIVE mode
        static float EditorTick = 0.1f;

        // Actor is base class for all GameActors 
        // holding the common properties of anything that could be manipulated by BlockBuster Editor 

        Actor m_actor = null;
        Behavior[] m_behavior ;



        bool
            b_front_X,          // determine if editor camera point roughly along X axis ( to keep a viewport relative block move ) 
            b_editsub,          // used to access sub properties of a behavior in blockbuster UI and freeze Live Move During the edition ( ismoving )    
            b_fixedstepedit,    // manipulate actor on defined step offset instead of using it block size 
            bb_dirty,           // used to add a limitation on how many selected objects are refreshed ( stop refresh loop after 10 objects ) 
            b_groupselectmode   // used to 
            ;
        float
            stepvalue = 0.0f,
            ofset = 0.0f
            ;
        int
            i = 0,
            APX = 0,
            assetbaseindex = 0,
            slideindex = 0,
            BBiteratemaxobjects = 10
            //bs.paramblock.targetindex=0

            ;

        public int toolbarInt;
        private int currentpreset;
        //Dataset  UIPB = new Dataset();
        //BaseActorProperties UIAP = new BaseActorProperties();

        private int bsz = 20;
        Vector3 BlockSize = new Vector3(0, 0, 0);
        //Vector3 dir = new Vector3(0, 0, 0);

        Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);

        public Texture2D uparrow;//= new Texture2D(20,20)  ;
        public Texture2D downarrow;//= new Texture2D(20,20)  ;
        public Texture2D leftarrow;//= new Texture2D(20,20)  ;
        public Texture2D rightarrow;//= new Texture2D(20,20)  ;

        public Texture2D d_uparrow;//= new Texture2D(20,20)  ;
        public Texture2D d_downarrow;//= new Texture2D(20,20)  ;
        public Texture2D d_leftarrow;//= new Texture2D(20,20)  ;
        public Texture2D d_rightarrow;//= new Texture2D(20,20)  ;
        public Texture2D duplicate_t;//= new Texture2D(20,20)  ;

        public ArrayList hidenobjectlist = new ArrayList();

        int oldindexstorage;


        ACTIVEBASENAME activebasename;
        string selectedbasename = "/PLATFORM/HIGHTECH/";
        public List<string> data;
        private int pathindex;
        private int levelID;
        public GameObject m_RePlayerObject;
        public List<GameObject> m_replayactors = new List<GameObject>();
        public List<string> xmlfoldercontent = new List<string>();
        private bool b_applyfilter;
        private RePlayer m_replayer;
        private float replayspeed;

        public static System.Enum behaviourenum;


        //dynamicenumtest = bh.GetEnumFromScriptFolder();



        //static GameObject handle = (GameObject)Resources.LoadAssetAtPath(("Assets/Editor/target.fbx"), typeof(GameObject));


        [MenuItem("Window/blockbuster")]
        static void ShowWindow()
        {

            Debug.Log("");

            EditorWindow.GetWindow(typeof(blockbuster));


            InitGUIValues();


        }










        void Start()
        {



        }


        public bool savescene(bool preset = false, string scenename = null)
        {
            string repo = "";
            string defname = "";
            Scene scenetosave = new Scene();
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

            var sfolder = Application.dataPath + "/PLATFORM/XML/" + repo;
            string path = "";
            if (scenename == null)
                path = EditorUtility.SaveFilePanel("filename to save", sfolder, defname, "xml");
            else
                path = sfolder + scenename;


            //Directory.CreateDirectory(path);
            foreach (object o in obj)
            {
                go = (GameObject)o;

                Actor localactor = (Actor)go.GetComponent(typeof(Actor));
                Behavior localbehavior = (Behavior)go.GetComponent(typeof(Behavior));
                BaseActorProperties baseactorprop = localactor.Actorprops;


              



                if (localactor != null)
                {
                    // do not forget to update paramblock value before saving 
                    var filepath = path + "/" + localactor.Actorprops.guid + ".xml";
                    localactor.Actorprops.last_pos = go.transform.position; // make sure the pos is right 
                    localactor.Actorprops.orig_transform = go.transform.rotation;
                    scenetosave.cluster.baseassetproplist.Add(baseactorprop);
                }
            }

            scenetosave.Save(path);
            return true;

        }





        public GameObject GroupSelection(GameObject go)
        {
            for (var c = 0; c < Selection.gameObjects.GetLength(0); c++)
            {
                if (go != Selection.gameObjects[c])
                {
                    Selection.gameObjects[c].transform.parent = go.transform;
                    Actor tbs = (Actor)Selection.gameObjects[c].GetComponent(typeof(Actor));
                    tbs.Actorprops.parentgui = m_actor.Actorprops.guid.ToString();
                    tbs.Actorprops.grouped = true;
                    //DestroyImmediate( Selection.gameObjects[c].GetComponent("Platform"));
                }
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
            var sfolder = Application.dataPath + "/PLATFORM/XML/" + repo;
            string path = "";
            scenecluster S = new scenecluster();
            if (scenename == null)
                path = EditorUtility.OpenFilePanel("load scene", sfolder, "xml");
            else
                path = sfolder + "/" + scenename;
            S = Scene.Load(path);
            foreach (BaseActorProperties blk in S.baseassetproplist)
            {
                GameObject tgo = (GameObject)Resources.LoadAssetAtPath(("Assets" + selectedbasename + blk.assetname + ".fbx"), typeof(GameObject));
                Actor tbs;
                tbs = (Actor)tgo.GetComponent(typeof(Actor));
                GameObject instance = (GameObject)Instantiate(tgo, blk.last_pos, blk.orig_transform);
                instance.name = blk.assetname + instance.GetInstanceID();
                instance.AddComponent(typeof(Behavior));
                tbs = (Actor)instance.GetComponent(typeof(Actor));
                //tbs.ba= blk;
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
                Debug.Log("Object do not have proper Constructor");
                return null;
            }
        }

        // cast origin actor to desired behavior 



        void AddActorComponent(GameObject obj,  Actor originalactor)
        {
            // ****************************************************************************
            // add a custom script on gameobject 
            // this function should be called for every new block in the scene

            if (obj == null)
            {
                Debug.Log("AddActorComponent Gameobject is Null");
                return;
            }

            MeshFilter M;

            obj.AddComponent(typeof(Actor)); 				// refer to block setup script ( block behaviour ) 
            m_actor = (Actor)obj.GetComponent(typeof(Actor));		// bs is at global scope 
            M = (MeshFilter)obj.GetComponent(typeof(MeshFilter));			// get some info from mesh to custom script 
            // -------------------------------------------------------------------------------------- init the block properties  ( parameterblock ) 
            if (originalactor == null)
                m_actor.Actorprops.block_size = M.renderer.bounds.size;
            else
            {
                m_actor.Actorprops.block_size = originalactor.Actorprops.block_size; 	// change size for all 
                //m_actor.block_transform = obj.transform;	 	// transform from source	
                m_actor.Actorprops.orig_transform = obj.transform.rotation; // origin transform help to re initialize a block in static mode 
                m_actor.Actorprops.orig_pos = obj.transform.position;		// same for pos ( sound weird could be done in oneline have to see that 	
                //m_actor.scenerefobj = obj;
            }
            // replicate the behavior on new instancied object 


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
            for (i = 0; i < Selection.gameObjects.GetLength(0); i++)
            {
                GameObject ts = (GameObject)Selection.gameObjects.GetValue(i);

                m_actor = (Actor)ts.GetComponent(typeof(Actor));						// should be there 
                if (m_actor == null)
                    continue;


                if (instanciate)
                {	// ------------------------- MOVE AND DUPLICATE

                    GameObject obj = (GameObject)Instantiate(Selection.gameObjects[i], Selection.gameObjects[i].transform.position, Selection.gameObjects[i].transform.rotation);
                    str = Selection.gameObjects[i].name;										// change the name  
                    string[] strarray = str.Split(new char[] { '-' });
                    obj.name = strarray[0] + obj.GetInstanceID();								// final name is block original name plus unique id 
                    if (Selection.gameObjects[i].transform.parent)
                        obj.transform.parent = Selection.gameObjects[i].transform.parent;
                    Actor tempactor = (Actor)obj.GetComponent(typeof(Actor));
                    Guid g;
                    g = Guid.NewGuid();
                    if (tempactor)
                    {
                        tempactor.Actorprops.guid = g.ToString();//obj.GetInstanceID().ToString();
                        tempactor.Actorprops.parentgui = m_actor.Actorprops.guid;
                        tempactor.Actorprops.orig_pos = Selection.gameObjects[i].transform.position;
                        tempactor.Actorprops.orig_transform = Selection.gameObjects[i].transform.rotation;
                    }

                    //AddPlatformComponent(obj, null);
                }
                Selection.gameObjects[i].transform.position += dir;

                // static block
                if (m_behavior == null)
                    return;

                /*
                if (moveallpath)
                    foreach (Pathnode pn in m_behavior.paramblock.m_pathnodes)
                        pn.pos += dir;
                else
                    m_behavior.paramblock.m_pathnodes[m_behavior.paramblock.GetSafeTargetIndex()].pos += dir;

                */

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
       //     if (B != null)
     //           B.paramblock.b_front_x = b_front_X; 

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



        public bool ReadAssetBase(String caller)
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
            String filepath = Application.dataPath + selectedbasename + "AssetList" + ".xml";
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


        public void BrowseAsset(int next, String caller)
        {
            // ***************************************************************
            // browse asset from base todo  should be changeed to manage multiple base 

            if (b_groupselectmode) return;
            GameObject tgo = (GameObject)Selection.activeObject;

            if (tgo == null)
            {
                //debug.Log( "no object selected "   )  ;
                return;
            }

            ReadAssetBase("BrowseAsset()" + "from " + caller);  																	// read the base definition ( generated by 3dsmax during  export ) 

            Behavior bs = (Behavior)Selection.gameObjects[0].GetComponent(typeof(Behavior));

            // first element of the selection is used as active transform to pop objects 
            if (bs == null)
            {
                //debug.Log( "no block setup script on source object " + Selection.activeObject.name  )  ;
                return;
            }


            int index;
            if (Mathf.Abs(next) < 2)
            {
                assetbaseindex = Mathf.Abs(assetbaseindex + next);								// loop index todo check index base seems to have a small bug here 
                index = assetbaseindex % data.Count;
            }
            else
                index = slideindex;

            ////debug.Log(index.ToString());


            string assetname = (data[index]);													// load and swap 

            GameObject prefab = (GameObject)Resources.LoadAssetAtPath(("Assets" + selectedbasename + assetname + ".fbx"), typeof(GameObject));
            if (prefab == null)
            {
                //debug.Log( "prefab load fail "+  ( "Assets"+selectedbasename  + assetname +".fbx") );
                return;
            }
            // the new instance need a block controller as well and get whatever it can grab on the 
            // original object // in AddPlatformComponent funbtion ( add new fresh script ut can also take props from source 
            GameObject instance = (GameObject)Instantiate(prefab, Selection.activeTransform.position, Selection.gameObjects[0].transform.rotation);
            instance.name = prefab.name + instance.GetInstanceID();

            AddActorComponent(instance,  m_actor);

            DestroyImmediate(Selection.activeObject);
            Selection.activeGameObject = instance;

        }



        public void placefromxmlfile()
        {
            // **************************************************************************
            // the asset base xml file is generated by 3dsmax exporter in asset folder 
            // this function place assets from unity base related to the assettransfert.xml generated by max tool 
            String filepath = Application.dataPath + selectedbasename + "assettransfert" + ".xml";
            //debug.Log(Application.dataPath);
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





        static bool InitGUIValues()
        {
            //  todo fill the array with all png in editor folder 
            // and texture would be avaiable on name  T.B.C 
            //**************************************************
            System.Type T = (System.Type)behaviorManager.castenum();
            behaviourenum = (System.Enum)System.Activator.CreateInstance(T);
            //System.Type TT = behaviorManager.GetClassDataset();
            return true;
        }


        void Update()
        {
            // **********************************************************************************************************************************************
            // update tool interface from selected object whn tool do not have the focus
            // that s part of the parameterblock management systen left to do 
            if (Selection.activeGameObject == null)
                return;


            if (Selection.activeGameObject.GetComponent(typeof(Behavior)) != null)
            {
                m_actor = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
                if (m_actor == null)
                    AddActorComponent(Selection.activeGameObject,  null);

                m_behavior = Selection.activeGameObject.GetComponents<Behavior>();

                //UIPB = m_behavior.paramblock;
            }

            Repaint();

        }

        void OnInspectorUpdate()
        {
            //***********************************************************************************
            // update inspector sheet according to the tool value 

            if (Selection.gameObjects.Length == 0 || EditorWindow.focusedWindow == null)
                return; 																// out of Unity nothing to refresh 
            var gg = Selection.activeGameObject.transform.parent;

            if (gg != null && b_groupselectmode)
            {
                Selection.activeObject = gg;
            }


            if ((EditorWindow.focusedWindow.title == "blockbuster"))
            {				// Block Buster got the focus: so play that funky music white boy  
                for (var i = 0; i < Selection.gameObjects.GetLength(0); i++)
                {// lay down the boogy and play that funky music 
                    m_behavior = Selection.gameObjects[i].GetComponents<Behavior>();

                }
                if (slideindex - 1 != oldindexstorage)
                    BrowseAsset(slideindex - 1, "CalculateSelectionSize()");												// till you die ....:::..::...::...::.::.:.:.:::
                oldindexstorage = slideindex - 1;
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

            Repaint();
            ////debug.Log(bs.parent.name) ;
        }

        string[] ReturnXmlContent()
        {



            var sfolder = Application.dataPath + "/PLATFORM/XML/Replays";
            string[] files = Directory.GetFiles(sfolder, "*.xml");
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


        void AddReplayerObject()
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
                return;
            }
            else
            {
                GameObject ReplayerObjectBody = (GameObject)Resources.LoadAssetAtPath(("Assets/ReplayerObjects/ReplayerObjectBody.FBX"), typeof(GameObject));
                GameObject O = (GameObject)Instantiate(ReplayerObjectBody, Vector3.zero, Quaternion.identity);
                O.AddComponent(typeof(RePlayer));
                RePlayer m_replayer = (RePlayer)O.GetComponent(typeof(RePlayer));
                m_replayer.m_replayfiletag = ".xml";
                O.name = "REPLAYERINSTANCE";
                m_replayactors.Add(O);
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
            foreach (Replay ri in m_replayer.replaylist)
                if (ri.m_xmlfilename.Contains(filename))
                    m_replayer.m_playerreplay = ri;
            m_replayer.targetindex = 0;
        }




        int selectedtab = 0;
        float replayerdtab = 0;

        Vector2 scrollPos = new Vector2();


        public void UpdateActorComponent(GameObject go, PLTF_TYPE pltf_sate)
        {

            // this function might be called at top of onGUI loop to validate Actor and Behavior 
            // consistency a static actor might not have behavior and dynamic ones should have the appropriated subclass
            // it s based on pltf_state ( structure that define the actor list and attached to actor component  ) 
            // do not forget to cast m_behavior according to the changes 



            switch (m_actor.Actorprops.pltf_sate) // CREATE APROPRIATE UI CONTENT 
            {

                case PLTF_TYPE.FALLING:
                    break;

                case PLTF_TYPE.MOVING:
                    break;

                case PLTF_TYPE.ROTATING:
                    /*
                    {
                        if (m_behavior == null)
                            m_behavior = (Behavior)go.AddComponent(typeof(RotatingPlatform));
                        else if (m_behavior.GetType() != typeof(RotatingPlatform))
                        {
                            DestroyImmediate(m_behavior);
                            go.AddComponent(typeof(RotatingPlatform));
                            m_behavior = (Behavior)go.GetComponent(typeof(RotatingPlatform));
                        }
                    }*/
                    break;

                case PLTF_TYPE.STATIC:
                    
                    break;

            }

 


        }

        private void changeEditorFlag(bool bstatic)
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
                Debug.Log("assname " + currentassembly.ToString());
                if (t != null)
                {

                    return t;
                }
            }
            return null;
        }

        

        void OnGUI()
        {

            if (Selection.activeGameObject == null)
                return;
            m_actor = (Actor) Selection.activeGameObject.GetComponent(typeof(Actor));

            if (m_actor == null)
                AddActorComponent(Selection.activeGameObject, null);





            BlockSize = CalculateSelectionSize(Selection.gameObjects);
            m_actor.Actorprops.block_size = BlockSize;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("MOVE", EditorStyles.toolbarButton))
                selectedtab = 0;
            if (GUILayout.Button("PROP", EditorStyles.toolbarButton))
                selectedtab = 1;
            if (GUILayout.Button("TOOLS", EditorStyles.toolbarButton))
                selectedtab = 2;
            if (GUILayout.Button("REPLAY", EditorStyles.toolbarButton))
                selectedtab = 3;

            if (Dataset.menubaritem())  // button from dataset ...   
            {
                selectedtab = 4;
            }
            GUILayout.EndHorizontal();

            if (selectedtab == 4)
            {
                if (blockbuster.behaviourenum == null)
                    blockbuster.InitGUIValues();



                blockbuster.behaviourenum = (System.Enum)EditorGUILayout.EnumPopup("behaviour:", blockbuster.behaviourenum, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add"))
                {
                    string s = blockbuster.behaviourenum.ToString();
                    System.Type T = GetTypeFromClassName(s);

                    bool add = true;
                    foreach (Behavior B in m_behavior)
                        if (B.GetType() == T)
                            add = false;
                    if (add)
                    {
                        //Behavior b = new Behavior();
                        Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));

                        Behavior b = (Behavior)Selection.activeGameObject.AddComponent(T) ;
                        // guid is the link for saving ad get back paramblocks
                        b.paramblock.actorguid = A.Actorprops.guid;

                    }

                }
                if (GUILayout.Button("remove"))
                {
                    string s = blockbuster.behaviourenum.ToString();
                    System.Type T = GetTypeFromClassName(s);

                    bool haveit = false;
                    foreach (Behavior B in m_behavior)
                        if (B.GetType() == T)
                            haveit = true;
                    if (haveit)
                    {
                        Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
                        Behavior b = (Behavior) Selection.activeGameObject.GetComponent(T.ToString());
                        
                        DestroyImmediate(b);
                    }

                }
                GUILayout.EndHorizontal();
                
                EditorWindow W = EditorWindow.GetWindow(typeof(blockbuster));
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(W.position.width), GUILayout.Height(W.position.height));
                foreach (Behavior B in m_behavior)
                {
                    B.DoGUILoop((Rect)W.position);
                    Repaint();
                }
                EditorGUILayout.EndScrollView();

            }





            if (selectedtab == 3)
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
                m_replayer = (RePlayer)m_replayactors[0].GetComponent(typeof(RePlayer));
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
                    if (!b_applyfilter)
                    {
                        m_replayer = (RePlayer)m_replayactors[0].GetComponent(typeof(RePlayer));
                        string[] L = ReturnXmlContent();
                        if (m_replayer.m_replayfiletag != ReturnXmlContent()[pathindex] + ".xml")
                        {
                            m_replayer.m_replayfiletag = ReturnXmlContent()[pathindex] + ".xml";
                            SwitchReplayerPath(m_replayer.m_replayfiletag);
                        }
                    }
                    else
                        m_replayer.m_replayfiletag = ".xml";
                    GUI.BeginGroup(new Rect(0, 0, 600, 600));

                    pathindex = EditorGUILayout.Popup(pathindex, ReturnXmlContent());
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




            //  trick to prevent deadloop when huge amount of objscts are selected 
            if (Selection.gameObjects.Length < 10)
                bb_dirty = true;
            else
                bb_dirty = false;


            //GUI.Toolbar(new Rect (25, 25, 250, 30), toolbarInt, toolbarStrings);

            if (selectedtab == 0)
            {


                GUI.BeginGroup(new Rect(10, bsz * 10, 300, 600));

                b_groupselectmode = EditorGUILayout.Toggle("GrpMode", b_groupselectmode, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                b_fixedstepedit = EditorGUILayout.Toggle("fixed predefined move  ", b_fixedstepedit);
                //------------------------------------------------------------------ SLIDER FOR FIXED OFSET MOVE #2
                stepvalue = EditorGUILayout.Slider("fixed step ", stepvalue, 0, 4, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                //------------------------------------------------------------------ NEXT ASSET BUTTON #3
                if (GUILayout.Button("NEXT ASSET", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                    BrowseAsset(1, "NEXT BUTTON");
                //------------------------------------------------------------------ PREV ASSET BUTTON #4
                if (GUILayout.Button("PREV ASSET", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                    BrowseAsset(-1, "PREV ASSET BUTTON");
                if (data != null)
                    slideindex = (int)EditorGUILayout.Slider("quick select", slideindex, 0, data.Count, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));

                if (GUILayout.Button("NEXT PRESET", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                {

                    GameObject original = Selection.activeGameObject;

                    currentpreset++;
                    var sfolder = Application.dataPath + "/PLATFORM/XML/preset";
                    string[] files = Directory.GetFiles(sfolder, "*.xml");
                    var i = currentpreset % files.Length;
                    Debug.Log(i);
                    List<GameObject> L = loadscene(true, System.IO.Path.GetFileName(files[i]));
                    Selection.objects = L.ToArray();
                    Selection.activeGameObject = GroupSelection(Selection.gameObjects[0]);
                    Selection.activeGameObject.transform.position = original.transform.transform.position;
                    DestroyImmediate(original);

                }
                if (GUILayout.Button("PREV PRESET", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                {
                    GameObject original = Selection.activeGameObject;
                    currentpreset--;
                    var sfolder = Application.dataPath + "/PLATFORM/XML/preset";
                    string[] files = Directory.GetFiles(sfolder, "*.xml");

                    if (currentpreset < 1)
                        currentpreset = files.Length - 1;
                    Debug.Log(currentpreset);
                    List<GameObject> L = loadscene(true, System.IO.Path.GetFileName(files[i]));
                    Selection.objects = L.ToArray();
                    Selection.activeGameObject = GroupSelection(Selection.gameObjects[0]);
                    Selection.activeGameObject.transform.position = original.transform.transform.position;
                    DestroyImmediate(original);
                }


                GUI.EndGroup();

                if (GUI.Button(new Rect(bsz * 3, bsz * 5, bsz, bsz), uparrow)) //-------------- FRONT BUTTON
                    DoBlockMove(false, (front * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 3, bsz * 7, bsz, bsz), downarrow)) //-------------- 	BACK BUTTON
                    DoBlockMove(false, (back * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 2, bsz * 6, bsz, bsz), leftarrow)) //-------------- 	LEFT BUTTON
                    DoBlockMove(false, (right * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 4, bsz * 6, bsz, bsz), rightarrow)) //-------------- 	RIGHT BUTTON
                    DoBlockMove(false, (left * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 5, bsz, bsz), uparrow)) //-------------- 	FRONT BUTTON
                    DoBlockMove(false, (Vector3.up * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 7, bsz, bsz), downarrow)) //-------------- 	DOWN BUTTON
                    DoBlockMove(false, (Vector3.down * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                // -----------------------------------------------------------------------------------------	SAME WITH DUPLICATE  

                if (GUI.Button(new Rect(bsz * 3, bsz * 4, bsz, bsz), duplicate_t)) //-------------- FRONT BUTTON
                    DoBlockMove(true, (front * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 3, bsz * 8, bsz, bsz), duplicate_t)) //-------------- BACK BUTTON
                    DoBlockMove(true, (back * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz, bsz * 6, bsz, bsz), duplicate_t)) //-------------- 	LEFT BUTTON
                    DoBlockMove(true, (right * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
                if (GUI.Button(new Rect(bsz * 5, bsz * 6, bsz, bsz), duplicate_t)) //-------------- RIGHT BUTTON
                    DoBlockMove(true, (left * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));

                if (GUI.Button(new Rect(bsz * 7, bsz * 4, bsz, bsz), duplicate_t)) //-------------- UP BUTTON
                    DoBlockMove(true, (Vector3.up * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));
                if (GUI.Button(new Rect(bsz * 7, bsz * 8, bsz, bsz), duplicate_t)) //-------------- DOWN BUTTON
                    DoBlockMove(true, (Vector3.down * (ofset = (b_fixedstepedit) ? BlockSize.y : stepvalue)));

                if (GUI.Button(new Rect(bsz * 9, bsz * 6, bsz, bsz), "Y")) //---------------------- ROTATE Y BUTTON  ( ZUP ?? ) 
                {
                    for (i = 0; i < Selection.gameObjects.GetLength(0); i++)
                    {

                        m_behavior = Selection.gameObjects[i].GetComponents<Behavior>(); // associated script 

                        Selection.gameObjects[i].transform.Rotate(Vector3.up * 90, Space.Self);

                    }

                }
                if (GUI.Button(new Rect(bsz * 11, bsz * 6, bsz, bsz), "X")) //----------------- ROTATE X BUTTON
                {
                    for (i = 0; i < Selection.gameObjects.GetLength(0); i++)
                    {
                        m_behavior = Selection.gameObjects[i].GetComponents<Behavior>(); // associated script 

                        Selection.gameObjects[i].transform.Rotate(Vector3.left * 90, Space.Self);

                    }

                }
                if (GUI.Button(new Rect(bsz * 13, bsz * 6, bsz, bsz), "Z")) ///---------------- ROTATE Z BUTTON
                {
                    for (i = 0; i < Selection.gameObjects.GetLength(0); i++)
                    {
                        m_behavior = Selection.gameObjects[i].GetComponents<Behavior>(); // associated script 

                        Selection.gameObjects[i].transform.Rotate(Vector3.forward * 90, Space.Self);
                    }

                }

                //--------------------------------------------------------------------------------------- END OF BLOCK MOVEE TAB PANNEL  

            }

            if (selectedtab == 1)
            {
                if (go == null || m_actor == null)
                {
                    GUI.Label(new Rect(0, 20, 200, 200), "SELECT A VALID GAME OBJECT");
                    return;
                }

                //---------------------------------------------------------------------		<BeginGroup>  
                //-------------------------------------------------------------- 4 Controls in the group  
                GUI.BeginGroup(new Rect(10, 80, 300, 600));


                b_groupselectmode = EditorGUILayout.Toggle("GrpMode", b_groupselectmode, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                String filepath;

                //Platform tblock = (Platform)Selection.activeGameObject.GetComponent("Platform");


                if (GUILayout.Button("SAVE", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    if (Selection.activeGameObject == null)
                        return;
                    filepath = Application.dataPath + "/PLATFORM/XML/paramblock/" + m_actor.Actorprops.guid + ".xml";
                    m_actor.Actorprops.Save(filepath, typeof(BaseActorProperties));
                }

                if (GUILayout.Button("LOAD", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    if (Selection.activeGameObject == null)
                        return;
                    filepath = Application.dataPath + "/PLATFORM/XML/paramblock/" + m_actor.Actorprops.guid + ".xml";
                    //Selection.activeObject.GetComponent(Platform).paramblock.Load(filepath);
                    if (!System.IO.File.Exists(filepath))
                        return;
                    //Dataset p = m_behavior.Load(filepath, typeof(Dataset)); // deserialise pblock 
                    //m_behavior.paramblock = p;
                    //UIPB = p;
                }


                m_actor.Actorprops.pltf_sate = (PLTF_TYPE)EditorGUILayout.EnumPopup("block action:", m_actor.Actorprops.pltf_sate, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                UpdateActorComponent(go, m_actor.Actorprops.pltf_sate);


                switch (m_actor.Actorprops.pltf_sate) // CREATE APROPRIATE UI CONTENT 
                {
                    /*
                    case PLTF_TYPE.MOVING: // ---------------------------------------- plateform moving state  
                        changeEditorFlag(false);
                        break;

                    case PLTF_TYPE.ROTATING:
                        //************************************************************************ ROTATE THE CRAP 
                        // need to implement a keyframes system that could be used a lot add props to exploit in Platform

                        //UpdateActorComponent(go, typeof(Platform),  PLTF_TYPE.ROTATING);
                        //List<Pathnode> pathnodes = m_behavior.paramblock.GetPathNodes();
                        //int pmax = pathnodes.Count;

                        changeEditorFlag(false);

                        if (!m_behavior)
                            return;
                        m_behavior.paramblock.rotationstepnumber = (int)EditorGUILayout.Slider("step", m_behavior.paramblock.rotationstepnumber, 2, 8, GUILayout.MaxWidth(280));

                        m_behavior.paramblock.rotationspeed = EditorGUILayout.Slider("speed", m_behavior.paramblock.rotationspeed, 0.0f, 5.0f, GUILayout.MaxWidth(280));
                        m_behavior.paramblock.rotationtempo = EditorGUILayout.Slider("temporisation", m_behavior.paramblock.rotationtempo, 0.0f, 2.0f, GUILayout.MaxWidth(280));
                        // editsub button shared over panels 
                        m_behavior.paramblock.editsub = EditorGUILayout.Toggle("editsub", m_behavior.paramblock.editsub);
                        if (m_behavior.paramblock.editsub)
                        {
                            m_behavior.paramblock.b_revert_rotation = EditorGUILayout.Toggle("invert", m_behavior.paramblock.b_revert_rotation, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                        }
                        else
                            m_behavior.paramblock.ismoving = false;

                        break;

                     */
                    case PLTF_TYPE.STATIC:

                        changeEditorFlag(true);
                        if (GUILayout.Button("RESET", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                            foreach (GameObject g in Selection.gameObjects)
                            {
                                g.transform.rotation = m_actor.Actorprops.orig_transform;
                                g.transform.position = m_actor.Actorprops.orig_pos;
                            }

                        break;
                    case PLTF_TYPE.FALLING:

                        break;
                }
                GUI.EndGroup();
            }

            if (selectedtab == 2)
            {

                GUI.BeginGroup(new Rect(bsz, bsz * 4, 220, 300));
                b_groupselectmode = EditorGUILayout.Toggle("GrpMode", b_groupselectmode, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                activebasename = (ACTIVEBASENAME)EditorGUILayout.EnumPopup("base:", activebasename, GUILayout.MinWidth(330), GUILayout.MaxWidth(330));
                switch (activebasename)
                {
                    case ACTIVEBASENAME.HIGHTECH:
                        selectedbasename = "/PLATFORM/HIGHTECH/";
                        break;
                    case ACTIVEBASENAME.JUNGLE:
                        selectedbasename = "/PLATFORM/JUNGLE/";
                        break;
                    case ACTIVEBASENAME.TEMPLE:
                        selectedbasename = "/PLATFORM/TEMPLE/";
                        break;
                    case ACTIVEBASENAME.SANDBOX:
                        selectedbasename = "/PLATFORM/SANDBOX/";
                        break;
                }
                if (GUILayout.Button("FROMMAX", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    placefromxmlfile();

                if (GUILayout.Button("SAVE SCENE", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    savescene();
                if (GUILayout.Button("SAVE PRESET", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    savescene(true);


                if (GUILayout.Button("LOAD SCENE", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    loadscene();
                if (GUILayout.Button("LOAD PRESET", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    loadscene(true);

                if (GUILayout.Button("reset Position", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    for (int ti = 0; ti < Selection.gameObjects.GetLength(0); ti++)
                    {
                        Actor temactor = (Actor)Selection.gameObjects[ti].GetComponent(typeof(Actor)); // associated script 
                        if (temactor != null)
                        { // pull back at original place ( where the go  has been spotted for the first time 
                            Selection.gameObjects[ti].transform.rotation = temactor.Actorprops.orig_transform;
                            Selection.gameObjects[ti].transform.position = temactor.Actorprops.orig_pos;
                        }
                    }


                if (GUILayout.Button("GROUP", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    GroupSelection(go);
                }

                if (GUILayout.Button("Hide Group", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    go.SetActive(false);
                    hidenobjectlist.Add(go);
                }
                if (GUILayout.Button("Unhide All", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    for (int count = 0; count < hidenobjectlist.Count; count++)
                    {
                        GameObject tgo = (GameObject)hidenobjectlist[count];
                        tgo.SetActive(true);
                    }
                    hidenobjectlist.Clear();
                }

                //**************************************************************************** READ WRITE PARAMETERBLOCK FILE

                if (GUILayout.Button("UNGROUP", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    for (var c = 0; c < Selection.activeGameObject.transform.childCount; c++)
                    {
                        GameObject tgo = Selection.activeGameObject.transform.GetChild(c).gameObject;
                        Actor tbs = (Actor)tgo.GetComponent(typeof(Behavior));
                        tbs.Actorprops.parentgui = null;
                        tbs.Actorprops.grouped = false;
                    }
                    Selection.activeGameObject.transform.DetachChildren();
                }
                if (GUILayout.Button("select same", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                {
                    GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                    ArrayList oblist = new ArrayList();
                    // define basename selected 

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
                if (GUILayout.Button("remove col", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
                    for (var c = 0; c < Selection.gameObjects.Length - 1; c++)
                        DestroyImmediate(Selection.gameObjects[c].collider);



                //Selection.objects. = oblist;
                GUI.EndGroup();
            }
        }

    }














