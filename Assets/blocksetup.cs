


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

[System.Serializable]
public class scenecluster
{
    public string name;
    public Vector3 rootnodepos;
    public List<ParameterBlock> pblist = new List<ParameterBlock>();
}

public class GUIDraggableObject
{
    protected Vector2 m_Position;
    private Vector2 m_DragStart;
    private bool m_Dragging;

    public GUIDraggableObject(Vector2 position)
    {
        m_Position = position;
    }

    public bool Dragging
    {
        get
        {
            return m_Dragging;
        }
    }

    public Vector2 Position
    {
        get
        {
            return m_Position;
        }

        set
        {
            m_Position = value;
        }
    }

    public void Drag(Rect draggingRect)
    {
        if (Event.current.type == EventType.MouseUp)
        {
            if (m_Dragging)
            {
                m_Position = Event.current.mousePosition - m_DragStart;
                m_Dragging = false;
            }

        }
        else if (Event.current.type == EventType.MouseDown && draggingRect.Contains(Event.current.mousePosition))
        {
            m_Dragging = true;
            m_DragStart = Event.current.mousePosition - m_Position;
            Event.current.Use();
        }

        if (m_Dragging)
        {
            m_Position = Event.current.mousePosition - m_DragStart;
        }
    }
}



public class Scene 
{
    //public Scene scene = new Scene();

    public string name;
    public scenecluster cluster = new scenecluster();


    public Scene()
    {
        name = "test";
    
    }

    public void Save(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(scenecluster));
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this.cluster);
        stream.Flush();
        stream.Close();
        //debug.Log ( serializer.ToString());
    }



    public static scenecluster Load(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            //debug.Log("file not exist");
            return null;
        }
        XmlSerializer serializer = new XmlSerializer(typeof(scenecluster));
        Stream stream = new FileStream(path, FileMode.Open);
        scenecluster result = serializer.Deserialize(stream) as scenecluster;
        stream.Close();
        return result;
    }

}


[System.Serializable]
public enum PLTF_TYPE
{
    STATIC = 0,
    ROTATING = 1,
    FALLING = 2,
    MOVING = 3
}
[System.Serializable]
public enum COLIDER_TYPE
{
    BOX = 0,
    SPHERE = 1,
    CAPSULE = 2,
    MESH = 3
}

[System.Serializable]
public class SplineNode  
{
    public string Name;
    public Vector3 Point;
    public GameObject objref;
}


[System.Serializable]
public class Pathnode
{

    public Vector3 pos;
    public int ilookatpoint;
    //public List<Vector3> orb = new List<Vector3>() ;
    public float lookatspeed;
    public float translatespeed;
    public float waitonnode;
    public float timer;
 
    public Pathnode()
    {
    }

    public  Vector3 Getlookatpoint(int lookatindex , float radius ,int step =8)

    {
        float a = ((360.0f / step) * Mathf.Deg2Rad) * lookatindex + ( Mathf.Deg2Rad * 45.0f );
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.0f, radius * sa + radius * ca);
        return ( RV );//+ pos) ;
    }
}







[System.Serializable]
public class ParameterBlock
{
    public string parentgui;
    public float timer;

    public string guid;
    public string assetname;
    public Vector3 block_size;
    public Quaternion orig_transform = Quaternion.identity;
    public Vector3 orig_pos;
    public Vector3 last_pos;
    public List<Pathnode> pathnodes = new List<Pathnode>() ;
    public int ilookatpoint;
    public float speed = 0.5f;
    public float testfloat;
    public float move_ampl;
    public PLTF_TYPE pltf_sate;
    public bool editsub = true;
    public Vector3 target;
    public bool ismoving = false;
    public bool b_rotate_X = false;
    public bool b_rotate_Y = false;
    public bool b_rotate_Z = false;
    public bool b_followpath ;
    public bool b_revert_rotation;
    public bool b_triggeronce;
    public float rotationspeed;
    public int rotationstepnumber;
    public float rotationtempo;
    public bool b_triggered;
    public int rotateindex;
    public int targetindex = 0;
    public int movedir = 1;
    public int maxhandle;
    public Pathnode rotatelookpoint = new Pathnode(); 
    //public Vector3[] quater = new Vector3[]{Vector3.forward,Vector3.left,Vector3.back, Vector3.right};
    public bool b_pathloop;
    public bool grouped;
    public COLIDER_TYPE colider_type ;
    

    public ParameterBlock()
    {
        
    }



}



[ExecuteInEditMode()]
public class blocksetup : MonoBehaviour
{

