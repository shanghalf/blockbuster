using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class BBGameObjectHandle
{
    public BBGameObjectHandle()
    {
    }

    [XmlIgnore]
    public GameObject go ;

    public string Name;
    public bool bfromscene;

    public string assetpath;

    public void GetonName ()
    {
       

    }
    


}





/// <summary>
/// BBUInodes store the GUI controll based nodes 
/// those nodes can only have a single parameter 
/// this parameter is passed to the graph execution and 
/// value is stored in a dictionnary 
/// ann kind of GUI are not managed yet 
/// sliders ok int and float right now 
/// 
/// </summary>
public class BBUInodes
{
    // nodes are called both in runtime game mode and from editor 
    // this is a editor mode switch ;
    
    public static float F;
    public static int I;
    public Rect Rwindow;


    [BBCtrlProp]
    [BBCtrlVisible]
    public  float  floatSliderNode(float F)
    {
        F = EditorGUILayout.Slider(F, 0, 10.0f);
        return F;
    }

    [BBCtrlProp]
    [BBCtrlVisible]
    public float clampslider(float F)
    {
        F = EditorGUILayout.Slider(F, 0, 1f);
        return F;
    }



    [BBCtrlProp]
    [BBCtrlVisible]
    public int IntSliderNode(int I)
    {
        //I=EditorGUI.IntSlider(new Rect(10, 80, 80, 30), I, 0, 100);
        I = EditorGUILayout.IntSlider(I, 0, 100);
        return I;
    }

    [BBCtrlProp]
    [BBCtrlVisible]
    public TXTINDEX EnumpopupNode (TXTINDEX t )
    {
        t = (TXTINDEX)EditorGUILayout.EnumPopup( t); 
        return t;
    }

    [BBCtrlProp]
    [BBCtrlVisible]
    public int popupNode(int t)
    {
        // use public static string list 
        List<string> L = new List<string>();
        L.Add("this");
        L.Add("is");
        L.Add("a");
        L.Add("string");
        L.Add("test");
        t = EditorGUILayout.Popup(t,L.ToArray());
        //new Rect(10, 80, 80, 30)
        //t = (TXTINDEX)EditorGUILayout.EnumPopup(c, t );
        return t;
    }

    [BBCtrlProp(true)] // this controll do not hold the return value and need invoke get the Gameobject 
    [BBCtrlVisible]
    public BBGameObjectHandle ObjectFieldNode(BBGameObjectHandle o)
    {
        // game is running stick with 
        // selected value 
        if (NodeGraph.gamemode)
            return o;


        Texture2D prev;
        if (o.go == null && o.assetpath != null)
        o.go = (GameObject)Resources.LoadAssetAtPath(o.assetpath, typeof(GameObject));
        prev = AssetPreview.GetAssetPreview(o.go);
        if (prev != null) 
                GUI.DrawTexture(new Rect(45, 80, Rwindow.width - 90, Rwindow.height - 90), prev);
        o.go = (GameObject)EditorGUILayout.ObjectField(o.go, typeof(GameObject), true);
        o.Name = o.go.name;
        o.assetpath = AssetDatabase.GetAssetPath(o.go);
        return o;

    }


    [BBCtrlProp]
    [BBCtrlVisible]
    public bool ToggleNode (bool b)
    {
        b = EditorGUILayout.Toggle (b);
        return b;
    }

    [BBCtrlProp]
    [BBCtrlVisible]
    public Vector3 Vector3Node(Vector3 v)
    {
        v= EditorGUILayout.Vector3Field("",   v);
        return v;
    }

