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

public class BBuildLogUtility
{
    public static void outlog(string s)
    {
        //string logpath = Application.dataPath +"/BLOCKBUSTER/XML/logoutput.txt";
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
        //System.Type[] extraTypes = { type  };

        XmlSerializer serializer = new XmlSerializer(type );
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


    // mandatory for node editor 
    public BaseActorProperties GetActorProps()
    {
        return Actorprops;
    }

    public void OutputString(string s, string s2)
    {
        EditorUtility.DisplayDialog("arg 1 ", s + "atg 2 " +s2 , "yes");
    }


    private bool b_front_x;
    private static Vector3 _left = new Vector3(-1.0f, 0.0f, 0.0f);
    private static Vector3 _front = new Vector3(0.0f, 0.0f, 1.0f);
    private static Vector3 _right = new Vector3(1.0f, 0.0f, 0.0f);
    private static Vector3 _back = new Vector3(0.0f, 0.0f, -1.0f);


    [BBCtrlVisible] 
    public Vector3 left() { return _left; }
    [BBCtrlVisible]
    public Vector3 front() { return _front; }
    [BBCtrlVisible]
    public Vector3 right() { return _right; }
    [BBCtrlVisible]
    public Vector3 back() { return _back; }
    [BBCtrlVisible]
    public Vector3 up() { return Vector3.up; }
    [BBCtrlVisible]
    public Vector3 down() { return Vector3.down; }



   





    [BBCtrlVisible] // define a function visible for BBControl 
    public static void Rotate(bool camspace,  Vector3 Axis , bool fullselection , int step  )
    {
        if (camspace)
        {
            Actor A = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
            A.GetDir();
        }

           if ( fullselection ) 
           {
            for (int c = 0; c < Selection.gameObjects.GetLength(0); c++)
                Selection.gameObjects[c].transform.Rotate(Axis * step, Space.Self);
           }
           else
               Selection.activeGameObject.transform.Rotate(Axis * step, Space.Self);
    }


    [BBCtrlVisible] // define a function visible for BBControl 
    public static void TestFunction (Vector3 pos, bool checkbool , string name)
    {

        UnityEngine.Debug.Log(name);
    }

    [BBCtrlVisible] // define a function visible for BBControl 
    public Actor GetActor()
    {
        return this;
    }

    [BBCtrlVisible] // define a function visible for BBControl 
    public int ANGLE45()
    {
        return 45;
    }
    [BBCtrlVisible] // define a function visible for BBControl 
    public int ANGLE90()
    {
        return 90;
    }

    ///DoBlockMove(false, (back * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));




    /// <summary>
    /// crap this is defined in several point but not in the same namespace
    /// editor for block move and for movepad 
    /// should find a way to simplify 
    /// and share the logic without adding useless value to paramblock base 
    /// </summary>
    /// 
    public void GetDir()
    {
        if (SceneView.currentDrawingSceneView == null) return;
        Transform cam = SceneView.currentDrawingSceneView.camera.transform;
        Vector3 flatcamvector = new Vector3(cam.forward.x, 0.0f, cam.forward.z);
        float AF = Vector3.Angle(flatcamvector, Vector3.forward);
        float AB = Vector3.Angle(flatcamvector, Vector3.back);
        float AL = Vector3.Angle(flatcamvector, Vector3.left);
        float AR = Vector3.Angle(flatcamvector, Vector3.right);

        float[] anglearray = new float[] { AF, AB, AL, AR };

        System.Array.Sort(anglearray);
        if (AF == anglearray[0])
        {
            _front = Vector3.forward;
            _back = Vector3.back;
            _left = Vector3.right;
            _right = Vector3.left;
            b_front_x = false;
        }
        if (AB == anglearray[0])
        {
            _front = Vector3.back;
            _back = Vector3.forward;
            _left = Vector3.left;
            _right = Vector3.right;
            b_front_x = false;
        }
        if (AL == anglearray[0])
        {
            _front = Vector3.left;
            _back = Vector3.right;
            _left = Vector3.forward;
            _right = Vector3.back;
            b_front_x = false;
        }
        if (AR == anglearray[0])
        {
            _front = Vector3.right;
            _back = Vector3.left;
            _left = Vector3.back;
            _right = Vector3.forward;
            b_front_x = false;
        }


    }


    [BBCtrlVisible]
    public float dummy (bool a,bool s , bool t )
    {
        return 0;

    }

    [BBCtrlVisible]
    public float GetSize()
    {
            Actor m_actor;
            m_actor = (Actor) Selection.activeGameObject.GetComponent(typeof(Actor));
            return m_actor.Actorprops.block_size.magnitude;

    }


    [BBCtrlVisible] 
    public void DoBlockMove( bool instanciate, Vector3 dir, bool moveallpath  , bool camspace)
    {



        //************************************************************************************************
        // perform block manipulation in move block section of the tool 
        // there s 2 diferent way to move a block during the runtime ( dynamicaly ) have to pay atention to 
        // rotation blocks where there s a bunch of generated sub component ( root node and looktarget ) 
        // rotation blocks use a lookat function which is more convenient to handle INSTANCIATE if the block 
        // nedd to be duplicated 
        string str;
        //if (b_followpath)
        // bs.paramblock.pathnodes[bs.paramblock.targetindex] = Selection.activeGameObject.transform.position;

        // iterate on the selection 
        foreach (GameObject go in Selection.gameObjects)
        {
            //GameObject ts = (GameObject)Selection.gameObjects.GetValue(i);
            Actor m_actor;
            m_actor = (Actor)go.GetComponent(typeof(Actor));						// should be there 
            if (camspace)
                m_actor.GetDir();

            m_actor.Actorprops.last_pos = go.transform.position; // make sure the pos is right 
            if (m_actor == null)
                continue;
            if (instanciate)
            {	// ------------------------- MOVE AND DUPLICATE
                GameObject obj = (GameObject)Instantiate(go, go.transform.position, go.transform.rotation);
                str = go.name;										// change the name  
                string[] strarray = str.Split(new char[] { '-' });
                obj.name = strarray[0] + obj.GetInstanceID();								// final name is block original name plus unique id 
                if (go.transform.parent)
                    obj.transform.parent = go.transform.parent;
                System.Guid g;
                g = System.Guid.NewGuid();
                if (m_actor)
                {
                    m_actor.Actorprops.guid = g.ToString();//obj.GetInstanceID().ToString();
                    m_actor.Actorprops.parentgui = m_actor.Actorprops.guid;
                    m_actor.Actorprops.orig_pos = go.transform.position;
                    m_actor.Actorprops.orig_rotation = go.transform.rotation;
                    m_actor.Actorprops.BehaviorListID.Clear();
                    // refresh dataset guiid list 
                    BBehavior[] BHL = obj.GetComponents<BBehavior>();
                    foreach (BBehavior B in BHL)
                    {
                        Dataset localdataset = B.GetDataset();
                        string newguid = System.Guid.NewGuid().ToString();
                        localdataset.SetGuid(newguid.ToString());
                        m_actor.Actorprops.BehaviorListID.Add(newguid);

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
                }

                //AddPlatformComponent(obj, null);
            }
            go.transform.position += dir;
            // static block
            BBehavior[] blist = go.GetComponents<BBehavior>();
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
