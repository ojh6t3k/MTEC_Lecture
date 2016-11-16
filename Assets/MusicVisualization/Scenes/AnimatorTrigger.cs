using UnityEngine;
using System.Collections;

public class AnimatorTrigger : MidiEventTrigger
{
	private Animator _animator;

	// Use this for initialization
	void Start ()
	{
		_animator = GetComponent<Animator>();	
	}

	// Update is called once per frame
	void Update () {

	}

	protected override void OnPlay()
	{
	}

	protected override void OnPause()
	{

	}

	protected override void OnResume()
	{

	}

	protected override void OnStop()
	{

	}

	protected override void OnNoteOn()
	{
		_animator.SetTrigger("Hit");
	}

	protected override void OnNoteOff()
	{

	}
}
