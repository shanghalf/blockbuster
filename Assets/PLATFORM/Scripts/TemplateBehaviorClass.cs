using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
// for editor update
#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class TemplateCustomPathnode : Pathnode
{
    public Vector3 pos;
    public float timer = 0.0f;
    /// <summary>
    /// custom constructor 
    /// </summary>
    public TemplateCustomPathnode()
    {
        // init here 
    }
    /// <summary>
    /// function to manipulate this specific data set 
    /// </summary>
    /// <param name="i"></param>
    /// <param name="f"></param>
    public override void DataSetFunc(int i, float f)
    {
        // do something else 
    }
}


[System.Serializable]
public class TemplateCustomDataset : Dataset
{
    // this class represent the specific dataset required to manipulate this behavior
    // it should include both NonSerialized data for UI implementation and RunTime Data for the Behavior execution
    [System.NonSerialized] 
    static float UIscrollup; // used for BLOCKBUSTER GUI 
    public float scrollup = UIscrollup; // the runtime value 

}




[ExecuteInEditMode()] // same no exec in editor might be done here 
public class TemplateBehaviorClass : Behavior
{
    // derived from dataset this is the overrided custom data set 
    public FallingPlatformDataset paramblock = new FallingPlatformDataset();
    public TemplateBehaviorClass() { } // public constructor 



    /// <summary>
    /// Draw custom Gizmo 
    /// </summary>
    public override void OnDrawGizmosSelected()
    {
        /// Gizmos.color = Color.yellow ; ... etc ..
    }

    /// <summary>
    /// used for data serialization 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Dataset Load(string path, System.Type type)
    {
        if (!System.IO.File.Exists(path))
            return null;
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Open);
        // do not forget to cast the result TemplateCustomDataset in this template
        Dataset result = serializer.Deserialize(stream) as TemplateCustomDataset;
        stream.Close();
        return result;
    }

    /// <summary>
    /// perform class init 
    /// </summary>
    public override void Start() { }

// Custom View Callback for Behavior edition
#if UNITY_EDITOR

    /// <summary>
    /// register and unregister the scene gui function
    /// </summary>
    private void OnEnable() { SceneView.onSceneGUIDelegate += OnCustomSceneGUI; }
    private void OnDisable() { SceneView.onSceneGUIDelegate -= OnCustomSceneGUI; }

    /// <summary>
    /// the Viewport Display callback
    /// </summary>
    /// <param name="sceneview"></param>
    protected void OnCustomSceneGUI(SceneView sceneview)
    {
        if (paramblock == null)
            return;

        for (int i = 0; i < paramblock.m_pathnodes.Count - 1; i++)
        {
            Pathnode p1 = paramblock.m_pathnodes[i];
            if (p1 == null)
                return;
            Handles.color = Color.blue;
            Actor AP = (Actor)GetComponent(typeof(Actor));
            Handles.Label(transform.position + Vector3.up,
                    transform.transform.position.ToString() + "\nName: " + AP.Actorprops.assetname);
        }
    }
#endif

    /// <summary>
    /// Update function called each frame in both editor view and runtime update 
    /// </summary>
    public override void Update()
    {
        // the action is called diferently according to the context in editor theres no Time.delta 
# if ! UNITY_EDITOR
        // mainly for * Time.deltaTime
# endif
# if  UNITY_EDITOR
        // can use the editortick for update defined in base class Behavior 
        transform.position += (Vector3.up * editortick);
#endif

    }




}

