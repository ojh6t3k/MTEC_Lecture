using UnityEngine;
using System.Collections;

public class MidiAsset : ScriptableObject
{
	[SerializeField]
	private MidiFile _midiFile;

	public void FileLoad(string path)
	{
		_midiFile = new MidiFile(path);
	}

	public string fileName
	{
		get
		{
			return _midiFile.FileName;
		}
	}

	public float pulseTime
	{
		get
		{
			return (_midiFile.Time.Tempo / _midiFile.Time.Quarter) / 1000000f;
		}
	}

	public float totalTime
	{
		get
		{
			return _midiFile.TotalPulses * pulseTime;
		}
	}

	public int numerator
	{
		get
		{
			return _midiFile.Time.Numerator;
		}
	}

	public int denominator
	{
		get
		{
			return _midiFile.Time.Denominator;
		}
	}

	public int PPQN
	{
		get
		{
			return _midiFile.Time.Quarter;
		}
	}

	public int BPM
	{
		get
		{
			return (int)(60000000f / _midiFile.Time.Tempo);
		}
	}
}
