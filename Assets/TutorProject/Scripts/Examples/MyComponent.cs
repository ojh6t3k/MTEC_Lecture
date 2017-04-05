using UnityEngine;
using System.Collections;


[AddComponentMenu("MyComponentMenu/MyComponent")]
public class MyComponent : MonoBehaviour
{
	public int intVariable;
	public float floatVariable;
	public GameObject[] gameObjectList;
	public Texture2D image;

	[SerializeField]
	private int _intVar;

	// Use this for initialization
	void Start ()
	{
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