    public Transform block_transform ;
    public GameObject triggerobject;
    public GameObject looktarget;
    public GameObject scenerefobj;
    public GameObject parent;
    public ParameterBlock paramblock = new ParameterBlock();
    public float editortick =0.02f;


    #if UNITY_EDITOR
    void  OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnCustomSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnCustomSceneGUI;
        Debug.Log("I was called.");
    }

    #endif




    public void OnDrawGizmosSelected()
    {
        switch (paramblock.pltf_sate)
        {
            case PLTF_TYPE.MOVING:
                Gizmos.color = Color.yellow;
                if (paramblock.maxhandle <= 0)
                    break;
                for (int c = 0; c <= paramblock.pathnodes.Count-1; c++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(paramblock.pathnodes[c].pos, 0.1f);
                    if (c > 0)
                        Gizmos.DrawLine(paramblock.pathnodes[c-1].pos, paramblock.pathnodes[c].pos);
                    Gizmos.color = Color.red;
                    //foreach (Vector3 v in paramblock.pathnodes[c].orb)
                    Vector3 lookatpoint = paramblock.pathnodes[c].Getlookatpoint(paramblock.pathnodes[c].ilookatpoint, paramblock.block_size.magnitude / 5) + paramblock.pathnodes[c].pos;

                    Gizmos.DrawSphere(lookatpoint, 0.1f);
                    Gizmos.DrawLine(lookatpoint, paramblock.pathnodes[c].pos);    

                }
                break;
            case PLTF_TYPE.ROTATING:
                for (int c = 0; c <= paramblock.rotationstepnumber; c++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(paramblock.rotatelookpoint.Getlookatpoint(paramblock.rotateindex,1.0f , paramblock.rotationstepnumber ) + paramblock.last_pos, 0.2f);
                                        
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(paramblock.rotatelookpoint.Getlookatpoint(c, 1.0f, paramblock.rotationstepnumber) + paramblock.last_pos, 0.1f);
                    
                    
                    
                }
                break;
        }
    }

    public  void  Save(string path)
 	{
        XmlSerializer serializer = new XmlSerializer(typeof(ParameterBlock));
 		Stream stream = new FileStream(path, FileMode.Create);
 		serializer.Serialize(stream, this.paramblock);
 		stream.Flush();
 		stream.Close();
 		//debug.Log ( serializer.ToString());
 	}

    public static ParameterBlock  Load(string  path )
 	{
	 	if (!System.IO.File.Exists(path))
	 	{
            //debug.Log("file not exist");
            return null;
	 	}
 		XmlSerializer serializer  = new XmlSerializer(typeof(ParameterBlock));
 		Stream  stream  = new FileStream(path, FileMode.Open);
 		ParameterBlock  result  = serializer.Deserialize(stream) as ParameterBlock;
 		stream.Close();
 		return result ;
 	}

    public static ParameterBlock LoadFromText(string text ) 
	{
		XmlSerializer serializer   = new XmlSerializer(typeof(ParameterBlock));
		return serializer.Deserialize(new StringReader(text)) as ParameterBlock;
	}



	// Use this for initialization
	void Start () 
    {
        if (!block_transform)
            return;
		paramblock.orig_transform =  block_transform.rotation ;
        paramblock.orig_pos = block_transform.position;
	}

    private static Vector3 pointSnap = Vector3.one * 0.001f;

#if UNITY_EDITOR
    void OnCustomSceneGUI(SceneView sceneview)
    {


        float angle = -360f / (5);
        for (int i = 0; i < paramblock.pathnodes.Count; i++)
        {
            //Quaternion rotation = Quaternion.Euler(0f, 0f, angle * i);
            Vector3 oldPoint = paramblock.pathnodes[i].pos;
            //Handles.FreeMoveHandle(oldPoint, Quaternion.identity, 0.2f, paramblock.pathnodes[i].pos, Handles.DotCap);

            Vector3 newPoint = Handles.FreeMoveHandle( oldPoint, Quaternion.identity, 0.1f, pointSnap, Handles.DotCap);
            if (oldPoint != newPoint)
                paramblock.pathnodes[i].pos = newPoint;
            Handles.color = Color.blue;
            Handles.Label( transform.position + Vector3.up ,
                    transform.transform.position.ToString() + "\nName: " +
                    paramblock.assetname);

            //float width = (float)HandleUtility.GetHandleSize(oldPoint) * 0.5f;
            if (i > 0)
                Handles.DrawLine(paramblock.pathnodes[i].pos, paramblock.pathnodes[i - 1].pos);
            
            //Handles.DrawBezier(transform.transform.position, oldPoint, oldPoint,-oldPoint,Color.red,null,width);


            Handles.FreeRotateHandle(Quaternion.identity, paramblock.pathnodes[i].pos, 0.2f);
        }
    }

