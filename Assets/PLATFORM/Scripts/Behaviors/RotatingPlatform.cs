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
/// <autoenum> ROTATING_PLATFORM RotatingPlatform </autoenum>







[System.Serializable]
public class RotatingPlatformPathnode : Pathnode
{
    public Vector3 pos;
    public int ilookatpoint = 0;
    //public List<Vector3> orb = new List<Vector3>() ;
    public float lookatspeed = 0.0f;
    public float translatespeed = 0.0f;
    public float waitonnode = 0.0f;
    //public float timer = 0.0f;
    public virtual Vector3 Getlookatpoint(int lookatindex, float radius, int step = 8)
    {
        float a = ((360.0f / step) * Mathf.Deg2Rad) * lookatindex + (Mathf.Deg2Rad * 45.0f);
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.0f, radius * sa + radius * ca);
        return (RV);//+ pos) ;
    }

}
[System.Serializable]
public class RotatingPlatformDataset : Dataset
{
    public List<RotatingPlatformPathnode> m_pathnodes = new List<RotatingPlatformPathnode>();
    private int targetindex = 0;
    public RotatingPlatformPathnode rotatelookpoint = new RotatingPlatformPathnode();
    //public bool ismoving;
    //public int ilookatpoint=0;
    public float speed = 0.5f;
    public float move_ampl=0.0f;
    public Vector3 target = Vector3.zero;
    public bool b_revert_rotation=false;
    public float rotationspeed=0.0f;
    public int rotationstepnumber=2;
    public float rotationtempo=0.0f;
    public int rotateindex=0;
    //public bool editsub;
    //public bool b_showdebuginfos;
    //public ARRAY_BOUND indexbound;

    //public Vector3[] quater = new Vector3[] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };
    //public bool b_pathloop =false;
    //public COLIDER_TYPE colider_type = COLIDER_TYPE.BOX;
    //public bool b_showdebuginfos;




    public bool SetSafeTargetIndex(int index)
    {
        if (m_pathnodes.Count == 0)
        {
            Debug.Log("no pathnodes target to set");
            return false;
        }
        int correctindex = Mathf.Clamp(index, 0, m_pathnodes.Count - 1);
        targetindex = correctindex;

        // array boundaries  
        if (targetindex == m_pathnodes.Count - 1)
        {
            indexbound = ARRAY_BOUND.UP;
        }
        else if (targetindex == 0)
        {
            indexbound = ARRAY_BOUND.DOWN;
        }
        else
            indexbound = ARRAY_BOUND.MIDDLE;

        return true;
    }


    public  int GetSafeTargetIndex()
    {
        if (m_pathnodes.Count == 0)
            return -666;
        int correctindex = Mathf.Clamp(targetindex, 0, m_pathnodes.Count - 1);
        targetindex = correctindex;

        // array boundaries  
        if (targetindex == m_pathnodes.Count - 1)
        {
            indexbound = ARRAY_BOUND.UP;
        }
        else if (targetindex == 0)
        {
            indexbound = ARRAY_BOUND.DOWN;
        }
        else
            indexbound = ARRAY_BOUND.MIDDLE;

        return (correctindex > 0) ? correctindex : 0;
    }


    public object GetPathNode(int index)
    {
        // protect the access of pathnodelist 
        int correctindex = Mathf.Clamp(index, 0, m_pathnodes.Count - 1);
        return (object)m_pathnodes[correctindex];

    }



}







public class RotatingPlatform : Behavior
{
    // should implement only shared props for all behaviors 


    public RotatingPlatformDataset paramblock = new RotatingPlatformDataset();

    public override Dataset GetDataset()
    {
        return (RotatingPlatformDataset)paramblock;
    }




 



