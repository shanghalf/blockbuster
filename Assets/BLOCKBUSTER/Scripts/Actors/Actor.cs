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


public class BBattribute : System.Attribute
{
    private string _value;

    public BBattribute(string value)
    {
        _value = value;
    }
    public string Value
    {
        get { return _value; }
    }
}


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

    public bool bbeditor_fixedstep;
    public bool b_front_X ;
    // array is used to pass args to the function invoked on BBcontroll 
    public List<object> argsbuff = new List<object>();


    public void OutputString(string s, string s2)
    {
                
        EditorUtility.DisplayDialog("arg 1 ", s + "atg 2 " +s2 , "yes");


    }


    private Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
    private Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
    private Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
    private Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);

    public void BBeditorMoveForward()
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (front * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

    
    public void BBeditorMovEBlock (Vector3 V )
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (V * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }



    public void BBeditorMoveBack()
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (front * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

    public void BBeditorMoveLeft()
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (front * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

    public void BBeditorMoveRight()
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (front * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

    public void BBeditorMoveUp()
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (front * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

    public void BBeditorMoveDown()
    {
        float ofset;
        float stepvalue = 2.0f; // default for stepvalue
        Vector3 BlockSize = Actorprops.block_size;
        BBEditorActorMove(false, (front * (ofset = (bbeditor_fixedstep) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }


    public void BBEditorActorMove(bool instanciate, Vector3 dir, bool moveallpath = true)
    {




        string str;
        Transform TT = this.transform ;
        Actorprops.last_pos = TT.position; // make sure the pos is right 
        if (instanciate)
        {	// ------------------------- MOVE AND DUPLICATE
            TT = this.transform ;
            GameObject obj = (GameObject)Instantiate(this.gameObject, TT.position, TT.rotation);
            str = name;										// change the name  
            string[] strarray = str.Split(new char[] { '-' });
            obj.name = strarray[0] + obj.GetInstanceID();								// final name is block original name plus unique id 
            if (this.transform.parent)
                obj.transform.parent = TT.parent;
            System.Guid g;
            g = System.Guid.NewGuid();
            Actorprops.guid = g.ToString();//obj.GetInstanceID().ToString();
            Actorprops.parentgui = Actorprops.guid;
            Actorprops.orig_pos = TT.position;
            Actorprops.orig_rotation = TT.rotation;
            Actorprops.BehaviorListID.Clear();
            // refresh dataset guiid list 
            BBehavior[] BHL = obj.GetComponents<BBehavior>();
            foreach (BBehavior B in BHL)
            {
                Dataset localdataset = B.GetDataset();
                string newguid = System.Guid.NewGuid().ToString();
                localdataset.SetGuid(newguid.ToString());
                Actorprops.BehaviorListID.Add(newguid);

                // translate pathnodes 
                List<Pathnode> pnodes = localdataset.GetPathNodes();
                if (pnodes == null)
                    continue;
                if (pnodes.Count == 0)
                    continue;
                // in edit sub mode we move only the current pathnode 
                if (!localdataset.editsub)
                    foreach (Pathnode pn in pnodes)
                        pn.pos += dir; // else move closest to obj 
                else
                    pnodes[localdataset.targetindex].pos += dir;
            }

            //AddPlatformComponent(obj, null);
        }
        TT.position += dir;
        // static block
        BBehavior[] blist = GetComponents<BBehavior>();
        foreach (BBehavior B in blist)
        {
            Dataset localdataset = B.GetDataset();
            List<Pathnode> pnodes = localdataset.GetPathNodes();
            if (pnodes == null)
                continue;
            // in edit sub mode we move only the current pathnode 
            foreach (Pathnode pn in pnodes)
                pn.pos += dir; // else move closest to obj 
        }

        

    }






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
