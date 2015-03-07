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


public abstract class Behavior : MonoBehaviour
{
    // should implement only shared props for all behaviors 
    public GameObject looktarget;
    public GameObject parent;
    public float editortick = 0.02f;
    public ReplayerLogOutput m_replayerlogoutput;
    //public object PB ;
    public Dataset paramblock;
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
        return paramblock;
    }


    public virtual string GetClassName()
    {
        return this.name;

    }


    public virtual void DoGUILoop(Rect Mainwindow)
    {
        // place here the common controls (simplify the guiloop on actor) 
        m_actor = (Actor)GetComponent(typeof(Actor));						// should be there 
        //EditorWindow W = EditorWindow.GetWindow( ;
    }

    public virtual  string GetClassname(bool shortname = false)
    {
        if (shortname)
            return "Behavior";

        string fullyQualifiedName = typeof(MovingPlatform).AssemblyQualifiedName;
        return fullyQualifiedName;


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
    public virtual Dataset Load(string path, System.Type type)
    {

        if (!System.IO.File.Exists(path))
        {
            return null;
        }
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Open);
        Dataset result = serializer.Deserialize(stream) as Dataset;
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
        Debug.Log("NOT IMPLEMENTED");
	}

     
 

}
