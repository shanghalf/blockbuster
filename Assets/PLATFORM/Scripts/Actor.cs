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










public class Actor : MonoBehaviour 




{



    // will replace later with a behavior table to manage behavior from actor 
    //public Behavior Behavior = null;



    public List<Dataset> DatasetTable = new List<Dataset>();
    
    public Transform block_transform;
    public GameObject scenerefobj;


 


    public BaseActorProperties Actorprops = new BaseActorProperties();


    public virtual void OnDrawGizmosSelected()
    {

    }


	// Use this for initialization
	public virtual void Start () 
    
    {
        // an actor is not suposed to be used in game mode 
	}
	
	// Update is called once per frame
	public virtual void Update () 
    
    {
	    // an actor is not suposed to be used in game mode 
	}


    // save function should serialize a table of Behaviors (should be broken now )
    public virtual void Save(string path, System.Type type)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
        //debug.Log ( serializer.ToString());
    }





}
