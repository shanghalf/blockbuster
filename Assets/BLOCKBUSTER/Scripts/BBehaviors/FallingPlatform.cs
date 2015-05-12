using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// for auto registration refer to template for more details  
/// <autoenum> FALLING_PLATFORM FallingPlatform </autoenum>




    public class FallingPlatformPathnode : Pathnode
    {
        //public Vector3 pos = new Vector3();
        //public float timer = 0.0f;
        /// <summary>
        /// custom constructor 
        /// </summary>
        public FallingPlatformPathnode()
        {
            // init here 
        }
        /// <summary>
        /// function to manipulate this specific data set 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="f"></param>
    }


    [System.Serializable]
    public class FallingPlatformDataset : Dataset
    {

        public float speed = 0.5f;
        public bool b_triggeronce = false;
        public bool b_triggered = false;
        public int movedir = 1;
        public bool respawn = false;
        public List<Pathnode> m_pathnodes = new List<Pathnode>();   // pathnodes array for any path 


        // add a public bbcontroll 
        public BBControll BBC = new BBControll();
        public NodeGraph theGraph ;





        public override Dataset Load(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                //debug.Log("file not exist");
                return null;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(FallingPlatformDataset));
            Stream stream = new FileStream(path, FileMode.Open);
            FallingPlatformDataset result = serializer.Deserialize(stream) as FallingPlatformDataset;
            stream.Close();
            return result;
        }


        public override List<Pathnode> GetPathNodes()
        {
            return  m_pathnodes;
        }


    }







    [ExecuteInEditMode()]
    public class FallingPlatform : BBehavior
    {
        // derived from dataset this is the overrided custom data set 
        public FallingPlatformDataset paramblock = new FallingPlatformDataset();
        public override void OnDrawGizmosSelected()
        {
            // draw specific gizmo
        }

        public override void SetDataset(Dataset o)
        {
            paramblock = (FallingPlatformDataset)o;
        }
        /// <summary>
        /// this is a test for node editor perfs
        /// </summary>
        /// <param name="go"></param>
        /// <param name="C"></param>
        /// <param name="time"></param>
        // store pos evaluated by the graph 
        public Vector3 Nodepos = Vector3.zero;
        public GameObject target ;


   




        public override Dataset GetDataset()
        {
            return paramblock;
        }
        public override void DoGUILoop(Rect Mainwindow)
        {
            BBControll.editgraph = GUILayout.Toggle(BBControll.editgraph, "Edit Mode");
            if (GUILayout.Button("Edit Graph"))
            {
                if (paramblock.BBC.thisgraph == null)
                {
                    string path = BBDir.Get(BBpath.SETING) + paramblock.BBC.guid.ToString() + ".bbxml";
                    if (!File.Exists(path))
                    {
                        paramblock.BBC.Graphfilename = EditorUtility.OpenFilePanel("open graph", BBDir.Get(BBpath.SETING), "xml");
                        NodeGraph.EditedControll = paramblock.BBC;
                        BBDebugLog.singleWarning("switch edited control to " + NodeGraph.EditedControll.guid.GetHashCode().ToString()); 

                        return;
                    }  
                    paramblock.BBC.thisgraph = NodeGraph.LoadGraph(paramblock.BBC);
                    paramblock.BBC.Graphfilename = path;
                }
                // NodeGraph.EditedControll  is the nodegraph actually edited by the graph editor 
                NodeGraph.EditedControll = paramblock.BBC;

                BBDebugLog.singleWarning("switch edited control to " + NodeGraph.EditedControll.guid.GetHashCode().ToString()); 
                
                EditorApplication.ExecuteMenuItem("BlockBuster/BBControllEditor");
            }
        }
        public override void OnCustomSceneGUI(SceneView sceneview)
        {
            // do scene editition callback
        }
        public override void Start()
        {
            // loading graph on start 
            paramblock.BBC.thisgraph = NodeGraph.LoadGraph(paramblock.BBC);
            base.Start();
        }


        public override void Update()
        {
            // bbc is a blockbusterController
            // invoke from a scene game object to 
            // use bbcontroll visible functions 
            // require a game object ( this ) as parameter 
            if (paramblock.BBC != null)
                paramblock.BBC.BBinvoke(this.gameObject);



        }







    }

