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


// todo .. do not use this class directly ( i will ) 

[ExecuteInEditMode()] // same no exec in editor might be done here 
public class RotatingPlatform  : Behavior
{
    // should implement only shared props for all behaviors 


    public RotatingPlatform()
    { 
        
    }

    
    public virtual  void OnDrawGizmosSelected()
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
        paramblock = new Dataset();

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

        for (int i = 0; i < paramblock.m_pathnodes.Count; i++)
        {
            Pathnode p1 = paramblock.GetPathNode(i);

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

