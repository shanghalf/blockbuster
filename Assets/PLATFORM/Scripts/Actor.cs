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
        /*
      eb.DefineLiteral("mon","AAAAA");
      eb.DefineLiteral("cul", "BBBB");
      eb.DefineLiteral("sur", "CCC");
      eb.DefineLiteral("ton", "nez");
      eb.DefineLiteral("nez", "DDD");
      eb.DefineLiteral("!!", "EEE");
        */
       


      //System.Type finished = 
          
      System.Type T = eb.CreateType();
      ab.Save(aName.Name + ".dll");

      System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFrom("blockbusterbehavior_a.dll");
      System.Type castenum = ass.GetType("blockbusterbehavior_a");
      return castenum;

    }

    public static System.Type GetClassDataset()
    {
        AssemblyName assembly = new AssemblyName("ALFTEST");

        System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        //System.AppDomain appDomain = System.Threading.Thread.GetDomain();
        AssemblyName aName = new AssemblyName("blockbusterbehavior_a");
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(aName.Name);

        //create the class
        TypeBuilder typeBuilder = moduleBuilder.DefineType("BBdataset", TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                            TypeAttributes.BeforeFieldInit, typeof(System.Object));




        //create the firstName field
        //FieldBuilder firstNameField = typeBuilder.DefineField("pathnodes", typeof(System.String), FieldAttributes.Private);

        //create the firstName attribute [FieldOrder(0)]

        //create the FirstName property
        PropertyBuilder Pathnodes  = typeBuilder.DefineProperty("Pathnodes", PropertyAttributes.None, typeof(List<Pathnode>), null);

        //create the FirstName Getter
        //MethodBuilder firstNamePropertyGetter = typeBuilder.DefineMethod("get_FirstName", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                                   //       MethodAttributes.HideBySig, typeof(System.String), System.Type.EmptyTypes);
        //ILGenerator firstNamePropertyGetterIL = firstNamePropertyGetter.GetILGenerator();
        //firstNamePropertyGetterIL.Emit(OpCodes.Ldarg_0);
        //firstNamePropertyGetterIL.Emit(OpCodes.Ldfld, firstNameField);
        //firstNamePropertyGetterIL.Emit(OpCodes.Ret);

        //create the FirstName Setter
        //MethodBuilder firstNamePropertySetter = typeBuilder.DefineMethod("set_FirstName", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                      //      MethodAttributes.HideBySig, null, new System.Type[] { typeof(System.String) });


        //MethodBuilder fNamePropertySetter = typeBuilder.DefineMethod ("set_FirstName", MethodAttributes.Public | MethodAttributes.SpecialName |
        /*                                                    MethodAttributes.HideBySig, null, new System.Type[] { typeof(System.String) });
        ILGenerator firstNamePropertySetterIL = firstNamePropertySetter.GetILGenerator();
        firstNamePropertySetterIL.Emit(OpCodes.Ldarg_0);
        firstNamePropertySetterIL.Emit(OpCodes.Ldarg_1);
        firstNamePropertySetterIL.Emit(OpCodes.Stfld, firstNameField);
        firstNamePropertySetterIL.Emit(OpCodes.Ret);

        //assign getter and setter
        firstNameProperty.SetGetMethod(firstNamePropertyGetter);
        firstNameProperty.SetSetMethod(firstNamePropertySetter);


        //create the lastName field
        FieldBuilder lastNameField = typeBuilder.DefineField("lastName", typeof(System.String), FieldAttributes.Private);

        //create the lastName attribute [FieldOrder(1)]

        //create the LastName property
        PropertyBuilder lastNameProperty = typeBuilder.DefineProperty("LastName", PropertyAttributes.HasDefault, typeof(System.String), null);

        //create the LastName Getter
        MethodBuilder lastNamePropertyGetter = typeBuilder.DefineMethod("get_LastName", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                                          MethodAttributes.HideBySig, typeof(System.String), System.Type.EmptyTypes);
        ILGenerator lastNamePropertyGetterIL = lastNamePropertyGetter.GetILGenerator();
        lastNamePropertyGetterIL.Emit(OpCodes.Ldarg_0);
        lastNamePropertyGetterIL.Emit(OpCodes.Ldfld, lastNameField);
        lastNamePropertyGetterIL.Emit(OpCodes.Ret);

        //create the FirstName Setter
        MethodBuilder lastNamePropertySetter = typeBuilder.DefineMethod("set_FirstName", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                            MethodAttributes.HideBySig, null, new System.Type[] { typeof(System.String) });
        ILGenerator lastNamePropertySetterIL = lastNamePropertySetter.GetILGenerator();
        lastNamePropertySetterIL.Emit(OpCodes.Ldarg_0);
        lastNamePropertySetterIL.Emit(OpCodes.Ldarg_1);
        lastNamePropertySetterIL.Emit(OpCodes.Stfld, lastNameField);
        lastNamePropertySetterIL.Emit(OpCodes.Ret);

        //assign getter and setter
        lastNameProperty.SetGetMethod(lastNamePropertyGetter);
        lastNameProperty.SetSetMethod(lastNamePropertySetter);
        */

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
