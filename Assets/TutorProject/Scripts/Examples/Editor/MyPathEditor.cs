using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MyPath))]
public class MyPathEditor : Editor
{
	void OnSceneGUI()
	{
		MyPath path = (MyPath)target;

		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0, 0, Color.white);

		for(int i = 0; i < (path.nodes.Length - 1); i++)
		{
			MyNode startNode = path.nodes[i];
			MyNode endNode = path.nodes[i+1];

			if(startNode != null && endNode != null)
			{
				Vector3 startPos = startNode.transform.position;
				Vector3 endPos = endNode.transform.position;
				Vector3 startTangent = startPos + startNode.transform.forward;
				Vector3 endTangent = endPos - endNode.transform.forward;

				Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.green, texture, 3f);
			}
		}
	}
}
