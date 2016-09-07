using UnityEngine;
using System.Collections;

public class MyComponent : MonoBehaviour
{
	public int intVariable;
	public float floatVariable;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public int IntVar
	{
		get
		{
			return intVariable;
		}
		set
		{
			intVariable = value;
		}
	}

	public void DoSomething()
	{
		intVariable++;
	}
}
