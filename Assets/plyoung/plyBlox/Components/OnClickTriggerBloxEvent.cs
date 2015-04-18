#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using plyCommon;
using plyBloxKit;

// by RaiuLyn
// https://twitter.com/RaiuLyn
// http://raiulyn.wordpress.com 
// http://forum.plyoung.com/users/raiulyn
// ============================================================================================================

[AddComponentMenu("plyGame/Misc/OnClick Trigger BloxEvent")]
public class OnClickTriggerBloxEvent : MonoBehaviour
{
	public enum Param
	{
		param1,
		param2,
		param3
	}

	public enum ParamType
	{
		None,
		String,
		Int,
		Float,
		Bool,
		vector3,
		vector2,
		Gameobject,
		component,
		CustomObject
	}

	[System.Serializable]
	public class ValueType
	{
		public string str_value;
		public int int_value;
		public float float_value;
		public bool bool_value;
		public Vector3 vector3_value;
		public Vector2 vector2_value;
		public GameObject gameobject_value;
		public Component component_value;
		public object object_value;
	}

	public string CustomName;

	public plyBlox blox;
	public plyEvent bloxEvent;

	public string EventName;

	public ValueType param1;
	public ParamType paramtype1;
	public ValueType param2;
	public ParamType paramtype2;
	public ValueType param3;
	public ParamType paramtype3;

	public Button button;
	public bool UseButtonOnclick;

	void Start()
	{
		if (button == null)
		{
			//Get Button component from Self gameobject
			button = GetComponent<UnityEngine.UI.Button>();
			if (button != null)
			{
				//If there's a Button component, Set it for calling else nothing happens
				button.onClick.AddListener(() => { ClickMethod(); });
			}
		}
		else
		{
			//Use predefined Button for calling instead
			button.onClick.AddListener(() => { ClickMethod(); });
		}
	}

	//Sets value in a specific Parameter
	public void SetValue(Param findParam, object val)
	{
		switch (findParam)
		{
			case Param.param1:
			param1.object_value = val;
			paramtype1 = ParamType.CustomObject;
			break;
			case Param.param2:
			param2.object_value = val;
			paramtype2 = ParamType.CustomObject;
			break;
			case Param.param3:
			param3.object_value = val;
			paramtype3 = ParamType.CustomObject;
			break;
		}
	}

	//Find which component to trigger by string parameter (SendMessage Only)
	public void ClickMethod(string customName)
	{
		if (CustomName == customName)
		{
			ClickMethod();
		}
	}

	//Sets values and sends to Blox
	public void ClickMethod()
	{
		bloxEvent = blox.GetEvent(EventName);

		switch (paramtype1)
		{
			case ParamType.String:
			bloxEvent.SetTempVarValue("param1", param1.str_value);
			break;
			case ParamType.Int:
			bloxEvent.SetTempVarValue("param1", param1.int_value);
			break;
			case ParamType.Float:
			bloxEvent.SetTempVarValue("param1", param1.float_value);
			break;
			case ParamType.Bool:
			bloxEvent.SetTempVarValue("param1", param1.bool_value);
			break;
			case ParamType.vector3:
			bloxEvent.SetTempVarValue("param1", param1.vector3_value);
			break;
			case ParamType.vector2:
			bloxEvent.SetTempVarValue("param1", param1.vector2_value);
			break;
			case ParamType.Gameobject:
			bloxEvent.SetTempVarValue("param1", param1.gameobject_value);
			break;
			case ParamType.component:
			bloxEvent.SetTempVarValue("param1", param1.component_value);
			break;
			case ParamType.CustomObject:
			bloxEvent.SetTempVarValue("param1", param1.object_value);
			break;
		}

		switch (paramtype2)
		{
			case ParamType.String:
			bloxEvent.SetTempVarValue("param2", param2.str_value);
			break;
			case ParamType.Int:
			bloxEvent.SetTempVarValue("param2", param2.int_value);
			break;
			case ParamType.Float:
			bloxEvent.SetTempVarValue("param2", param2.float_value);
			break;
			case ParamType.Bool:
			bloxEvent.SetTempVarValue("param2", param2.bool_value);
			break;
			case ParamType.vector3:
			bloxEvent.SetTempVarValue("param2", param2.vector3_value);
			break;
			case ParamType.vector2:
			bloxEvent.SetTempVarValue("param2", param2.vector2_value);
			break;
			case ParamType.Gameobject:
			bloxEvent.SetTempVarValue("param2", param2.gameobject_value);
			break;
			case ParamType.component:
			bloxEvent.SetTempVarValue("param2", param2.component_value);
			break;
			case ParamType.CustomObject:
			bloxEvent.SetTempVarValue("param2", param2.object_value);
			break;
		}

		switch (paramtype3)
		{
			case ParamType.String:
			bloxEvent.SetTempVarValue("param3", param3.str_value);
			break;
			case ParamType.Int:
			bloxEvent.SetTempVarValue("param3", param3.int_value);
			break;
			case ParamType.Float:
			bloxEvent.SetTempVarValue("param3", param3.float_value);
			break;
			case ParamType.Bool:
			bloxEvent.SetTempVarValue("param3", param3.bool_value);
			break;
			case ParamType.vector3:
			bloxEvent.SetTempVarValue("param3", param3.vector3_value);
			break;
			case ParamType.vector2:
			bloxEvent.SetTempVarValue("param3", param3.vector2_value);
			break;
			case ParamType.Gameobject:
			bloxEvent.SetTempVarValue("param3", param3.gameobject_value);
			break;
			case ParamType.component:
			bloxEvent.SetTempVarValue("param3", param3.component_value);
			break;
			case ParamType.CustomObject:
			bloxEvent.SetTempVarValue("param3", param3.object_value);
			break;
		}

		blox.RunEvent(bloxEvent);

	}

	// ============================================================================================================
}

#endif