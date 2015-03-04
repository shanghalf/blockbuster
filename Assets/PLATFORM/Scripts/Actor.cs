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




public static class behaviorManager
{
    // this class is a Helper to manage Actor Behaviors 

    //private  Dictionary<string, string> BehaviorDic = new Dictionary<string, string>();

    /// <summary>
    /// this function register a new behavior in Blockbuster 
    /// </summary>
    /// <returns></returns>


 
    
    public static  System.Type castenum()
    {

      System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
      AssemblyName aName = new AssemblyName("blockbusterbehavior_a");
      AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave);
      ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");
      EnumBuilder eb = mb.DefineEnum("blockbusterbehavior_a", TypeAttributes.Public, typeof(int));
      int i = 0;
      foreach (var entry in GetEnumFromScriptFolder())
      {
          string s = entry.Key;
          eb.DefineLiteral(s, i );
          i++;
      }
          
      System.Type T = eb.CreateType();
      ab.Save(aName.Name + ".dll");

      System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFrom("blockbusterbehavior_a.dll");
      System.Type castenum = ass.GetType("blockbusterbehavior_a");
      return castenum;

    }

    public static System.Type GetClassDataset(string behaviorclassclass)
    {
        AssemblyName assembly = new AssemblyName("ALFTEST");

        System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        //System.AppDomain appDomain = System.Threading.Thread.GetDomain();
        AssemblyName aName = new AssemblyName(behaviorclassclass);
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(aName.Name);
        //create the class
        TypeBuilder typeBuilder = moduleBuilder.DefineType("BBdataset", TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                            TypeAttributes.BeforeFieldInit, typeof(Behaviour));
        System.Type TT = typeBuilder.CreateType();
        //lastNamePropertySetter.CreateMethodBody(
        return TT;
    }
    


    public static bool Allreadyregistered(BehaviorHandle BS)
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


    public static Dictionary<string, string>  GetEnumFromScriptFolder()    
    { 
        string filepath = Application.dataPath + "/PLATFORM/blockbustersetings/registeredbehaviors.xml";
        Dictionary<string, string> behaviorenumbuilderlist = new Dictionary<string, string>();
        string[] files = Directory.GetFiles(Application.dataPath + "/PLATFORM/Scripts/Behaviors","*.cs");

        int i = 0;
        foreach (string   f in files)
        {
            System.IO.StreamReader file = new StreamReader (f);
            string line;
            while ((line = file.ReadLine()) != null)
                if (line.Contains("<autoenum>"))
                {
                    var a = line.Split(char.Parse(" "));
                    behaviorenumbuilderlist.Add(a[3],a[2]);
                }
            i++;
        }
        return behaviorenumbuilderlist;
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






[System.Serializable]
public class Actor : MonoBehaviour 
{

    //public List<Dataset> DatasetTable = new List<Dataset>();


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
	
    public  void Add(System.Object o)
    {
        

    }

	// Update is called once per frame
	public virtual void Update () 
    
    {
        Debug.Log("not implemented at actor level");
	}


    // save function should serialize a table of Behaviors (should be broken now )





}
