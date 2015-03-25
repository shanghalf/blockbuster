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
        public Vector3 pos;
        public float timer = 0.0f;
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


    
    public class FallingPlatformDataset : Dataset
    {

        public float speed = 0.5f;
        public bool b_triggeronce = false;
        public bool b_triggered = false;
        public int movedir = 1;
        public bool respawn = false;
        public List<Pathnode> m_pathnodes = new List<Pathnode>();   // pathnodes array for any path 

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





    [ExecuteInEditMode()] // same no exec in editor might be done here 
    public class FallingPlatform : BBehavior
    {
        // derived from dataset this is the overrided custom data set 
        public FallingPlatformDataset paramblock = new FallingPlatformDataset();
        public override void OnDrawGizmosSelected()
        {
            // draw specific gizmo
        }

        public override void SetDataset(object o)
        {
            throw new System.NotImplementedException();
        }
  


        public override Dataset GetDataset()
        {
            return paramblock;
        }
        public override void DoGUILoop(Rect Mainwindow)
        {
            // do gui loop
        }
        public override void OnCustomSceneGUI(SceneView sceneview)
        {
            // do scene editition callback
        }
        public override void Start()
        {
            base.Start();
        }
        public override void Update()
        {
            // do update
        }






    }

