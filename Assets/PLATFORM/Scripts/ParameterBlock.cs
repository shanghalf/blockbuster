


using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;



[System.Serializable]
public enum COLIDER_TYPE
{
    BOX = 0,
    SPHERE = 1,
    CAPSULE = 2,
    MESH = 3
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


    public Vector3 Getlookatpoint(int lookatindex, float radius, int step = 8)
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




[System.Serializable]
public class Dataset 
{

    public float timer;
    public List<Pathnode> m_pathnodes = new List<Pathnode>();
    public int ilookatpoint;
    public float speed = 0.5f;
    public float testfloat;
    public float move_ampl;
    public bool editsub = true;
    public Vector3 target;
    public bool ismoving = false;
    public bool b_rotate_X = false;
    public bool b_rotate_Y = false;
    public bool b_rotate_Z = false;
    public bool b_followpath;
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
    public Vector3[] quater = new Vector3[] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };
    public bool b_pathloop;
    public COLIDER_TYPE colider_type;

    public Dataset()
    {

    }

    public List<Pathnode> GetPathNodes()
    {
        return m_pathnodes;
    }
    public Pathnode GetPathNode(int index)
    {
        if (m_pathnodes.Count > index && index > -1)
            return m_pathnodes[index];
        else
            return null;
    }
    public  void AddPathNode(Pathnode o)
    {
        m_pathnodes.Add(o);
    }
    public void RemoveLastPathNode()
    {
        m_pathnodes.Remove(m_pathnodes[m_pathnodes.Count - 1]);
    }

    public void RemovePathNodeAt(int index)
    {
        m_pathnodes.RemoveAt(index);
    }




}








