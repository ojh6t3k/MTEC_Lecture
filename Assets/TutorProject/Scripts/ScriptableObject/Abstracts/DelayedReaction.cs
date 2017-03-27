using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DelayedReaction : Reaction
{
	public float delay;

	protected WaitForSeconds wait;

	public new void Init()
	{
		wait = new WaitForSeconds (delay);

		SpecificInit ();
	}
		
	public new void React(MonoBehaviour monoBehaviour)
	{
		monoBehaviour.StartCoroutine(ReactCoroutine());
	}

	protected IEnumerator ReactCoroutine()
	{
		yield return wait;

		ImmediateReaction();
	}
}
