


using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using UnityEditor;


[System.Serializable]
public enum COLIDER_TYPE
{
    BOX = 0,
    SPHERE = 1,
    CAPSULE = 2,
    MESH = 3
}

[System.Serializable]
public enum ARRAY_BOUND
{
    UP= 2,
    MIDDLE= 0,
    DOWN = -1,
    OUT= 666
}




[System.Serializable]
public class Pathnode
{

    public Vector3 pos ;
    public int ilookatpoint=0;
    //public List<Vector3> orb = new List<Vector3>() ;
    public float lookatspeed =  0.0f;
    public float translatespeed = 0.0f;
    public float waitonnode = 0.0f;
    public float timer = 0.0f;

    public Pathnode()
    {
        
    }

    public virtual  void DataSetFunc(int i, float f)
    {
        // do something else 
    }

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
public class BaseActorProperties
{
    public string parentgui;
    public string guid;
    public string assetname;
    public Quaternion orig_transform = Quaternion.identity;
    public Vector3 orig_pos;
    public Vector3 last_pos;
    public Vector3 block_size;
    public PLTF_TYPE pltf_sate;
    public bool grouped;

}
 
// definitely need a way to acess safely an array 



[System.Serializable]
public class Dataset 
{
    [System.NonSerialized]
    public float timer=0.0f;
    public  List<Pathnode> m_pathnodes = new List<Pathnode>();
    public int ilookatpoint=0;
    public float speed = 0.5f;
    public float move_ampl=0.0f;
    public bool editsub = true;
    public Vector3 target = Vector3.zero;
    public bool ismoving = false;
    public bool b_revert_rotation=false;
    public bool b_triggeronce=false;
    public float rotationspeed=0.0f;
    public int rotationstepnumber=2;
    public float rotationtempo=0.0f;
    public bool b_triggered=false;
    public int rotateindex=0;
    private  int targetindex = 0;
    public int movedir = 1;
    public int maxhandle=0;
    public Pathnode rotatelookpoint = new Pathnode();
    public Vector3[] quater = new Vector3[] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };
    public bool b_pathloop =false;
    public COLIDER_TYPE colider_type = COLIDER_TYPE.BOX;
    public bool b_showdebuginfos;

    
    [System.NonSerialized]
    static  int submenu=4;
    [System.NonSerialized]
    public ARRAY_BOUND indexbound;




    //public int indexbound;


    public static bool  menubaritem()
    {
        return GUILayout.Button("Dataset", EditorStyles.toolbarButton) ;
    }
    
    public virtual Pathnode GetPathNode( int index )
    {
        // protect the access of pathnodelist 
        int correctindex = Mathf.Clamp(index, 0, m_pathnodes.Count - 1);
        return m_pathnodes[correctindex];

    }

    public int getpathnodelenght()
    {
        return m_pathnodes.Count;
    }

    public int GetSafeTargetIndex ( )
    {
        if (m_pathnodes.Count == 0)
            return -666;
        int correctindex = Mathf.Clamp(targetindex, 0, m_pathnodes.Count - 1);
        targetindex = correctindex;

        // array boundaries  
        if (targetindex == m_pathnodes.Count - 1)
        {
            indexbound = ARRAY_BOUND.UP;
            movedir = -1;
        }
        else if (targetindex == 0)
        {
            indexbound = ARRAY_BOUND.DOWN;
            movedir = 1;
        }
        else
            indexbound = ARRAY_BOUND.MIDDLE;

        return (correctindex > 0) ? correctindex : 0;
    }

    public bool  SetSafeTargetIndex(int index)
    {
        if (m_pathnodes.Count == 0)
        {
            Debug.Log("no pathnodes target to set");
            return false;
        }
        int correctindex = Mathf.Clamp(index , 0, m_pathnodes.Count - 1);
        targetindex = correctindex;

        // array boundaries  
        if (targetindex == m_pathnodes.Count - 1)
        {
            indexbound = ARRAY_BOUND.UP;
            movedir = -1;
        }
        else if (targetindex == 0)
        {
            indexbound = ARRAY_BOUND.DOWN;
            movedir = 1;
        }
        else
            indexbound = ARRAY_BOUND.MIDDLE;

        return true;
    }



    public List<Pathnode> GetPathNodes()
    {
        return m_pathnodes;
    }


    public void RemoveLastPathNode()
    {
        m_pathnodes.Remove(m_pathnodes[m_pathnodes.Count - 1]);
    }



    public void RemovePathNodeAt(int index)
    {
        int local_int = Mathf.Clamp(index, 1, m_pathnodes.Count);
        m_pathnodes.RemoveAt(local_int-1);

    }

   





}








