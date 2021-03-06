﻿using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;





[System.Serializable]
[XmlInclude(typeof(BaseActorProperties))]
public class scenecluster
{
    public string name;
    public Vector3 rootnodepos;
    public List<BaseActorProperties> baseassetproplist = new List<BaseActorProperties>();
    //public List<Dataset>  datasetlist = new List<Dataset>();
}



[System.Serializable]
[XmlInclude(typeof(scenecluster))]
public class BBScene
{
    //public Scene scene = new Scene();

    public string name;
    public scenecluster cluster = new scenecluster();
    public BBScene()
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