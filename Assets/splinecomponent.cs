using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

using System.Xml;

public class splinecomponent : MonoBehaviour {

    static GameObject prefab = (GameObject)Resources.LoadAssetAtPath(("Assets/Editor/target.fbx"), typeof(GameObject));

    [System.Serializable]
    public class SplineNode
    {
        public string Name;
        public Vector3 Point;
        public GameObject objref;
        /*
        public SplineNode(string n, Vector3 p)
        {
            Name = n;
            Point = p;
            //objref = (GameObject)Instantiate(prefab, p, Quaternion.identity);
        }
         */
         

        public void delete()
        {
            DestroyImmediate(objref);
        }
    
    }


    List <SplineNode> mNodes = new List<SplineNode>();


    public SplineNode[] handles = new SplineNode[] { new SplineNode() , new SplineNode() , new SplineNode() , new SplineNode()  }; 

	// Use this for initialization
    void Start() 
    {

	}




	// Update is called once per frame
	void Update () 
    {

	}
    
    void OnDrawGizmos()
    {
        Vector3 p = transform.position;

        for (int c = 0; c < handles.GetLength(0); c++)
            if (handles[c].objref == null)
                handles[c].objref = (GameObject)Instantiate(prefab, handles[c].Point, Quaternion.identity);


            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(handles[0].objref.transform.position,  handles[1].objref.transform.position);
            Gizmos.DrawLine(handles[1].objref.transform.position,  handles[2].objref.transform.position);
            Gizmos.DrawLine(handles[2].objref.transform.position,  handles[3].objref.transform.position);
            for (int c = 0; c < handles.GetLength(0); c++)
            {
                handles[c].Point = handles[c].objref.transform.position;
                
            }
           
        

    }
    


}
