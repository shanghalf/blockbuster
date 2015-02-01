

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



public class ReplayerLogOutput : MonoBehaviour
{
    public XmlDocument playerposlog = new XmlDocument();
    public string m_mapname;
    public int m_loginternval = 10;
    public string m_entityname;

    public void ReplayOutputTag(string key, string logevent)
    {

        return;

    }
    public void Start()
    {

        // Setup components


        m_mapname = Application.loadedLevelName;
        XmlNode ROOT = playerposlog.CreateElement("ROOT");
        playerposlog.AppendChild(ROOT);
    }

    public void Update()
    {
        int frame = Time.frameCount;
        if (frame % m_loginternval == 0)
            OutputPlayerTrack(frame);
    }


    public void OutputPlayerTrack(int frame)
    {
        //DebugUtils.Log(Core.LogCategory.Stats, "output playerinfo log tag ");
        string elstr = frame.ToString();
        XmlElement tag = playerposlog.CreateElement("FRAME_" + elstr);
        string data = transform.position.ToString();
        data += "_" + transform.rotation;

        XmlText pos = playerposlog.CreateTextNode(data);
        tag.AppendChild(pos);
        playerposlog.FirstChild.AppendChild(tag);
    }

    protected void OnDestroy()
    {

        string outPath = string.Format("{0}/PLATFORM/XML/Replays/{1}{2}{3}.xml", Application.dataPath, m_mapname, System.DateTime.Now.ToString().Replace('/', '_').Replace(' ', '-').Replace(':', '_'), m_entityname);
        //DebugUtils.Log(Core.LogCategory.Stats, (string.Format("save player log :{0} ", outPath)));
        playerposlog.Save(outPath);
        //DebugUtils.Log(Core.LogCategory.Stats, "save player log ");
    }

}