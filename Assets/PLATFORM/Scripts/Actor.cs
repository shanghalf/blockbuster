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
using System.Reflection;
using System.Reflection.Emit;


[System.Serializable]
public enum PLTF_TYPE
{
    STATIC = 0,
    ROTATING = 1,
    FALLING = 2,
    MOVING = 3
}

public class BehaviorHandle
{
    public System.Type T;
    public Behavior m_behavior;
    public string m_behaviorclassname;
    public GUI guid;
    public GUI GetGuid()
    {
        return guid;
    }
}



public  class behaviorManager
{
    // this class is a Helper to manage Actor Behaviors 
    public List<BehaviorHandle> m_registeredbehaviors = new List<BehaviorHandle>();

    /// <summary>
    /// this function register a new behavior in Blockbuster 
    /// </summary>
    /// <returns></returns>
    public System.Type assemblytest()
    {
      System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
      AssemblyName aName = new AssemblyName("monculsurlacomode");
      AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave);
      ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");
      EnumBuilder eb = mb.DefineEnum("Elevation", TypeAttributes.Public, typeof(int));
      eb.DefineLiteral("AKUNA", 0);
      eb.DefineLiteral("MATATA", 1);
      eb.DefineLiteral("QUELLE", 2);
      eb.DefineLiteral("CHOSE", 3);
      eb.DefineLiteral("FANTASTIQUE", 4);
      eb.DefineLiteral("DUCUL", 5);
      System.Type finished = eb.CreateType();
      ab.Save(aName.Name + ".dll");
      System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFrom("monculsurlacomode.dll");
      System.Type enumTest = ass.GetType("Elevation");
      return enumTest;

    }
    



    static bool Allreadyregistered(BehaviorHandle BS)
    {
        /*
            
        foreach (BehaviorHandle item in m_registeredbehaviors)
        {
            if (BS == item)
                return true;
        }
         */ 
        return false;
    }


    static public void RegisterBehavior(BehaviorHandle BS)
    {
        // allready here 
        if (Allreadyregistered(BS))
            return;

        //m_registeredbehaviors.Add(BS);
    }

    static public void UnRegisterBehavior(BehaviorHandle BS)
    {
        /*
        if (Allreadyregistered(BS))
            m_registeredbehaviors.Remove(BS);
         * */
    }



}







public class Actor : MonoBehaviour 




{



    // will replace later with a behavior table to manage behavior from actor 
    //public Behavior Behavior = null;



    public List<Dataset> DatasetTable = new List<Dataset>();
    
    public Transform block_transform;
    public GameObject scenerefobj;


 


    public BaseActorProperties Actorprops = new BaseActorProperties();


    public virtual void OnDrawGizmosSelected()
    {

    }


	// Use this for initialization
	public virtual void Start () 
    
    {
        // an actor is not suposed to be used in game mode 
	}
	
	// Update is called once per frame
	public virtual void Update () 
    
    {
	    // an actor is not suposed to be used in game mode 
	}


    // save function should serialize a table of Behaviors (should be broken now )
    public virtual void Save(string path, System.Type type)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        Stream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, this);
        stream.Flush();
        stream.Close();
        //debug.Log ( serializer.ToString());
    }





}
