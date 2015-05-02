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



public class BBUInodes
{
    public static float F;
    public static int I;
    
    [BBCtrlProp]
    [BBCtrlVisible]
    public float floatSlider(float max=5.0f)
    {
        F = EditorGUILayout.Slider(F, 0, max);
        return F;
    }
    [BBCtrlProp]
    [BBCtrlVisible]
    public int IntSlider()
    {
        I = EditorGUILayout.IntSlider(I, 0, 100);
        return I;
    }
}




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

        /// <summary>
        /// delete selected object 
        /// </summary>
        [BBCtrlVisible]
        public static void DeleteSelection()
        {
            foreach (GameObject tgo in Selection.objects)
                GameObject.DestroyImmediate(tgo);
        }

        /// <summary>
        /// vector operation 
        /// </summary>
        /// <param name="VIN"></param>
        /// <returns></returns>
        [BBCtrlVisible] // define a function visible for BBControl 
        public static Vector3 invertVector(Vector3 VIN)
        {
            return -VIN;
        }

        /// <summary>
        /// extend to add nodes with embeded ui control like slider ddlist etc 
        /// </summary>


   




        [BBCtrlVisible] // define a function visible for BBControl 
        public static bool FALSE()
        {
            return false;
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public static bool TRUE()
        {
            return true;
        }



        [BBCtrlVisible] // define a function visible for BBControl 
        public static int BBMathsAddint(int a, int b)
        {
            Debug.Log((a + b).ToString());
            return a + b;

        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public static int BBMathsGenint()
        {
            Debug.Log("1967");
            return 1967;

        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public static int increment()
        {
            return 1;
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public static int decrement()
        {
            return -1;
        }



        [BBCtrlVisible] // define a function visible for BBControl 
        public static Vector3 BBMathsAddVector(Vector3 a, Vector3 b)
        {
            return a + b;
        }
        [BBCtrlVisible] // define a function visible for BBControl 
        public static BaseActorProperties returnbaseactorprop(Actor A)
        {
            return A.Actorprops;
        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public static Vector3 Multiplyvector(Vector3 vin, float by)
        {
            return vin * by;
        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public static object Convert(object objin ,Type T , bool canconvert )
        {
            if (canconvert)
            {
                var instance = Activator.CreateInstance(T);
                instance = objin;
                return instance;
            }
            else
                return null;
           
        }


        [BBCtrlVisible] // define a function visible for BBControl 
        public static Type GetType (object value)
        {

            return value.GetType();

        }



        [BBCtrlVisible] // define a function visible for BBControl 
        public static Type ZCanChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
                return null;
            if (value == null)
                return null;
            IConvertible convertible = value as IConvertible;
            if (convertible == null)
                return null;
            return conversionType;
        }



        /// <summary>
        /// read content of the selected base 
        /// </summary>
        /// <returns></returns>
        public static bool ReadAssetBase()
        {
            if (data != null)
                data.Clear();  // clear the base 
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
        public static void BrowseAsset(int next)
        {
            if (Actor.b_groupselectmode) return;
            GameObject tgo = (GameObject)Selection.activeObject;
            if (tgo == null)
                return;
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
                Debug.Log("prefab load fail " + ("Assets" + selectedbasename + assetname + ".fbx"));
                return;
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
        }



}
