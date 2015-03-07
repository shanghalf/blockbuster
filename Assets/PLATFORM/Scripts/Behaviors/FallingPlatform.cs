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


    [System.Serializable]
    public class FallingPlatformDataset : Dataset
    {

        public float speed = 0.5f;
        public bool b_triggeronce = false;
        public bool b_triggered = false;
        public int movedir = 1;
        public bool respawn = false;
    }





    [ExecuteInEditMode()] // same no exec in editor might be done here 
    public class FallingPlatform : Behavior
    {
        // derived from dataset this is the overrided custom data set 
        public FallingPlatformDataset paramblock = new FallingPlatformDataset();
        public FallingPlatform() { }


        /// <summary>
        /// Draw custom Gizmo 
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            /// Gizmos.color = Color.yellow ; ... etc ..
        }
        public override Dataset GetDataset()
        {
            return (FallingPlatformDataset)paramblock;
        }


        public void Start() { }


    }