#endif


    public void  Wait(float tempo)
    {
        paramblock.ismoving = false;
        var a = new WaitForSeconds(tempo);
        
        WaitForSeconds s = new WaitForSeconds(tempo);
        paramblock.ismoving = true;

    }



// Update is called once per frame
	void Update () 
    {
		switch (paramblock.pltf_sate) 
	    {
		    case PLTF_TYPE.STATIC:
                //DestroyImmediate(this);
			break;
		    case PLTF_TYPE.ROTATING:
                RotatePlatform(  ) ; 
			    break;
		    case PLTF_TYPE.FALLING:
			    break;
		    case PLTF_TYPE.MOVING:
                if (paramblock.targetindex >= paramblock.pathnodes.Count || paramblock.editsub)
                    break;


                if (paramblock.targetindex >= paramblock.pathnodes.Count - 1)
                {
                    if (paramblock.b_pathloop)
                        if ((Vector3.Distance(transform.position, paramblock.pathnodes[paramblock.targetindex].pos) == 0.0f))
                        {
                            paramblock.targetindex = 0;
                            paramblock.movedir = (1);
                            // cannot pop at exact pos 
                            Vector3 offs =  Vector3.forward/100; // slight offset 
                            transform.position = paramblock.pathnodes[paramblock.targetindex].pos+offs;
                        }
                    paramblock.movedir = (-1);
                }
                else if (paramblock.targetindex <= 0 )
                    paramblock.movedir = (1);


                Vector3 target = paramblock.pathnodes[paramblock.targetindex].pos;
                if (Vector3.Distance(transform.position, target) == 0.0f)
                {

                    if (paramblock.pathnodes[paramblock.targetindex].timer > 0)
#if UNITY_EDITOR   
                        paramblock.pathnodes[paramblock.targetindex].timer -= editortick ;
#endif   
#if !UNITY_EDITOR   
                        paramblock.pathnodes[paramblock.targetindex].timer -= Time.deltaTime;
#endif
                    else
                    {
                        
                        paramblock.pathnodes[paramblock.targetindex].timer = paramblock.pathnodes[paramblock.targetindex].waitonnode;
                        paramblock.targetindex += paramblock.movedir;
                    }
                
                    
  
                }
        


  


                if (paramblock.ismoving)//|| (Vector3.Distance(transform.position, paramblock.pathnodes[0].pos) > 0.0f))
                {
                    int ti = paramblock.targetindex;
                    int lkp =paramblock.pathnodes[ti].ilookatpoint;
                    //Debug.Log(lkp);
                    float rspeed, tspeed;
                    Vector3 targetpos = paramblock.pathnodes[ti].Getlookatpoint ( lkp , 1.0f) ;
                    // ude local speed for TR or global 
                    if (paramblock.move_ampl == 0.0f)
                    {
                        rspeed = paramblock.pathnodes[ti].lookatspeed;
                        tspeed = paramblock.pathnodes[ti].translatespeed;
                    }
                    else
                    {
                        rspeed = paramblock.move_ampl/3; // arbitrary could find a better way 
                        tspeed = paramblock.move_ampl;
                    }
                    if (looktarget != null)
                        looktarget.transform.position = targetpos ;

                    var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                    //Qr *= Quaternion.Euler(Vector3.forward);
#if UNITY_EDITOR

                    transform.position = Vector3.MoveTowards(transform.position, target, tspeed * editortick);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * editortick));
#endif
#if !UNITY_EDITOR

                    transform.position = Vector3.MoveTowards(transform.position, target, tspeed * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * Time.deltaTime));
#endif


                }

                //debug.Log((Vector3.Distance(transform.position, paramblock.pathnodes[paramblock.targetindex].pos) > 0.0f));
                //RotatePlatform();
			    break; 
        }
	}

     

    public void RotatePlatform (  ) 

    {   
        if ( paramblock.rotationstepnumber == 0 ) 
            return;
	    int i   = (int)  Mathf.Abs( Time.realtimeSinceStartup * paramblock.rotationtempo) ;
        paramblock.rotateindex = i % paramblock.rotationstepnumber;

	    if ( paramblock.b_revert_rotation )
            paramblock.rotateindex = (paramblock.rotationstepnumber - paramblock.rotateindex) - 1; // should revert the sequence 

        Vector3 v = new Vector3(0,0,0);
        v = paramblock.rotatelookpoint.Getlookatpoint(paramblock.rotateindex, 1.0f , paramblock.rotationstepnumber);
        var direction =  v + transform.position ;
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

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v), (paramblock.rotationspeed * Time.deltaTime));
    }

}
