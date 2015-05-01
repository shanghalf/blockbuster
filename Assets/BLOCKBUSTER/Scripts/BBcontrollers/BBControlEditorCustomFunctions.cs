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



/// <summary>
/// WRITE YOUR OWN FUNCTION BLOCK HERE 
/// </summary>
/// 
    public class BBBLocks
    {
        public BBBLocks()
        { 
        }


        [BBCtrlVisible]
        public static void DeleteSelection()
        {
            foreach (GameObject tgo in Selection.objects)
                GameObject.DestroyImmediate(tgo);
        }


        [BBCtrlVisible] // define a function visible for BBControl 
        public static Vector3 invertVector(Vector3 VIN)
        {
            return -VIN;
        }

        public static int I;



        [BBCtrlProp( "testint")]
        public int IntSlider()
        {
            I =EditorGUILayout.IntSlider(I, 0, 100);
            return I;
        }




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
}
