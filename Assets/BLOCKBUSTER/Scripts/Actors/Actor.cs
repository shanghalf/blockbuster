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
        string logpath = Application.dataPath +"/BLOCKBUSTER/XML/logoutput.txt";
        //TextWriterTraceListener TL = new TextWriterTraceListener(logpath);
        ConsoleTraceListener TL = new ConsoleTraceListener();
        Trace.Listeners.Add(TL);//               ["console"].TraceOutputOptions = TraceOptions.DateTime;
        Trace.WriteLine(string.Format(" \n OUTLOG {0}", s));
        Trace.Flush();
        Trace.Close();
    }
}










[XmlInclude(typeof(RotatingPlatformDataset))]
[XmlInclude(typeof(MovingPlatformDataset))]
[XmlInclude(typeof(FallingPlatformDataset))]
[XmlInclude(typeof(Dataset))]
[System.Serializable]
public class BaseActorProperties
{
    public string parentgui;
    public string guid;
    public string assetname;
    public Quaternion orig_rotation = Quaternion.identity;
    public Vector3 orig_pos;
    public Vector3 last_pos;
    public Vector3 block_size;
    public List<string> BehaviorListID = new List<string>();
    //public List<Dataset> DatasetList = new List<Dataset>();    
    public bool grouped;

    // hold the behavior dataset 


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

    public virtual void Save(string path, System.Type type)
    {
        System.Type[] extraTypes = { type  };

        XmlSerializer serializer = new XmlSerializer(type, extraTypes);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
    }


}


/// <summary>
/// actor derive from MonoBehaviour and not supporting
/// serialization using Dataset and overrides to support this 
/// </summary>
public class Actor : MonoBehaviour 
{
    //public List<Dataset> DatasetTable = new List<Dataset>();
    // serializable properties 
    public BaseActorProperties Actorprops = new BaseActorProperties();
    public virtual void OnDrawGizmosSelected()
    {
    }
	// Use this for initialization
	public virtual void Start () 
    {
	}
	// Update is called once per frame
	public virtual void Update () 
    {
	}
    // acessor for serialized properties 
    public virtual BaseActorProperties GetActorprops()
    {
        return Actorprops;
    }
    public virtual void SetActorprops(BaseActorProperties A )
    {
        Actorprops = A;
    }


}
