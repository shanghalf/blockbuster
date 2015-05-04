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
public class RotatingPlatformDataset : Dataset
{

    public Pathnode rotatelookpoint = new Pathnode();
    public float speed = 0.5f;
    public float move_ampl=0.0f;
    public Vector3 target = Vector3.zero;
    public bool b_revert_rotation=false;
    public float rotationspeed=0.0f;
    public int rotationstepnumber=2;
    public float rotationtempo=0.0f;
    public int rotateindex=0;
    public List<Pathnode> m_pathnodes = new List<Pathnode>();   // pathnodes array for any path 


    public object GetPathNode(int index)
    {
        // protect the access of pathnodelist 
        int correctindex = Mathf.Clamp(index, 0, m_pathnodes.Count - 1);
        return (object)m_pathnodes[correctindex];

    }

    public override List<Pathnode> GetPathNodes()
    {
        return m_pathnodes;
    }

    public override  Dataset Load(string path)
    {
        if (!System.IO.File.Exists(path))
            return null;
        XmlSerializer serializer = new XmlSerializer(typeof(RotatingPlatformDataset));
        Stream stream = new FileStream(path, FileMode.Open);
        RotatingPlatformDataset result = serializer.Deserialize(stream) as RotatingPlatformDataset;
        stream.Close();
        return result;
    }

}






//[ExecuteInEditMode()]
public class RotatingPlatform : BBehavior
{
    // should implement only shared props for all behaviors 
    public RotatingPlatformDataset paramblock = new RotatingPlatformDataset();

    [BBCtrlVisible] // define a function visible for BBControl 
    public override Dataset GetDataset()
    {
        return paramblock;
    }

    [BBCtrlVisible] // define a function visible for BBControl 
    public override void SetDataset(Dataset o )  
    {
        
        paramblock = (RotatingPlatformDataset)o;
    }
  

    public override void DoGUILoop(Rect mainwindow)
    {
        paramblock.b_hideedition = EditorGUILayout.Toggle("show rotation", paramblock.b_hideedition);
        paramblock.rotationstepnumber = (int)EditorGUILayout.Slider("step", paramblock.rotationstepnumber, 2, 8);
        paramblock.rotationspeed = EditorGUILayout.Slider("speed", paramblock.rotationspeed, 0.0f, 5.0f);
        paramblock.rotationtempo = EditorGUILayout.Slider("temporisation", paramblock.rotationtempo, 0.0f, 2.0f);
        if (paramblock.editsub)
            paramblock.b_revert_rotation = EditorGUILayout.Toggle("invert", paramblock.b_revert_rotation);
        else
            paramblock.ismoving = false;
    }









    public override  void OnDrawGizmosSelected()
    {

        // replaced by scene handle callback 
        /*
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
        */

    }


    
  

	// Use this for initialization
	public override void Start () 
    {
        MovingPlatform D = (MovingPlatform)GetComponent(typeof(MovingPlatform));
        if (D != null) // desactivate the path direction 
            D.paramblock.b_path_rotation = false;

	}


#if UNITY_EDITOR


    public  override  void OnCustomSceneGUI(SceneView sceneview)
    {
        //float angle = -360f / (5);

        if (paramblock == null || paramblock.b_hideedition == false)
            return;

        for (int c = 0; c <= paramblock.rotationstepnumber; c++)
        {
            Handles.color = Color.yellow;
            Handles.FreeRotateHandle(Quaternion.identity, paramblock.rotatelookpoint.Getlookatpoint(paramblock.rotateindex, 1.0f, paramblock.rotationstepnumber) + transform.position, 0.2f);
            Handles.color = Color.green;
            Handles.FreeRotateHandle(Quaternion.identity, paramblock.rotatelookpoint.Getlookatpoint(c, 1.0f, paramblock.rotationstepnumber) + transform.position, 0.1f);
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
        //var direction = v + transform.position;
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

