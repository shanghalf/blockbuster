


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
    public float timer = 0.0f;

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
    //public System.Type BHVTYPE;
    public bool grouped;



    public void Save(string path, System.Type type)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
        
        //debug.Log ( serializer.ToString());
    }


}
 
// definitely need a way to acess safely an array 



public   class Dataset 
{
    public bool b_front_x;
    public float timer=0.0f;
    public bool editsub = true;
    public bool b_showdebuginfos;
    static  int submenu=4;
    public bool ismoving = false;
    public ARRAY_BOUND indexbound;
    public string actorguid = "";
    public static bool  menubaritem()
    {
        return GUILayout.Button("Dataset", EditorStyles.toolbarButton) ;
    }

    public string GetGuid()
    {
        return actorguid;
    }
}