    public override void DoGUILoop(Rect mainwindow)
    {
        base.DoGUILoop(mainwindow);
        //paramblock.editsub = EditorGUILayout.Toggle("EditSub", paramblock.editsub);

        paramblock.rotationstepnumber = (int)EditorGUILayout.Slider("step", paramblock.rotationstepnumber, 2, 8, GUILayout.MaxWidth(280));

        paramblock.rotationspeed = EditorGUILayout.Slider("speed", paramblock.rotationspeed, 0.0f, 5.0f, GUILayout.MaxWidth(280));
        paramblock.rotationtempo = EditorGUILayout.Slider("temporisation", paramblock.rotationtempo, 0.0f, 2.0f, GUILayout.MaxWidth(280));
        // editsub button shared over panels 

        if (paramblock.editsub)
        {
            paramblock.b_revert_rotation = EditorGUILayout.Toggle("invert", paramblock.b_revert_rotation, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
        }
        else
            paramblock.ismoving = false;

    }









    public override  void OnDrawGizmosSelected()
    {
        Actor A = (Actor) GetComponent(typeof(Actor));
        if (A == null)
        {
            // shit happens .. 
            Debug.Log("selected Gameobject Hve No Actor component");
            return;     
        }

        for (int c = 0; c <= paramblock.rotationstepnumber; c++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(paramblock.rotatelookpoint.Getlookatpoint(paramblock.rotateindex, 1.0f, paramblock.rotationstepnumber) + transform.position, 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(paramblock.rotatelookpoint.Getlookatpoint(c, 1.0f, paramblock.rotationstepnumber) + transform.position, 0.1f);
        }
    }
    //public virtual void Save(string path)
    public override  Dataset Load(string path, System.Type type)
    {
        if (!System.IO.File.Exists(path))
        {
            //debug.Log("file not exist");
            return null;
        }
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Open);
        Dataset result = serializer.Deserialize(stream) as Dataset;
        stream.Close();
        return result;
    }


	// Use this for initialization
	public override void Start () 
    {
        MovingPlatform D = (MovingPlatform)GetComponent(typeof(MovingPlatform));
        if (D != null) // desactivate the path direction 
            D.paramblock.b_path_rotation = false;

	}


#if UNITY_EDITOR

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnCustomSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnCustomSceneGUI;
        Debug.Log("I was called.");
    }

    protected void OnCustomSceneGUI(SceneView sceneview)
    {
        //float angle = -360f / (5);

        if (paramblock == null)
            return;

        for (int i = 0; i < paramblock.m_pathnodes.Count-1; i++)
        {
            Pathnode p1 = paramblock.m_pathnodes[i];

            if (p1 == null)
                return;

            Handles.color = Color.blue;
            Actor AP = (Actor) GetComponent(typeof(Actor));
            Handles.Label(transform.position + Vector3.up,
                    transform.transform.position.ToString() + "\nName: " + AP.Actorprops.assetname );

            //float width = (float)HandleUtility.GetHandleSize(oldPoint) * 0.5f;
            //if (i > 0)

            //Handles.DrawLine(transform.position ,pa p1.ilookatpoint );
            //Handles.DrawBezier(transform.transform.position, oldPoint, oldPoint,-oldPoint,Color.red,null,width);
            //Handles.FreeRotateHandle(Quaternion.identity, paramblock.pathnodes[i].pos, 0.2f);
        }
    }



#endif

// Update is called once per frame
	public override void Update () 
    {

        if (paramblock.rotationstepnumber == 0)
            return;
        int i = (int)Mathf.Abs(Time.realtimeSinceStartup * paramblock.rotationtempo);
         paramblock.rotateindex = i % paramblock.rotationstepnumber;

        if (paramblock.b_revert_rotation)
            paramblock.rotateindex = (paramblock.rotationstepnumber - paramblock.rotateindex) - 1; // should revert the sequence 

        Vector3 v = new Vector3(0, 0, 0);
        v = paramblock.rotatelookpoint.Getlookatpoint(paramblock.rotateindex, 1.0f, paramblock.rotationstepnumber);
        var direction = v + transform.position;
        var rr = Quaternion.LookRotation(Vector3.up, v);
        rr *= Quaternion.Euler(Vector3.forward);
        if (v.magnitude < 0.1)
            return;

# if ! UNITY_EDITOR
        transform.rotation = Quaternion.Lerp(transform.rotation, rr, (paramblock.rotationspeed * Time.deltaTime));
        Debug.Log(editortick);
# endif
# if  UNITY_EDITOR
        transform.rotation = Quaternion.Lerp(transform.rotation, rr, (paramblock.rotationspeed * editortick));
#endif	
        
	}

     


}

