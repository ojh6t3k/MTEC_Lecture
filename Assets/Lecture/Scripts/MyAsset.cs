using UnityEngine;
using System.Collections;

public class MyAsset : ScriptableObject
{
	public int intVar = 0;
	public float floatVar = 0f;

	public void Revert()
	{
		intVar = 0;
		floatVar = 0f;
	}
}
