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


		GUILayout.Box(myComponent.image, GUILayout.Width(100), GUILayout.Height(100));

		Event currentEvent = Event.current;
		Rect rect = GUILayoutUtility.GetLastRect();
		if(rect.Contains(currentEvent.mousePosition))
		{
			switch (currentEvent.type)
			{
			case EventType.DragUpdated:
				DragAndDrop.visualMode = IsDragValid() ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
				currentEvent.Use();
				break;

			case EventType.DragPerform:
				DragAndDrop.AcceptDrag();

				for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
				{
					myComponent.image = DragAndDrop.objectReferences[i] as Texture2D;
				}
				currentEvent.Use();
				break;
			}
		}
	}

	bool IsDragValid()
	{
		for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
		{
			if (DragAndDrop.objectReferences[i].GetType() != typeof(Texture2D))
				return false;
		}

		return true;
	}
}
