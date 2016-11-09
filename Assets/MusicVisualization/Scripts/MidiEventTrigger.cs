using UnityEngine;
using System.Collections;

public class MidiEventTrigger : MonoBehaviour
{
	public bool[] instrumentFilter = new bool[129];
	public bool[] noteFilter = new bool[128];
	
	private bool _noteOn = false;

	public void Play()
	{
		_noteOn = false;
		OnPlay();
	}

	public void Pause()
	{
		OnPause();
	}

	public void Resume()
	{
		OnResume();
	}

	public void Stop()
	{
		OnStop();
	}

	public void NoteOn(int instrument, int noteNumber)
	{
		if(_noteOn == false)
		{
			_noteOn = true;

			if(instrumentFilter[instrument] == true && noteFilter[noteNumber] == true)
			{
				Debug.Log("NoteOn");
				OnNoteOn();
			}
		}
	}

	public void NoteOff(int instrument, int noteNumber)
	{
		_noteOn = false;

		if(instrumentFilter[instrument] == true && noteFilter[noteNumber] == true)
		{
			Debug.Log("NoteOff");
			OnNoteOff();
		}
	}

	protected virtual void OnPlay()
	{
	}

	protected virtual void OnPause()
	{
	}

	protected virtual void OnResume()
	{
	}

	protected virtual void OnStop()
	{
	}

	protected virtual void OnNoteOn()
	{
	}

	protected virtual void OnNoteOff()
	{
	}
}
