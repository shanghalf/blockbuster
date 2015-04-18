#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using plyBloxKit;

// by RaiuLyn
// https://twitter.com/RaiuLyn
// http://raiulyn.wordpress.com 
// http://forum.plyoung.com/users/raiulyn
// ============================================================================================================

[CustomEditor(typeof(OnClickTriggerBloxEvent))]
public class OnClickTriggerBloxEvent_Inspector : Editor
{

	public override void OnInspectorGUI()
	{
		OnClickTriggerBloxEvent OnClickInfo = (OnClickTriggerBloxEvent)target;

		//Use Button component? --------------------------------------------------------------------------------------------------------------------------------------------
		OnClickInfo.UseButtonOnclick = EditorGUILayout.Toggle(new GUIContent("Use Button Onclick", "Only works on Start() and cannot be used during runtime"), OnClickInfo.UseButtonOnclick);
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if (OnClickInfo.UseButtonOnclick)
		{
			if (OnClickInfo.button == null)
			{
				EditorGUILayout.LabelField("No Button Component is found");
				GUI.color = Color.red;
				OnClickInfo.button = EditorGUILayout.ObjectField(new GUIContent("Button", "Set Button here else automatically set self if self has Button component"), OnClickInfo.button, typeof(UnityEngine.UI.Button), true) as UnityEngine.UI.Button;
				GUI.color = Color.white;
			}
			else
			{
				EditorGUILayout.LabelField("Button Component is set");
				OnClickInfo.button = EditorGUILayout.ObjectField(new GUIContent("Button", "Set Button here else automatically set self if self has Button component"), OnClickInfo.button, typeof(UnityEngine.UI.Button), true) as UnityEngine.UI.Button;
			}
		}
		else
		{
			if (OnClickInfo.button != null)
			{
				OnClickInfo.button = null;
			}
		}
		EditorGUILayout.Space();
		EditorGUILayout.Space();


		//If multiple components detected, show custom Name to define --------------------------------------------------------------------------------------------------
		if (OnClickInfo.GetComponents<OnClickTriggerBloxEvent>().Length > 1)
		{
			OnClickInfo.CustomName = EditorGUILayout.TextField(new GUIContent("Custom Name", "Define name for SendMessage to filter out which is which for ClickMethod(string customName)"), OnClickInfo.CustomName);
		}
		else
		{
			if (OnClickInfo.CustomName != null)
			{
				OnClickInfo.CustomName = null;
			}
		}


		//Blox -----------------------------------------------------------------------------------------------------------------------------------------------------------
		EditorGUILayout.LabelField("Trigger Event Settings");

		if (OnClickInfo.blox == null)
		{
			GUI.color = Color.red;
			OnClickInfo.blox = EditorGUILayout.ObjectField(new GUIContent("Blox", "The Blox in which to trigger the Event"), OnClickInfo.blox, typeof(plyBlox), true) as plyBlox;
			GUI.color = Color.white;
		}
		else
		{
			OnClickInfo.blox = EditorGUILayout.ObjectField(new GUIContent("Blox", "The Blox in which to trigger the Event"), OnClickInfo.blox, typeof(plyBlox), true) as plyBlox;
		}


		//Event Name -----------------------------------------------------------------------------------------------------------------------------------------------------------
		if (OnClickInfo.EventName == string.Empty)
		{
			GUI.color = Color.red;
			OnClickInfo.EventName = EditorGUILayout.TextField(new GUIContent("Custom Event", "Name of the Custom Event to Trigger"), OnClickInfo.EventName);
			GUI.color = Color.white;
		}
		else
		{
			OnClickInfo.EventName = EditorGUILayout.TextField(new GUIContent("Custom Event", "Name of the Custom Event to Trigger"), OnClickInfo.EventName);
		}

		//Param 1 -----------------------------------------------------------------------------------------------------------------------------------------------------------
		OnClickInfo.paramtype1 = (OnClickTriggerBloxEvent.ParamType)EditorGUILayout.EnumPopup("param1", OnClickInfo.paramtype1);
		switch (OnClickInfo.paramtype1)
		{
			case OnClickTriggerBloxEvent.ParamType.String:
			OnClickInfo.param1.str_value = EditorGUILayout.TextField(OnClickInfo.param1.str_value);
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.String;
			break;
			case OnClickTriggerBloxEvent.ParamType.Int:
			OnClickInfo.param1.int_value = EditorGUILayout.IntField(OnClickInfo.param1.int_value);
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.Int;
			break;
			case OnClickTriggerBloxEvent.ParamType.Float:
			OnClickInfo.param1.float_value = EditorGUILayout.FloatField(OnClickInfo.param1.float_value);
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.Float;
			break;
			case OnClickTriggerBloxEvent.ParamType.Bool:
			OnClickInfo.param1.bool_value = EditorGUILayout.Toggle(OnClickInfo.param1.bool_value.ToString(), OnClickInfo.param1.bool_value);
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.Bool;
			break;
			case OnClickTriggerBloxEvent.ParamType.vector3:
			OnClickInfo.param1.vector3_value = EditorGUILayout.Vector3Field("V3", OnClickInfo.param1.vector3_value);
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.vector3;
			break;
			case OnClickTriggerBloxEvent.ParamType.vector2:
			OnClickInfo.param1.vector2_value = EditorGUILayout.Vector2Field("V2", OnClickInfo.param1.vector2_value);
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.vector2;
			break;
			case OnClickTriggerBloxEvent.ParamType.Gameobject:
			OnClickInfo.param1.gameobject_value = EditorGUILayout.ObjectField(OnClickInfo.param1.gameobject_value, typeof(GameObject), true) as GameObject;
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.Gameobject;
			break;
			case OnClickTriggerBloxEvent.ParamType.component:
			OnClickInfo.param1.component_value = EditorGUILayout.ObjectField(OnClickInfo.param1.component_value, typeof(Component), true) as Component;
			OnClickInfo.paramtype1 = OnClickTriggerBloxEvent.ParamType.component;
			break;
		}

		//Param 2 -----------------------------------------------------------------------------------------------------------------------------------------------------------
		OnClickInfo.paramtype2 = (OnClickTriggerBloxEvent.ParamType)EditorGUILayout.EnumPopup("param2", OnClickInfo.paramtype2);
		switch (OnClickInfo.paramtype2)
		{
			case OnClickTriggerBloxEvent.ParamType.String:
			OnClickInfo.param2.str_value = EditorGUILayout.TextField(OnClickInfo.param2.str_value);
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.String;
			break;
			case OnClickTriggerBloxEvent.ParamType.Int:
			OnClickInfo.param2.int_value = EditorGUILayout.IntField(OnClickInfo.param2.int_value);
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.Int;
			break;
			case OnClickTriggerBloxEvent.ParamType.Float:
			OnClickInfo.param2.float_value = EditorGUILayout.FloatField(OnClickInfo.param2.float_value);
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.Float;
			break;
			case OnClickTriggerBloxEvent.ParamType.Bool:
			OnClickInfo.param2.bool_value = EditorGUILayout.Toggle(OnClickInfo.param2.bool_value.ToString(), OnClickInfo.param2.bool_value);
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.Bool;
			break;
			case OnClickTriggerBloxEvent.ParamType.vector3:
			OnClickInfo.param2.vector3_value = EditorGUILayout.Vector3Field("V3", OnClickInfo.param2.vector3_value);
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.vector3;
			break;
			case OnClickTriggerBloxEvent.ParamType.vector2:
			OnClickInfo.param2.vector2_value = EditorGUILayout.Vector2Field("V2", OnClickInfo.param2.vector2_value);
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.vector2;
			break;
			case OnClickTriggerBloxEvent.ParamType.Gameobject:
			OnClickInfo.param2.gameobject_value = EditorGUILayout.ObjectField(OnClickInfo.param2.gameobject_value, typeof(GameObject), true) as GameObject;
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.Gameobject;
			break;
			case OnClickTriggerBloxEvent.ParamType.component:
			OnClickInfo.param2.component_value = EditorGUILayout.ObjectField(OnClickInfo.param2.component_value, typeof(Component), true) as Component;
			OnClickInfo.paramtype2 = OnClickTriggerBloxEvent.ParamType.component;
			break;
		}

		//Param 3 -----------------------------------------------------------------------------------------------------------------------------------------------------------
		OnClickInfo.paramtype3 = (OnClickTriggerBloxEvent.ParamType)EditorGUILayout.EnumPopup("param3", OnClickInfo.paramtype3);
		switch (OnClickInfo.paramtype3)
		{
			case OnClickTriggerBloxEvent.ParamType.String:
			OnClickInfo.param3.str_value = EditorGUILayout.TextField(OnClickInfo.param3.str_value);
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.String;
			break;
			case OnClickTriggerBloxEvent.ParamType.Int:
			OnClickInfo.param3.int_value = EditorGUILayout.IntField(OnClickInfo.param3.int_value);
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.Int;
			break;
			case OnClickTriggerBloxEvent.ParamType.Float:
			OnClickInfo.param3.float_value = EditorGUILayout.FloatField(OnClickInfo.param3.float_value);
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.Float;
			break;
			case OnClickTriggerBloxEvent.ParamType.Bool:
			OnClickInfo.param3.bool_value = EditorGUILayout.Toggle(OnClickInfo.param3.bool_value.ToString(), OnClickInfo.param3.bool_value);
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.Bool;
			break;
			case OnClickTriggerBloxEvent.ParamType.vector3:
			OnClickInfo.param3.vector3_value = EditorGUILayout.Vector3Field("V3", OnClickInfo.param3.vector3_value);
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.vector3;
			break;
			case OnClickTriggerBloxEvent.ParamType.vector2:
			OnClickInfo.param3.vector2_value = EditorGUILayout.Vector2Field("V2", OnClickInfo.param3.vector2_value);
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.vector2;
			break;
			case OnClickTriggerBloxEvent.ParamType.Gameobject:
			OnClickInfo.param3.gameobject_value = EditorGUILayout.ObjectField(OnClickInfo.param3.gameobject_value, typeof(GameObject), true) as GameObject;
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.Gameobject;
			break;
			case OnClickTriggerBloxEvent.ParamType.component:
			OnClickInfo.param3.component_value = EditorGUILayout.ObjectField(OnClickInfo.param3.component_value, typeof(Component), true) as Component;
			OnClickInfo.paramtype3 = OnClickTriggerBloxEvent.ParamType.component;
			break;
		}

		//Save changes -----------------------------------------------------------------------------------------------------------------------------------------------------------
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
			GUI.changed = false;
		}

	}
}

#endif