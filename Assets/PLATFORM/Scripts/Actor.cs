#define TRACE

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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;





public class BuildLogUtility
{
    public static void outlog(string s)
    {
        string logpath = Application.dataPath +"/PLATFORM/XML/logoutput.txt";
        //TextWriterTraceListener TL = new TextWriterTraceListener(logpath);
        ConsoleTraceListener TL = new ConsoleTraceListener();
        Trace.Listeners.Add(TL);//               ["console"].TraceOutputOptions = TraceOptions.DateTime;
        Trace.WriteLine(string.Format(" \n OUTLOG {0}", s));
        Trace.Flush();
        Trace.Close();
    }
}









[System.Serializable]
public class BaseActorProperties
{
    public string parentgui;
    public string guid;
    public string assetname;
    public Quaternion orig_transform = Quaternion.identity;
    public Vector3 orig_pos;
    public Vector3 last_pos;
    public Vector3 block_size;
    //public System.Type BHVTYPE;
    public bool grouped;
    public List<string> BehaviorListID = new List<string>();

    public static BaseActorProperties Load(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            //debug.Log("file not exist");
            return null;
        }
        XmlSerializer serializer = new XmlSerializer(typeof(BaseActorProperties));
        Stream stream = new FileStream(path, FileMode.Open);
        BaseActorProperties result = serializer.Deserialize(stream) as BaseActorProperties;
        stream.Close();
        return result;
    }


}


[System.Serializable]
public class Actor : MonoBehaviour 
{

    //public List<Dataset> DatasetTable = new List<Dataset>();


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
	
    public  void Add(System.Object o)
    {
        

    }

	// Update is called once per frame
	public virtual void Update () 
    
    {
       // Debug.Log("not implemented at actor level");
	}


    // save function should serialize a table of Behaviors (should be broken now )





}
