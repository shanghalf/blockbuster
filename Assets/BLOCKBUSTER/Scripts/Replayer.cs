
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
public class ReplayPathnode
{
    public Vector3 pos;
    public Quaternion rot;
    public int ilookatpoint;
    //public List<Vector3> orb = new List<Vector3>() ;
    public float lookatspeed;
    public float translatespeed;
    public ReplayPathnode()
    {
    }
    public Vector3 Getlookatpoint(int lookatindex, float radius)
    {
        float a = ((360 / 8) * Mathf.Deg2Rad) * lookatindex;
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.1f, radius * sa + radius * ca);
        return (RV);//+ pos) ;
    }
}

[System.Serializable]
public class Replay
{
    public Vector3 m_last_pos;
    public List<ReplayPathnode> m_replayPathnodes = new List<ReplayPathnode>();
    public float m_replayspeed = 0.5f;
    public Vector3 m_v3target;
    public bool m_bismoving = false;
    public int m_itargetindex = 0;
    public int m_imovedir = 1;
    public bool m_bpathloop;
    public string m_strplayername;
    public XmlDocument m_replayxmldoc = new XmlDocument();
    public Color m_color;
    public string m_xmlfilename;

    public Replay(string filename)
    {
        m_xmlfilename = filename;
        m_replayxmldoc.Load(filename);
        System.Random rnd = new System.Random(); ;
        Color rndcolor = new Color(1.0F / 255 * rnd.Next(0, 255), 1.0F / 255 * rnd.Next(0, 255), 1.0F / 255 * rnd.Next(0, 255));
        m_color = rndcolor;
    }

}

#if UNITY_EDITOR


[CustomEditor(typeof(RePlayer)), CanEditMultipleObjects]
public class Replayinspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_replayfiletag"));
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("replayspeed"));
        EditorGUILayout.Slider(serializedObject.FindProperty("replayspeed"), 0.0f, 5.0f);

        serializedObject.ApplyModifiedProperties();

        Repaint();

    }
}

#endif





[ExecuteInEditMode()]
public class RePlayer : MonoBehaviour
{

    //public Replay m_replay = new Replay();
    public string m_replayfiletag;
    public float replayspeed = 0.05f;

    public XmlDocument m_doc = new XmlDocument();
    public string m_loadedfile = "";
    private static Vector3 pointSnap = Vector3.one * 0.001f;
    public List<Replay> replaylist = new List<Replay>();
    // Use this for initialization
    public int targetindex = 0;
    public Replay m_playerreplay;
    public bool bviewportcamfollowing = false;

#if UNITY_EDITOR
    void Start()
    {
        RefreshXmlBase();
    }

    public void RefreshXmlBase()
    {

        replaylist.Clear();
        var sfolder = Application.dataPath + "/BLOCKBUSTER/XML/Replays";
        string[] files = Directory.GetFiles(sfolder, "*.xml");
        string[] path = EditorApplication.currentScene.Split(char.Parse("/"));
        string currenteditormapname = path[path.Length - 1].Split(char.Parse("."))[0];
        foreach (string xmlname in files)
        {
            string[] mapname = xmlname.Split(char.Parse("\\"));
            if (mapname[mapname.Length - 1].Contains(currenteditormapname))
            {
                Replay R = new Replay(xmlname);
                if (xmlname.Contains("PM.xml") | xmlname.Contains("AM.xml"))
                    m_playerreplay = R;

                ReadPathNodes(R);
                replaylist.Add(R);
            }
        }
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnCustomSceneGUI;
    }



    void ReadPathNodes(Replay r)
    {
        foreach (XmlNode i in r.m_replayxmldoc.ChildNodes[0].ChildNodes)
        {
            string[] tokens = i.InnerText.Split(new char[] { ',', '(', ')', '_' });
            ReplayPathnode pn = new ReplayPathnode();
            pn.pos.x = float.Parse(tokens[1]);
            pn.pos.y = float.Parse(tokens[2]);
            pn.pos.z = float.Parse(tokens[3]);
            r.m_replayPathnodes.Add(pn);
            pn.rot = new Quaternion(float.Parse(tokens[6]), float.Parse(tokens[7]), float.Parse(tokens[8]), float.Parse(tokens[9]));

            //DebugUtils.Log(Core.LogCategory.Default, i.InnerText );
        }
    }
#endif
    // Update is called once per frame
    public void Update()
    {


        if (m_playerreplay == null)
            return;

        if (m_playerreplay.m_replayPathnodes.Count <= targetindex)
            return;

        Vector3 target = m_playerreplay.m_replayPathnodes[targetindex].pos;
        Quaternion q = m_playerreplay.m_replayPathnodes[targetindex].rot;

        transform.rotation = Quaternion.Slerp(transform.rotation, q, replayspeed * 2.0f);

        //transform.rotation = q;
        float d = Vector3.Distance(transform.position, target);


        if (d < 0.1f)
        {

            targetindex += 1;
            targetindex = targetindex % m_playerreplay.m_replayPathnodes.Count;


        }
        else if (d > 5.0f)
            transform.position = target; // teleport > 5.0

        //transform.position = target;
        transform.position = Vector3.MoveTowards(transform.position, target, replayspeed);

#if UNITY_EDITOR
        if (bviewportcamfollowing)
        {

            var cview = SceneView.currentDrawingSceneView;
            if (cview != null)
                cview.pivot = transform.position;
        }
#endif

    }

#if UNITY_EDITOR
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnCustomSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnCustomSceneGUI;
    }
#endif

#if UNITY_EDITOR

    void DrawReplay(Replay R)
    {

        bool bypass = false;
        if (m_replayfiletag.Length > 0)
            if (!R.m_xmlfilename.Contains(m_replayfiletag))
                bypass = true;
        if (bypass)
            return;
        for (int i = 1; i < R.m_replayPathnodes.Count; i++)
        {
            Handles.color = R.m_color;
            Handles.Label(R.m_replayPathnodes[0].pos + Vector3.up, R.m_xmlfilename.Split(char.Parse("-"))[3]);
            if (Vector3.Distance(R.m_replayPathnodes[i].pos, R.m_replayPathnodes[i - 1].pos) < 5.0f) // dont display teleport
                Handles.DrawLine(R.m_replayPathnodes[i].pos, R.m_replayPathnodes[i - 1].pos);
        }

    }

    void OnCustomSceneGUI(SceneView sceneview)
    {
        foreach (Replay RI in replaylist)
        {
            DrawReplay(RI);
        }
    }

#endif


}



