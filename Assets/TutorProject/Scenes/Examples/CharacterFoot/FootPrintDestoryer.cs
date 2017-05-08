using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPrintDestoryer : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		Invoke("DestroyObject", 1f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void DestroyObject()
	{
		DestroyImmediate(gameObject);
	}
}
