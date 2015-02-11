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
public class Behavior  : MonoBehaviour
{
    // should implement only shared props for all behaviors 
    public GameObject looktarget;
    public GameObject parent;
    public float editortick =0.02f;
    public ReplayerLogOutput m_replayerlogoutput;
    public Dataset paramblock = new Dataset();
    public Transform block_transform;
    public GameObject triggerobject;
    public Vector3 pointSnap = Vector3.one * 0.001f;    

    /// <summary>
    /// common init for behavior 
    /// </summary>
    public Behavior()
    { 
        
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
        paramblock = new Dataset();
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
        Actor A = (Actor)GetComponent(typeof(Actor));
        Pathnode p ;

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

                 
                p = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];

                if (paramblock.b_pathloop)
                    if ((Vector3.Distance(transform.position, p.pos) == 0.0f))
                    {
                        paramblock.SetSafeTargetIndex (0);
                        paramblock.movedir = (1);
                        // cannot pop at exact pos 
                        Vector3 offs =  Vector3.forward/100; // slight offset 
                        transform.position = p.pos+offs;
                    }
                    
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
                        paramblock.SetSafeTargetIndex(paramblock.GetSafeTargetIndex() + paramblock.movedir);
                    }
                }
                if (paramblock.ismoving)//|| (Vector3.Distance(transform.position, paramblock.pathnodes[0].pos) > 0.0f))
                {
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
