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
public enum PLTF_TYPE
{
    STATIC = 0,
    ROTATING = 1,
    FALLING = 2,
    MOVING = 3
}

public static class behaviorManager
{
    // this class is a Helper to manage Actor Behaviors 

    public class BehaviorHandle 
    {
        public System.Type T;
        public Behavior m_behavior;
        public string m_behaviorclassname ;
        public GUI guid;

        public GUI GetGuid ()
        {
            return guid;
        }

    }

    private static List<BehaviorHandle> m_registeredbehaviors;

    static bool Allreadyregistered(BehaviorHandle BS)
    {
        foreach (BehaviorHandle item in m_registeredbehaviors)
        {
            if ( BS  == item )
                return true;
        }
        return false;
    }


    static public void RegisterBehavior(BehaviorHandle BS)
    {
        // allready here 
        if (Allreadyregistered(BS))
            return;

        m_registeredbehaviors.Add(BS);
    }

    static public void UnRegisterBehavior(BehaviorHandle BS)
    {
        if (Allreadyregistered(BS))
            m_registeredbehaviors.Remove(BS);
    }



}


public class Behavior  : MonoBehaviour
{
    // should implement only shared props for all behaviors 
    public GameObject looktarget;
    public GameObject parent;
    public float editortick =0.02f;
    public ReplayerLogOutput m_replayerlogoutput;
    public Dataset paramblock;
    public Transform block_transform;
    public GameObject triggerobject;
    public Vector3 pointSnap = Vector3.one * 0.001f;    

    public Behavior()
    { 
        
    }

    
    public virtual  void OnDrawGizmosSelected()
    {
        Debug.Log("not implemented at this level cast an appropriated behavior class");
    }

    public virtual void  Awake ()
    {
        //paramblock = new Dataset();
    }




    public virtual Dataset Load(string path, System.Type type)
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
	public virtual  void Start () 
    {
        if (paramblock.ismoving)
        {
            //rp = (ReplayerLogOutput)gameObject.AddComponent(typeof(ReplayerLogOutput));
            //rp.m_entityname = this.name;*/
        }

        if (!block_transform)
            return;
        Actor A = (Actor)GetComponent(typeof(Actor));
        A.Actorprops.orig_transform = block_transform.rotation;
        A.Actorprops.orig_pos = block_transform.position;
	}




    public virtual void  Wait(float tempo)
    {
        var a = new WaitForSeconds(tempo);
        WaitForSeconds s = new WaitForSeconds(tempo);
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
            Pathnode p0 = paramblock.m_pathnodes[i];
            Pathnode p1 = paramblock.GetPathNode(i - 1);

            if (p0 == null)
                return;



            //Quaternion rotation = Quaternion.Euler(0f, 0f, angle * i);
            Vector3 oldPoint = p0.pos;
            //Handles.FreeMoveHandle(oldPoint, Quaternion.identity, 0.2f, paramblock.pathnodes[i].pos, Handles.DotCap);
            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, Quaternion.identity, 0.1f, pointSnap, Handles.DotCap);
            if (oldPoint != newPoint)
                p0.pos = newPoint;
            Handles.color = Color.blue;
            Actor AP = (Actor) GetComponent(typeof(Actor));

            Handles.Label(transform.position + Vector3.up,
                    transform.transform.position.ToString() + "\nName: " + AP.Actorprops.assetname );

            //float width = (float)HandleUtility.GetHandleSize(oldPoint) * 0.5f;
            if (i > 0)
                Handles.DrawLine(p0.pos, p1.pos);
            //Handles.DrawBezier(transform.transform.position, oldPoint, oldPoint,-oldPoint,Color.red,null,width);
            //Handles.FreeRotateHandle(Quaternion.identity, paramblock.pathnodes[i].pos, 0.2f);
        }
    }



#endif

// Update is called once per frame
	public virtual  void Update () 
    {
        Actor A = (Actor)GetComponent(typeof(Actor));

		switch (A.Actorprops.pltf_sate) 
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

                int pmax = paramblock.m_pathnodes.Count;
                if (pmax < 2) // need 2 point for a move at least 
                    break;
                if (paramblock.targetindex > pmax)
                    break; // something went wront in target definition 

                Pathnode p = paramblock.GetPathNode ( paramblock.targetindex) ;
                
                if (paramblock.targetindex >=  pmax || paramblock.editsub)
                    break;
                if (paramblock.targetindex >= pmax - 1)
                {
                    if (paramblock.b_pathloop)
                        if ((Vector3.Distance(transform.position, p.pos) == 0.0f))
                        {
                            paramblock.targetindex = 0;
                            paramblock.movedir = (1);
                            // cannot pop at exact pos 
                            Vector3 offs =  Vector3.forward/100; // slight offset 
                            transform.position = p.pos+offs;
                        }
                    paramblock.movedir = (-1);
                }
                else if (paramblock.targetindex <= 0 )
                    paramblock.movedir = (1);
                Vector3 target = p.pos;
                if (Vector3.Distance(transform.position, target) == 0.0f)
                {
                    if (p.timer > 0)
                    #if UNITY_EDITOR   
                    p.timer -= editortick ;
                    #endif   
                    #if !UNITY_EDITOR   
                        paramblock.pathnodes[paramblock.targetindex].timer -= Time.deltaTime;
                    #endif
                    else
                    {
                        p.timer = p.waitonnode;
                        paramblock.targetindex += paramblock.movedir;
                    }
                }
                if (paramblock.ismoving)//|| (Vector3.Distance(transform.position, paramblock.pathnodes[0].pos) > 0.0f))
                {
                    int ti = paramblock.targetindex;
                    int lkp =p.ilookatpoint;
                    //Debug.Log(lkp);
                    float rspeed, tspeed;
                    Vector3 targetpos = p.Getlookatpoint ( lkp , 1.0f) ;
                    // ude local speed for TR or global 
                    if (paramblock.move_ampl == 0.0f)
                    {
                        rspeed = p.lookatspeed;
                        tspeed = p.translatespeed;
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