    [BBCtrlProp]
    [BBCtrlVisible]
    public Vector3 Vector2Node(Vector3 v)
    {
        v = EditorGUILayout.Vector2Field("", v);
        return v;
    }
    [BBCtrlProp]
    [BBCtrlVisible]
    public AnimationCurve  AnimcurveNode(AnimationCurve c)
    {
        c = EditorGUILayout.CurveField(c);
        return c;
    }





}

    /// <summary>
    /// maths Blocks 
    /// </summary>
    public class BBMathnodes
    {
        // vector3 invert
        [BBCtrlVisible] // define a function visible for BBControl 
        public  Vector3 invertVector(Vector3 VIN)
        {
            return -VIN;
        }
        // vector3 addition 
        [BBCtrlVisible] // define a function visible for BBControl 
        public  Vector3 BBMathsAddVector(Vector3 a, Vector3 b)
        {
            return a + b;
        }
        // integer addition 
        [BBCtrlVisible] // define a function visible for BBControl 
        public  int BBMathsAddint(int a, int b)
        {
            Debug.Log((a + b).ToString());
            return a + b;
        }
        // Vector3 multiply 
        [BBCtrlVisible] // define a function visible for BBControl 
        public  Vector3 Multiplyvector(Vector3 vin, float by)
        {
            return vin * by;
        }
        // compose a vector with 3 float
        [BBCtrlVisible] // define a function visible for BBControl 
        public Vector3 ComposeVector(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public float FloatMultiply(float a, float b)
        {
            return a * b;
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public float FloatAdd(float a, float b)
        {
            return a + b;
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public float FloatDivide(float a, float b=1)
        {
            return a / b;
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public int Mod(int max, int by)
        {
            return max % by;
        }
        [BBCtrlVisible]
        public int FloatToInt(float floattoconvert)
        {
            return (int)floattoconvert;
        }
        [BBCtrlVisible]
        public float IntToFloat(int inttoconvert)
        {
            return (float)inttoconvert;
        }



        


    }


    /// <summary>
    /// misc Blocks for diferent purpose 
    /// can use it before sorting to another class to  get tiddy 
    /// </summary>
    public class BBBLocks
    {

        public static int assetbaseindex;
        public static string selectedbasename = BBDir.Get(BBpath.BBGBASE) + "HIGHTECH/";
        public static List<string> data = new List<string>();
  

        /// <summary>
        /// constructor
        /// </summary>
        public BBBLocks()
        { 
        }

        // compose a vector with 3 float
        [BBCtrlVisible] // define a function visible for BBControl 
        public float  GetCurveValueAtTime(AnimationCurve C, float time)
        {
            return C.Evaluate(time);
        }

        // compose a vector with 3 float
        [BBCtrlVisible] // define a function visible for BBControl 
        public bool  CombineGraph (bool graphA , bool graphB )
        {
            return true;
        }
        




        /// <summary>
        /// delete selected object 
        /// </summary>
        [BBCtrlVisible]
        public  void DeleteSelection()
        {
            foreach (GameObject tgo in Selection.objects)
                GameObject.DestroyImmediate(tgo);
        }
        /// realtimeSinceStartup
        [BBCtrlVisible]
        public float TimeNode()
        {
           return (float) Time.realtimeSinceStartup;
        }
        /// temporary 
        [BBCtrlVisible] // define a function visible for BBControl 
        public  int increment()
        {
            return 1;
        }
        // temp
        [BBCtrlVisible] // define a function visible for BBControl 
        public  int decrement()
        {
            return -1;
        }
        // gettype
        [BBCtrlVisible] // define a function visible for BBControl 
        public  Type GetType (object value)
        {
            return value.GetType();
        }

        // read content of the selected base 
        public static bool ReadAssetBase()
        {

            if (data.Count > 0 )
                return true;  // read once 
            String filepath =  selectedbasename + "AssetList.xml";
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(filepath))
            {
                xmlDoc.Load(filepath);
                if (xmlDoc == null)
                    return false;
                XmlNodeList Asset_list = xmlDoc.GetElementsByTagName("AssetsList");
                if (Asset_list == null)
                    return false;
                XmlNodeList Item_list = Asset_list.Item(0).ChildNodes;
    
                for (int i = 0; i < Item_list.Count; i++)
                {
                    XmlNodeList l = Item_list.Item(i).ChildNodes;
                    if (data != null)
                        data.Add(l[0].InnerText);
                }
                return true;
            }
            else
                Debug.Log(data.Count.ToString() + " no XML database");
            return false;
        }



        [BBCtrlVisible] // define a function visible for BBControl 
        public int iterator (int max , int current)
        {
            return max % current;
        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public int currentframe ()
        {
            return Time.frameCount;
        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public float Deltatime()
        {
            return Time.deltaTime;
        }
        // getlookatpoint
        [BBCtrlVisible] // define a function visible for BBControl 
        public static Vector3 Getlookatpoint(int lookatindex, float deltatime, int max=8)
        {
            float a = ((360.0f / max) * Mathf.Deg2Rad) *  (lookatindex * deltatime ) ;
            float ca = Mathf.Cos(a);
            float sa = Mathf.Sin(a);
            // radius 1
            Vector3 RV = new Vector3(1 * ca - 1 * sa, 0.0f, 1 * sa + 1 * ca);
            return (RV);//+ pos) ;
        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public Quaternion Lookat (GameObject obj,Vector3 lookatpoint, float clamp )
        {
            //var direction = v + transform.position;
            var rr = Quaternion.LookRotation(Vector3.up, lookatpoint);
            rr *= Quaternion.Euler(Vector3.forward);
            return  Quaternion.Lerp(obj.transform.rotation, rr, clamp);

        }
        [BBCtrlVisible]
        public Quaternion SetRotationEulerAngles(float rotX, float rotY, float rotZ)
        {
            Quaternion Q = Quaternion.identity;
            Q.eulerAngles.Set(rotX, rotY, rotZ);
            return Q;
        }



   




        [BBCtrlVisible] 
        public static void MainLayerInstanciate()
        {
            BBMovepad.Mainlayer.Load(BBDir.Get(BBpath.SETING) + "movepadmainduplicate.MVPL", BBMovepad.Mainlayer);
        }
        [BBCtrlVisible]
        public static void MainLayer()
        {
            BBMovepad.Mainlayer.Load(BBDir.Get(BBpath.SETING) + "movepadmain.MVPL", BBMovepad.Mainlayer);
        }


        [BBCtrlVisible]
        public static bool  SwitchAsset (GameObject go , int index)
        {
            ReadAssetBase();  																	// read the base definition ( generated by 3dsmax during  export ) 
            Actor A = (Actor)go.GetComponent(typeof(Actor));
            BaseActorProperties BAP = A.Actorprops; ;
            BBehavior[] blist = go.GetComponents<BBehavior>();
            index = index % data.Count;
            string b = "HIGHTECH/";
            string assetname = (data[index]);
            string assetpath = (BBDir.Get(BBpath.ROOTGBASE, true) + b + assetname + ".fbx");
            GameObject prefab = (GameObject)Resources.LoadAssetAtPath(assetpath, typeof(GameObject));
            if (prefab == null)
            {
                BBDebugLog.singleWarning("prefab load fail " + ("Assets" + selectedbasename + assetname + ".fbx"));
                return false ;
            }
            Mesh initialMesh = go.GetComponent<MeshFilter>().sharedMesh;
            Mesh swapMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            go.GetComponent<MeshFilter>().sharedMesh = swapMesh;

            return true;

        }

        static string fullLog="";

        [BBCtrlVisible] 
        public static bool  BrowseAsset(int next)
        {
            GameObject tgo = (GameObject)Selection.activeObject;
            if (tgo == null)
                return false;
            ReadAssetBase();  																	// read the base definition ( generated by 3dsmax during  export ) 
            Actor A = (Actor)tgo.GetComponent(typeof(Actor));
            BaseActorProperties BAP = A.Actorprops; ;
            BBehavior[] blist = tgo.GetComponents<BBehavior>();
            int index;
            assetbaseindex = Mathf.Abs(assetbaseindex + next);								// loop index todo check index base seems to have a small bug here 
            index = assetbaseindex % data.Count;
            assetbaseindex = index;
            string b  = "HIGHTECH/";
            string assetname = (data[index]);
            string assetpath = (BBDir.Get(BBpath.ROOTGBASE, true) + b + assetname + ".fbx"); 
            GameObject prefab = (GameObject)Resources.LoadAssetAtPath(assetpath, typeof(GameObject));
            if (prefab == null)
            {

                BBDebugLog.singleWarning("prefab load fail " + ("Assets" + selectedbasename + assetname + ".fbx"));
                return false;
            }
            GameObject instance = (GameObject)GameObject.Instantiate(prefab, tgo.transform.position, tgo.transform.rotation);
            instance.name = prefab.name + instance.GetInstanceID();
            instance.AddComponent(typeof(Actor));
            Actor swapactor = instance.GetComponent<Actor>();
            swapactor.Actorprops = BAP;
            swapactor.Actorprops.assetname = assetname;
            foreach (BBehavior B in blist)
            {
                System.Type T = B.GetType();
                instance.AddComponent(T.Name);
                BBehavior SB = (BBehavior)instance.GetComponent(T.Name);
                //SB = B;
                Dataset D = B.GetDataset();
                SB.SetDataset(D);
            }
            GameObject.DestroyImmediate(Selection.activeObject);
            Selection.activeGameObject = instance;
            return true;
        }



}
