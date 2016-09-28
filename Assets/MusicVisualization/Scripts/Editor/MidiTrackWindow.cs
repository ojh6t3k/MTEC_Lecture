using UnityEngine;
using UnityEditor;
using System.Collections;

public class MidiTrackWindow : EditorWindow
{
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(MidiTrackWindow), false, "Midi Track");
	}

	void OnGUI()
	{
		Rect rect;
		float titleHeight = 30f;
		float musicalScaleWidth = 60f;
		float timeHeight = 50f;

		// Draw Title Area
		rect = new Rect(0, 0, position.width, titleHeight);
		GUI.Box(rect, "");
		GUILayout.BeginArea(rect);
		GUILayout.EndArea();

		// Draw Musical Scale Area
		rect = new Rect(0, titleHeight, musicalScaleWidth, position.height - titleHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		GUI.EndGroup();

		// Draw Track List Area
		rect = new Rect(position.width * 0.75f, titleHeight, position.width * 0.25f, position.height - titleHeight);
		GUI.Box(rect, "");
		GUILayout.BeginArea(rect);
		GUILayout.EndArea();

		// Draw Time Area
		rect = new Rect(musicalScaleWidth, titleHeight, position.width - musicalScaleWidth - position.width * 0.25f, timeHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		GUI.EndGroup();

		// Draw Note Area
		rect = new Rect(musicalScaleWidth, titleHeight + timeHeight, position.width - musicalScaleWidth - position.width * 0.25f, position.height - titleHeight - timeHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		GUI.EndGroup();
	}
}
