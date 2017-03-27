using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{  
	public float smoothing = 7f;
	public Vector3 offset = new Vector3 (0f, 1.5f, 0f);
	public Transform player;

	void Start ()
	{
		transform.rotation = Quaternion.LookRotation(player.position - transform.position + offset);		
	}

	void Update ()
	{
		Quaternion newRotation = Quaternion.LookRotation (player.position - transform.position + offset);
		transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * smoothing);
	}
}
