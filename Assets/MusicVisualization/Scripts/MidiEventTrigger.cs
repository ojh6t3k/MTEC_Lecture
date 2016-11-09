using UnityEngine;
using System.Collections;

public class MidiEventTrigger : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Play()
	{
		Debug.Log("MidiEventTrigger - Play");
	}

	public void Pause()
	{
		Debug.Log("MidiEventTrigger - Pause");
	}

	public void Resume()
	{
		Debug.Log("MidiEventTrigger - Resume");
	}

	public void Stop()
	{
		Debug.Log("MidiEventTrigger - Stop");
	}

	public void NoteOn(int instrument, int noteNumber)
	{
		Debug.Log(string.Format("MidiEventTrigger - NoteOn {0:d}, {1:d}", instrument, noteNumber));
	}

	public void NoteOff(int instrument, int noteNumber)
	{
		Debug.Log(string.Format("MidiEventTrigger - NoteOn {0:d}, {1:d}", instrument, noteNumber));
	}
}
