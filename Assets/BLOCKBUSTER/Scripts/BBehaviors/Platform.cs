using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
#if UNITY_EDITOR
    using UnityEditor;
#endif

// base class for game behaviors 
// should implement all function to comunicate with actor class
// actor class is used to provide a layer to bind multiple behaviors to a single actor and 
// comunicate with Blockbuster through it s function familly 
[System.Serializable]
public enum COLIDER_TYPE
{
    BOX = 0,
    SPHERE = 1,
    CAPSULE = 2,
    MESH = 3
}

[System.Serializable]
public enum ARRAY_BOUND
{
    UP = 2,
    MIDDLE = 0,
    DOWN = -1,
    OUT = 666
}




/// <summary>
/// the Dataset Class is the base class used by the framework to support data serialization
/// to enforce the framework implementation a dataset should be only accessed by Ge/set[Dataset] function  
/// the derived class serialize correctly both base and own fields at once 
/// Dataset is the serializable part of a behavior class and each behavior might get one override instance as propertie 
/// </summary>

[XmlInclude(typeof(Pathnode))]
[System.Serializable]
public   class Dataset
{
    
    public int targetindex;
    public bool b_front_x;                                      // used to define the camera angle 
    public float timer = 0.0f;                                  // a generic multi purpose timer 
    public bool editsub = true;                                 // right now used to modify GUI according to the level of edition
    public bool b_showdebuginfos;                               // no comment 
    public bool b_hideedition = false;                          // hide edition in viewport ( scene custom gui )  
    static int submenu = 4;                                     // <todo> flush
    public bool ismoving = false;                               // execute or not a transformation in behavior update 
    public ARRAY_BOUND indexbound;                              // used to define where is an index in a pathnode array 
    public string guid = "";                                    // base of identification for any component 
    public string fullqualifiedclassname;                       // for reflection purpose in class use 
    public string suportedclassname;                            // the class that could use this kind of dataset <todo> use a list instead  


    /// <summary>
    /// <todo>useless remove</todo>
    /// </summary>
    /// <returns></returns>
    public static bool menubaritem()
    {
        return GUILayout.Button("Dataset", EditorStyles.toolbarButton);
    }





    /// <summary>
    /// for reflection purpose 
    /// <todo>change for a list in case of a dataset is usable by multiple behavior overrides</todo>
    /// </summary>
    /// <returns></returns>
    public virtual string Getsuportedclassname ()
    {
        return suportedclassname;
    }

    /// <summary>
    /// guid is unique fo each component this is mainly because 
    /// the framework is not using unity scene to save and need to 
    /// save runtime values and reload them once the tuning is fine 
    /// </summary>
    /// <returns></returns>
    public virtual string GetGuid()
    {
        return guid;
    }
    /// <summary>
    /// set a GUID
    /// </summary>
    /// <param name="s"></param>
    public virtual void SetGuid(string G)
    {
        guid = G;
    }


    /// <summary>
    /// override this to safely access an array index according to 
    /// the behavior's Behaviour .. hummm ! 
    /// </summary>
    /// <returns></returns>
    public virtual int GetSafeTargetIndex()
    {
        return -1;
    }

    /// <summary>
    /// set a pathnode index in a safe way 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual bool SetSafeTargetIndex(int index)
    {
        targetindex= index;
        return true;
    }

    public virtual List<Pathnode> GetPathNodes()
    {
        return null;
    }


    /// <summary>
    /// serialization 
    /// <todo>could be moved in a utility static class</todo>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    public virtual void Save(string path, System.Type type)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
    }
    




    /// <summary>
    /// return the full qualified name string
    /// for reflection type cast 
    /// </summary>
    /// <returns></returns>
    public virtual string GetFullQualifiedClassName()
    {
        return GetType().AssemblyQualifiedName;
    }

    /// <summary>
    /// load might be overrided 
    /// in each derived class 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public virtual  Dataset Load(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            //debug.Log("file not exist");
            return null;
        }
        XmlSerializer serializer = new XmlSerializer(typeof(Dataset));
        Stream stream = new FileStream(path, FileMode.Open);
        Dataset result = serializer.Deserialize(stream) as Dataset;
        stream.Close();
        return result;
    }


}


