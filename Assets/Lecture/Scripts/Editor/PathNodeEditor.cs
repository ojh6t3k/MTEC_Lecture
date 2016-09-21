using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PathNode))]
public class PathNodeEditor : Editor
{
	void OnSceneGUI()
	{
		PathNode pathNode = (PathNode)target;
		
		Handles.color = Color.red;
		Handles.Label(pathNode.transform.position, pathNode.name);
	}
}
