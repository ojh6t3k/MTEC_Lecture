using UnityEngine;
using System.Collections;


[AddComponentMenu("Effects/MyComponent Script")]
public class MyComponent : MonoBehaviour
{
	public int intVariable;
	public float floatVariable;
	public GameObject[] gameObjectList;

	[SerializeField]
	private int _intVar;

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
			return _intVar;
		}
		set
		{
			_intVar = value;
		}
	}

	public void DoSomething()
	{
		intVariable++;
	}
}
