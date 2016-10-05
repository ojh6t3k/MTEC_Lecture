using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;


public class MidiTrackWindow : EditorWindow
{
	// GUI Design
	const float titleHeight = 30f;
	const float musicalScaleWidth = 60f;
	const float timeHeight = 50f;
	const float GridX = 0.5f;
	const float GridY = 30f;

	static private MidiAsset _midi;

	private Vector2 _trackListScroll;
	private Vector2 _noteAreaScroll;
	private bool[] _enableTracks;

	public static void ShowWindow(MidiAsset midi)
	{
		_midi = midi;
		EditorWindow.GetWindow(typeof(MidiTrackWindow), false, "Midi Track");
	}

	void OnGUI()
	{
		if(_midi == null)
			return;
		
		Rect rect;

		// Draw Title Area
		rect = new Rect(0, 0, position.width, titleHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		DrawTitleArea(rect.width, rect.height);
		GUI.EndGroup();

		// Draw Musical Scale Area
		rect = new Rect(0, titleHeight, musicalScaleWidth, position.height - titleHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		DrawMusicalScaleArea(rect.width, rect.height);
		GUI.EndGroup();

		// Draw Track List Area
		rect = new Rect(position.width * 0.75f, titleHeight, position.width * 0.25f, position.height - titleHeight);
		GUI.Box(rect, "");
		GUILayout.BeginArea(rect);
		DrawTrackListArea();
		GUILayout.EndArea();

		// Draw Time Area
		rect = new Rect(musicalScaleWidth, titleHeight, position.width - musicalScaleWidth - position.width * 0.25f, timeHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		DrawTimeArea(rect.width, rect.height);
		GUI.EndGroup();

		// Draw Note Area
		rect = new Rect(musicalScaleWidth, titleHeight + timeHeight, position.width - musicalScaleWidth - position.width * 0.25f, position.height - titleHeight - timeHeight);
		GUI.Box(rect, "");
		GUI.BeginGroup(rect);
		DrawNoteArea(rect.width, rect.height);
		GUI.EndGroup();
	}

	void DrawTitleArea(float width, float height)
	{
		Rect rect = new Rect(0, 0, width, height);
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.MiddleCenter;
		style.fontStyle = FontStyle.Italic;
		GUI.Label(rect, Path.GetFileNameWithoutExtension(_midi.fileName), style);
	}

	void DrawMusicalScaleArea(float width, float height)
	{
		Rect rect = new Rect(0, 0, width, height);
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.MiddleCenter;

		Rect rect2 = new Rect(0, 0, width, GridY);
		for(int i = 0; i < 128; i++)
		{
			GUI.Box(rect2, "");
			GUI.Label(rect2, NoteNumberToString(i), style);
			rect2.y += GridY;
		}
	}

	void DrawTrackListArea()
	{
		if(_enableTracks == null)
		{
			_enableTracks = new bool[_midi.tracks.Length];
			for(int i = 0; i < _enableTracks.Length; i++)
				_enableTracks[i] = true;
		}

		_trackListScroll = EditorGUILayout.BeginScrollView(_trackListScroll);
		for(int i=0; i<_midi.tracks.Length; i++)
		{
			_enableTracks[i] = GUILayout.Toggle(_enableTracks[i], _midi.tracks[i].InstrumentName);
		}
		EditorGUILayout.EndScrollView();
	}

	void DrawTimeArea(float width, float height)
	{
	}

	void DrawNoteArea(float width, float height)
	{
		Rect viewRect = new Rect(0, 0, width, height);
		Rect contextRect = new Rect(0, 0, GridX * _midi.totalTime * 1000f, GridY * 128);
		_noteAreaScroll = GUI.BeginScrollView(viewRect, _noteAreaScroll, contextRect);

		Rect scrollRect = new Rect(_noteAreaScroll.x, _noteAreaScroll.y, width, height);
		Texture2D boxTexture = new Texture2D(1, 1);
		boxTexture.SetPixel(0, 0, Color.gray);
		boxTexture.Apply();
		for(int i = 0; i < _midi.tracks.Length; i++)
		{
			if(_enableTracks[i] == false)
				continue;

			MidiNote[] notes = _midi.tracks[i].Notes.ToArray();
			for(int j = 0; j < notes.Length; j++)
			{
				float sTime = notes[j].StartTime * _midi.pulseTime * 1000f;
				float eTime = notes[j].EndTime * _midi.pulseTime * 1000f;
				Rect noteRect = new Rect(GridX * sTime, GridY * notes[j].Number, GridX * (eTime - sTime), GridY);
				if(scrollRect.Overlaps(noteRect) == true)
					GUI.DrawTexture(noteRect, boxTexture);
			}
		}

		GUI.EndScrollView();
	}

	string NoteNumberToString(int number)
	{
		int index = (int)(number % 12);
		int octave = (int)(number / 12);

		switch(index)
		{
		case 0:
			return string.Format("C{0:d}", octave);
		
		case 1:
			return string.Format("C#{0:d}", octave);
		
		case 2:
			return string.Format("D{0:d}", octave);
		
		case 3:
			return string.Format("D#{0:d}", octave);
		
		case 4:
			return string.Format("E{0:d}", octave);
		
		case 5:
			return string.Format("F{0:d}", octave);
		
		case 6:
			return string.Format("F#{0:d}", octave);
		
		case 7:
			return string.Format("G{0:d}", octave);
		
		case 8:
			return string.Format("G#{0:d}", octave);
		
		case 9:
			return string.Format("A{0:d}", octave);
		
		case 10:
			return string.Format("A#{0:d}", octave);
		
		case 11:
			return string.Format("B{0:d}", octave);
		}

		return "Unknown";
	}
}
