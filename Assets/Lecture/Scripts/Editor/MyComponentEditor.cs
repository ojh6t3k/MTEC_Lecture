using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(MyComponent))]
public class MyComponentEditor : Editor
{
	void OnEnable()
	{
	}

	public override void OnInspectorGUI()
	{
		MyComponent myComponent = (MyComponent)target;

		myComponent.intVariable = EditorGUILayout.IntField("Int Variable", myComponent.intVariable);
		myComponent.floatVariable = EditorGUILayout.Slider("Float Variable", myComponent.floatVariable, 0f, 100f);
		myComponent.IntVar = EditorGUILayout.IntField("Int Var", myComponent.IntVar);
		if(GUILayout.Button("Do something") == true)
		{
			myComponent.DoSomething();
		}
	}
}
