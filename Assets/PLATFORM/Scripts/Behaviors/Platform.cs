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

[System.Serializable]
public class Dataset
{
    public bool b_front_x;
    public float timer = 0.0f;
    public bool editsub = true;
    public bool b_showdebuginfos;
    public bool b_hideedition = false;

    static int submenu = 4;
    public bool ismoving = false;
    public ARRAY_BOUND indexbound;
    public string guid = "";
    public List<Pathnode> m_pathnodes = new List<Pathnode>();
    public string fullqualifiedclassname;
    public string suportedclassname;



    public static bool menubaritem()
    {
        return GUILayout.Button("Dataset", EditorStyles.toolbarButton);
    }


    public virtual string Getsuportedclassname ()
    {
        return suportedclassname;
    }

    public virtual string GetGuid()
    {
        return guid;
    }

    public virtual int GetSafeTargetIndex()
    {
        return 0;
    }

    
    public virtual void Save(string path, System.Type type)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
    }
    

    public virtual List<Pathnode> GetPathNodes()
    {
        return m_pathnodes;
    }

    public virtual string GetFullQualifiedClassName()
    {
        return GetType().AssemblyQualifiedName;
    }

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


public abstract class Behavior : MonoBehaviour
{
    // should implement only shared props for all behaviors 
    public GameObject looktarget;
    public GameObject parent;
    public float editortick = 0.02f;
    public ReplayerLogOutput m_replayerlogoutput;
    //public object PB ;
    public Transform block_transform;
    public  GameObject triggerobject;
    public Vector3 pointSnap = Vector3.one * 0.001f;
    public Actor m_actor = new Actor();
   
    /// <summary>
    /// common init for behavior 
    /// </summary>
    public Behavior()
    {
    }

    public virtual Dataset GetDataset ()
    {
        return null;
    }

    public virtual void SetDataset(Dataset D)
    {
        return ;
    }


    public virtual void DoGUILoop(Rect Mainwindow)
    {
        // place here the common controls (simplify the guiloop on actor) 
        m_actor = (Actor)GetComponent(typeof(Actor));						// should be there 
 
        //EditorWindow W = EditorWindow.GetWindow( ;
    }

    
    public virtual  string GetFullQualifiedClassName()
    {
        return GetType().AssemblyQualifiedName;
    }
    

    /// <summary>
    /// not that much to do at this level 
    /// </summary>
    public virtual void OnDrawGizmosSelected()
    {
        Debug.Log("not implemented at this level cast an appropriated behavior class");
    }

    /// <summary>
    /// performed at deeper level than start
    /// could be interesting 
    /// </summary>
    public virtual void  Awake ()
    {
        // for example to be sure that the dataset have a ref object 
    }




    /// <summary>
    ///  for serialisation 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual BaseActorProperties Load(string path, System.Type type)
    {

        if (!System.IO.File.Exists(path))
        {
            return null;
        }
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Open);
        BaseActorProperties result = serializer.Deserialize(stream) as BaseActorProperties;
        stream.Close();
        return result;
    }

    /// <summary>
    /// right place to init what needed by a behavior 
    /// </summary>
	public virtual  void Start () 
    {
        if (!block_transform)
            return;
        Actor A = (Actor)GetComponent(typeof(Actor));
        A.Actorprops.orig_transform = block_transform.rotation;
        A.Actorprops.orig_pos = block_transform.position;
	}

    protected virtual void OnCustomSceneGUI(SceneView sceneview)
    {
        // no implementation at this level 
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
	public virtual  void Update () 
    {
	}

     
 

}
