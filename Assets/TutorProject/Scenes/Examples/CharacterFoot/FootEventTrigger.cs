using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootEventTrigger : MonoBehaviour
{
	public GameObject footprintPrefab;
	public Transform leftFoot;
	public Transform rightFoot;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnFoot(int val)
	{
		GameObject footClone = GameObject.Instantiate(footprintPrefab);

		if(val == 0)
		{
			footClone.transform.position = leftFoot.position;
			footClone.transform.rotation = leftFoot.rotation;
		}
		else if(val == 1)
		{
			footClone.transform.position = rightFoot.position;
			footClone.transform.rotation = rightFoot.rotation;
		}
	}
}
