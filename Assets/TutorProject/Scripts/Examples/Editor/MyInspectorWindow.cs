using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyInspectorWindow : EditorWindow
{
	[MenuItem("My Menu/Show My Inspector")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(MyInspectorWindow), false, "My Inspector");
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnGUI()
	{
		if(Selection.activeGameObject != null)
		{
			Component[] objs = Selection.activeGameObject.GetComponents<Component>();
			foreach(Component obj in objs)
			{
				Editor editor = Editor.CreateEditor(obj);
				editor.OnInspectorGUI();
				EditorGUILayout.Separator();
			}
		}
	}
}
