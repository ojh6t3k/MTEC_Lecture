using UnityEngine;
using System.Collections;


public class MidiPlayer : MonoBehaviour
{
	public MidiAsset midi;
	public AudioClip music;
	public AudioSource audioSource;
	public float playDelayTime = 0f;
	public float playSpeed = 1f;

	private bool _isPlaying = false;
	private float _playTime = 0f;
	private float _totalTime = 0f;
	private float _audioDelayTime;
	private float _midiDelayTime;
	private bool _audioStarted = false;
	private MidiTrack[] _tracks;
	private int[] _noteIndex;
	private float _pulseTime;
	private MidiEventTrigger[] _triggers;


	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		if(_isPlaying == true)
		{
			if(_audioDelayTime > 0f)
				_audioDelayTime -= Time.deltaTime;
			else
			{
				if(_audioStarted == false)
				{
					_audioStarted = true;
					if(audioSource != null)
						audioSource.Play();
				}
			}

			if(_midiDelayTime > 0f)
				_midiDelayTime -= Time.deltaTime;
			else
			{
				_playTime += (Time.deltaTime * playSpeed);
				for(int i=0; i<_tracks.Length; i++)
				{
					int noteCount = _tracks[i].Notes.Count;
					for(int j = _noteIndex[i]; j < noteCount; j++)
					{
						MidiNote note = _tracks[i].Notes[j];
						float sTime = note.StartTime * _pulseTime;
						float eTime = note.EndTime * _pulseTime;

						if(_playTime < sTime)
							break;

						// Midi Event Trigger Call
						foreach(MidiEventTrigger trigger in _triggers)
						{
							trigger.NoteOn(_tracks[i].Instrument, note.Number);
						}

						if(_playTime > eTime)
						{
							_noteIndex[i] = j + 1;

							// Midi Event Trigger Call
							foreach(MidiEventTrigger trigger in _triggers)
							{
								trigger.NoteOff(_tracks[i].Instrument, note.Number);
							}
						}
					}
				}
			}
		}	
	}

	public void Play()
	{
		if(midi == null)
			return;

		if(music != null && audioSource != null)
			audioSource.clip = music;

		_isPlaying = true;
		_playTime = 0f;
		_totalTime = midi.totalTime;
		_pulseTime = midi.pulseTime;
		if(playDelayTime == 0f)
		{
			_audioDelayTime = 0f;
			_midiDelayTime = 0f;
		}
		else if(playDelayTime > 0f)
		{
			_audioDelayTime = playDelayTime;
			_midiDelayTime = 0f;
		}
		else
		{
			_audioDelayTime = 0f;
			_midiDelayTime = -playDelayTime;
		}
		_audioStarted = false;
		_tracks = midi.tracks;
		_noteIndex = new int[_tracks.Length];
		for(int i = 0; i < _noteIndex.Length; i++)
			_noteIndex[i] = 0;

		// Find Midi Event Trigger
		_triggers = GameObject.FindObjectsOfType<MidiEventTrigger>();

		// Midi Event Trigger Call
		foreach(MidiEventTrigger trigger in _triggers)
		{
			trigger.Play();
		}
	}

	public void Pause()
	{
		_isPlaying = false;

		if(audioSource != null)
			audioSource.Pause();

		// Midi Event Trigger Call
		foreach(MidiEventTrigger trigger in _triggers)
		{
			trigger.Pause();
		}
	}

	public void Resume()
	{
		_isPlaying = true;

		if(audioSource != null)
			audioSource.UnPause();

		// Midi Event Trigger Call
		foreach(MidiEventTrigger trigger in _triggers)
		{
			trigger.Resume();
		}
	}

	public void Stop()
	{
		_isPlaying = false;
		_playTime = 0f;
		_audioStarted = false;

		if(audioSource != null)
			audioSource.Stop();

		// Midi Event Trigger Call
		foreach(MidiEventTrigger trigger in _triggers)
		{
			trigger.Stop();
		}
	}

	public bool isPlaying
	{
		get
		{
			return _isPlaying;
		}
	}

	public float playTime
	{
		get
		{
			return _playTime;
		}
	}

	public float totalTime
	{
		get
		{
			return _totalTime;
		}
	}
}