/// <summary>
/// pathnodes is a type to store basic pathnode
/// info a pathnode actually got a lookatpoint 
/// but this section could be handled with the proper 
/// access on an extended class hierarchy for optimization 
/// i believe it s not necessary to store so much info or
/// a ccurved path would need another type for handles 
/// </summary>
[System.Serializable]
public class Pathnode
{
    public Vector3 pos;
    public int ilookatpoint = 0;
    //public List<Vector3> orb = new List<Vector3>() ;
    public float lookatspeed = 0.0f;
    public float translatespeed = 0.0f;
    public float waitonnode = 0.0f;
    public float timer = 0.0f;
    public virtual Vector3 Getlookatpoint(int lookatindex, float radius, int step = 8)
    {
        float a = ((360.0f / step) * Mathf.Deg2Rad) * lookatindex + (Mathf.Deg2Rad * 45.0f);
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.0f, radius * sa + radius * ca);
        return (RV);//+ pos) ;
    }
}

/// <summary>
/// 
/// </summary>
public abstract class BBehavior : MonoBehaviour
{
    // should implement only shared props for all behaviors 
    //public GameObject looktarget;
    //public GameObject parent;
    public float editortick = 0.02f;
    //public ReplayerLogOutput m_replayerlogoutput = null;
    //public object PB ;
    //public Transform block_transform;
    //public  GameObject triggerobject;
    public Vector3 pointSnap = Vector3.one * 0.001f;
    public Actor m_actor = null;

    public List<object> argsbuff = new List<object>();
    
    /// <summary>
    /// need override ( no meaning to get any generic implementation 
    /// </summary>
    /// <returns></returns>
    public abstract Dataset GetDataset();
    //public  abstract void SetDataset(System.Type T);
    public  abstract void DoGUILoop(Rect Mainwindow);
    public abstract void OnDrawGizmosSelected();
    public abstract void OnCustomSceneGUI(SceneView sceneview);
    public abstract void SetDataset(object o);



    public  virtual string GetFullQualifiedClassName()
    {
        return GetType().Assembly.FullName;
    }



    public  virtual void  Awake ()
    {
        // pre init if required not now but never know 
    }

    /// <summary>
    ///  for serialisation could be moved in a utility static class 
    ///  there s one utility in class in editor scope need another in 
    ///  unity namespace 
    /// </summary>
    /// <param name="path">where to save this</param>
    /// <param name="type">type to serialize</param>
    /// <returns></returns>
    public virtual BaseActorProperties Load(string path, System.Type type)
    {
        if (!System.IO.File.Exists(path))
            return null;
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Open);
        BaseActorProperties result = serializer.Deserialize(stream) as BaseActorProperties;
        stream.Close();
        return result;
    }

    /// <summary>
    /// set the prop at actor level ( non serialized for Ienumeratable crap )
    /// basically the part that comes from Monobehaviour everything else is 
    /// encapsulated in Actorprop Class 
    /// </summary>
	public virtual  void Start () 
    {
        Actor A = (Actor)GetComponent(typeof(Actor));
        A.Actorprops.orig_rotation = transform.rotation;
        A.Actorprops.orig_pos = transform.position;
	}




    /// <summary>
    /// register protected display callback of the behavior 
    /// </summary>
    public virtual void OnEnable() 
    { 
        SceneView.onSceneGUIDelegate += OnCustomSceneGUI; 
    }

    /// <summary>
    /// unregister display callback 
    /// </summary>
    public virtual void OnDisable() 
    { 
        SceneView.onSceneGUIDelegate -= OnCustomSceneGUI; 
    }

    /// <summary>
    /// could be used in waiting loop 
    /// </summary>
    /// <param name="tempo"></param>
    public virtual void  Wait(float tempo  )
    {
        //var a = new WaitForSeconds(tempo);
        WaitForSeconds s = new WaitForSeconds(tempo);
    }


// Update is called once per frame
    public abstract void Update(); 

     
 

}
