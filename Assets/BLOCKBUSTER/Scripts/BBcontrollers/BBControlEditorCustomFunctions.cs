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
//using BlockbusterControll;

/// <summary>
/// WRITE YOUR OWN FUNCTION BLOCK HERE 
/// </summary>
/// 
namespace BBBLocks
{
    public class BBBLocks
    {

        public BBBLocks()
        { 
        }



        [BBCtrlVisible] // define a function visible for BBControl 
        public static Vector3 invertVector(Vector3 VIN)
        {
            return -VIN;
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
        public static Type CanChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;

            if (convertible == null)
            {
                return null;
            }

            return conversionType;
        }





    }
}