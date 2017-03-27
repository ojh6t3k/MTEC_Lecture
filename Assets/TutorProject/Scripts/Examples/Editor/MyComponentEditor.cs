using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;


[CustomEditor(typeof(MyComponent))]
public class MyComponentEditor : Editor
{
	SerializedProperty intVariable;
	SerializedProperty floatVariable;
	SerializedProperty gameObjectList;

	void OnEnable()
	{
		intVariable = serializedObject.FindProperty("intVariable");
		floatVariable = serializedObject.FindProperty("floatVariable");
		gameObjectList = serializedObject.FindProperty("gameObjectList");
	}

	public override void OnInspectorGUI()
	{
		// Automatic management
		serializedObject.Update();

		EditorGUILayout.PropertyField(intVariable, new GUIContent("Var1"));
		EditorGUILayout.PropertyField(floatVariable, new GUIContent("Var2"));
		EditorGUILayout.PropertyField(gameObjectList, new GUIContent("List"), true);

		serializedObject.ApplyModifiedProperties();


		// Manual management
		MyComponent myComponent = (MyComponent)target;

		int a = EditorGUILayout.IntField("Int Var", myComponent.IntVar);
		if(a != myComponent.IntVar)
		{
			myComponent.IntVar = a;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if(GUILayout.Button("Do something") == true)
		{
			myComponent.DoSomething();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
	}
}
