using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResettableScriptableObject : ScriptableObject
{
	public abstract void Reset ();
}
