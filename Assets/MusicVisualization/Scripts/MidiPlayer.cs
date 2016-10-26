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


	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		if(_isPlaying == true)
		{
			_playTime += Time.deltaTime;
		}	
	}

	public void Play()
	{
		if(midi == null)
			return;
		
		_isPlaying = true;
		_playTime = 0f;
		_totalTime = midi.totalTime;
	}

	public void Pause()
	{
		_isPlaying = false;
	}

	public void Resume()
	{
		_isPlaying = true;
	}

	public void Stop()
	{
		_isPlaying = false;
		_playTime = 0f;
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
