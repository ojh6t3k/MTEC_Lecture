using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PathDrawer))]
public class PathDrawerEditor : Editor
{
	void OnSceneGUI()
	{
		PathDrawer pathDrawer = (PathDrawer)target;

		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0, 0, Color.white);

		for(int i = 0; i < (pathDrawer.nodes.Length - 1); i++)
		{
			GameObject startNode = pathDrawer.nodes[i];
			GameObject endNode = pathDrawer.nodes[i+1];

			Vector3 startPos = startNode.transform.position;
			Vector3 endPos = endNode.transform.position;
			Vector3 startTangent = startPos + startNode.transform.forward;
			Vector3 endTangent = endPos - endNode.transform.forward;

			Handles.DrawBezier(startPos, endPos
							,startTangent, endTangent
							,Color.green, texture, 3f);
		}
	}
}
