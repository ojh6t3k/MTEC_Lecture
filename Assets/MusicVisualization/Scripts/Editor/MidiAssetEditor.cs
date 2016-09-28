using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[CustomEditor(typeof(MidiAsset))]
public class MidiAssetEditor : Editor
{
	private bool _foldout;

	void OnEnable()
	{
		_foldout = true;
	}

	public override void OnInspectorGUI()
	{
		MidiAsset midiAsset = (MidiAsset)target;

		GUILayout.Label("File Name: " + Path.GetFileNameWithoutExtension(midiAsset.fileName));
		GUILayout.Label(string.Format("Total Time: {0:f1} sec", midiAsset.totalTime));

		_foldout = EditorGUILayout.Foldout(_foldout, "Time Signiture");
		if(_foldout == true)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField(string.Format("PPQN: {0:d}", midiAsset.PPQN));
			EditorGUILayout.LabelField(string.Format("Pulse: {0:f} sec", midiAsset.pulseTime));
			EditorGUILayout.LabelField(string.Format("BPM: {0:d}", midiAsset.BPM));
			EditorGUILayout.LabelField(string.Format("Nemerator: {0:d}", midiAsset.numerator));
			EditorGUILayout.LabelField(string.Format("Denominator: {0:d}", midiAsset.denominator));
			EditorGUI.indentLevel--;
		}

		if(GUILayout.Button("Track Viewer") == true)
		{
			MidiTrackWindow.ShowWindow();
		}
	}
}
