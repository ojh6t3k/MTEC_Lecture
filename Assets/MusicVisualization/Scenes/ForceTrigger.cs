using UnityEngine;
using System.Collections;

public class ForceTrigger : MidiEventTrigger
{
	private Rigidbody _rigidbody;

	// Use this for initialization
	void Start ()
	{
		_rigidbody = GetComponent<Rigidbody>();	
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
		Vector3 force = Vector3.up;
		force *= 1f;
		_rigidbody.AddForce(force, ForceMode.Impulse);
	}

	protected override void OnNoteOff()
	{
		
	}
}
