using UnityEngine;
using System.Collections;

public class MyNode : MonoBehaviour
{
	public Color color = Color.white;
	public float size = 1f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	void OnDrawGizmos()
	{
		Gizmos.color = color;

		Gizmos.DrawSphere(transform.position, size);
	}
}
